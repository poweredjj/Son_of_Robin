using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {
        public class RandomMovement
        {
            private Sprite sprite;
            public Vector2 StartPos { get; private set; }
            public float StartRot { get; private set; }

            public bool Initialized
            { get { return this.sprite != null; } }

            public RandomMovement()
            {
                // TODO add code
            }

            public void Initialize(Sprite sprite)
            {
                if (this.sprite != null) throw new ArgumentException("Cannot initialize twice.");

                this.sprite = sprite;
                this.StartPos = sprite.position;
                this.StartRot = sprite.rotation;
            }

            public void Serialize(Dictionary<string, Object> pieceData)
            {
                pieceData["randomMovement_Initialized"] = this.Initialized;
                pieceData["randomMovement_StartPosX"] = this.StartPos.X;
                pieceData["randomMovement_StartPosY"] = this.StartPos.Y;
                pieceData["randomMovement_StartRot"] = this.StartRot;
            }

            public void Deserialize(Dictionary<string, Object> pieceData, Sprite sprite)
            {
                bool initialized = (bool)pieceData["randomMovement_Initialized"];
                if (!initialized) return; // pieceData will not contain correct information if uninitialized

                this.sprite = sprite;

                float startPosX = (float)pieceData["randomMovement_StartPosX"];
                float startPosY = (float)pieceData["randomMovement_StartPosY"];
                this.StartPos = new Vector2(startPosX, startPosY);

                this.StartRot = (float)pieceData["randomMovement_StartRot"];

                this.sprite.SetNewPosition(newPos: this.StartPos, ignoreCollisions: true); // restoring original position (instead of serialized "temporal" one)
                this.sprite.rotation = this.StartRot; // restoring original rotation
            }

            public void Process()
            {
                if (this.sprite == null) throw new ArgumentException("Cannot process before initialization.");

                // TODO add process code here
            }
        }

        private readonly RandomMovement randomMovement;

        public VisualEffect(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool fadeInAnim = false, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, RandomMovement randomMovement = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity)
        {
            this.randomMovement = randomMovement;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            if (this.randomMovement != null) this.randomMovement.Serialize(pieceData);

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            if (this.randomMovement != null) this.randomMovement.Deserialize(pieceData: pieceData, sprite: this.sprite);
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

            if (!this.randomMovement.Initialized) this.randomMovement.Initialize(this.sprite);
            if (!this.sprite.IsInCameraRect) return;

            this.randomMovement.Process();
        }
    }
}