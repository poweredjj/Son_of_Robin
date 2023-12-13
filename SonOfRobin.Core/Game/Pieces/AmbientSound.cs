using Microsoft.Xna.Framework;
using System;

namespace SonOfRobin
{
    public class AmbientSound : BoardPiece
    {
        private const float visFullOpacity = 1f;
        private const float visMinOpacity = 0.3f;

        private readonly int checkDelay;
        private int currentDelay;
        private int waitUntilFrame;

        public AmbientSound(World world, int id, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
           bool visible = false) :

            base(world: world, id: id, animPackage: AnimData.PkgName.MusicNoteBig, animSize: 0, animName: "default", name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, visible: visible, activeState: State.PlayAmbientSound)
        {
            this.waitUntilFrame = 0;

            this.checkDelay = 100 + Random.Next(60); // to avoid playing all ambient sound in the same frame
            if (this.sprite.Visible) this.sprite.opacity = visMinOpacity;
        }

        private bool CanBePlayedNow
        {
            get
            {
                if (this.pieceInfo.ambsoundPlayOnlyWhenIsSunny && this.world.weather.SunVisibility < 0.85f) return false;
                return this.pieceInfo.ambsoundPartOfDayList == null ? true : this.pieceInfo.ambsoundPartOfDayList.Contains(this.world.islandClock.CurrentPartOfDay);
            }
        }

        public override void DrawStatBar()
        {
            Sound sound = this.activeSoundPack.GetPlayingSound(PieceSoundPackTemplate.Action.Ambient);
            if (sound == null) return;

            if (sound.isLooped) new StatBar(label: "vol", value: (int)(sound.FadeVolume * 100), valueMax: 100, colorMin: new Color(0, 128, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false);
            else new StatBar(label: "", value: this.currentDelay - (this.waitUntilFrame - this.world.CurrentUpdate), valueMax: this.currentDelay, colorMin: new Color(0, 128, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.GetCroppedFrameForPackage(AnimData.PkgName.MusicNoteSmall).Texture);

            StatBar.FinishThisBatch();
        }

        public override void SM_PlayAmbientSound()
        {
            if (this.world.CurrentUpdate % checkDelay != 0) return;

            bool isInCameraRect = this.sprite.IsInCameraRect;
            bool isLooped = this.pieceInfo.pieceSoundPackTemplate.IsLooped(PieceSoundPackTemplate.Action.Ambient);

            if (this.activeSoundPack.IsPlaying(PieceSoundPackTemplate.Action.Ambient))
            {
                if (isLooped && (!isInCameraRect || !this.CanBePlayedNow))
                {
                    this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.Ambient);
                    if (Preferences.debugShowSounds) this.sprite.opacity = visMinOpacity;
                }
                if (this.pieceInfo.ambsoundGeneratesWind) this.world.weather.AddLocalizedWind(this.sprite.position);
                return;
            }

            if (!isInCameraRect) return;

            if (this.world.CurrentUpdate < this.waitUntilFrame || !this.CanBePlayedNow) return;

            if (this.pieceInfo.ambsoundPlayDelay > 0 || this.pieceInfo.ambsoundPlayDelayMaxVariation > 0)
            {
                this.currentDelay = this.pieceInfo.ambsoundPlayDelay;
                if (this.pieceInfo.ambsoundPlayDelayMaxVariation > 0) this.currentDelay += Random.Next(this.pieceInfo.ambsoundPlayDelayMaxVariation);
                this.waitUntilFrame = this.world.CurrentUpdate + this.currentDelay;
                this.showStatBarsTillFrame = this.waitUntilFrame;
            }

            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Ambient);

            if (Preferences.debugShowSounds)
            {
                this.sprite.opacity = visFullOpacity;
                if (!isLooped) new OpacityFade(sprite: this.sprite, destOpacity: visMinOpacity, duration: 60);
                else this.showStatBarsTillFrame = int.MaxValue;
            }
        }
    }
}