using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Shelter : BoardPiece
    {
        public readonly SleepEngine sleepEngine;

        public Shelter(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, SleepEngine sleepEngine, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: false, readableName: readableName, description: description, lightEngine: lightEngine, activeState: State.Empty, boardTask: Scheduler.TaskName.OpenShelterMenu)
        {
            this.sleepEngine = sleepEngine;
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