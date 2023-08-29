using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Fruit : BoardPiece
    {
        public readonly bool mightContainSeeds;

        public Fruit(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool mightContainSeeds,
            byte animSize = 0, string animName = "default", bool rotatesWhenDropped = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain,   rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty)
        {
            this.mightContainSeeds = mightContainSeeds;
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