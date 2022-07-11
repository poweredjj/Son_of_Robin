using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Sound
    {
        public static bool globalOn = true;
        public static bool menuOn = true;
        public static bool textWindowAnimOn = true;
        public static float globalVolume = 1f;
        public static Dictionary<string, Sound> currentlyPlaying = new Dictionary<string, Sound>();

        private readonly float volume;
        public float FadeVolume { get { return this.fadeVolume; } }
        private float fadeVolume;
        public float TargetVolume { get { return this.targetVolume; } }
        private float targetVolume;
        private bool VolumeFadeEnded { get { return !this.isLooped || this.targetVolume == this.fadeVolume; } }
        private float Volume { get { return this.volume * this.fadeVolume * globalVolume; } }
        private readonly bool isLooped;
        public bool IsLooped { get { return this.isLooped; } }
        public bool IsPlaying { get { return SoundInstanceManager.GetPlayingInstance(this.id) != null; } }

        private readonly int cooldown;
        private int lastFramePlayed;
        private readonly bool ignore3DAlways;
        private bool ignore3DThisPlay;
        private readonly float maxPitchVariation;
        private readonly float pitchChange;
        private readonly int volumeFadeFrames;
        private readonly float volumeFadePerFrame;

        private readonly string id;
        public string Id { get { return this.id; } }
        public readonly bool isEmpty;
        public bool playAfterAssign; // to allow for playing empty sounds, that will be reassigned after deserialization

        public List<SoundData.Name> SoundNameList { get { return this.soundNameList; } }
        private readonly List<SoundData.Name> soundNameList;
        private BoardPiece boardPiece;
        private BoardPiece visPiece;

        private static readonly AudioListener audioListener = new AudioListener();
        private static readonly AudioEmitter audioEmitter = new AudioEmitter();
        public bool Ignore3D { get { return this.ignore3DAlways || this.ignore3DThisPlay; } }
        public bool HasBoardPiece { get { return this.boardPiece != null; } }
        private bool IsInCameraRect { get { return this.boardPiece == null || this.boardPiece.sprite.IsInCameraRect; } }

        public Sound(SoundData.Name name = SoundData.Name.Empty, List<SoundData.Name> nameList = null, BoardPiece boardPiece = null, float volume = 1f, bool isLooped = false, int cooldown = 0, bool ignore3DAlways = false, float maxPitchVariation = 0f, float pitchChange = 0f, int volumeFadeFrames = 30)
        {
            this.id = Helpers.GetUniqueHash();

            this.playAfterAssign = false;
            this.isEmpty = name == SoundData.Name.Empty && nameList == null;
            if (nameList != null && nameList.Count == 1 && nameList[0] == SoundData.Name.Empty) this.isEmpty = true;

            if (this.isEmpty) return;

            this.volume = volume;
            this.isLooped = isLooped;
            this.volumeFadeFrames = volumeFadeFrames;
            this.volumeFadePerFrame = 1f / volumeFadeFrames;
            this.fadeVolume = this.isLooped ? 0f : 1f;
            this.targetVolume = 1f;
            this.cooldown = cooldown;
            this.lastFramePlayed = 0;
            this.ignore3DAlways = ignore3DAlways;
            this.maxPitchVariation = maxPitchVariation;
            if (this.maxPitchVariation > 1) throw new ArgumentException($"Pitch variation {this.maxPitchVariation} > 1.");
            this.pitchChange = pitchChange;
            if (this.pitchChange < -1 || this.pitchChange > 1) throw new ArgumentException($"Pitch change {this.pitchChange} is over the limit (-1, 1).");

            this.soundNameList = new List<SoundData.Name>();
            if (nameList == null) this.soundNameList.Add(name);
            else
            {
                foreach (SoundData.Name soundName in nameList)
                {
                    this.soundNameList.Add(soundName);
                }
            }

            if (boardPiece != null) this.AddBoardPiece(boardPiece);
        }

        public void AddBoardPiece(BoardPiece boardPieceToAssign)
        {
            if (this.boardPiece != null) throw new ArgumentException($"Cannot add boardPiece {boardPieceToAssign.name} {boardPieceToAssign.id} to sound with boardPiece already assigned {this.boardPiece.name} {this.boardPiece.id} ");

            this.boardPiece = boardPieceToAssign;
        }

        public void Play(bool ignore3DThisPlay = false, bool ignoreCooldown = false)
        {
            if (!Scene.currentlyProcessedScene.soundActive || !globalOn || this.isEmpty) return;

            this.ignore3DThisPlay = ignore3DThisPlay;

            if (this.cooldown > 0 && !ignoreCooldown)
            {
                int currentUpdate = this.boardPiece == null || this.Ignore3D ? SonOfRobinGame.currentUpdate : this.boardPiece.world.currentUpdate;

                if (currentUpdate < this.lastFramePlayed + cooldown) return;
                this.lastFramePlayed = currentUpdate;
            }

            if (this.IsLooped) this.targetVolume = 1f;

            if (this.boardPiece != null && !this.Ignore3D && !this.isLooped && !this.IsInCameraRect) return;

            SoundData.Name soundName = this.soundNameList.Count == 1 ? this.soundNameList[0] : this.soundNameList[SonOfRobinGame.random.Next(0, this.soundNameList.Count)];
            SoundEffectInstance instance = SoundInstanceManager.GetNewOrStoppedInstance(soundName: soundName, id: this.id);

            float pitch = this.pitchChange;
            if (this.maxPitchVariation > 0)
            {
                double pitchChange = ((SonOfRobinGame.random.NextDouble() * 2d) - 1d) * this.maxPitchVariation;
                pitch += (float)pitchChange;
                pitch = Math.Max(Math.Min(pitch, 1), -1);
            }

            instance.Pitch = pitch;
            instance.Volume = this.Volume;
            instance.IsLooped = this.isLooped;

            if (this.boardPiece != null)
            {
                this.CreateSoundVisual();
                this.UpdatePosition(instance);
            }

            try
            {
                instance.Play();
            }
            catch (InstancePlayLimitException)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"InstancePlayLimitException reached, sound '{soundName}' will not be played.");
            }

            currentlyPlaying[this.id] = this;
        }

        private void CreateSoundVisual()
        {
            if (!Preferences.debugShowSounds) return;

            this.visPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this.boardPiece.world, position: this.boardPiece.sprite.position, templateName: PieceTemplate.Name.MusicNote);
            new Tracking(world: this.boardPiece.world, targetSprite: this.boardPiece.sprite, followingSprite: this.visPiece.sprite);
        }

        public static void StopAll()
        {
            SoundInstanceManager.StopAll();
            currentlyPlaying.Clear();
        }

        public void Stop(bool skipFade = false)
        {
            if (this.isEmpty) return;

            this.targetVolume = 0f;
            if (!skipFade && !this.VolumeFadeEnded)
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.id} {this.soundNameList[0]} starting fade out...");
                new Scheduler.Task(taskName: Scheduler.TaskName.StopSound, delay: this.volumeFadeFrames, executeHelper: this);
                return;
            }

            SoundInstanceManager.StopInstance(this.id);
            if (currentlyPlaying.ContainsKey(this.id)) currentlyPlaying.Remove(this.id);
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
            SoundEffectInstance instance = SoundInstanceManager.GetPlayingInstance(this.id);
            if (instance == null)
            {
                currentlyPlaying.Remove(this.id);
                this.RemoveVisPiece();
                return;
            }

            this.UpdateFade(instance);

            if (this.boardPiece != null) this.UpdatePosition(instance);
        }


        private void UpdateFade(SoundEffectInstance instance)
        {
            if (this.VolumeFadeEnded) return;

            if (this.targetVolume > this.fadeVolume) this.fadeVolume += this.volumeFadePerFrame;
            else this.fadeVolume -= this.volumeFadePerFrame;

            if (Math.Abs(this.targetVolume - this.fadeVolume) < this.volumeFadePerFrame * 2) this.fadeVolume = this.targetVolume;

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.id} {this.soundNameList[0]} fade updated {this.fadeVolume} --> {this.targetVolume}");
            instance.Volume = this.Volume;
        }

        private void RemoveVisPiece()
        {
            if (!Preferences.debugShowSounds || this.visPiece == null) return;

            new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.visPiece.world, delay: 20, boardPiece: this.visPiece);
            this.visPiece = null;
        }

        private void UpdatePosition(SoundEffectInstance instance)
        {
            if (this.IsLooped && this.boardPiece.GetType() == typeof(AmbientSound))
            {
                if (!this.boardPiece.exists || !this.boardPiece.sprite.IsInCameraRect)
                {
                    // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Stopping sound ({this.boardPiece.readableName})");
                    this.Stop();
                    return;
                }
            }

            if (this.Ignore3D) audioEmitter.Position = audioListener.Position;
            else
            {
                if (!this.boardPiece.exists) return;

                Vector2 cameraPos = this.boardPiece.world.camera.CurrentPos;
                audioListener.Position = new Vector3(cameraPos.X, 0, cameraPos.Y);

                Vector2 piecePos = this.boardPiece.sprite.position;
                audioEmitter.Position = new Vector3(piecePos.X, 0, piecePos.Y);

                // MessageLog.AddMessage(msgType: MsgType.User, message: $"Distance {Vector3.Distance(audioEmitter.Position, audioListener.Position)}");
            }

            instance.Apply3D(audioListener, audioEmitter);
        }
        public static void QuickPlay(SoundData.Name name, float volume = 1f)
        {
            if (name == SoundData.Name.Empty) return;
            new Sound(name: name, volume: volume).Play();
        }
    }
}
