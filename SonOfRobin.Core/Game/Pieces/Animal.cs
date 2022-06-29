using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SonOfRobin
{
    public class Animal : BoardPiece
    {
        public static readonly int maxAnimalsPerName = 100; // was 40 before unlocking the limit
        public static readonly int attackDistanceDynamic = 16;
        public static readonly int attackDistanceStatic = 4;

        private readonly bool female;
        private readonly int maxMass;
        private readonly float massBurnedMultiplier;
        private readonly byte awareness;
        private readonly int matureAge;
        private readonly uint pregnancyDuration;
        private readonly byte maxChildren;
        private uint pregnancyMass;
        public int pregnancyFramesLeft;
        public bool isPregnant;
        private int attackCooldown;
        private int regenCooldown;
        private readonly int maxFedLevel;
        private int fedLevel;
        private readonly float maxStamina;
        private float stamina;
        private readonly ushort sightRange;
        public AiData aiData;
        public BoardPiece target;
        public readonly List<PieceTemplate.Name> eats;
        private readonly List<PieceTemplate.Name> isEatenBy;

        private float FedPercentage // float 0-1
        { get { return (float)this.fedLevel / (float)maxFedLevel; } }

        private float RealSpeed
        { get { return stamina > 0 ? this.speed : Math.Max(this.speed / 2, 1); } }

        public float MaxMassPercentage { get { return this.Mass / this.maxMass; } }

        public Animal(World world, Vector2 position, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, int mass, int maxMass, byte awareness, bool female, int maxAge, int matureAge, uint pregnancyDuration, byte maxChildren, float maxStamina, int maxHitPoints, ushort sightRange, string readableName, string description, List<PieceTemplate.Name> eats, int strength, float massBurnedMultiplier,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, bool fadeInAnim = true) :

            base(world: world, position: position, animPackage: animPackage, mass: mass, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, speed: speed, maxAge: maxAge, maxHitPoints: maxHitPoints, yield: yield, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, staysAfterDeath: 30 * 60, strength: strength, category: Category.Animal)
        {
            this.activeState = State.AnimalAssessSituation;
            this.target = null;
            this.maxMass = maxMass;
            this.massBurnedMultiplier = massBurnedMultiplier;
            this.awareness = awareness;
            this.female = female;
            this.matureAge = matureAge;
            this.pregnancyDuration = pregnancyDuration;
            this.pregnancyMass = 0;
            this.pregnancyFramesLeft = 0;
            this.isPregnant = false;
            this.maxChildren = maxChildren;
            this.attackCooldown = 0; // earliest world.currentUpdate, when attacking will be possible
            this.regenCooldown = 0; // earliest world.currentUpdate, when increasing hit points will be possible
            this.maxFedLevel = 1000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = maxStamina;
            this.stamina = maxStamina;
            this.sightRange = sightRange;
            this.eats = eats;
            this.isEatenBy = PieceInfo.GetIsEatenBy(this.name);
            this.aiData = new AiData();
            this.strength = strength;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["animal_female"] = this.female; // used in PieceTemplate, to create animal of correct sex
            pieceData["animal_attackCooldown"] = this.attackCooldown;
            pieceData["animal_regenCooldown"] = this.regenCooldown;
            pieceData["animal_fedLevel"] = this.fedLevel;
            pieceData["animal_pregnancyMass"] = this.pregnancyMass;
            pieceData["animal_pregnancyFramesLeft"] = this.pregnancyFramesLeft;
            pieceData["animal_isPregnant"] = this.isPregnant;
            pieceData["animal_aiData"] = this.aiData;
            pieceData["animal_target_old_id"] = this.target?.id;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            this.attackCooldown = (int)pieceData["animal_attackCooldown"];
            this.regenCooldown = (int)pieceData["animal_regenCooldown"];
            this.fedLevel = (int)pieceData["animal_fedLevel"];
            this.pregnancyMass = (uint)pieceData["animal_pregnancyMass"];
            this.pregnancyFramesLeft = (int)pieceData["animal_pregnancyFramesLeft"];
            this.isPregnant = (bool)pieceData["animal_isPregnant"];
            this.aiData = (AiData)pieceData["animal_aiData"];
            this.aiData.UpdatePosition(); // needed to update Vector2
        }

        public override void DrawStatBar()
        {
            if (!this.alive) return;

            int posX = this.sprite.gfxRect.Center.X;
            int posY = this.sprite.gfxRect.Bottom;

            new StatBar(label: "hp", value: (int)this.hitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY);

            if (Preferences.debugShowStatBars)
            {
                new StatBar(label: "stam", value: (int)this.stamina, valueMax: (int)this.maxStamina, colorMin: new Color(100, 100, 100), colorMax: new Color(255, 255, 255), posX: posX, posY: posY);
                new StatBar(label: "food", value: (int)this.fedLevel, valueMax: (int)this.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY);
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
                if (this.Mass >= this.startingMass * 2)
                { this.Mass = Math.Max(this.Mass - energyAmount, this.startingMass); }
                else
                { this.hitPoints = Math.Max(this.hitPoints - 0.05f, 0); }
            }

            this.stamina = Math.Max(this.stamina - Math.Max((energyAmount / 2), 1), 0);
        }

        public void AcquireEnergy(float energyAmount)
        {
            energyAmount *= this.efficiency;
            int massGained = Math.Max(Convert.ToInt32(energyAmount / 4), 1);

            if (this.world.currentUpdate >= this.regenCooldown) this.hitPoints = Math.Min(this.hitPoints + (energyAmount / 3), this.maxHitPoints);
            this.fedLevel = Math.Min(this.fedLevel + Convert.ToInt16(energyAmount * 2), this.maxFedLevel);
            this.stamina = Math.Min(this.stamina + 1, this.maxStamina);

            if (this.pregnancyMass > 0 && this.pregnancyMass < this.startingMass * this.maxChildren)
            { this.pregnancyMass += (uint)massGained; }
            else
            { this.Mass = Math.Min(this.Mass + massGained, this.maxMass); }
        }

        public List<BoardPiece> AssessAsMatingPartners(List<BoardPiece> pieces)
        {
            var sameSpecies = pieces.Where(piece =>
                piece.name == this.name &&
                piece.alive &&
                piece.exists
                ).ToList();

            List<Animal> matingPartners = sameSpecies.Cast<Animal>().ToList();

            matingPartners = matingPartners.Where(animal =>
            animal.female != this.female &&
            animal.pregnancyMass == 0 &&
            animal.currentAge >= animal.matureAge
            ).ToList();

            return matingPartners.Cast<BoardPiece>().ToList();
        }

        public List<BoardPiece> GetSeenPieces()
        { return world.grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: this.sightRange); }

        private void UpdateAttackCooldown()
        {
            this.attackCooldown = this.world.currentUpdate + 20;
        }

        public void UpdateRegenCooldown()
        {
            this.regenCooldown = this.world.currentUpdate + (60 * 60);
        }

        public override void SM_AnimalAssessSituation()
        {
            this.target = null;
            this.sprite.CharacterStand();

            if (this.world.random.Next(0, 5) != 0) return; // to avoid processing this (very heavy) state every frame

            if (this.isPregnant && this.pregnancyFramesLeft <= 0)
            {
                this.activeState = State.AnimalGiveBirth;
                this.aiData.Reset(this);
                return;
            }

            if (this.world.random.Next(0, 30) == 0) // to avoid getting blocked
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset(this);
                this.aiData.SetTimeLeft(700);
                return;
            }

            // looking around

            List<BoardPiece> seenPieces = this.GetSeenPieces().Where(piece => piece.exists).ToList();

            if (seenPieces.Count == 0)
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset(this);
                this.aiData.SetTimeLeft(16000);
                this.aiData.SetDontStop(true);
                return;
            }

            // looking for enemies

            float enemyDistance = 10000000;
            BoardPiece enemyPiece = null;

            var enemyList = seenPieces.Where(piece => this.isEatenBy.Contains(piece.name)).ToList();
            if (enemyList.Count > 0)
            {
                enemyPiece = FindClosestPiece(sprite: this.sprite, pieceList: enemyList);
                enemyDistance = Vector2.Distance(this.sprite.position, enemyPiece.sprite.position);
            }

            // looking for food

            var foodList = seenPieces.Where(piece => this.eats.Contains(piece.name) && piece.Mass > 0 && this.sprite.allowedFields.CanStandHere(world: this.world, position: piece.sprite.position)).ToList();

            BoardPiece foodPiece = null;

            if (foodList.Count > 0)
            {
                var deadFoodList = foodList.Where(piece => !piece.alive).ToList();
                if (deadFoodList.Count > 0) foodList = deadFoodList;
                foodPiece = this.world.random.Next(0, 8) != 0 ? FindClosestPiece(sprite: this.sprite, pieceList: foodList) : foodList[world.random.Next(0, foodList.Count)];
            }

            // looking for mating partner

            BoardPiece matingPartner = null;
            if (this.currentAge >= matureAge && this.pregnancyMass == 0 && enemyPiece == null)
            {
                List<BoardPiece> matingPartners = this.AssessAsMatingPartners(seenPieces);
                if (matingPartners.Count > 0)
                {
                    if (this.world.random.Next(0, 8) != 0)
                    { matingPartner = FindClosestPiece(sprite: this.sprite, pieceList: matingPartners); }
                    else
                    { matingPartner = matingPartners[world.random.Next(0, matingPartners.Count)]; }

                }
            }

            // choosing what to do

            DecisionEngine decisionEngine = new DecisionEngine();
            decisionEngine.AddChoice(action: DecisionEngine.Action.Flee, piece: enemyPiece, priority: 1.2f - ((float)enemyDistance / this.sightRange));
            decisionEngine.AddChoice(action: DecisionEngine.Action.Eat, piece: foodPiece, priority: (this.hitPoints / this.maxHitPoints) > 0.3f ? 1.2f - this.FedPercentage : 1.5f);
            decisionEngine.AddChoice(action: DecisionEngine.Action.Mate, piece: matingPartner, priority: 1f);
            var bestChoice = decisionEngine.GetBestChoice();

            if (bestChoice == null)
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset(this);
                this.aiData.SetTimeLeft(10000);
                this.aiData.SetDontStop(true);
                return;
            }

            this.aiData.Reset(this);
            this.target = bestChoice.piece;

            if (Preferences.debugShowAnimalTargets)
            {
                BoardPiece crossHair = PieceTemplate.CreateOnBoard(world: world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Crosshair);
                BoardPiece backlight = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Backlight);

                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: crossHair.sprite);
                new Tracking(world: world, targetSprite: this.sprite, followingSprite: backlight.sprite, targetYAlign: YAlign.Bottom);
                crossHair.sprite.opacityFade = new OpacityFade(sprite: crossHair.sprite, destOpacity: 0, duration: 60, destroyPiece: true);
                backlight.sprite.opacityFade = new OpacityFade(sprite: backlight.sprite, destOpacity: 0, duration: 60, destroyPiece: true);

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
                        if (this.world.random.Next(0, Convert.ToInt32(animalTarget.awareness / 3)) == 0)
                        {
                            animalTarget.target = this;
                            animalTarget.aiData.Reset(this);

                            animalTarget.activeState = (animalTarget.pregnancyMass > 0 || (animalTarget.HitPointsPercent > 0.4f && world.random.Next(0, 5) == 0)) ?
                                State.AnimalChaseTarget : State.AnimalFlee; // sometimes the target will attack the predator
                        }
                    }

                    if (this.target.GetType().Equals(typeof(Player)))
                    {
                        this.visualAid = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Exclamation);
                        new Tracking(world: world, targetSprite: this.sprite, followingSprite: this.visualAid.sprite, targetYAlign: YAlign.Top, targetXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, offsetX: 0, offsetY: 5);

                        this.world.hintEngine.CheckForPieceHintToShow(forcedMode: true, typesToCheckOnly: new List<PieceHint.Type> { PieceHint.Type.RedExclamation });
                    }

                    this.activeState = State.AnimalChaseTarget;
                    break;

                case DecisionEngine.Action.Mate:
                    this.activeState = State.AnimalChaseTarget;
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported choice action - {bestChoice.action}.");
            }
        }

        public override void SM_AnimalWalkAround()
        {
            if (this.aiData.timeLeft <= 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            this.aiData.timeLeft--;

            if (this.stamina < 20)
            {
                this.activeState = State.AnimalRest;
                this.aiData.Reset(this);
                return;
            }

            if (this.aiData.Coordinates == null)
            {
                while (true)
                {
                    var coordinates = new List<int> {
                        Math.Min(Math.Max((int)this.sprite.position.X + this.world.random.Next(-2000, 2000), 0), this.world.width - 1),
                        Math.Min(Math.Max((int)this.sprite.position.Y + this.world.random.Next(-2000, 2000), 0), this.world.height - 1)
                    };
                    if (this.sprite.allowedFields.CanStandHere(world: this.world, position: new Vector2(coordinates[0], coordinates[1])))
                    {
                        this.aiData.SetCoordinates(coordinates);
                        break;
                    }
                }
            }

            bool successfullWalking = this.GoOneStepTowardsGoal(goalPosition: this.aiData.Position, splitXY: false, walkSpeed: Math.Max(this.speed / 2, 1));
            this.ExpendEnergy(Math.Max(this.RealSpeed / 6, 1));

            if (successfullWalking && Vector2.Distance(this.sprite.position, this.aiData.Position) < 10)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            // after some walking, it would be a good idea to stop and look around

            if (!successfullWalking || !this.aiData.dontStop && (this.world.random.Next(0, this.awareness * 2) == 0))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }
        }

        public override void SM_AnimalRest()
        {
            if (this.visualAid == null || this.visualAid.name != PieceTemplate.Name.Zzz)
            {
                this.visualAid = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Zzz);
                new Tracking(world: world, targetSprite: this.sprite, followingSprite: this.visualAid.sprite);
            }

            this.target = null;
            this.sprite.CharacterStand();

            this.stamina = Math.Min(this.stamina + 3, this.maxStamina);
            if (this.stamina == this.maxStamina || this.world.random.Next(0, this.awareness * 10) == 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
            }
        }

        public override void SM_AnimalChaseTarget()
        {
            if (this.target == null || !this.target.exists || Vector2.Distance(this.sprite.position, this.target.sprite.position) > this.sightRange)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            if (this.sprite.CheckIfOtherSpriteIsWithinRange(target: target.sprite, range: this.target.IsAnimalOrPlayer ? attackDistanceDynamic : attackDistanceStatic))
            {
                if (this.name == target.name && this.AssessAsMatingPartners(new List<BoardPiece> { this.target }) != null)
                {
                    this.activeState = State.AnimalMate;
                    this.aiData.Reset(this);
                    return;
                }
                else // attacking everything else, that is not mate (because it can retaliate against an enemy, that is not food)
                {
                    this.activeState = State.AnimalAttack;
                    this.aiData.Reset(this);
                    return;
                }
            }

            if (this.world.random.Next(0, this.awareness) == 0) // once in a while it is good to look around and assess situation
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            bool successfullWalking = this.GoOneStepTowardsGoal(goalPosition: this.target.sprite.position, splitXY: false, walkSpeed: this.RealSpeed);

            if (successfullWalking)
            {
                this.ExpendEnergy(Convert.ToInt32(Math.Max(this.RealSpeed / 2, 1)));
                if (this.stamina <= 0)
                {
                    this.activeState = State.AnimalRest;
                    this.aiData.Reset(this);
                    return;
                }
            }
            else
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset(this);
                this.aiData.SetTimeLeft(1000);
            }
        }

        public override void SM_AnimalAttack()
        {
            this.sprite.CharacterStand();

            if (this.target == null ||
                !this.target.exists ||
                this.target.Mass <= 0 ||
                !this.sprite.CheckIfOtherSpriteIsWithinRange(target: this.target.sprite, range: this.target.IsAnimalOrPlayer ? attackDistanceDynamic : attackDistanceStatic))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            this.sprite.SetOrientationByMovement(this.target.sprite.position - this.sprite.position);

            if (!this.target.alive || !this.target.IsAnimalOrPlayer)
            {
                this.activeState = State.AnimalEat;
                this.aiData.Reset(this);
                return;
            }

            if (this.world.currentUpdate < this.attackCooldown) return;
            this.UpdateAttackCooldown();

            float targetSpeed;

            if (this.target.GetType() == typeof(Animal))
            {
                Animal animalTarget = (Animal)this.target;
                animalTarget.target = this;
                animalTarget.aiData.Reset(animalTarget);
                animalTarget.activeState = State.AnimalFlee;

                targetSpeed = (float)animalTarget.RealSpeed;
            }
            else if (this.target.GetType() == typeof(Player))
            {
                Player playerTarget = (Player)this.target;

                if (playerTarget.activeState == State.PlayerControlledSleep)
                {
                    playerTarget.WakeUp(force: true);
                    HintEngine.ShowPieceDuringPause(world: world, pieceToShow: this,
                        messageList: new List<HintMessage> { new HintMessage(text: $"{Helpers.FirstCharToUpperCase(this.readableName)} is attacking me!", boxType: HintMessage.BoxType.Dialogue) });
                }

                targetSpeed = playerTarget.speed;
                this.attackCooldown += this.world.random.Next(0, 40); // additional cooldown after attacking player
            }
            else throw new ArgumentException($"Unsupported target class - '{this.target.GetType()}'.");

            int attackChance = (int)Math.Max(Math.Min((float)targetSpeed / (float)this.RealSpeed, 30), 1); // 1 == guaranteed hit, higher values == lower chance

            if (this.world.random.Next(0, attackChance) == 0)
            {
                BoardPiece attackEffect = PieceTemplate.CreateOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Attack);
                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: attackEffect.sprite);

                if (this.world.random.Next(0, 2) == 0) PieceTemplate.CreateOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

                if (this.target.yield != null) this.target.yield.DropDebris();

                int attackStrength = Convert.ToInt32(this.world.random.Next(Convert.ToInt32(this.strength * 0.75), Convert.ToInt32(this.strength * 1.5)) * this.efficiency);
                this.target.hitPoints = Math.Max(0, this.target.hitPoints - attackStrength);
                if (this.target.hitPoints <= 0) this.target.Kill();

                Vector2 movement = (this.sprite.position - this.target.sprite.position) * -0.3f * attackStrength;
                this.target.AddPassiveMovement(movement: Helpers.VectorAbsMax(vector: movement, maxVal: 400f));

                if (this.target.GetType() == typeof(Player))
                {
                    Player playerTarget = (Player)this.target;
                    playerTarget.Fatigue += 15f;

                    Vector2 screenShake = movement * 0.02f;

                    this.world.transManager.AddMultipleTransitions(outTrans: true, duration: this.world.random.Next(4, 10), playCount: -1, replaceBaseValue: false, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.02f, paramsToChange: new Dictionary<string, float> { { "PosX", screenShake.X }, { "PosY", screenShake.Y } });

                    if (this.target.hitPoints > 0) // red screen flash if player is still alive
                    {
                        this.world.colorOverlay.color = Color.Red;
                        this.world.colorOverlay.viewParams.Opacity = 0f;
                        this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f));

                        if (!this.eats.Contains(PieceTemplate.Name.Player)) this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.AnimalCounters, ignoreDelay: true, piece: this);
                    }
                }
            }
            else // if attack had missed
            {
                BoardPiece miss = PieceTemplate.CreateOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Miss);
                new Tracking(world: world, targetSprite: this.target.sprite, followingSprite: miss.sprite);
            }
        }

        public override void SM_AnimalEat()
        {
            this.sprite.CharacterStand();

            if (this.target == null || !this.target.exists || this.target.Mass <= 0 || !this.sprite.CheckIfOtherSpriteIsWithinRange(target: this.target.sprite, range: attackDistanceDynamic))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            if (this.target.IsAnimalOrPlayer && this.target.yield != null && this.world.random.Next(0, 25) == 0)
            {
                PieceTemplate.CreateOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.Attack);
                this.target.yield.DropDebris();
                this.target.AddPassiveMovement(movement: new Vector2(this.world.random.Next(60, 130) * this.world.random.Next(-1, 1), this.world.random.Next(60, 130) * this.world.random.Next(-1, 1)));
            }

            bool eatingPlantOrFruit = !this.target.IsAnimalOrPlayer;  // meat is more nutricious than plants

            var bittenMass = Math.Min(2, this.target.Mass);
            this.AcquireEnergy(bittenMass * (eatingPlantOrFruit ? 0.5f : 6f));

            this.target.Mass = Math.Max(this.target.Mass - bittenMass, 0);

            this.sprite.SetOrientationByMovement(this.target.sprite.position - this.sprite.position);

            if (this.target.Mass <= 0)
            {
                if (this.target.IsAnimalOrPlayer && this.target.yield != null)
                {
                    PieceTemplate.CreateOnBoard(world: this.world, position: this.target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);
                    this.target.yield.DropDebris();
                }
                this.target.Destroy();
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            if ((this.Mass >= this.maxMass && this.pregnancyMass == 0) || this.world.random.Next(0, this.awareness) == 0)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }
        }

        public override void SM_AnimalMate()
        {
            this.sprite.CharacterStand();

            if (this.target == null)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            Animal animalMate = (Animal)this.target;

            if (!(this.pregnancyMass == 0 && animalMate.pregnancyMass == 0 && this.target.alive && this.sprite.CheckIfOtherSpriteIsWithinRange(animalMate.sprite, range: attackDistanceDynamic)))
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            var heart1 = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Heart);
            var heart2 = PieceTemplate.CreateOnBoard(world: world, position: animalMate.sprite.position, templateName: PieceTemplate.Name.Heart);
            new Tracking(world: world, targetSprite: this.sprite, followingSprite: heart1.sprite);
            new Tracking(world: world, targetSprite: animalMate.sprite, followingSprite: heart2.sprite);

            Animal female = this.female ? this : animalMate;
            female.pregnancyMass = 1; // starting mass should be greater than 0

            female.pregnancyFramesLeft = (int)female.pregnancyDuration;

            new WorldEvent(world: this.world, delay: (int)female.pregnancyDuration, boardPiece: female, eventName: WorldEvent.EventName.Birth);

            this.stamina = 0;
            animalMate.stamina = 0;

            this.activeState = State.AnimalRest;
            this.aiData.Reset(this);

            animalMate.activeState = State.AnimalRest;
            animalMate.aiData.Reset(animalMate);
        }

        public override void SM_AnimalGiveBirth()
        {
            this.isPregnant = false;
            this.pregnancyFramesLeft = 0;

            this.sprite.CharacterStand();

            // excess fat cam be converted to pregnancy
            if (this.Mass > this.startingMass * 2 && this.pregnancyMass < this.startingMass * this.maxChildren)
            {
                var missingMass = (this.startingMass * this.maxChildren) - this.pregnancyMass;
                var convertedFat = Convert.ToInt32(Math.Floor(Math.Min(this.Mass - (this.startingMass * 2), missingMass)));
                this.Mass -= convertedFat;
                this.pregnancyMass += (uint)convertedFat;
            }

            uint noOfChildren = Convert.ToUInt32(Math.Min(Math.Floor(this.pregnancyMass / this.startingMass), this.maxChildren));
            uint childrenBorn = 0;

            for (int i = 0; i < noOfChildren; i++)
            {
                if (this.world.pieceCountByName[this.name] >= maxAnimalsPerName)
                // to avoid processing too many animals, which are heavy on CPU
                {
                    var fat = this.pregnancyMass;
                    this.Mass = Math.Min(this.Mass + fat, this.maxMass);
                    this.pregnancyMass = 0;

                    this.activeState = State.AnimalAssessSituation;
                    this.aiData.Reset(this);
                    return;
                }

                BoardPiece child = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: this.name, generation: this.generation + 1);
                if (child.sprite.placedCorrectly)
                {
                    childrenBorn++;
                    this.pregnancyMass -= (uint)this.startingMass;

                    var backlight = PieceTemplate.CreateOnBoard(world: world, position: child.sprite.position, templateName: PieceTemplate.Name.Backlight);
                    new Tracking(world: world, targetSprite: child.sprite, followingSprite: backlight.sprite, targetXAlign: XAlign.Center, targetYAlign: YAlign.Center);
                }
            }

            if (childrenBorn > 0) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.name} has been born ({childrenBorn}).");

            if (this.pregnancyMass > this.startingMass)
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
            this.aiData.Reset(this);
        }

        public override void SM_AnimalFlee()
        {
            if (this.stamina == 0)
            {
                this.activeState = State.AnimalRest;
                this.aiData.Reset(this);
                return;
            }

            if (this.target == null || !this.target.alive)
            {
                this.activeState = State.AnimalAssessSituation;
                this.aiData.Reset(this);
                return;
            }

            // adrenaline raises maximum speed without using more energy than normal
            bool successfullRunningAway = this.GoOneStepTowardsGoal(goalPosition: this.target.sprite.position, splitXY: false, walkSpeed: Math.Max(this.speed * 1.2f, 1), runFrom: true);

            if (successfullRunningAway)
            {
                this.ExpendEnergy(Convert.ToInt32(Math.Max(this.RealSpeed / 2, 1)));
                if (Vector2.Distance(this.sprite.position, this.target.sprite.position) > 400)
                {
                    this.activeState = State.AnimalAssessSituation;
                    this.aiData.Reset(this);
                    return;
                }
            }
            else
            {
                this.activeState = State.AnimalWalkAround;
                this.aiData.Reset(this);
                this.aiData.SetTimeLeft(120);
                return;
            }
        }

    }

}