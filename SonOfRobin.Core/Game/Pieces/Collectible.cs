﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Collectible : BoardPiece
    {
        public Collectible(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, Yield yield = null, int maxHitPoints = 1, int mass = 1, Scheduler.ActionName toolbarAction = Scheduler.ActionName.Empty, Scheduler.ActionName boardAction = Scheduler.ActionName.Empty, bool rotatesWhenDropped = false, bool fadeInAnim = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, checksFullCollisions: false, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, mass: mass, toolbarAction: toolbarAction, boardAction: boardAction, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim)
        {

        }

    }
}