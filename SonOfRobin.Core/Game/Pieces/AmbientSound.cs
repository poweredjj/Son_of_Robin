using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AmbientSound : BoardPiece
    {
        private static readonly float visFullOpacity = 1f;
        private static readonly float visMinOpacity = 0.3f;


        private readonly int playDelay;
        private readonly int playDelayMaxVariation;
        private readonly int checkDelay;
        private readonly List<IslandClock.PartOfDay> partOfDayList;

        private int currentDelay;
        private int waitUntilFrame;

        public AmbientSound(World world, string id, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, Sound sound, int playDelay,
           int destructionDelay = 0, bool serialize = true, bool visible = false, AllowedDensity allowedDensity = null, int playDelayMaxVariation = 0, List<IslandClock.PartOfDay> partOfDayList = null) :

            base(world: world, id: id, animPackage: AnimData.PkgName.MusicNoteBig, animSize: 0, animName: "default", blocksMovement: false, minDistance: 0, maxDistance: 100, ignoresCollisions: false, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: true, maxMassBySize: null, generation: 0, canBePickedUp: false, fadeInAnim: false, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: State.PlayAmbientSound, allowedDensity: allowedDensity)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.Ambient, sound: sound);

            this.playDelay = playDelay;
            this.playDelayMaxVariation = playDelayMaxVariation;
            this.partOfDayList = partOfDayList;

            this.waitUntilFrame = 0;

            this.checkDelay = 100 + Random.Next(0, 60); // to avoid playing all ambient sound in the same frame
            if (this.sprite.Visible) this.sprite.opacity = visMinOpacity;
        }
        private bool CanPlayAtThisPartOfDay
        { get { return this.partOfDayList == null ? true : this.partOfDayList.Contains(this.world.islandClock.CurrentPartOfDay); } }

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
        public override void DrawStatBar()
        {
            Sound sound = this.soundPack.GetSound(PieceSoundPack.Action.Ambient);
            if (sound == null) return;

            if (sound.isLooped) new StatBar(label: "vol", value: (int)(sound.FadeVolume * 100), valueMax: 100, colorMin: new Color(0, 128, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false);
            else new StatBar(label: "", value: this.currentDelay - (this.waitUntilFrame - this.world.currentUpdate), valueMax: this.currentDelay, colorMin: new Color(0, 128, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.MusicNoteSmall].texture);

            StatBar.FinishThisBatch();
        }

        public override void SM_PlayAmbientSound()
        {
            if (this.world.currentUpdate % checkDelay != 0) return;

            bool isInCameraRect = this.sprite.IsInCameraRect;
            bool isLooped = this.soundPack.IsLooped(PieceSoundPack.Action.Ambient);

            if (this.soundPack.IsPlaying(PieceSoundPack.Action.Ambient))
            {
                if (isLooped && (!isInCameraRect || !this.CanPlayAtThisPartOfDay))
                {
                    this.soundPack.Stop(PieceSoundPack.Action.Ambient);
                    if (Preferences.debugShowSounds) this.sprite.opacity = visMinOpacity;
                }
                return;
            }

            if (!isInCameraRect) return;

            if (this.world.currentUpdate < this.waitUntilFrame || !this.CanPlayAtThisPartOfDay) return;

            if (this.playDelay > 0 || this.playDelayMaxVariation > 0)
            {
                this.currentDelay = this.playDelay;
                if (this.playDelayMaxVariation > 0) this.currentDelay += Random.Next(0, this.playDelayMaxVariation);
                this.waitUntilFrame = this.world.currentUpdate + this.currentDelay;
                this.showStatBarsTillFrame = this.waitUntilFrame;
            }

            this.soundPack.Play(PieceSoundPack.Action.Ambient);

            if (Preferences.debugShowSounds)
            {
                this.sprite.opacity = visFullOpacity;
                if (!isLooped) this.sprite.opacityFade = new OpacityFade(sprite: this.sprite, destOpacity: visMinOpacity, duration: 60);
                else this.showStatBarsTillFrame = 2147483647;
            }
        }

    }
}