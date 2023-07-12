﻿namespace SonOfRobin
{
    public class Debris : BoardPiece
    {
        public Debris(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = true, bool isAffectedByWind = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty, ignoresCollisions: true, isAffectedByWind: isAffectedByWind)
        {
            this.soundPack.RemoveAction(PieceSoundPack.Action.IsDropped);
        }

    }
}