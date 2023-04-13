using Microsoft.Xna.Framework;
using System;

namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        private BoardPiece burningPiece;
        private bool targetSpriteChecked; // one time check, after deserialization

        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, int[] maxMassForSize = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: 0f, lightEngine: new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: false), mass: 60)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
            this.targetSpriteChecked = false;
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            if (this.burningPiece == null && !this.targetSpriteChecked)
            {
                // one time check after deserialization, to properly assign burningPiece again

                Sprite burningSprite = Tracking.GetTargetSprite(world: this.world, followingSprite: this.sprite);
                if (burningSprite != null) this.burningPiece = burningSprite.boardPiece;

                this.targetSpriteChecked = true;
            }

            if (this.burningPiece != null && this.burningPiece.exists && this.burningPiece.sprite.IsInWater)
            {
                this.StopBurning();
                return;
            }

            int affectedDistance = Math.Min((int)(this.Mass / 10), 250);

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                float burnVal = Math.Max(this.Mass / 200, 1);

                var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: affectedDistance, compareWithBottom: true);
                foreach (BoardPiece piece in piecesWithinRange)
                {
                    piece.BurnLevel += burnVal;

                    if (piece != this.burningPiece && piece.GetType() == typeof(Animal))
                    {
                        Animal animal = (Animal)piece;
                        animal.soundPack.Play(PieceSoundPack.Action.Cry);
                        animal.target = this;
                        animal.aiData.Reset(animal);
                        animal.activeState = State.AnimalFlee;
                    }
                }

                float hitPointsLost = (float)burnVal / 20f;

                this.burningPiece.hitPoints = Math.Max(this.burningPiece.hitPoints - hitPointsLost, 0);

                this.Mass += burnVal;
                if (this.burningPiece.hitPoints == 0)
                {
                    this.burningPiece.Destroy();
                    this.burningPiece = null;
                }
            }
            else
            {
                this.Mass *= 0.991f;
                if (this.Mass <= 10) this.StopBurning();
            }

            this.sprite.lightEngine.Size = Math.Max(affectedDistance * 5, 50);
            if (!this.sprite.lightEngine.IsActive) this.sprite.lightEngine.Activate();
        }

        private void StopBurning()
        {
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            if (this.burningPiece != null) new WorldEvent(eventName: WorldEvent.EventName.CoolDownAfterBurning, world: this.world, delay: 10 * 60, boardPiece: this.burningPiece, eventHelper: this.burningPiece.BurnLevel);
            new OpacityFade(sprite: this.sprite, destOpacity: 0f, duration: 20, destroyPiece: true);
            this.RemoveFromStateMachines();
        }
    }
}