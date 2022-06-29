using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Equipment : BoardPiece
    {
        public Equipment(World world, Vector2 position, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, List<BuffEngine.Buff> buffList, string readableName, string description, Category category,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, Yield yield = null, int maxHitPoints = 1, int mass = 1, Scheduler.TaskName toolbarTask = Scheduler.TaskName.Empty, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, bool rotatesWhenDropped = false, bool fadeInAnim = false, bool placeAtBeachEdge = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, checksFullCollisions: false, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, mass: mass, toolbarTask: toolbarTask, boardTask: boardTask, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, placeAtBeachEdge: placeAtBeachEdge, isShownOnMiniMap: true, buffList: buffList, readableName: readableName, description: description, category: category)
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