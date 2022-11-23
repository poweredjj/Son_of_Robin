using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {
        private Vector2 startPos;
        private float startRot;
        private Tweener tweener;

        public VisualEffect(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool fadeInAnim = false, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity)
        {
        }

        public override Dictionary<string, Object> Serialize()
        {
            float rotation = this.sprite.rotation;
            Vector2 position = this.sprite.position;

            if (this.sprite.IsOnBoard && this.tweener != null)
            {
                // tweener will change parameters and original values must be restored first
                this.sprite.rotation = this.startRot;
                this.sprite.SetNewPosition(newPos: this.startPos, ignoreCollisions: true);
            }

            Dictionary<string, Object> pieceData = base.Serialize(); // serializing with proper values

            if (this.sprite.IsOnBoard && this.tweener != null)
            {
                // restoring "temporary" values after serializing
                this.sprite.rotation = rotation;
                this.sprite.SetNewPosition(newPos: position, ignoreCollisions: true);
            }

            return pieceData;
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
                animal.aiData.Reset(animal);
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

        public override void SM_FogMoveRandomly()
        {
            // suitable only for passive decorations, that will never be moved "manually"

            if (!this.sprite.IsInCameraRect) return;

            if (this.tweener == null)
            {
                this.tweener = new Tweener();

                this.startPos = this.sprite.position;
                this.startRot = this.sprite.rotation;

                int maxDistance = 100;

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

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.opacity, toValue: (float)(Random.NextDouble() * 0.4), duration: this.world.random.Next(4, 20), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly
        }
    }
}