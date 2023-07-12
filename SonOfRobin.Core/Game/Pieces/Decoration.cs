using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Decoration : BoardPiece
    {
        public Decoration(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool isAffectedByWind,
             byte animSize = 0, string animName = "default", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, bool canBePickedUp = false, Yield yield = null, int maxHitPoints = 1, bool ignoresCollisions = false, bool rotatesWhenDropped = false, PieceSoundPack soundPack = null, AllowedDensity allowedDensity = null) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, ignoresCollisions: ignoresCollisions, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, stackSize: stackSize, canBePickedUp: canBePickedUp, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, rotatesWhenDropped: rotatesWhenDropped, soundPack: soundPack, allowedDensity: allowedDensity, isAffectedByWind: isAffectedByWind)
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