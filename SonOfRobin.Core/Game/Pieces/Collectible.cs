using System.Collections.Generic;

namespace SonOfRobin
{
    public class Collectible : BoardPiece
    {
        public Collectible(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", int generation = 0, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = false, List<Buff> buffList = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty, soundPack: soundPack)
        {
        }
    }
}