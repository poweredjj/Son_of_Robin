using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SongPlayer
    {
        // adds new functionality to MediaPlayer, should be used instead of MediaPlayer

        private static bool globalOn = true;

        public static bool GlobalOn
        {
            get { return globalOn; }

            set
            {
                globalOn = value;
                if (!value) ClearQueueAndStop();
            }
        }

        private static float globalVolume = 0.6f;

        public static float GlobalVolume
        {
            get { return globalVolume; }
            set
            {
                globalVolume = value;
                if (targetVolume != 0) targetVolume = value;
                if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Volume = globalVolume;
            }
        }

        public static bool IsFadingOut { get { return targetVolume < MediaPlayer.Volume; } }

        public static bool IsPlaying { get { return CurrentSongName != SongData.Name.Empty; } }
        public static float TargetVolume { get { return targetVolume; } }
        private static float targetVolume;
        private static float fadeValPerFrame;

        private static readonly Queue<QueueEntry> queue = new();
        public static SongData.Name CurrentSongName { get; private set; } = SongData.Name.Empty;

        public static void AddToQueue(SongData.Name songName, int fadeDurationFrames = 0, bool repeat = false)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            queue.Enqueue(new QueueEntry(songName: songName, fadeDurationFrames: fadeDurationFrames, repeat: repeat));
            MediaPlayer.IsRepeating = false; // to ensure that the current song will end playing
        }

        public static void ClearQueueFadeCurrentAndPlay(SongData.Name songName, bool repeat = false, int fadeDurationFrames = 0)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            queue.Clear();

            if (MediaPlayer.State == MediaState.Playing)
            {
                if (CurrentSongName == songName) return;
                else FadeOut(fadeDurationFrames: fadeDurationFrames);
            }

            queue.Enqueue(new QueueEntry(songName: songName, repeat: repeat, fadeDurationFrames: fadeDurationFrames));
        }

        private static void Play(SongData.Name songName, bool repeat = false, bool clearQueue = false, bool startSilent = false)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            if (clearQueue) queue.Clear();

            if (MediaPlayer.State == MediaState.Playing && CurrentSongName == songName) return;

            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Volume = startSilent ? 0f : GlobalVolume;
            targetVolume = GlobalVolume;
            MediaPlayer.Play(song: SongData.GetSong(songName));
            CurrentSongName = songName;

            if (SongData.songDescriptionsDict.TryGetValue(songName, out string value))
            {
                MessageLog.Add(text: $"{value}", imageObj: PieceInfo.GetImageObj(PieceTemplate.Name.MusicNote), bgColor: new Color(0, 58, 92));
            }
        }

        public static void ClearQueueAndStop(int fadeDurationFrames = 0)
        {
            queue.Clear();

            if (fadeDurationFrames == 0)
            {
                MediaPlayer.Stop();
                CurrentSongName = SongData.Name.Empty;
            }
            else FadeOut(fadeDurationFrames);
        }

        public static void FadeOut(int fadeDurationFrames = 0)
        {
            if (!Sound.GlobalOn || !GlobalOn || MediaPlayer.Volume == 0 || IsFadingOut) return;

            Fade(volume: 0, fadeDurationFrames: fadeDurationFrames);
        }

        public static void Fade(float volume, int fadeDurationFrames = 100)
        {
            if (!Sound.GlobalOn || !GlobalOn || MediaPlayer.State != MediaState.Playing) return;

            targetVolume = volume * GlobalVolume;
            fadeValPerFrame = (targetVolume - MediaPlayer.Volume) / (float)fadeDurationFrames;
        }

        public static void Update()
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            if (MediaPlayer.State == MediaState.Playing && MediaPlayer.Volume != targetVolume)
            {
                MediaPlayer.Volume += fadeValPerFrame;

                if (Math.Abs(targetVolume - MediaPlayer.Volume) <= fadeValPerFrame) MediaPlayer.Volume = targetVolume;
            }

            if (MediaPlayer.Volume == 0 && MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();

            if (MediaPlayer.State == MediaState.Stopped)
            {
                CurrentSongName = SongData.Name.Empty;

                if (queue.Count > 0)
                {
                    QueueEntry queueEntry = queue.Dequeue();
                    if (queueEntry.fadeDurationFrames > 0)
                    {
                        Play(queueEntry.songName, repeat: queueEntry.repeat, startSilent: true);
                        Fade(volume: 1f, fadeDurationFrames: queueEntry.fadeDurationFrames);
                    }
                    else Play(queueEntry.songName, repeat: queueEntry.repeat);
                }
            }
        }

        public readonly struct QueueEntry(SongData.Name songName, int fadeDurationFrames = 0, bool repeat = false, float targetVolume = 1f)
        {
            public readonly SongData.Name songName = songName;
            public readonly int fadeDurationFrames = fadeDurationFrames;
            public readonly bool repeat = repeat;
            public readonly float targetVolume = targetVolume * globalVolume;
        }
    }
}