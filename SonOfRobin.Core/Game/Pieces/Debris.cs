using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public class Debris : BoardPiece
    {

        public Debris(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 20, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, bool canBePickedUp = false, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false, bool rotatesWhenDropped = true) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay + world.random.Next(0, 90), allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, stackSize: stackSize, canBePickedUp: canBePickedUp, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, rotatesWhenDropped: rotatesWhenDropped)
        {
            this.activeState = State.Empty;
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