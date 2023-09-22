using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Decoration : BoardPiece
    {
        public Decoration(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
             byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, rotatesWhenDropped: rotatesWhenDropped)
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