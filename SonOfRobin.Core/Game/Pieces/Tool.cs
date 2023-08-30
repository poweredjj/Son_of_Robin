using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tool : BoardPiece
    {
        public int hitCooldown;

        public Tool(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int maxHitPoints, string readableName, string description,
            byte animSize = 0, string animName = "default", bool rotatesWhenDropped = true, List<Buff> buffList = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty)
        {
            this.hitCooldown = 0; // earliest world.currentUpdate, when hitting will be possible
        }

        public Projectile CheckForAmmo(bool removePiece)
        {
            // toolStorage should be checked first (so that the player could place preferred ammo there)
            var ammoStorages = new List<PieceStorage> { this.world.Player.ToolStorage, this.world.Player.PieceStorage };

            foreach (PieceStorage ammoStorage in ammoStorages)
            {
                foreach (PieceTemplate.Name projectileName in this.pieceInfo.toolCompatibleAmmo)
                {
                    BoardPiece projectilePiece = ammoStorage.GetFirstPieceOfName(name: projectileName, removePiece: removePiece);
                    if (projectilePiece != null) return (Projectile)projectilePiece;
                }
            }

            return null;
        }

        private void ShootProjectile(int shootingPower)
        {
            Projectile projectile = this.CheckForAmmo(removePiece: true);
            if (projectile == null) return;

            if (!this.pieceInfo.toolIndestructible)
            {
                this.HitPoints = Math.Max(0, this.HitPoints - this.world.random.Next(1, 5));
                if (this.HitPoints == 0) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BrokenItem, ignoreDelay: true, text: this.readableName, texture: this.sprite.AnimFrame.texture);
            }

            float angle = this.world.Player.ShootingAngle;

            int offsetDist = 1;
            int movementDist = 1000;

            Vector2 offset = new((int)Math.Round(offsetDist * Math.Cos(angle)), (int)Math.Round(offsetDist * Math.Sin(angle)));
            Vector2 movement = new((int)Math.Round(movementDist * Math.Cos(angle)), (int)Math.Round(movementDist * Math.Sin(angle)));

            Sprite playerSprite = this.world.Player.sprite;
            Vector2 startingPos = playerSprite.position + new Vector2(offset.X * playerSprite.AnimFrame.colWidth * 1.5f, offset.Y * playerSprite.AnimFrame.colHeight * 1.5f);
            projectile.GetThrown(startPosition: startingPos, movement: movement, hitPowerMultiplier: (this.pieceInfo.toolMultiplierByCategory[Category.Flesh] * 3f) + this.world.Player.strength, shootingPower: shootingPower);
        }

        public void Use(List<BoardPiece> targets, int shootingPower = 0, bool highlightOnly = false)
        {
            Player player = this.world.Player;
            bool isVeryTired = player.IsVeryTired;

            if (this.pieceInfo.toolShootsProjectile)
            {
                if (isVeryTired) shootingPower /= 2;
                this.ShootProjectile(shootingPower);
                return;
            }

            if (targets.Count == 0 || this.world.CurrentUpdate < this.hitCooldown) return;

            bool anyTargetHit = false;
            bool fieldTipShown = false;

            var targetsForCurrentTool = targets.Where(target => this.pieceInfo.toolMultiplierByCategory.ContainsKey(target.pieceInfo.category));
            var targetsThatCanBeHit = targetsForCurrentTool.Where(target => target.canBeHit).ToList();
            var targetsThatCannotBeHit = targetsForCurrentTool.Where(target => !target.canBeHit).ToList();

            if (!highlightOnly && targetsThatCanBeHit.Count == 0 && targetsThatCannotBeHit.Count > 0)
            {
                string piecesText = targetsThatCannotBeHit.Count == 1 ? targetsThatCannotBeHit[0].readableName : $"these targets ({targetsThatCannotBeHit.Count})";

                var confirmationData = new Dictionary<string, Object> { { "question", $"Do you really want to hit {piecesText}?" }, { "taskName", Scheduler.TaskName.AllowPiecesToBeHit }, { "executeHelper", targetsThatCannotBeHit }, { "blocksUpdatesBelow", true } };
                MenuTemplate.CreateConfirmationMenu(confirmationData: confirmationData);

                return;
            }

            if (highlightOnly)
            {
                foreach (BoardPiece target in targetsForCurrentTool)
                {
                    Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Hit, world: this.world);
                    target.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: target.sprite.AnimFrame.textureSize, priority: 0));

                    VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                    ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                    if (!fieldTipShown)
                    {
                        FieldTip.AddUpdateTip(world: this.world, texture: InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece), targetSprite: target.sprite, alignment: FieldTip.Alignment.RightIn);
                        fieldTipShown = true;
                    }
                }

                return;
            }

            foreach (BoardPiece currentTarget in targetsThatCanBeHit)
            {
                float currentMultiplier = this.pieceInfo.toolMultiplierByCategory.ContainsKey(currentTarget.pieceInfo.category) ? this.pieceInfo.toolMultiplierByCategory[currentTarget.pieceInfo.category] : 0;
                if (isVeryTired) currentMultiplier /= 2;
                if (currentMultiplier == 0) continue;

                switch (currentTarget.pieceInfo.category)
                {
                    case Category.Wood:
                        this.world.HintEngine.Disable(PieceHint.Type.WoodNegative);
                        this.world.HintEngine.Disable(PieceHint.Type.WoodPositive);
                        this.world.HintEngine.Disable(Tutorials.Type.GetWood);

                        if (currentTarget.name == PieceTemplate.Name.CrateRegular) this.world.HintEngine.Disable(PieceHint.Type.CrateAnother);
                        break;

                    case Category.Stone:
                        this.world.HintEngine.Disable(PieceHint.Type.StoneNegative);
                        this.world.HintEngine.Disable(PieceHint.Type.StonePositive);
                        this.world.HintEngine.Disable(Tutorials.Type.Mine);
                        break;

                    case Category.Crystal:
                        this.world.HintEngine.Disable(PieceHint.Type.CrystalNegative);
                        this.world.HintEngine.Disable(PieceHint.Type.CrystalPositive);
                        break;

                    case Category.Metal:
                        break;

                    case Category.SmallPlant:
                        break;

                    case Category.Flesh:
                        this.world.HintEngine.Disable(PieceHint.Type.AnimalNegative);
                        if (this.readableName.Contains("spear")) this.world.HintEngine.Disable(PieceHint.Type.AnimalSpear);
                        if (this.readableName.Contains("axe")) this.world.HintEngine.Disable(PieceHint.Type.AnimalAxe);
                        break;

                    case Category.Leather:
                        break;

                    case Category.Indestructible:
                        break;

                    case Category.Dirt:
                        this.world.HintEngine.Disable(PieceHint.Type.DigSiteNegative);
                        this.world.HintEngine.Disable(PieceHint.Type.DigSitePositive);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported targetCategory - {currentTarget.pieceInfo.category}.");
                }

                if (this.name == PieceTemplate.Name.KnifeSimple) this.world.HintEngine.Disable(Tutorials.Type.BreakThing);
                if (currentTarget.name == PieceTemplate.Name.CoalDeposit)
                {
                    this.world.HintEngine.Disable(PieceHint.Type.CoalDepositNegative);
                    this.world.HintEngine.Disable(PieceHint.Type.CoalDepositPositive);
                }
                if (currentTarget.name == PieceTemplate.Name.IronDeposit)
                {
                    this.world.HintEngine.Disable(PieceHint.Type.IronDepositNegative);
                    this.world.HintEngine.Disable(PieceHint.Type.IronDepositPositive);
                }
                this.world.HintEngine.Disable(Tutorials.Type.Hit);

                int currentHitPower = (int)Math.Max(this.world.Player.strength * currentMultiplier, 1);
                HitTarget(player: player, target: currentTarget, hitPower: currentHitPower, targetPushMultiplier: 1f, buffList: this.buffList);
                anyTargetHit = true;
            } // target iteration end

            if (anyTargetHit)
            {
                this.hitCooldown = this.world.CurrentUpdate + this.pieceInfo.toolHitCooldown;
                if (!this.pieceInfo.toolIndestructible)
                {
                    this.HitPoints -= player.Skill == Player.SkillName.Maintainer ? 0.7f : 1f;
                    this.HitPoints = Math.Max(0, this.HitPoints);

                    if (this.HitPointsPercent < 0.4f && this.HitPoints > 0) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BreakingItem, ignoreDelay: true, text: this.readableName, texture: this.sprite.AnimFrame.texture);

                    if (this.HitPoints == 0) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BrokenItem, ignoreDelay: true, text: this.readableName, texture: this.sprite.AnimFrame.texture);
                }
            }
        }

        public static void HitTarget(Player player, BoardPiece target, int hitPower, float targetPushMultiplier, List<Buff> buffList = null)
        {
            World world = player.world;

            BoardPiece attackEffect = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.Attack);
            new Tracking(world: world, targetSprite: target.sprite, followingSprite: attackEffect.sprite);

            if (target.GetType() == typeof(Plant))
            {
                Plant plant = (Plant)target;

                plant.DropFruit(showMessage: false);
                if (target.Mass < plant.pieceInfo.plantAdultSizeMass) hitPower *= 3; // making the impression of the plant being weaker, without messing with its max HP
            }

            target.HitPoints -= hitPower;
            if (buffList != null)
            {
                foreach (Buff buff in buffList)
                {
                    target.buffEngine.AddBuff(buff: buff, world: world);
                }
            }

            if (target.pieceInfo.Yield != null && !target.IsBurning) target.pieceInfo.Yield.DropFirstPieces(piece: target, hitPower: hitPower);
            if (target.GetType() == typeof(Animal) && world.random.Next(2) == 0) PieceTemplate.CreateAndPlaceOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

            if (target.HitPoints <= 0 || (!target.alive && target.GetType() == typeof(Animal)))
            {
                if (target.GetType() == typeof(Animal))
                {
                    PieceTemplate.CreateAndPlaceOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);
                    target.pieceInfo.Yield.DropDebris(piece: target, particlesToEmit: 100);
                    if (target.alive) target.Kill(); // has to be killed now...
                    target.world.worldEventManager.RemovePieceFromQueue(pieceToRemove: target); // ...to remove from queue (making sure animal corpse won't disappear)
                }
                else
                {
                    Grid.RemoveFromGroup(sprite: target.sprite, groupName: Cell.Group.ColMovement); // to ensure proper yield placement
                    if (target.pieceInfo.Yield != null && target.exists && !target.IsBurning)
                    {
                        float countMultiplier = player.Skill == Player.SkillName.Plunderer && world.random.Next(8) == 0 ? 1.5f : 1f;
                        int droppedPiecesCount = target.pieceInfo.Yield.DropFinalPieces(piece: target, countMultiplier: countMultiplier);

                        if (target.pieceInfo.category == Category.Dirt && droppedPiecesCount > 1) // hole is the first "dropped" piece, so the real "count" starts at 2
                        {
                            BoardPiece particleEmitter = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.ParticleEmitter, precisePlacement: true);
                            particleEmitter.sprite.AssignNewPackage(AnimData.PkgName.WhiteSpotLayerZero);
                            ParticleEngine.TurnOn(sprite: particleEmitter.sprite, preset: ParticleEngine.Preset.Excavated, update: true, duration: 6, particlesToEmit: droppedPiecesCount * 5);
                        }

                        world.HintEngine.CheckForPieceHintToShow(ignorePlayerState: true, typesToCheckOnly: new List<PieceHint.Type> { PieceHint.Type.TreasureJar });
                    }
                }

                if (target.GetType() == typeof(Plant))
                {
                    ((Plant)target).DropSeeds();

                    target.PieceStorage?.DropAllPiecesToTheGround(addMovement: true); // plants do not drop their contents (fruits) by themselves
                }

                target.soundPack.Play(PieceSoundPack.Action.IsDestroyed);
                new RumbleEvent(force: 0.3f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.5f);

                if (target.GetType() != typeof(Animal)) target.Destroy();

                int numberOfExplosions = world.random.Next(5, 12);
                if (target.pieceInfo.category == Category.SmallPlant) numberOfExplosions = 0;
                for (int i = 0; i < numberOfExplosions; i++)
                {
                    Vector2 posOffset = new(world.random.Next(target.sprite.AnimFrame.gfxWidth), world.random.Next(target.sprite.AnimFrame.gfxHeight));
                    posOffset += target.sprite.AnimFrame.gfxOffset;

                    var attack = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: target.sprite.position + posOffset, templateName: PieceTemplate.Name.Attack);
                    attack.sprite.color = Color.LightSteelBlue;
                }
            }
            else
            {
                target.soundPack.Play(PieceSoundPack.Action.IsHit);
                new RumbleEvent(force: 0.25f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.2f);

                target.showStatBarsTillFrame = world.CurrentUpdate + 1200;

                if (target.GetType() == typeof(Animal))
                {
                    if (target.HitPointsPercent < 0.4f || world.random.Next(2) == 0) target.soundPack.Play(PieceSoundPack.Action.Cry);

                    Animal animalTarget = (Animal)target;

                    animalTarget.target = player;
                    animalTarget.aiData.Reset();

                    if (animalTarget.HitPointsPercent <= 0.4f && world.random.Next(6) == 0) animalTarget.activeState = State.AnimalCallForHelp;
                    else
                    {
                        double hitPowerPercent = (double)hitPower / animalTarget.maxHitPoints;
                        double fleeChance = hitPowerPercent * 1.6666;
                        bool flee = world.random.NextSingle() <= fleeChance;

                        bool willAttackPlayer = !flee &&
                            (animalTarget.Eats.Contains(world.PlayerName) ||
                            animalTarget.world.random.NextSingle() < animalTarget.pieceInfo.animalRetaliateChance);

                        // MessageLog.AddMessage(msgType: MsgType.User, message: $"hitPowerPercent {hitPowerPercent} fleeChance {fleeChance} flee {flee}"); // for testing

                        if (willAttackPlayer)
                        {
                            animalTarget.MakeAnimalEatNewName(world.PlayerName);

                            if (animalTarget.HitPointsPercent <= 0.2f && world.random.Next(3) != 0 ||
                                world.random.Next(animalTarget.strength / 10) == 0)
                            {
                                animalTarget.activeState = State.AnimalFlee;
                            }
                            else animalTarget.activeState = State.AnimalChaseTarget;
                        }
                        else animalTarget.activeState = State.AnimalFlee;
                    }

                    animalTarget.UpdateRegenCooldown(); // animal will not heal for a while

                    var movement = (player.sprite.position - animalTarget.sprite.position) * targetPushMultiplier * -0.5f * hitPower;
                    animalTarget.AddPassiveMovement(movement: Helpers.VectorKeepBelowSetValue(vector: movement, maxVal: 400f));
                }
                else // not animal
                {
                    float hitPercentage = Math.Min((float)hitPower / (float)target.maxHitPoints, target.maxHitPoints);

                    float rotationChange = Math.Min(0.5f * hitPercentage, 0.25f);
                    if ((target.sprite.position - player.sprite.position).X > 0) rotationChange *= -1;

                    // MessageLog.AddMessage(msgType: MsgType.User, message: $"rotationChange {rotationChange} hitPower {hitPower} exists {target.exists} hp {target.HitPointsPercent}"); // for testing

                    world.swayManager.AddSwayEvent(targetSprite: target.sprite, sourceSprite: null, targetRotation: target.sprite.rotation - rotationChange, playSound: false);
                }
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // data to deserialize here
        }
    }
}