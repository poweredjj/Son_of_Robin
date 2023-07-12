using System.Collections.Generic;

namespace SonOfRobin
{
    public class Potion : BoardPiece
    {
        public readonly PieceTemplate.Name convertsToWhenUsed;

        public Potion(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, PieceTemplate.Name convertsToWhenUsed,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool rotatesWhenDropped = false, List<Buff> buffList = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty, soundPack: soundPack)
        {
            this.convertsToWhenUsed = convertsToWhenUsed;
        }
    }
}