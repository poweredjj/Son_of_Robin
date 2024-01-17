using System.Collections.Generic;

namespace SonOfRobin
{
    public class Potion : BoardPiece
    {
        public Potion(World world, int id, AnimDataNew.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false, List<Buff> buffList = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain,   maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty)
        {
        }
    }
}