using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        private BoardPiece burningPiece;

        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, int[] maxMassForSize = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: 0f, lightEngine: new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: true, castShadows: false), mass: 60)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
        }

        private void StopBurning()
        {
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.Destroy();
            if (this.burningPiece != null) new WorldEvent(eventName: WorldEvent.EventName.CoolDownAfterBurning, world: this.world, delay: 10 * 60, boardPiece: this.burningPiece, eventHelper: this.burningPiece.BurnLevel);
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            if (this.burningPiece == null)
            {
                var reallyClosePieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 30, compareWithBottom: true);
                if (reallyClosePieces.Any()) this.burningPiece = reallyClosePieces.First();
            }

            if (this.burningPiece != null && this.burningPiece.exists && this.burningPiece.sprite.IsInWater)
            {
                this.StopBurning();
                return;
            }

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                float burnVal = Math.Max(this.Mass / 100, 1);

                int distance = Math.Min((int)(this.Mass / 50), 500);

                var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: distance, compareWithBottom: true);
                foreach (BoardPiece piece in piecesWithinRange)
                {
                    piece.BurnLevel += burnVal;
                }

                this.burningPiece.hitPoints = Math.Max(this.burningPiece.hitPoints - (burnVal / 20), 0);
                this.Mass += burnVal;
                if (this.burningPiece.hitPoints == 0)
                {
                    this.burningPiece.Destroy();
                    this.burningPiece = null;
                }
            }
            else
            {
                this.Mass--;
            }

            this.sprite.lightEngine.Size = Math.Max((int)(this.Mass / 5), 100);
            if (this.Mass == 0) this.StopBurning();
        }
    }
}