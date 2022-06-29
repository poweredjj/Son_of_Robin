using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public class Tool : BoardPiece
    {
        public enum TargetCategory { Wood, Stone, Metal, SmallPlant, Animal }

        public readonly bool shootsProjectile;
        private readonly int hitPower;
        public int hitCooldown;
        private readonly Dictionary<TargetCategory, float> multiplierByCategory;
        private readonly List<PieceTemplate.Name> compatibleAmmo;

        private static readonly Dictionary<TargetCategory, List<PieceTemplate.Name>> targetsByCategory = new Dictionary<TargetCategory, List<PieceTemplate.Name>>
        {{TargetCategory.Wood, new List<PieceTemplate.Name> {
            PieceTemplate.Name.TreeSmall,
            PieceTemplate.Name.TreeBig,
            PieceTemplate.Name.AppleTree,
            PieceTemplate.Name.CherryTree,
            PieceTemplate.Name.BananaTree,
            PieceTemplate.Name.Cactus,
            PieceTemplate.Name.PalmTree,
            PieceTemplate.Name.RegularWorkshop,
            PieceTemplate.Name.ChestWooden,
            PieceTemplate.Name.CrateRegular,
            PieceTemplate.Name.CrateStarting,
            PieceTemplate.Name.PickaxeWood,
            PieceTemplate.Name.AxeWood,
            PieceTemplate.Name.WoodLog,
            PieceTemplate.Name.BatWood,
            PieceTemplate.Name.TentMedium,
            PieceTemplate.Name.TentBig,
            PieceTemplate.Name.WoodPlank,
        } },

         {TargetCategory.Stone, new List<PieceTemplate.Name> {
             PieceTemplate.Name.WaterRock,
             PieceTemplate.Name.MineralsSmall,
             PieceTemplate.Name.MineralsBig,
             PieceTemplate.Name.AxeStone,
             PieceTemplate.Name.PickaxeStone,
             PieceTemplate.Name.IronDeposit,
             PieceTemplate.Name.CoalDeposit,
             PieceTemplate.Name.TentSmall,
             PieceTemplate.Name.TentMedium,
             PieceTemplate.Name.TentBig,
         } },

         {TargetCategory.Metal, new List<PieceTemplate.Name> {
             PieceTemplate.Name.ChestIron,
             PieceTemplate.Name.AxeIron,
             PieceTemplate.Name.PickaxeIron,
         } },

         {TargetCategory.SmallPlant, new List<PieceTemplate.Name> {
             PieceTemplate.Name.GrassRegular,
             PieceTemplate.Name.GrassDesert,
             PieceTemplate.Name.Rushes,
             PieceTemplate.Name.WaterLily,
             PieceTemplate.Name.FlowersPlain,
             PieceTemplate.Name.FlowersMountain,
             PieceTemplate.Name.Apple,
             PieceTemplate.Name.Cherry,
             PieceTemplate.Name.Banana,
             PieceTemplate.Name.Tomato,
             PieceTemplate.Name.TomatoPlant,
         } },

         {TargetCategory.Animal, new List<PieceTemplate.Name> {
             PieceTemplate.Name.Rabbit,
             PieceTemplate.Name.Fox,
             PieceTemplate.Name.Frog,
             PieceTemplate.Name.BackpackMedium,
             PieceTemplate.Name.BeltMedium,
         } },

        };

        public Tool(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, int hitPower, Dictionary<TargetCategory, float> multiplierByCategory, int maxHitPoints, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool indestructible = false, Yield yield = null, bool shootsProjectile = false, List<PieceTemplate.Name> compatibleAmmo = null, bool rotatesWhenDropped = true, bool fadeInAnim = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, indestructible: indestructible, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description)
        {
            this.activeState = State.Empty;
            this.hitPower = hitPower;
            this.hitCooldown = 0; // earliest world.currentUpdate, when hitting will be possible
            this.shootsProjectile = shootsProjectile;
            this.multiplierByCategory = multiplierByCategory;
            this.toolbarTask = Scheduler.TaskName.Hit;
            this.compatibleAmmo = compatibleAmmo == null ? new List<PieceTemplate.Name> { } : compatibleAmmo;
        }

        private static TargetCategory GetTargetCategory(BoardPiece targetPiece)
        {
            TargetCategory category;
            List<PieceTemplate.Name> nameList;

            foreach (var kvp in targetsByCategory)
            {
                category = kvp.Key;
                nameList = kvp.Value;

                foreach (PieceTemplate.Name name in nameList)
                { if (targetPiece.name == name) return category; }
            }

            throw new DivideByZeroException($"'{targetPiece.name}' does not belong to any tool category.");
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

            //if (!removePiece) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: "Out of ammo.");

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
            projectile.GetThrown(startPosition: startingPos, movement: movement, hitPowerMultiplier: this.hitPower, shootingPower: shootingPower);
        }

        public void Use(BoardPiece targetPiece, int shootingPower = 0)
        {
            Player player = this.world.player;
            bool isVeryTired = player.IsVeryTired;

            if (this.shootsProjectile)
            {
                if (isVeryTired) shootingPower /= 2;
                this.ShootProjectile(shootingPower);
                return;
            }

            if (this.world.currentUpdate < this.hitCooldown || this.world.player.Stamina < 80) return;

            if (targetPiece == null) return;

            TargetCategory targetCategory = GetTargetCategory(targetPiece: targetPiece);
            if (!this.multiplierByCategory.ContainsKey(targetCategory)) return;

            float currentMultiplier = this.multiplierByCategory[targetCategory];
            if (currentMultiplier == 0) return;
            if (isVeryTired) currentMultiplier /= 2;

            player.Stamina = Math.Max(player.Stamina - 50, 0);

            this.hitCooldown = this.world.currentUpdate + 30;
            int currentHitPower = (int)Math.Max(this.hitPower * currentMultiplier, 1);

            HitTarget(attacker: player, target: targetPiece, hitPower: currentHitPower, targetPushMultiplier: 1f);
            if (!this.indestructible)
            {
                this.hitPoints -= currentMultiplier == 0 ? 2 : 1;
                this.hitPoints = Math.Max(0, this.hitPoints);
            }
        }

        public static void HitTarget(BoardPiece attacker, BoardPiece target, int hitPower, float targetPushMultiplier)
        {
            World world = attacker.world;

            PieceTemplate.CreateOnBoard(world: attacker.world, position: target.sprite.position, templateName: PieceTemplate.Name.Attack);

            target.hitPoints -= hitPower;
            if (target.yield != null) target.yield.DropFirstPieces(hitPower: hitPower);
            if (target.GetType() == typeof(Animal) && world.random.Next(0, 2) == 0) PieceTemplate.CreateOnBoard(world: world, position: target.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

            if (target.hitPoints <= 0)
            {
                if (target.yield != null) target.yield.DropFinalPieces();
                target.Destroy();

                int numberOfExplosions = world.random.Next(5, 12);
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
                    animalTarget.aiData.Reset();
                    animalTarget.activeState = State.AnimalFlee;

                    animalTarget.AddPassiveMovement(movement: (attacker.sprite.position - animalTarget.sprite.position) * targetPushMultiplier * -0.5f * hitPower);
                    animalTarget.buffEngine.AddBuff(buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Speed, value: -animalTarget.speed / 2, autoRemoveDelay: 180)); // animal will be slower for a while
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