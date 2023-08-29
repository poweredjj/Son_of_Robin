using System.Collections.Generic;

namespace SonOfRobin
{
    public class Collectible : BoardPiece
    {
        public Collectible(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false, List<Buff> buffList = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty, soundPack: soundPack)
        {
        }
    }
}