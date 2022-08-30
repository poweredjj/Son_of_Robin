using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {

        public VisualEffect(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool fadeInAnim = false, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, canBePickedUp: canBePickedUp, fadeInAnim: fadeInAnim, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity)
        {

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
                Vector2.Distance(this.sprite.position, this.world.player.sprite.position) < 150) // 200
            {
                this.Destroy();
            }
        }
    }
}