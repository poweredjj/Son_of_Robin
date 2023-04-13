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
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            if (this.burningPiece == null)
            {
                var reallyClosePieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 30, compareWithBottom: true);

                if (reallyClosePieces.Any()) this.burningPiece = reallyClosePieces.First();
                else
                {
                    this.StopBurning();
                    return;
                }
            }

            if (!this.burningPiece.exists || this.burningPiece.sprite.IsInWater)
            {
                this.StopBurning();
                return;
            }

            var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: (ushort)(this.Mass / 2), compareWithBottom: true);

            foreach (BoardPiece piece in piecesWithinRange)
            {
                float burnVal = Math.Max(this.Mass / 100, 1);

                piece.BurnLevel += burnVal;
                if (burnVal > 0)
                {
                    piece.hitPoints = Math.Max(piece.hitPoints - (burnVal / 4), 0);
                    if (piece.hitPoints == 0) piece.Destroy();
                }

                this.Mass += burnVal;
            }

            this.sprite.lightEngine.Size = Math.Max((int)(this.Mass / 5), 100);
            if (!this.burningPiece.IsBurning) this.StopBurning();
        }
    }
}