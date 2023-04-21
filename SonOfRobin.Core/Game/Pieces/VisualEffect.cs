using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {
        private Tweener tweener;
        private int rainStepsLeft;
        private Vector2 rainStep;
        private readonly bool fogExplodesWhenBurns;

        public VisualEffect(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize, float fireAffinity,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, PieceSoundPack soundPack = null, bool fogExplodesWhenBurns = false) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: null, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: fireAffinity, soundPack: soundPack)
        {
            this.fogExplodesWhenBurns = fogExplodesWhenBurns;
        }

        public override void SM_ScarePredatorsAway()
        {
            if (this.world.CurrentUpdate % 60 != 0) return;

            var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 700, compareWithBottom: true);
            var predatorPieces = nearbyPieces.Where(piece => PieceInfo.GetInfo(piece.name).isCarnivorous);

            foreach (BoardPiece piece in predatorPieces)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} - scaring off {piece.readableName}.");

                Animal animal = (Animal)piece;
                animal.target = this;
                animal.aiData.Reset();
                animal.activeState = State.AnimalFlee;
            }
        }

        public override void SM_MapMarkerShowAndCheck()
        {
            if (this.world.CurrentUpdate % 10 != 0) return;

            if (!this.world.Player.buffEngine.HasBuff(BuffEngine.BuffType.EnableMap) ||
                !this.world.map.CheckIfPlayerCanReadTheMap(showMessage: false) ||
                Vector2.Distance(this.sprite.position, this.world.Player.sprite.position) < 100)
            {
                this.Destroy(); // will be destroyed right away if map was enabled by using god mode
                this.world.map.soundMarkerRemove.Play();
                MessageLog.AddMessage(msgType: MsgType.User, message: "Map marker has been reached.");
            }
        }

        public override void SM_RainInitialize()
        {
            int distance = this.world.random.Next(this.world.camera.viewRect.Height / 2, this.world.camera.viewRect.Height);

            Vector2 rainTargetPos = new Vector2(this.sprite.position.X, this.sprite.position.Y + distance);

            float rainPercentage = this.world.weather.RainPercentage;
            if (rainPercentage == 0)
            {
                // if the raindrop is processed after the rain has stopped
                this.Destroy();
                return;
            }

            this.rainStepsLeft = (int)(this.world.random.Next(15, 100) / (4 * rainPercentage));
            this.rainStepsLeft = Math.Max(this.rainStepsLeft, 5); // to avoid value 0, that would make the rain stay forever
            this.rainStep = (rainTargetPos - this.sprite.position) / this.rainStepsLeft;

            this.activeState = State.RainFall;
            new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: this.rainStepsLeft, boardPiece: this); // to ensure eventual destruction
        }

        public override void SM_RainFall()
        {
            Vector2 currentStep = this.rainStep;
            float windPercentage = this.world.weather.WindPercentage;
            if (windPercentage > 0)
            {
                int windModifier = (int)(windPercentage * 10) + world.random.Next(0, 2);
                if (this.world.weather.WindOriginX == 1) windModifier *= -1; // wind blowing from the right
                currentStep.X += Math.Min(windModifier, currentStep.Y);

                float targetRotation = 0.9f * windPercentage;
                if (this.world.weather.WindOriginX == 0) targetRotation *= -1;

                if (Math.Abs(this.sprite.rotation) < Math.Abs(targetRotation)) this.sprite.rotation = targetRotation;
            }

            this.sprite.Move(currentStep);

            this.rainStepsLeft--;
            if (this.rainStepsLeft <= 0 || this.world.weather.RainPercentage == 0)
            {
                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 20, destroyPiece: true);
                this.RemoveFromStateMachines();
            }
        }

        public override void SM_FogMoveRandomly()
        {
            // Suitable only for passive temporary decorations, that will never be moved "manually".
            // Position and rotation change will cause drift over time, if such a piece be serialized and saved multiple times.
            // It should only be used for temporary decorations or pieces that will not get saved.

            if (this.fogExplodesWhenBurns && this.IsBurning)
            {
                this.world.HintEngine.Disable(PieceHint.Type.ExplosiveGas);

                BoardPiece explosion = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.sprite.position, templateName: PieceTemplate.Name.Explosion, closestFreeSpot: true);
                explosion.sprite.AssignNewSize(3);

                var ignoredTypesList = new List<Type> { typeof(Debris), typeof(Flame) };

                var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 230, compareWithBottom: true);
                foreach (BoardPiece piece in piecesWithinRange)
                {
                    if (!ignoredTypesList.Contains(piece.GetType())) piece.BurnLevel += 2;
                }

                this.BurnLevel = 0;
                this.sprite.color = Color.Yellow;
                this.sprite.opacity = 1f;

                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 60, destroyPiece: true);
                this.RemoveFromStateMachines();

                return;
            }

            if (!this.sprite.IsInCameraRect) return;

            if (this.tweener == null && this.sprite.opacityFade == null) // tween should not start before opacityFade finishes
            {
                this.tweener = new Tweener();

                int maxDistance = 180;

                Vector2 newPos = this.sprite.position + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));
                newPos = this.sprite.world.KeepVector2InWorldBounds(newPos);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: newPos, duration: this.world.random.Next(6, 15), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.rotation, toValue: (float)(Random.NextDouble() * 2) - 1, duration: this.world.random.Next(6, 15), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.opacity, toValue: (float)(Random.NextDouble() * 0.3) + 0.2f, duration: this.world.random.Next(4, 20), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            if (this.tweener != null)
            {
                this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);
                this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly
            }
        }
    }
}