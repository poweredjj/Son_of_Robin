using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Shelter : BoardPiece
    {
        public readonly SleepEngine sleepEngine;

        public Shelter(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, SleepEngine sleepEngine, string readableName, string description,
            byte animSize = 0, string animName = "default", Yield yield = null, int maxHitPoints = 1, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain,  yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: false, readableName: readableName, description: description, lightEngine: lightEngine, activeState: State.Empty)
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