namespace SonOfRobin
{
    public class Trigger : BoardPiece
    {
        public Trigger(World world, int id, AnimDataNew.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, string animName = "default") :

            base(world: world, id: id, animPackage: animPackage, animName: animName, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, activeState: State.Empty)
        {
        }
    }
}