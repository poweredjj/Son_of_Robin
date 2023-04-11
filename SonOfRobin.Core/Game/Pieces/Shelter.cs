using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Shelter : BoardPiece
    {
        public readonly SleepEngine sleepEngine;

        public Shelter(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, SleepEngine sleepEngine, string readableName, string description, Category category, float fireAffinity,
            byte animSize = 0, string animName = "default", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, bool canBePickedUp = false, Yield yield = null, int maxHitPoints = 1, List<Buff> buffList = null, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, stackSize: stackSize, canBePickedUp: canBePickedUp, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: false, readableName: readableName, description: description, buffList: buffList, category: category, lightEngine: lightEngine, activeState: State.Empty, boardTask: Scheduler.TaskName.OpenShelterMenu, fireAffinity: fireAffinity)
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