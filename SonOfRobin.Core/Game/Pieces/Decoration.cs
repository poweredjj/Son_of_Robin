using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Decoration : BoardPiece
    {
        public Decoration(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool isAffectedByWind,
             byte animSize = 0, string animName = "default", int generation = 0, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = false, PieceSoundPack soundPack = null, AllowedDensity allowedDensity = null) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, generation: generation, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, rotatesWhenDropped: rotatesWhenDropped, soundPack: soundPack, allowedDensity: allowedDensity, isAffectedByWind: isAffectedByWind)
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