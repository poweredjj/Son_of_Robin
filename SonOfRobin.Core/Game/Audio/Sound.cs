using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Sound
    {
        private static bool globalOn = true;

        public static bool menuOn = true;
        public static bool textWindowAnimOn = true;
        public static float globalVolume = 1f;
        public static Dictionary<int, Sound> currentlyPlaying = new Dictionary<int, Sound>();
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
        public int Id { get; private set; }
        public readonly bool isEmpty;
        public bool playAfterAssign; // to allow for playing empty sounds, that will be reassigned after deserialization

        public List<SoundData.Name> SoundNameList { get; private set; }
        private BoardPiece boardPiece;
        private BoardPiece visPiece;

        public static readonly AudioListener audioListener = new();
        public static readonly AudioEmitter audioEmitter = new();

        public Sound(SoundData.Name name = SoundData.Name.Empty, List<SoundData.Name> nameList = null, BoardPiece boardPiece = null, float volume = 1f, bool isLooped = false, int cooldown = 0, bool ignore3DAlways = false, float maxPitchVariation = 0f, float pitchChange = 0f, int volumeFadeFrames = 30)
        {
            this.Id = Helpers.GetUniqueID();

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
                if (!globalOn)
                {
                    StopAll();
                    SongPlayer.ClearQueueAndStop();
                }
            }
        }

        public void AddBoardPiece(BoardPiece boardPieceToAssign)
        {
            if (this.boardPiece != null) throw new ArgumentException($"Cannot add boardPiece {boardPieceToAssign.name} {boardPieceToAssign.id} to sound with boardPiece already assigned {this.boardPiece.name} {this.boardPiece.id} ");

            this.boardPiece = boardPieceToAssign;
        }

        public Sound MakeCopyForPiece(BoardPiece boardPiece)
        {
            // this constructor must contain all Sound input params

            return new Sound(
                name: this.Name,
                nameList: this.SoundNameList,
                boardPiece: boardPiece,
                volume: this.volume,
                isLooped: this.isLooped,
                cooldown: this.cooldown,
                ignore3DAlways: this.ignore3DAlways,
                maxPitchVariation: this.maxPitchVariation,
                pitchChange: this.pitchChange,
                volumeFadeFrames: this.volumeFadeFrames);
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

        public void Play(bool ignore3DThisPlay = false, bool ignoreCooldown = false, int coolDownExtension = 0)
        {
            if (!Scene.currentlyProcessedScene.soundActive || !GlobalOn || this.isEmpty) return;

            this.ignore3DThisPlay = ignore3DThisPlay || this.boardPiece != null && !this.boardPiece.sprite.IsOnBoard;

            int cooldownToUse = this.cooldown + coolDownExtension;

            if (cooldownToUse > 0 && !ignoreCooldown)
            {
                int currentUpdate = this.Ignore3D ? SonOfRobinGame.CurrentUpdate : this.boardPiece.world.CurrentUpdate;

                if (currentUpdate < this.lastFramePlayed + cooldownToUse) return;
                this.lastFramePlayed = currentUpdate;
            }

            if (this.isLooped) this.TargetVolume = 1f;

            if (this.boardPiece != null && this.boardPiece.sprite.IsOnBoard && !this.Ignore3D && !this.isLooped && !this.IsInCameraRect) return;

            SoundData.Name soundName = this.SoundNameList.Count == 1 ? this.SoundNameList[0] : this.SoundNameList[SonOfRobinGame.random.Next(this.SoundNameList.Count)];
            ManagedSoundInstance managedSoundInstance = ManagedSoundInstance.GetNewOrStoppedInstance(soundName: soundName);

            float pitch = this.pitchChange;
            if (this.maxPitchVariation > 0)
            {
                double pitchChange = ((SonOfRobinGame.random.NextSingle() * 2d) - 1d) * this.maxPitchVariation;
                pitch += (float)pitchChange;
                pitch = Math.Clamp(value: pitch, min: -1, max: 1);
            }

            managedSoundInstance.Pitch = pitch;
            managedSoundInstance.Volume = this.Volume;
            managedSoundInstance.IsLooped = this.isLooped;

            if (this.boardPiece != null && this.boardPiece.sprite.IsOnBoard)
            {
                this.CreateSoundVisual();
                this.UpdatePosition(managedSoundInstance);
            }

            bool instanceStartedCorrectly = managedSoundInstance.Play(this.Id);
            if (instanceStartedCorrectly) currentlyPlaying[this.Id] = this;
            else MessageLog.Add(debugMessage: true, text: $"InstancePlayLimitException reached, sound '{soundName}' will not be played.");
        }

        private void CreateSoundVisual()
        {
            if (!Preferences.debugShowSounds) return;

            this.visPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this.boardPiece.world, position: this.boardPiece.sprite.position, templateName: PieceTemplate.Name.MusicNote);
            new Tracking(level: this.boardPiece.level, targetSprite: this.boardPiece.sprite, followingSprite: this.visPiece.sprite);
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
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.id} {this.soundNameList[0]} starting fade out...");

                Scheduler.ExecutionDelegate stopSoundDlgt = () =>
                {
                    // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{sound.Id} {sound.SoundNameList[0]} fade out ended - stopping.");
                    this.Stop(skipFade: true);
                };
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: this.volumeFadeFrames, executeHelper: stopSoundDlgt);
                return;
            }

            ManagedSoundInstance.Stop(this.Id);
            if (currentlyPlaying.ContainsKey(this.Id)) currentlyPlaying.Remove(this.Id);
        }

        public static void UpdateAll()
        {
            foreach (Sound sound in currentlyPlaying.Values.ToList())
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

            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.id} {this.soundNameList[0]} fade updated {this.fadeVolume} --> {this.targetVolume}");
            instance.Volume = this.Volume;
        }

        private void RemoveVisPiece()
        {
            if (!Preferences.debugShowSounds || this.visPiece == null) return;

            new LevelEvent(eventName: LevelEvent.EventName.Destruction, level: this.visPiece.level, delay: 20, boardPiece: this.visPiece);
            this.visPiece = null;
        }

        private void UpdatePosition(ManagedSoundInstance managedSoundInstance)
        {
            if (this.Ignore3D) return;

            if (this.isLooped && this.boardPiece.GetType() == typeof(AmbientSound))
            {
                if (!this.boardPiece.exists || !this.boardPiece.sprite.IsInCameraRect)
                {
                    // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Stopping sound ({this.boardPiece.readableName})");
                    this.Stop();
                    return;
                }
            }

            if (!this.boardPiece.exists) return;

            Vector2 cameraPos = this.boardPiece.world.camera.CurrentPos;
            audioListener.Position = new Vector3(cameraPos.X, 0, cameraPos.Y);

            Vector2 piecePos = this.boardPiece.sprite.position;
            audioEmitter.Position = new Vector3(piecePos.X, 0, piecePos.Y);

            // SonOfRobinGame.messageLog.AddMessage(text: $"Distance {Vector3.Distance(audioEmitter.Position, audioListener.Position)}");

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