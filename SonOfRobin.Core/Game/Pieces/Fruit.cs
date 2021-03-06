using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Fruit : BoardPiece
    {

        public Fruit(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 2500, bool floatsOnWater = false, int generation = 0, byte stackSize = 10, Yield yield = null, int mass = 1, bool rotatesWhenDropped = true, bool fadeInAnim = true, bool canBeEatenRaw = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, canBePickedUp: true, yield: yield, mass: mass, toolbarTask: canBeEatenRaw ? Scheduler.TaskName.GetEaten : Scheduler.TaskName.Empty, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, readableName: readableName, description: description, category: Category.Indestructible, activeState: State.Empty)
        {
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