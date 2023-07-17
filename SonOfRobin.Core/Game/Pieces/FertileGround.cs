using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FertileGround : BoardPiece
    {
        public readonly float wealthMultiplier;

        public FertileGround(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, float wealthMultiplier) :

            base(world: world, id: id, animPackage: animPackage, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, activeState: State.Empty, maxHitPoints: 250)
        {
            this.wealthMultiplier = wealthMultiplier;

            // TODO add code for destroying all overlapping plants here
        }

        public override void Destroy()
        {
            if (!this.exists) return;

            // TODO add code for destroying all overlapping plants here

            base.Destroy();
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