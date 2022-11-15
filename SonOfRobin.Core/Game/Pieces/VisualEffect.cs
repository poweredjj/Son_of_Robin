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
        private Vector2 startPos;
        private float startRot;

        public VisualEffect(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool fadeInAnim = false, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity)
        {

        }

        public override Dictionary<string, Object> Serialize()
        {
            if (this.tweener != null) // tweener will change parameters (temporarily) and original values must be restored first
            {
                this.sprite.rotation = this.startRot;
                this.sprite.SetNewPosition(newPos: this.startPos, ignoreCollisions: true);
            }

            Dictionary<string, Object> pieceData = base.Serialize();

            return pieceData;
        }

        public override void SM_ScarePredatorsAway()
        {
            if (this.world.currentUpdate % 60 != 0) return;

            var nearbyPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 700, compareWithBottom: true);
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
            if (this.world.currentUpdate % 10 != 0) return;

            if (!this.world.player.buffEngine.HasBuff(BuffEngine.BuffType.EnableMap) ||
                !this.world.map.CheckIfPlayerCanReadTheMap(showMessage: false) ||
                Vector2.Distance(this.sprite.position, this.world.player.sprite.position) < 100)
            {
                this.Destroy(); // will be destroyed right away if map was enabled by using god mode
                this.world.map.soundMarkerRemove.Play();
                MessageLog.AddMessage(msgType: MsgType.User, message: "Map marker has been reached.");
            }
        }

        public override void SM_ProcessRandomMovement()
        {
            // suitable only for passive decorations, that will never be moved "manually"

            if (!this.sprite.IsInCameraRect) return;

            if (this.tweener == null)
            {
                this.tweener = new Tweener();
                this.startPos = this.sprite.position;
                this.startRot = this.sprite.rotation;

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.rotation, toValue: 1f, duration: 10, delay: 0)
                 .RepeatForever(repeatDelay: 0.0f)
                 .AutoReverse()
                 .Easing(EasingFunctions.QuadraticInOut);

                Vector2 newPos = this.sprite.position + new Vector2(this.world.random.Next(-50, 50));
                newPos = this.sprite.world.KeepVector2InWorldBounds(newPos);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: newPos, duration: 10, delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            this.sprite.UpdateRects(); // because tweener will change position directly
            this.sprite.UpdateBoardLocation(); // because tweener will change position directly
        }
    }
}