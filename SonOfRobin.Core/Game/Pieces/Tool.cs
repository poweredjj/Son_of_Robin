using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public class Tool : BoardPiece
    {
        public enum TargetCategory { Wood, Stone, Metal, SmallPlant }

        private readonly int hitPower;
        private readonly bool indestructible;
        private int hitCooldown;
        private readonly Dictionary<TargetCategory, float> multiplierByCategory;

        private static readonly Dictionary<TargetCategory, List<PieceTemplate.Name>> targetsByCategory = new Dictionary<TargetCategory, List<PieceTemplate.Name>>
        {{TargetCategory.Wood, new List<PieceTemplate.Name> {
            PieceTemplate.Name.TreeSmall,
            PieceTemplate.Name.TreeBig,
            PieceTemplate.Name.AppleTree,
            PieceTemplate.Name.CherryTree,
            PieceTemplate.Name.BananaTree,
            PieceTemplate.Name.Cactus,
            PieceTemplate.Name.PalmTree,
            PieceTemplate.Name.WoodWorkshop,
            PieceTemplate.Name.ChestWooden,
        } },
         {TargetCategory.Stone, new List<PieceTemplate.Name> {
             PieceTemplate.Name.WaterRock,
             PieceTemplate.Name.Minerals,
         } },

         {TargetCategory.Metal, new List<PieceTemplate.Name> {
             PieceTemplate.Name.ChestMetal, } },

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
         } },

        };


        public Tool(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, int hitPower, Dictionary<TargetCategory, float> multiplierByCategory, int maxHitPoints,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool indestructible = false, Yield yield = null) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints)
        {
            this.activeState = State.Empty;
            this.hitPower = hitPower;
            this.hitCooldown = 0; // earliest world.currentUpdate, when hitting will be possible
            this.indestructible = indestructible;
            this.multiplierByCategory = multiplierByCategory;
            this.toolbarAction = Scheduler.ActionName.Hit;
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

        public void Hit(BoardPiece targetPiece)
        {
            if (targetPiece == null || this.world.currentUpdate < this.hitCooldown) return;

            float currentMultiplier = this.multiplierByCategory[GetTargetCategory(targetPiece: targetPiece)];
            if (currentMultiplier == 0) return;

            this.hitCooldown = this.world.currentUpdate + 30;
            PieceTemplate.CreateOnBoard(world: this.world, position: targetPiece.sprite.position, templateName: PieceTemplate.Name.Attack);

            int currentHitPower = (int)Math.Max(this.hitPower * currentMultiplier, 1);

            targetPiece.hitPoints -= currentHitPower;

            if (!this.indestructible) this.hitPoints -= currentMultiplier == 0 ? 4 : 2;

            if (targetPiece.yield != null) targetPiece.yield.DropFirstPieces(hitPower: this.hitPower);

            if (targetPiece.hitPoints <= 0)
            {
                if (targetPiece.yield != null) targetPiece.yield.DropFinalPieces();
                targetPiece.Destroy();

                int numberOfExplosions = this.world.random.Next(5, 12);
                for (int i = 0; i < numberOfExplosions; i++)
                {
                    Vector2 posOffset = new Vector2(this.world.random.Next(0, targetPiece.sprite.frame.gfxWidth), this.world.random.Next(0, targetPiece.sprite.frame.gfxHeight));
                    posOffset += targetPiece.sprite.frame.gfxOffset;

                    var attack = PieceTemplate.CreateOnBoard(world: this.world, position: targetPiece.sprite.position + posOffset, templateName: PieceTemplate.Name.Attack);
                    attack.sprite.color = Color.LightSteelBlue;
                }
            }
            else
            { targetPiece.showHitPointsTillFrame = this.world.currentUpdate + 1200; }
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