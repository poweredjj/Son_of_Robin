using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Spectator : BoardPiece
    {

        public Spectator(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, string readableName, string description,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, int generation = 0, bool fadeInAnim = true, bool canBePickedUp = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: 15f, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: true, name: name, destructionDelay: destructionDelay, allowedFields: new AllowedFields(), floatsOnWater: true, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: true, readableName: readableName, description: description, category: Category.Indestructible)
        {
            this.activeState = State.SpectatorFloatAround;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }
        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // data to deserialize here
        }

        public override void SM_SpectatorFloatAround()
        {
            this.world.CalculateMovementFromInput();
            Vector2 movement = this.world.CalculateMovementFromInput();

            var currentSpeed = 3f;
            movement *= currentSpeed;

            Vector2 goalPosition = this.sprite.position + movement;
            this.GoOneStepTowardsGoal(goalPosition, splitXY: false, walkSpeed: currentSpeed, setOrientation: true, slowDownInWater: false);

            if (this.world.actionKeyList.Contains(World.ActionKeys.MapToggle)) this.world.ToggleMapMode();
        }

    }
}