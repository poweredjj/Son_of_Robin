using System;

namespace SonOfRobin
{
    public class Debris : BoardPiece
    {
        public Debris(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 20, int destructionDelayMaxOffset = 90, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, bool canBePickedUp = false, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = true, bool isAffectedByWind = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay + Random.Next(0, destructionDelayMaxOffset), allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, stackSize: stackSize, canBePickedUp: canBePickedUp, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, category: Category.Indestructible, activeState: State.Empty, ignoresCollisions: true, serialize: false, isAffectedByWind: isAffectedByWind, fireAffinity: 0f)
        {
            this.soundPack.RemoveAction(PieceSoundPack.Action.IsDropped);
        }

    }
}