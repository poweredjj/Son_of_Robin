using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Fruit : BoardPiece
    {
        public PieceTemplate.Name spawnerName;

        public Fruit(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 2500, bool floatsOnWater = false, int generation = 0, byte stackSize = 10, Yield yield = null, int mass = 1, bool rotatesWhenDropped = true, bool fadeInAnim = true) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, checksFullCollisions: false, canBePickedUp: true, yield: yield, mass: mass, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim)
        {
            this.activeState = State.Empty;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["fruit_spawnerName"] = this.spawnerName;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            this.spawnerName = (PieceTemplate.Name)pieceData["fruit_spawnerName"];         
        }


    }
}