using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public class Tool : BoardPiece
    {
        public readonly bool shootsProjectile;
        private readonly int hitPower;
        public readonly int range;
        public int hitCooldown;
        private readonly Dictionary<Category, float> multiplierByCategory;
        private readonly List<PieceTemplate.Name> compatibleAmmo;

        public Tool(World world, Vector2 position, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, int hitPower, Dictionary<Category, float> multiplierByCategory, int maxHitPoints, string readableName, string description, Category category,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool indestructible = false, Yield yield = null, bool shootsProjectile = false, List<PieceTemplate.Name> compatibleAmmo = null, bool rotatesWhenDropped = true, bool fadeInAnim = false, int range = 0, List<BuffEngine.Buff> buffList = null) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, indestructible: indestructible, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, buffList: buffList)
        {
            this.activeState = State.Empty;
            this.hitPower = hitPower;
            this.hitCooldown = 0; // earliest world.currentUpdate, when hitting will be possible
            this.range = range;
            this.shootsProjectile = shootsProjectile;
            this.multiplierByCategory = multiplierByCategory;
            this.toolbarTask = Scheduler.TaskName.Hit;
            this.compatibleAmmo = compatibleAmmo == null ? new List<PieceTemplate.Name> { } : compatibleAmmo;
        }

        public Projectile CheckForAmmo(bool removePiece)
        {
            // toolStorage should be checked first (so that the player could place preferred ammo there)
            var ammoStorages = new List<PieceStorage> { this.world.player.toolStorage, this.world.player.pieceStorage };

            BoardPiece projectilePiece;

            foreach (PieceStorage ammoStorage in ammoStorages)
            {
                foreach (PieceTemplate.Name projectileName in this.compatibleAmmo)
                {
                    projectilePiece = ammoStorage.GetFirstPieceOfName(name: projectileName, removePiece: removePiece);
                    if (projectilePiece != null) return (Projectile)projectilePiece;
                }

            }

            return null;
        }

        private void ShootProjectile(int shootingPower)
        {
            Projectile projectile = this.CheckForAmmo(removePiece: true);
            if (projectile == null) return;

            if (!this.indestructible) this.hitPoints = Math.Max(0, this.hitPoints - this.world.random.Next(1, 5));

            float angle = this.world.player.shootingAngle;

            int offsetDist = 1;
            int movementDist = 1000;

            Vector2 offset = new Vector2((int)Math.Round(offsetDist * Math.Cos(angle)), (int)Math.Round(offsetDist * Math.Sin(angle)));
            Vector2 movement = new Vector2((int)Math.Round(movementDist * Math.Cos(angle)), (int)Math.Round(movementDist * Math.Sin(angle)));

            Sprite playerSprite = this.world.player.sprite;
            Vector2 startingPos = playerSprite.position + new Vector2(offset.X * playerSprite.frame.colWidth * 1.5f, offset.Y * playerSprite.frame.colHeight * 1.5f);
            projectile.GetThrown(startPosition: startingPos, movement: movement, hitPowerMultiplier: this.hitPower + this.world.player.strength, shootingPower: shootingPower);
        }

        public void Use(List<BoardPiece> targets, int shootingPower = 0, bool highlightOnly = false)
        {
            Player player = this.world.player;
            bool isVeryTired = player.IsVeryTired;

            if (this.shootsProjectile)
            {
                if (isVeryTired) shootingPower /= 2;
                this.ShootProjectile(shootingPower);
                return;
            }

            if (targets.Count == 0 || this.world.currentUpdate < this.hitCooldown || player.Stamina < 80) return;

            bool anyTargetHit = false;
            bool fieldTipShown = false;

            foreach (BoardPiece currentTarget in targets)
            {
                float currentMultiplier = 0;
                if (this.multiplierByCategory.ContainsKey(currentTarget.category)) currentMultiplier = this.multiplierByCategory[currentTarget.category];
                if (isVeryTired) currentMultiplier /= 2;
                if (currentMultiplier == 0) continue;

                if (!highlightOnly)
                {
                    switch (currentTarget.category)
                    {
                        case Category.Wood:
                            this.world.hintEngine.Disable(PieceHint.Type.WoodNegative);
                            this.world.hintEngine.Disable(PieceHint.Type.WoodPositive);
                            this.world.hintEngine.Disable(Tutorials.Type.GetWood);

                            if (currentTarget.name == PieceTemplate.Name.CrateRegular) this.world.hintEngine.Disable(PieceHint.Type.CrateAnother);

                            break;

                        case Category.Stone:
                            this.world.hintEngine.Disable(PieceHint.Type.StoneNegative);
                            this.world.hintEngine.Disable(PieceHint.Type.StonePositive);
                            this.world.hintEngine.Disable(Tutorials.Type.Mine);
                            break;

                        case Category.Metal:
                            break;

                        case Category.SmallPlant:
                            break;

                        case Category.Animal:
                            this.world.hintEngine.Disable(PieceHint.Type.AnimalNegative);
                            if (this.name == PieceTemplate.Name.SpearStone) this.world.hintEngine.Disable(PieceHint.Type.AnimalBat);

                            if (this.name == PieceTemplate.Name.AxeWood ||
                                this.name == PieceTemplate.Name.AxeStone ||
                                this.name == PieceTemplate.Name.AxeIron)
                                this.world.hintEngine.Disable(PieceHint.Type.AnimalAxe);
                            break;

                        case Category.Indestructible:
                            break;

                        default:
                            throw new ArgumentException($"Unsupported targetCategory - {currentTarget.category}.");
                    }

                    if (this.name == PieceTemplate.Name.Hand) this.world.hintEngine.Disable(Tutorials.Type.BreakThing);
                    if (currentTarget.name == PieceTemplate.Name.CoalDeposit)
                    {
                        this.world.hintEngine.Disable(PieceHint.Type.CoalDepositNegative);
                        this.world.hintEngine.Disable(PieceHint.Type.CoalDepositPositive);
                    }
                    if (currentTarget.name == PieceTemplate.Name.IronDeposit)
                    {
                        this.world.hintEngine.Disable(PieceHint.Type.IronDepositNegative);
                        this.world.hintEngine.Disable(PieceHint.Type.IronDepositPositive);
                    }
                    this.world.hintEngine.Disable(Tutorials.Type.Hit);
                }

                if (highlightOnly)
                {
                    Tutorials.ShowTutorial(type: Tutorials.Type.Hit, ignoreIfShown: true, ignoreDelay: false);
                    currentTarget.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: currentTarget.sprite.frame.textureSize, priority: 0));

                    VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                    ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                    if (!fieldTipShown)
                    {
                        FieldTip.AddUpdateTip(world: this.world, texture: InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece), targetSprite: currentTarget.sprite, alignment: FieldTip.Alignment.RightIn);
                        fieldTipShown = true;
                    }
                }
                else
                {
                    int currentHitPower = (int)Math.Max((this.hitPower + this.world.player.strength) * currentMultiplier, 1);
                    HitTarget(attacker: player, target: currentTarget, hitPower: currentHitPower, targetPushMultiplier: 1f, buffList: this.buffList);
                    anyTargetHit = true;
                }
            } // target iteration end

            if (!highlightOnly && anyTargetHit)
            {
                player.Stamina = Math.Max(player.Stamina - 50, 0);
                this.hitCooldown = this.world.currentUpdate + 30;
                if (!this.indestructible)
                {
                    this.hitPoints -= 1;
                    this.hitPoints = Math.Max(0, this.hitPoints);

                    if (this.HitPointsPercent < 0.4f && this.hitPoints > 0) this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.BreakingItem, ignoreDelay: true, text: this.readableName, texture: this.sprite.frame.texture);
                    if (this.hitPoints == 0) this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.BrokenItem, ignoreDelay: true, text: this.readableName, texture: this.sprite.frame.texture);
                }
            }
        }

        public static void HitTarget(BoardPiece attacker, BoardPiece target, int hitPower, float targetPushMultiplier, List<BuffEngine.Buff> buffList = null)
        {
            World world = attacker.world;

            PieceTemplate.CreateOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.Attack);

            target.hitPoints -= hitPower;
            if (buffList != null)
            {
                foreach (BuffEngine.Buff buff in buffList)
                {
                    target.buffEngine.AddBuff(buff: buff, world: world);
                }
            }

            if (target.yield != null) target.yield.DropFirstPieces(hitPower: hitPower);
            if (target.GetType() == typeof(Animal) && world.random.Next(0, 2) == 0) PieceTemplate.CreateOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

            if (!target.alive || target.hitPoints <= 0)
            {
                if (target.yield != null && target.exists) target.yield.DropFinalPieces();
                target.Destroy();

                int numberOfExplosions = world.random.Next(5, 12);
                if (target.category == Category.SmallPlant) numberOfExplosions = 0;
                for (int i = 0; i < numberOfExplosions; i++)
                {
                    Vector2 posOffset = new Vector2(world.random.Next(0, target.sprite.frame.gfxWidth), world.random.Next(0, target.sprite.frame.gfxHeight));
                    posOffset += target.sprite.frame.gfxOffset;

                    var attack = PieceTemplate.CreateOnBoard(world: world, position: target.sprite.position + posOffset, templateName: PieceTemplate.Name.Attack);
                    attack.sprite.color = Color.LightSteelBlue;
                }
            }
            else
            {
                target.showStatBarsTillFrame = world.currentUpdate + 1200;

                if (target.GetType() == typeof(Animal))
                {
                    Animal animalTarget = (Animal)target;

                    animalTarget.target = attacker;
                    animalTarget.aiData.Reset(animalTarget);

                    animalTarget.activeState = (animalTarget.HitPointsPercent > 0.4f && world.random.Next(0, 4) == 0) ?
                        State.AnimalChaseTarget : State.AnimalFlee;

                    animalTarget.UpdateRegenCooldown(); // animal will not heal for a while

                    var movement = (attacker.sprite.position - animalTarget.sprite.position) * targetPushMultiplier * -0.5f * hitPower;
                    animalTarget.AddPassiveMovement(movement: Helpers.VectorAbsMax(vector: movement, maxVal: 400f));

                    animalTarget.buffEngine.AddBuff(world: attacker.world, buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Speed, value: -animalTarget.speed / 2, autoRemoveDelay: 180, isPositive: false)); // animal will be slower for a while
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