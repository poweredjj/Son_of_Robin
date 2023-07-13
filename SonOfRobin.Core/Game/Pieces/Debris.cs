namespace SonOfRobin
{
    public class Debris : BoardPiece
    {
        public Debris(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", int generation = 0, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = true, bool isAffectedByWind = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty, isAffectedByWind: isAffectedByWind)
        {
            this.soundPack.RemoveAction(PieceSoundPack.Action.IsDropped);
        }

    }
}