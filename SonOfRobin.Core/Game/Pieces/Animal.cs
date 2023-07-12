using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Animal : BoardPiece
    {
        public const int attackDistanceDynamic = 16;
        public const int attackDistanceStatic = 4;

        private readonly int maxMass;
        private readonly float massBurnedMultiplier;
        private readonly byte awareness;
        private readonly int matureAge;
        private readonly uint pregnancyDuration;
        private readonly byte maxChildren;
        public readonly float retaliateChance; // 0 - 1, used only for animals that do not eat player

        private int pregnancyMass;
        public int pregnancyFramesLeft;
        public bool isPregnant;
        private int attackCooldown;
        private int regenCooldown;
        private readonly int maxFedLevel;
        private int fedLevel;
        public bool IsFemale { get; private set; }
        private readonly float maxStamina;
        private float stamina;
        public readonly ushort sightRange;
        public AiData aiData;
        public BoardPiece target;

        public List<PieceTemplate.Name> Eats { get; private set; }
        public List<PieceTemplate.Name> IsEatenBy { get; private set; }

        public Animal(World world, string id, AnimData.PkgName maleAnimPkgName, AnimData.PkgName femaleAnimPkgName, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int maxMass, byte awareness, int maxAge, int matureAge, uint pregnancyDuration, byte maxChildren, float maxStamina, int maxHitPoints, ushort sightRange, string readableName, string description, List<PieceTemplate.Name> eats, int strength, float massBurnedMultiplier, float retaliateChance,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, Yield yield = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: maleAnimPkgName, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, speed: speed, maxAge: maxAge, maxHitPoints: maxHitPoints, yield: yield, readableName: readableName, description: description, staysAfterDeath: 30 * 60, strength: strength, activeState: State.AnimalAssessSituation, soundPack: soundPack, isAffectedByWind: false)
        {
            this.IsFemale = Random.Next(2) == 1;
            if (this.IsFemale) this.sprite.AssignNewPackage(femaleAnimPkgName);
            this.target = null;
            this.maxMass = maxMass;
            this.massBurnedMultiplier = massBurnedMultiplier;
            this.awareness = awareness;
            this.matureAge = matureAge;
            this.pregnancyDuration = pregnancyDuration;
            this.pregnancyMass = 0;
            this.pregnancyFramesLeft = 0;
            this.isPregnant = false;
            this.maxChildren = maxChildren;
            this.retaliateChance = retaliateChance;
            this.attackCooldown = 0; // earliest world.currentUpdate, when attacking will be possible
            this.regenCooldown = 0; // earliest world.currentUpdate, when increasing hit points will be possible
            this.maxFedLevel = 1000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = maxStamina;
            this.stamina = maxStamina;
            this.sightRange = sightRange;
            this.Eats = eats;
            this.IsEatenBy = PieceInfo.GetIsEatenBy(this.name);
            this.aiData = new AiData(this);
            this.aiData.Reset();
            this.strength = strength;
        }

        private float FedPercentage // float 0-1
        { get { return (float)this.fedLevel / (float)maxFedLevel; } }

        private float RealSpeed
        { get { return stamina > 0 ? this.speed : Math.Max(this.speed / 2, 1); } }

        public float MaxMassPercentage
        { get { return this.Mass / this.maxMass; } }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["animal_IsFemale"] = this.IsFemale;
            pieceData["animal_fedLevel"] = this.fedLevel;
            if (this.IsFemale && this.isPregnant)
            {
                pieceData["animal_isPregnant"] = this.isPregnant;
                pieceData["animal_pregnancyMass"] = this.pregnancyMass;
                pieceData["animal_pregnancyFramesLeft"] = this.pregnancyFramesLeft;
            }

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            // animPackage does not have to be reassigned - it is set by Sprite.Deserialize()
            this.IsFemale = (bool)pieceData["animal_IsFemale"];
            this.fedLevel = (int)(Int64)pieceData["animal_fedLevel"];
            if (pieceData.ContainsKey("animal_isPregnant")) this.isPregnant = (bool)pieceData["animal_isPregnant"];
            if (pieceData.ContainsKey("animal_pregnancyMass")) this.pregnancyMass = (int)(Int64)pieceData["animal_pregnancyMass"];
            if (pieceData.ContainsKey("animal_pregnancyFramesLeft")) this.pregnancyFramesLeft = (int)(Int64)pieceData["animal_pregnancyFramesLeft"];
            this.activeState = State.AnimalAssessSituation; // to avoid using (non-serialized) aiData
        }

        public override void DrawStatBar()
        {
            if (!this.alive) return;

            int posX = this.sprite.GfxRect.Center.X;
            int posY = this.sprite.GfxRect.Bottom;

            new StatBar(label: "hp", value: (int)this.HitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, texture: AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

            if (Preferences.debugShowStatBars)
            {
                new StatBar(label: "stam", value: (int)this.stamina, valueMax: (int)this.maxStamina, colorMin: new Color(100, 100, 100), colorMax: new Color(255, 255, 255), posX: posX, posY: posY, texture: AnimData.framesForPkgs[AnimData.PkgName.Biceps].texture);
                new StatBar(label: "food", value: (int)this.fedLevel, valueMax: (int)this.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, texture: AnimData.framesForPkgs[AnimData.PkgName.Burger].texture);
                new StatBar(label: "age", value: (int)this.currentAge, valueMax: (int)this.maxAge, colorMin: new Color(180, 0, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY);
                new StatBar(label: "weight", value: (int)this.Mass, valueMax: (int)this.maxMass, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY);
            }

            StatBar.FinishThisBatch();
        }

        public void ExpendEnergy(float energyAmount)
        {
            energyAmount *= massBurnedMultiplier;

            if (this.fedLevel > 0)
            {
                this.fedLevel = Convert.ToInt16(Math.Max(this.fedLevel - Math.Max(energyAmount / 2, 1), 0));
            }
            else // burning "fat"
            {
                if (this.Mass >= this.pieceInfo.startingMass * 2)
                { this.Mass = Math.Max(this.Mass - energyAmount, this.pieceInfo.startingMass); }
                else
                { this.HitPoints = Math.Max(this.HitPoints - 0.05f, 0); }
            }

            this.stamina = Math.Max(this.stamina - Math.Max((energyAmount / 2), 1), 0);
        }

        public void AcquireEnergy(float energyAmount)
        {
            energyAmount *= this.efficiency;
            int massGained = Math.Max(Convert.ToInt32(energyAmount / 4), 1);

            if (this.world.CurrentUpdate >= this.regenCooldown) this.HitPoints += energyAmount / 3;
            this.fedLevel = Math.Min(this.fedLevel + Convert.ToInt16(energyAmount * 2), this.maxFedLevel);
            this.stamina = Math.Min(this.stamina + 1, this.maxStamina);

            if (this.pregnancyMass > 0 && this.pregnancyMass < this.pieceInfo.startingMass * this.maxChildren)
            { this.pregnancyMass += massGained; }
            else
            { this.Mass = Math.Min(this.Mass + massGained, this.maxMass); }
        }

        public IEnumerable<BoardPiece> AssessAsMatingPartners(IEnumerable<BoardPiece> pieces)
        {
            var sameSpecies = pieces.Where(piece =>
                piece.name == this.name &&
                piece.alive &&
                piece.exists
                );

            var matingPartners = sameSpecies.Cast<Animal>();

            matingPartners = matingPartners.Where(animal =>
            animal.IsFemale != this.IsFemale &&
            animal.pregnancyMass == 0 &&
            animal.currentAge >= animal.matureAge
            );

            return matingPartners;
        }

        public IEnumerable<BoardPiece> GetSeenPieces()
        { return world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: this.sightRange); }

        private void UpdateAttackCooldown()
        {
            this.attackCooldown = this.world.CurrentUpdate + 20;
        }

        public void UpdateRegenCooldown()
        {
            this.regenCooldown = this.world.CurrentUpdate + (60 * 60);
        }

        public void MakeAnimalEatNewName(PieceTemplate.Name name)
        {
            if (!this.Eats.Contains(name)) this.Eats.Add(name);
            if (this.IsEatenBy.Contains(name)) this.IsEatenBy.Remove(name);
        }

        public override void SM_AnimalAssessSituation()
        {
            this.target = null;
            this.sprite.CharacterStand();

            if (this.world.random.Next(0, 5) != 0) return; // to avoid processing this (very heavy) state every frame

            if (this.isPregnant && this.pregnancyFramesLeft <= 0)
            {
                this.activeState = State.AnimalGiveBirth;
                this.aiData.Reset();
                return;
            }

            if (this.world.random.Next(0, 30) == 0) // to avoid getting blocked
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset();
                this.aiData.timeLeft = 700;
                return;
            }

            // looking around

            var seenPieces = this.GetSeenPieces().Where(piece => piece.exists);

            if (!seenPieces.Any())
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset();
                this.aiData.timeLeft = 16000;
                this.aiData.dontStop = true;
                return;
            }

            // looking for enemies

            float enemyDistance = 10000000;
            BoardPiece enemyPiece = null;

            var enemyList = seenPieces.Where(piece => this.IsEatenBy.Contains(piece.name));
            if (enemyList.Any())
            {
                enemyPiece = FindClosestPiece(sprite: this.sprite, pieceList: enemyList);
                enemyDistance = Vector2.Distance(this.sprite.position, enemyPiece.sprite.position);
            }

            // looking for food

            var foodList = seenPieces.Where(piece => this.Eats.Contains(piece.name) && piece.Mass > 0 && this.sprite.allowedTerrain.CanStandHere(world: this.world, position: piece.sprite.position));

            BoardPiece foodPiece = null;

            if (foodList.Any())
            {
                var deadFoodList = foodList.Where(piece => !piece.alive);
                if (deadFoodList.Any()) foodList = deadFoodList;
                foodPiece = this.world.random.Next(0, 8) != 0 ? FindClosestPiece(sprite: this.sprite, pieceList: foodList) : foodList.ElementAt(world.random.Next(0, foodList.Count()));
            }

            // looking for mating partner

            BoardPiece matingPartner = null;

            if (this.currentAge >= matureAge &&
                this.pregnancyMass == 0 &&
                enemyPiece == null &&
                seenPieces.Where(piece => piece.name == this.name).Count() <= 3) // will not mate in a crowded area
            {
                var matingPartners = this.AssessAsMatingPartners(seenPieces);
                if (matingPartners.Any())
                {
                    if (this.world.random.Next(0, 8) != 0)
                    { matingPartner = FindClosestPiece(sprite: this.sprite, pieceList: matingPartners); }
                    else
                    { matingPartner = matingPartners.ElementAt(world.random.Next(0, matingPartners.Count())); }
                }
            }

            // choosing what to do

            DecisionEngine decisionEngine = new DecisionEngine();
            decisionEngine.AddChoice(action: DecisionEngine.Action.Flee, piece: enemyPiece, priority: 1.2f - ((float)enemyDistance / this.sightRange));
            decisionEngine.AddChoice(action: DecisionEngine.Action.Eat, piece: foodPiece, priority: this.HitPointsPercent > 0.3f ? 1.2f - this.FedPercentage : 1.5f);
            decisionEngine.AddChoice(action: DecisionEngine.Action.Mate, piece: matingPartner, priority: 1f);
            var bestChoice = decisionEngine.GetBestChoice();

            if (bestChoice == null)
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset();
                this.aiData.timeLeft = 10000;
                this.aiData.dontStop = true;
                return;
            }

            this.aiData.Reset();
            this.target = bestChoice.piece;

            if (Preferences.debugShowAnimalTargets)
            {
                BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Crosshair);
                BoardPiece backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Backlight);

                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: crossHair.sprite);
                new Tracking(world: world, targetSprite: this.sprite, followingSprite: backlight.sprite, targetYAlign: YAlign.Bottom);
                new OpacityFade(sprite: crossHair.sprite, destOpacity: 0, duration: 60, destroyPiece: true);
                new OpacityFade(sprite: backlight.sprite, destOpacity: 0, duration: 60, destroyPiece: true);

                switch (bestChoice.action)
                {
                    case DecisionEngine.Action.Eat:
                        crossHair.sprite.color = backlight.sprite.color = Color.Red;
                        break;

                    case DecisionEngine.Action.Mate:
                        crossHair.sprite.color = backlight.sprite.color = Color.Pink;
                        break;

                    case DecisionEngine.Action.Flee:
                        crossHair.sprite.color = backlight.sprite.color = Color.Blue;
                        break;

                    default:
                        break;
                }
            }

            switch (bestChoice.action)
            {
                case DecisionEngine.Action.Flee:
                    this.activeState = State.AnimalFlee;
                    break;

                case DecisionEngine.Action.Eat:
                    if (this.target.GetType() == typeof(Animal)) // target animal has a chance to flee early
                    {
                        Animal animalTarget = (Animal)this.target;
                        if (this.world.random.Next(0, (int)(animalTarget.awareness / 3)) == 0)
                        {
                            animalTarget.target = this;
                            animalTarget.aiData.Reset();

                            animalTarget.activeState = (animalTarget.pregnancyMass > 0 || (animalTarget.HitPointsPercent <= 0.38f && world.random.Next(6) == 0)) ?
                                State.AnimalCallForHelp : State.AnimalFlee; // sometimes the target will call others to attack the predator
                        }
                    }

                    if (this.target.GetType() == typeof(Player))
                    {
                        if (this.visualAid != null) this.visualAid.Destroy();
                        this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.BubbleExclamationRed);
                        new Tracking(world: world, targetSprite: this.sprite, followingSprite: this.visualAid.sprite, targetYAlign: YAlign.Top, targetXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, offsetX: 0, offsetY: 5);

                        this.world.HintEngine.CheckForPieceHintToShow(typesToCheckOnly: new List<PieceHint.Type> { PieceHint.Type.RedExclamation }, fieldPieceNameToCheck: this.visualAid.name);
                    }

                    this.activeState = State.AnimalChaseTarget;
                    break;

                case DecisionEngine.Action.Mate:
                    this.activeState = State.AnimalChaseTarget;
                    break;

                default:
                    throw new ArgumentException($"Unsupported choice action - {bestChoice.action}.");
            }
        }

        public override void SM_AnimalWalkAround()
        {
            if (this.aiData.timeLeft <= 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            this.aiData.timeLeft--;

            if (this.stamina < 20)
            {
                this.activeState = State.AnimalRest;
                this.aiData.Reset();
                return;
            }

            if (!this.aiData.targetPosIsSet)
            {
                for (int i = 0; i < 10; i++) // trying to set target, which can be reached
                {
                    Vector2 targetPos = new Vector2(
                        Math.Min(Math.Max((int)this.sprite.position.X + this.world.random.Next(-2000, 2000), 0), this.world.width - 1),
                        Math.Min(Math.Max((int)this.sprite.position.Y + this.world.random.Next(-2000, 2000), 0), this.world.height - 1)
                        );

                    if (this.sprite.allowedTerrain.CanStandHere(world: this.world, position: targetPos))
                    {
                        this.aiData.TargetPos = targetPos;
                        break;
                    }
                }
            }

            bool successfullWalking = this.GoOneStepTowardsGoal(goalPosition: this.aiData.TargetPos, splitXY: false, walkSpeed: Math.Max(this.speed / 2, 1), slowDownOnRocks: false);
            this.ExpendEnergy(Math.Max(this.RealSpeed / 6, 1));

            if (successfullWalking && Vector2.Distance(this.sprite.position, this.aiData.TargetPos) < 10)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            // after some walking, it would be a good idea to stop and look around

            if (!successfullWalking || !this.aiData.dontStop && (this.world.random.Next(0, this.awareness * 2) == 0))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }
        }

        public override void SM_AnimalRest()
        {
            if (this.visualAid == null || this.visualAid.name != PieceTemplate.Name.Zzz)
            {
                if (this.visualAid != null) this.visualAid.Destroy();

                if (this.sprite.IsInCameraRect)
                {
                    this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Zzz);
                    new Tracking(world: world, targetSprite: this.sprite, followingSprite: this.visualAid.sprite);
                }
            }

            this.target = null;
            this.sprite.CharacterStand();

            this.stamina = Math.Min(this.stamina + 3, this.maxStamina);
            if (this.stamina == this.maxStamina || this.world.random.Next(0, this.awareness * 10) == 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
            }
        }

        public override void SM_AnimalChaseTarget()
        {
            if (this.target == null || !this.target.exists || !this.target.sprite.IsOnBoard || Vector2.Distance(this.sprite.position, this.target.sprite.position) > this.sightRange)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            if (this.sprite.CheckIfOtherSpriteIsWithinRange(target: target.sprite, range: this.target.IsAnimalOrPlayer ? attackDistanceDynamic : attackDistanceStatic))
            {
                if (this.name == target.name && this.AssessAsMatingPartners(new List<BoardPiece> { this.target }) != null)
                {
                    this.activeState = State.AnimalMate;
                    this.aiData.Reset();
                    return;
                }
                else // attacking everything else, that is not mate (because it can retaliate against an enemy, that is not food)
                {
                    this.activeState = State.AnimalAttack;
                    this.aiData.Reset();
                    return;
                }
            }

            if (this.world.random.Next(0, this.awareness) == 0) // once in a while it is good to look around and assess situation
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            bool successfullWalking = this.GoOneStepTowardsGoal(goalPosition: this.target.sprite.position, splitXY: false, walkSpeed: this.RealSpeed, slowDownOnRocks: false);

            if (successfullWalking)
            {
                this.ExpendEnergy(Convert.ToInt32(Math.Max(this.RealSpeed / 2, 1)));
                if (this.stamina <= 0)
                {
                    this.activeState = State.AnimalRest;
                    this.aiData.Reset();
                    return;
                }
            }
            else
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset();
                this.aiData.timeLeft = 1000;
            }
        }

        public override void SM_AnimalAttack()
        {
            this.sprite.CharacterStand();

            if (this.target == null ||
                !this.target.exists ||
                !this.target.sprite.IsOnBoard ||
                this.target.Mass <= 0 ||
                !this.sprite.CheckIfOtherSpriteIsWithinRange(target: this.target.sprite, range: this.target.IsAnimalOrPlayer ? attackDistanceDynamic : attackDistanceStatic))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            this.sprite.SetOrientationByMovement(this.target.sprite.position - this.sprite.position);

            if (!this.target.alive || !this.target.IsAnimalOrPlayer)
            {
                this.activeState = State.AnimalEat;
                this.aiData.Reset();
                return;
            }

            if (this.world.CurrentUpdate < this.attackCooldown) return;
            this.UpdateAttackCooldown();

            float targetSpeed;

            if (this.target.GetType() == typeof(Animal))
            {
                Animal animalTarget = (Animal)this.target;
                animalTarget.target = this;
                animalTarget.aiData.Reset();
                animalTarget.activeState = State.AnimalFlee;

                targetSpeed = (float)animalTarget.RealSpeed;
            }
            else if (this.target.GetType() == typeof(Player))
            {
                Player playerTarget = (Player)this.target;

                if (playerTarget.activeState == State.PlayerControlledSleep)
                {
                    playerTarget.Fatigue = Math.Min(playerTarget.Fatigue, playerTarget.maxFatigue * 0.8f); // to avoid hit-sleep loop
                    playerTarget.WakeUp(force: true);
                    HintEngine.ShowPieceDuringPause(world: world, pieceToShow: this,
                        messageList: new List<HintMessage> { new HintMessage(text: $"{Helpers.FirstCharToUpperCase(this.readableName)} is attacking me!", boxType: HintMessage.BoxType.Dialogue, blockInput: true) });
                }

                targetSpeed = playerTarget.speed;
                this.attackCooldown += this.world.random.Next(0, 40); // additional cooldown after attacking player
            }
            else throw new ArgumentException($"Unsupported target class - '{this.target.GetType()}'.");

            int attackChance = (int)Math.Max(Math.Min((float)targetSpeed / (float)this.RealSpeed, 30), 1); // 1 == guaranteed hit, higher values == lower chance

            if (this.world.random.Next(0, attackChance) == 0)
            {
                BoardPiece attackEffect = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Attack);
                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: attackEffect.sprite);

                if (this.world.random.Next(0, 2) == 0) PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

                if (this.target.yield != null) this.target.yield.DropDebris();

                this.soundPack.Play(PieceSoundPack.Action.Cry);

                target.soundPack.Play(PieceSoundPack.Action.IsHit);
                if (target.HitPointsPercent < 0.4f || world.random.Next(0, 2) == 0) target.soundPack.Play(PieceSoundPack.Action.Cry);

                int attackStrength = Convert.ToInt32(this.world.random.Next((int)(this.strength * 0.75f), (int)(this.strength * 1.5f)) * this.efficiency);
                this.target.HitPoints = Math.Max(0, this.target.HitPoints - attackStrength);
                if (this.target.HitPoints <= 0 && this.target.IsAnimalOrPlayer) this.target.Kill();

                Vector2 movement = (this.sprite.position - this.target.sprite.position) * -0.3f * attackStrength;
                this.target.AddPassiveMovement(movement: Helpers.VectorAbsMax(vector: movement, maxVal: 400f));

                if (this.target.GetType() == typeof(Player))
                {
                    Player playerTarget = (Player)this.target;
                    playerTarget.Fatigue += 15f;

                    Vector2 screenShake = movement * 0.02f;
                    float durationSecs = (float)(this.world.random.NextDouble() * 0.09f) + 0.12f;
                    this.world.camera.AddShake(movement: screenShake, durationSecs: durationSecs);

                    if (this.target.HitPoints > 0) // red screen flash if player is still alive
                    {
                        SolidColor redOverlay = new SolidColor(color: Color.DarkRed, viewOpacity: 0.0f);
                        redOverlay.transManager.AddTransition(new Transition(transManager: redOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f, endRemoveScene: true));
                        this.world.solidColorManager.Add(redOverlay);

                        if (!PieceInfo.ContainsPlayer(this.Eats)) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.AnimalCounters, ignoreDelay: true, piece: this);
                    }
                }
            }
            else // if attack had missed
            {
                BoardPiece miss = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Miss);
                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: miss.sprite);
            }
        }

        public override void SM_AnimalEat()
        {
            this.sprite.CharacterStand();

            if (this.target == null || !this.target.exists || !this.target.sprite.IsOnBoard || this.target.Mass <= 0 || !this.sprite.CheckIfOtherSpriteIsWithinRange(target: this.target.sprite, range: attackDistanceDynamic))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            this.soundPack.Play(PieceSoundPack.Action.Eat);

            if (this.target.IsAnimalOrPlayer && this.target.yield != null && this.world.random.Next(0, 25) == 0)
            {
                BoardPiece attackEffect = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Attack);
                new Tracking(world: this.world, targetSprite: this.target.sprite, followingSprite: attackEffect.sprite);

                this.target.yield.DropDebris();
                this.target.AddPassiveMovement(movement: new Vector2(this.world.random.Next(60, 130) * this.world.random.Next(-1, 1), this.world.random.Next(60, 130) * this.world.random.Next(-1, 1)));
            }

            bool eatingPlantOrFruit = this.target.IsPlantOrFruit;  // meat is more nutricious than plants

            var bittenMass = Math.Min(2, this.target.Mass);
            this.AcquireEnergy(bittenMass * (eatingPlantOrFruit ? 0.5f : 6f));

            this.target.Mass = Math.Max(this.target.Mass - bittenMass, 0);

            this.sprite.SetOrientationByMovement(this.target.sprite.position - this.sprite.position);

            if (this.target.Mass <= 0)
            {
                if (this.target.IsAnimalOrPlayer && this.target.yield != null)
                {
                    target.soundPack.Play(PieceSoundPack.Action.IsHit);

                    PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);
                    this.target.yield.DropDebris();
                }
                this.target.Destroy();
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            if (this.sprite.IsInCameraRect)
            {
                float targetRotation = (float)(this.world.random.NextDouble() * 0.2f) + 0.05f;
                float finalRotation = (sprite.position - this.target.sprite.position).X > 0 ? targetRotation : -targetRotation;

                this.world.swayManager.AddSwayEvent(targetSprite: this.sprite, sourceSprite: null, targetRotation: finalRotation * 0.8f, playSound: true, rotationSlowdown: 2);

                if (this.target.GetType() == typeof(Plant)) this.world.swayManager.AddSwayEvent(targetSprite: this.target.sprite, sourceSprite: null, targetRotation: finalRotation, playSound: true, rotationSlowdown: 3);
            }

            if ((this.Mass >= this.maxMass && this.pregnancyMass == 0) || this.world.random.Next(0, this.awareness) == 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }
        }

        public override void SM_AnimalMate()
        {
            this.sprite.CharacterStand();

            if (this.target == null)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            Animal animalMate = (Animal)this.target;

            if (!(this.pregnancyMass == 0 && animalMate.pregnancyMass == 0 && this.target.alive && this.target.sprite.IsOnBoard && this.sprite.CheckIfOtherSpriteIsWithinRange(animalMate.sprite, range: attackDistanceDynamic)))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            this.yield?.DropDebris(debrisTypeListOverride: new List<Yield.DebrisType> { Yield.DebrisType.Heart });
            animalMate.yield?.DropDebris(debrisTypeListOverride: new List<Yield.DebrisType> { Yield.DebrisType.Heart });

            Animal female = this.IsFemale ? this : animalMate;
            female.pregnancyMass = 1; // starting mass should be greater than 0
            female.pregnancyFramesLeft = (int)female.pregnancyDuration;

            new WorldEvent(world: this.world, delay: (int)female.pregnancyDuration, boardPiece: female, eventName: WorldEvent.EventName.Birth);

            this.stamina = 0;
            animalMate.stamina = 0;

            this.activeState = State.AnimalRest;
            this.aiData.Reset();

            animalMate.activeState = State.AnimalRest;
            animalMate.aiData.Reset();
        }

        public override void SM_AnimalGiveBirth()
        {
            this.isPregnant = false;
            this.pregnancyFramesLeft = 0;

            this.sprite.CharacterStand();

            // excess fat can be converted to pregnancy
            if (this.Mass > this.pieceInfo.startingMass * 2 && this.pregnancyMass < this.pieceInfo.startingMass * this.maxChildren)
            {
                var missingMass = (this.pieceInfo.startingMass * this.maxChildren) - this.pregnancyMass;
                var convertedFat = Convert.ToInt32(Math.Floor(Math.Min(this.Mass - (this.pieceInfo.startingMass * 2), missingMass)));
                this.Mass -= convertedFat;
                this.pregnancyMass += convertedFat;
            }

            int noOfChildren = (int)Math.Min(Math.Floor(this.pregnancyMass / this.pieceInfo.startingMass), this.maxChildren);
            int childrenBorn = 0;

            for (int i = 0; i < noOfChildren; i++)
            {
                if (this.world.pieceCountByName[this.name] >= this.world.maxAnimalsPerName)
                {
                    var fat = this.pregnancyMass;
                    this.Mass = Math.Min(this.Mass + fat, this.maxMass);
                    this.pregnancyMass = 0;

                    this.activeState = State.AnimalAssessSituation;
                    this.aiData.Reset();
                    return;
                }

                BoardPiece child = PieceTemplate.Create(world: world, templateName: this.name, generation: this.generation + 1);
                if (this.sprite.allowedTerrain.HasBeenChanged) child.sprite.allowedTerrain.CopyTerrainFromTemplate(this.sprite.allowedTerrain);
                child.PlaceOnBoard(randomPlacement: false, position: this.sprite.position, closestFreeSpot: true);

                if (child.sprite.IsOnBoard)
                {
                    childrenBorn++;
                    this.pregnancyMass -= (int)this.pieceInfo.startingMass;

                    var backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: child.sprite.position, templateName: PieceTemplate.Name.Backlight);
                    new Tracking(world: world, targetSprite: child.sprite, followingSprite: backlight.sprite, targetXAlign: XAlign.Center, targetYAlign: YAlign.Center);
                }
            }

            if (childrenBorn > 0) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.name} has been born ({childrenBorn}).");

            if (this.pregnancyMass > this.pieceInfo.startingMass)
            {
                this.isPregnant = true;
                this.pregnancyFramesLeft = 90;
            }
            else
            {
                this.Mass = Math.Min(this.Mass + this.pregnancyMass, this.maxMass);
                this.pregnancyMass = 0;
            }

            this.activeState = State.AnimalRest;
            this.aiData.Reset();
        }

        public override void SM_AnimalFlee()
        {
            if (this.stamina == 0)
            {
                this.activeState = State.AnimalRest;
                this.aiData.Reset();
                return;
            }

            if (this.target == null || !this.target.alive || !this.target.sprite.IsOnBoard)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset();
                return;
            }

            // adrenaline raises maximum speed without using more energy than normal
            bool successfullRunningAway = this.GoOneStepTowardsGoal(goalPosition: this.target.sprite.position, splitXY: false, walkSpeed: Math.Max(this.speed * 1.2f, 1), runFrom: true, slowDownOnRocks: false);

            if (successfullRunningAway)
            {
                this.ExpendEnergy((int)Math.Max(this.RealSpeed / 2, 1));
                if (Vector2.Distance(this.sprite.position, this.target.sprite.position) > 400)
                {
                    this.activeState = State.AnimalAssessSituation;
                    this.aiData.Reset();
                    return;
                }
            }
            else
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset();
                this.aiData.timeLeft = 120;
                return;
            }
        }

        public override void SM_AnimalRunForClosestWater()
        {
            if (!this.IsBurning)
            {
                this.activeState = State.AnimalRest;
                this.aiData.Reset();
                return;
            }

            if (!this.aiData.targetPosIsSet)
            {
                int searchRange = 0;
                while (true)
                {
                    searchRange += this.sightRange;

                    var cellsWithinDistance = (IEnumerable<Cell>)this.world.Grid.GetCellsWithinDistance(position: this.sprite.position, distance: searchRange);

                    bool canGoToWater = this.sprite.allowedTerrain.IsInRange(terrainName: Terrain.Name.Height, Terrain.waterLevelMax - 1);

                    if (canGoToWater) cellsWithinDistance = cellsWithinDistance
                            .Where(cell => cell.IsAllWater && cell != this.sprite.currentCell)
                            .OrderBy(cell => Vector2.Distance(this.sprite.position, cell.center)); // sorting by distance

                    if (cellsWithinDistance.Any())
                    {
                        Cell targetCell;

                        if (this.world.random.Next(0, 3) == 0 || !canGoToWater) // sometimes a random cell is chosen
                        {
                            int randomCellNo = this.world.random.Next(0, cellsWithinDistance.Count());
                            targetCell = cellsWithinDistance.ElementAt(randomCellNo);
                        }
                        else targetCell = cellsWithinDistance.First();

                        this.aiData.TargetPos = targetCell.center;
                        break;
                    }
                }
            }

            // ExpendEnergy is not used, because it is a desperate life-saving measure for an animal

            float runSpeed = Math.Min(Math.Max(this.speed * 1.7f, 1), 2.5f); // should not exceed these bounds
            bool successfullWalking = this.GoOneStepTowardsGoal(goalPosition: this.aiData.TargetPos, splitXY: false, walkSpeed: runSpeed, slowDownOnRocks: false);
            if (!successfullWalking) this.aiData.Reset();
        }

        public override void SM_AnimalCallForHelp()
        {
            this.soundPack.Play(PieceSoundPack.Action.Cry);

            var piecesWithinSoundRange = world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: this.sightRange * 3);
            var allyList = piecesWithinSoundRange.Where(piece => piece.name == this.name && piece.alive && piece.activeState != State.AnimalCallForHelp);

            bool targetIsPlayer = this.target.GetType() == typeof(Player);

            bool willFollowPlayerAnywhere = targetIsPlayer && !this.sprite.IsInWater && this.world.random.Next(4) == 0;

            if (willFollowPlayerAnywhere)
            {
                this.sprite.allowedTerrain.AddUpdateTerrain(terrainName: Terrain.Name.Height, allowedRange: new AllowedRange(min: Terrain.waterLevelMax, max: (byte)(Terrain.volcanoEdgeMin - 1)));
            }

            foreach (BoardPiece allyPiece in allyList)
            {
                Animal allyAnimal = (Animal)allyPiece;

                allyAnimal.target = this.target;
                allyAnimal.aiData.Reset();
                allyAnimal.activeState = State.AnimalChaseTarget;

                allyAnimal.MakeAnimalEatNewName(this.target.name); // the point is to kill the attacker

                if (allyAnimal.visualAid != null) this.visualAid.Destroy();

                if (targetIsPlayer)
                {
                    allyAnimal.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: allyAnimal.sprite.position, templateName: PieceTemplate.Name.BubbleExclamationRed);
                    new Tracking(world: world, targetSprite: allyAnimal.sprite, followingSprite: allyAnimal.visualAid.sprite, targetYAlign: YAlign.Top, targetXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, offsetX: 0, offsetY: 5);

                    if (willFollowPlayerAnywhere) allyAnimal.sprite.allowedTerrain.AddUpdateTerrain(terrainName: Terrain.Name.Height, allowedRange: new AllowedRange(min: Terrain.waterLevelMax, max: (byte)(Terrain.volcanoEdgeMin - 1)));
                }
            }

            this.MakeAnimalEatNewName(this.target.name);

            if (this.visualAid != null) this.visualAid.Destroy();

            if (targetIsPlayer)
            {
                this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.BubbleExclamationBlue);
                new Tracking(world: world, targetSprite: this.sprite, followingSprite: this.visualAid.sprite, targetYAlign: YAlign.Top, targetXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, offsetX: 0, offsetY: 5);
            }

            this.activeState = State.AnimalChaseTarget;

            if (this.world.random.Next(3) == 0) // a chance for a following call for help
            {
                new WorldEvent(eventName: WorldEvent.EventName.AnimalCallForHelp, world: world, delay: 250, boardPiece: this, eventHelper: this.target);
            }
        }
    }
}