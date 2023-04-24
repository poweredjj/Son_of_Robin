using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Sound
    {
        private static bool globalOn = true;

        public static bool menuOn = true;
        public static bool textWindowAnimOn = true;
        public static float globalVolume = 1f;
        public static Dictionary<string, Sound> currentlyPlaying = new Dictionary<string, Sound>();
        public SoundData.Name Name { get; private set; }
        private float volume;
        public float FadeVolume { get; private set; }
        public float TargetVolume { get; private set; }
        public readonly bool isLooped;
        private readonly int cooldown;
        private int lastFramePlayed;
        private readonly bool ignore3DAlways;
        private bool ignore3DThisPlay;
        private readonly float maxPitchVariation;
        private readonly float pitchChange;
        private readonly int volumeFadeFrames;
        private readonly float volumeFadePerFrame;
        public string Id { get; private set; }
        public readonly bool isEmpty;
        public bool playAfterAssign; // to allow for playing empty sounds, that will be reassigned after deserialization

        public List<SoundData.Name> SoundNameList { get; private set; }
        private BoardPiece boardPiece;
        private BoardPiece visPiece;

        private static readonly AudioListener audioListener = new AudioListener();
        private static readonly AudioEmitter audioEmitter = new AudioEmitter();

        public Sound(SoundData.Name name = SoundData.Name.Empty, List<SoundData.Name> nameList = null, BoardPiece boardPiece = null, float volume = 1f, bool isLooped = false, int cooldown = 0, bool ignore3DAlways = false, float maxPitchVariation = 0f, float pitchChange = 0f, int volumeFadeFrames = 30)
        {
            this.Id = Helpers.GetUniqueHash();

            this.playAfterAssign = false;
            this.isEmpty = name == SoundData.Name.Empty && nameList == null;
            if (nameList != null && nameList.Count == 1 && nameList[0] == SoundData.Name.Empty) this.isEmpty = true;

            if (this.isEmpty) return;

            this.Name = name;
            this.volume = volume;
            this.isLooped = isLooped;
            this.volumeFadeFrames = volumeFadeFrames;
            this.volumeFadePerFrame = 1f / volumeFadeFrames;
            this.FadeVolume = this.isLooped ? 0f : 1f;
            this.TargetVolume = 1f;
            this.cooldown = cooldown;
            this.lastFramePlayed = 0;
            this.ignore3DAlways = ignore3DAlways;
            this.maxPitchVariation = maxPitchVariation;
            if (this.maxPitchVariation > 1) throw new ArgumentException($"Pitch variation {this.maxPitchVariation} > 1.");
            this.pitchChange = pitchChange;
            if (this.pitchChange < -1 || this.pitchChange > 1) throw new ArgumentException($"Pitch change {this.pitchChange} is over the limit (-1, 1).");

            this.SoundNameList = new List<SoundData.Name>();
            if (nameList == null) this.SoundNameList.Add(name);
            else
            {
                foreach (SoundData.Name soundName in nameList)
                {
                    this.SoundNameList.Add(soundName);
                }
            }

            if (boardPiece != null && !this.ignore3DAlways) this.AddBoardPiece(boardPiece);
        }

        public bool Ignore3D
        { get { return this.ignore3DAlways || this.ignore3DThisPlay || this.boardPiece == null; } }

        public bool HasBoardPiece
        { get { return this.boardPiece != null; } }

        private bool IsInCameraRect
        { get { return this.boardPiece == null || this.boardPiece.sprite.IsInCameraRect; } }

        public bool IsPlaying
        { get { return ManagedSoundInstance.GetPlayingInstance(this.Id) != null; } }

        private bool VolumeFadeEnded
        { get { return !this.isLooped || this.TargetVolume == this.FadeVolume; } }

        private float Volume
        { get { return this.volume * this.FadeVolume * globalVolume; } }

        public static bool GlobalOn
        {
            get { return globalOn; }
            set
            {
                if (value == globalOn) return;

                globalOn = value;
                if (!globalOn) StopAll();
            }
        }

        public void AddBoardPiece(BoardPiece boardPieceToAssign)
        {
            if (this.boardPiece != null) throw new ArgumentException($"Cannot add boardPiece {boardPieceToAssign.name} {boardPieceToAssign.id} to sound with boardPiece already assigned {this.boardPiece.name} {this.boardPiece.id} ");

            this.boardPiece = boardPieceToAssign;
        }

        public void AdjustVolume(float newVolume)
        {
            // for manual volume control (does not go well with TargetVolume, FadeVolume, etc.)
            if (this.volume == newVolume) return;

            this.volume = newVolume;
            this.TargetVolume = newVolume;

            ManagedSoundInstance managedSoundInstance = ManagedSoundInstance.GetPlayingInstance(this.Id);
            if (managedSoundInstance != null) managedSoundInstance.Volume = this.Volume;
        }

        public void Play(bool ignore3DThisPlay = false, bool ignoreCooldown = false)
        {
            if (!Scene.currentlyProcessedScene.soundActive || !GlobalOn || this.isEmpty) return;

            this.ignore3DThisPlay = ignore3DThisPlay;

            if (this.cooldown > 0 && !ignoreCooldown)
            {
                int currentUpdate = this.Ignore3D ? SonOfRobinGame.CurrentUpdate : this.boardPiece.world.CurrentUpdate;

                if (currentUpdate < this.lastFramePlayed + cooldown) return;
                this.lastFramePlayed = currentUpdate;
            }

            if (this.isLooped) this.TargetVolume = 1f;

            if (this.boardPiece != null && !this.Ignore3D && !this.isLooped && !this.IsInCameraRect) return;

            SoundData.Name soundName = this.SoundNameList.Count == 1 ? this.SoundNameList[0] : this.SoundNameList[SonOfRobinGame.random.Next(0, this.SoundNameList.Count)];
            ManagedSoundInstance managedSoundInstance = ManagedSoundInstance.GetNewOrStoppedInstance(soundName: soundName);

            float pitch = this.pitchChange;
            if (this.maxPitchVariation > 0)
            {
                double pitchChange = ((SonOfRobinGame.random.NextDouble() * 2d) - 1d) * this.maxPitchVariation;
                pitch += (float)pitchChange;
                pitch = Math.Max(Math.Min(pitch, 1), -1);
            }

            managedSoundInstance.Pitch = pitch;
            managedSoundInstance.Volume = this.Volume;
            managedSoundInstance.IsLooped = this.isLooped;

            if (this.boardPiece != null)
            {
                this.CreateSoundVisual();
                this.UpdatePosition(managedSoundInstance);
            }

            managedSoundInstance.AssignSoundID(this.Id);
            bool instanceStartedCorrectly = managedSoundInstance.Play();
            if (instanceStartedCorrectly) currentlyPlaying[this.Id] = this;
            else MessageLog.AddMessage(msgType: MsgType.Debug, message: $"InstancePlayLimitException reached, sound '{soundName}' will not be played.");
        }

        private void CreateSoundVisual()
        {
            if (!Preferences.debugShowSounds) return;

            this.visPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this.boardPiece.world, position: this.boardPiece.sprite.position, templateName: PieceTemplate.Name.MusicNote);
            new Tracking(world: this.boardPiece.world, targetSprite: this.boardPiece.sprite, followingSprite: this.visPiece.sprite);
        }

        public static void StopAll()
        {
            ManagedSoundInstance.StopAll();
            currentlyPlaying.Clear();
        }

        public void Stop(bool skipFade = false)
        {
            if (this.isEmpty) return;

            this.TargetVolume = 0f;
            if (!skipFade && !this.VolumeFadeEnded)
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.id} {this.soundNameList[0]} starting fade out...");
                new Scheduler.Task(taskName: Scheduler.TaskName.StopSound, delay: this.volumeFadeFrames, executeHelper: this);
                return;
            }

            ManagedSoundInstance.Stop(this.Id);
            if (currentlyPlaying.ContainsKey(this.Id)) currentlyPlaying.Remove(this.Id);
        }

        public static void UpdateAll()
        {
            foreach (Sound sound in currentlyPlaying.Values)
            {
                sound.Update();
            }
        }

        private void Update()
        {
            ManagedSoundInstance instance = ManagedSoundInstance.GetPlayingInstance(this.Id);
            if (instance == null)
            {
                currentlyPlaying.Remove(this.Id);
                this.RemoveVisPiece();
                return;
            }

            this.UpdateFade(instance);

            if (this.boardPiece != null) this.UpdatePosition(instance);
        }

        private void UpdateFade(ManagedSoundInstance instance)
        {
            if (this.VolumeFadeEnded) return;

            if (this.TargetVolume > this.FadeVolume) this.FadeVolume += this.volumeFadePerFrame;
            else this.FadeVolume -= this.volumeFadePerFrame;

            if (Math.Abs(this.TargetVolume - this.FadeVolume) < this.volumeFadePerFrame * 2) this.FadeVolume = this.TargetVolume;

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.id} {this.soundNameList[0]} fade updated {this.fadeVolume} --> {this.targetVolume}");
            instance.Volume = this.Volume;
        }

        private void RemoveVisPiece()
        {
            if (!Preferences.debugShowSounds || this.visPiece == null) return;

            new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.visPiece.world, delay: 20, boardPiece: this.visPiece);
            this.visPiece = null;
        }

        private void UpdatePosition(ManagedSoundInstance managedSoundInstance)
        {
            if (this.Ignore3D) return;

            if (this.isLooped && this.boardPiece.GetType() == typeof(AmbientSound))
            {
                if (!this.boardPiece.exists || !this.boardPiece.sprite.IsInCameraRect)
                {
                    // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Stopping sound ({this.boardPiece.readableName})");
                    this.Stop();
                    return;
                }
            }

            if (!this.boardPiece.exists) return;

            Vector2 cameraPos = this.boardPiece.world.camera.CurrentPos;
            audioListener.Position = new Vector3(cameraPos.X, 0, cameraPos.Y);

            Vector2 piecePos = this.boardPiece.sprite.position;
            audioEmitter.Position = new Vector3(piecePos.X, 0, piecePos.Y);

            // MessageLog.AddMessage(msgType: MsgType.User, message: $"Distance {Vector3.Distance(audioEmitter.Position, audioListener.Position)}");

            float pitch = managedSoundInstance.Pitch;
            managedSoundInstance.Apply3D(audioListener, audioEmitter);
            managedSoundInstance.Pitch = pitch; // fixing Pitch reset glitch on Windows
        }

        public static void QuickPlay(SoundData.Name name, float volume = 1f, float pitchChange = 0f)
        {
            if (name == SoundData.Name.Empty) return;
            new Sound(name: name, volume: volume, pitchChange: pitchChange).Play();
        }
    }
}