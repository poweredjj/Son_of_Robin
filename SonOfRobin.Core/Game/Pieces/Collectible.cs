﻿using System.Collections.Generic;

namespace SonOfRobin
{
    public class Collectible : BoardPiece
    {
        public Collectible(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, Category category, ReadOnlyParams readOnlyParams,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, Yield yield = null, int maxHitPoints = 1, int mass = 1, Scheduler.TaskName toolbarTask = Scheduler.TaskName.Empty, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, bool rotatesWhenDropped = false, List<Buff> buffList = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, readOnlyParams: readOnlyParams, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, stackSize: stackSize, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, mass: mass, toolbarTask: toolbarTask, boardTask: boardTask, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, category: category, buffList: buffList, activeState: State.Empty, soundPack: soundPack)
        {
        }
    }
}