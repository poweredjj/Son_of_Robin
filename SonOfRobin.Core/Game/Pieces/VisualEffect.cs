using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {

        public VisualEffect(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool serialize = true, bool fadeInAnim = false, bool canBePickedUp = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: true, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: serialize)
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