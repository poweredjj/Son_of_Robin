using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SongPlayer
    {
        // adds new functionality to MediaPlayer, should be used instead of MediaPlayer

        private const float defaultFadeValPerFrame = 0.01f;

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

        private static float globalVolume = 1f;

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

        private static float targetVolume;
        private static float fadeValPerFrame;

        private static readonly Queue<QueueEntry> queue = new();
        public static SongData.Name CurrentSongName { get; private set; } = SongData.Name.Empty;

        public static void AddToQueue(SongData.Name songName, float fadeVal = 1, bool repeat = false)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            queue.Enqueue(new QueueEntry(songName: songName, fadeVal: fadeVal, repeat: repeat));
            MediaPlayer.IsRepeating = false; // to ensure that the current song will end playing
        }

        public static void ClearQueueFadeCurrentAndPlay(SongData.Name songName, bool repeat = false, float fadeVal = 0f)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            queue.Clear();

            if (MediaPlayer.State == MediaState.Playing && CurrentSongName == songName) return;
            FadeOut(fadeVal: fadeVal);

            queue.Enqueue(new QueueEntry(songName: songName, repeat: repeat, fadeVal: fadeVal));
        }

        public static void ClearQueueAndPlay(SongData.Name songName, bool repeat = false)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            queue.Clear();

            if (MediaPlayer.State == MediaState.Playing && CurrentSongName == songName) return;

            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Volume = GlobalVolume;
            MediaPlayer.Play(song: SongData.GetSong(songName));
            CurrentSongName = songName;
        }

        public static void ClearQueueAndStop(float fadeVal = 1f)
        {
            queue.Clear();

            if (fadeVal == 1)
            {
                MediaPlayer.Stop();
                CurrentSongName = SongData.Name.Empty;
            }
            else FadeOut(fadeVal);
        }

        public static void FadeOut(float fadeVal = 0f)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            Fade(volume: 0, fadeVal: fadeVal);
        }

        public static void Fade(float volume, float fadeVal = 0f)
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            if (MediaPlayer.State != MediaState.Playing) return;

            fadeValPerFrame = fadeVal != 0f ? fadeVal : defaultFadeValPerFrame;
            targetVolume = volume * GlobalVolume;
        }

        public static void Update()
        {
            if (!Sound.GlobalOn || !GlobalOn) return;

            if (MediaPlayer.State == MediaState.Playing && MediaPlayer.Volume != targetVolume)
            {
                MediaPlayer.Volume += (MediaPlayer.Volume < targetVolume ? fadeValPerFrame : -fadeValPerFrame) * globalVolume;

                if (Math.Abs(MediaPlayer.Volume - targetVolume) < (fadeValPerFrame * globalVolume))
                {
                    MediaPlayer.Volume = targetVolume;
                    if (MediaPlayer.Volume == 0)
                    {
                        MediaPlayer.Stop();
                        CurrentSongName = SongData.Name.Empty;
                    }
                }
            }

            if (MediaPlayer.State == MediaState.Stopped && queue.Count > 0)
            {
                QueueEntry queueEntry = queue.Dequeue();
                if (queueEntry.fadeVal != 1)
                {
                    MediaPlayer.Volume = 0;
                    ClearQueueAndPlay(queueEntry.songName, repeat: queueEntry.repeat);
                    Fade(volume: GlobalVolume, fadeVal: queueEntry.fadeVal);
                }
                else ClearQueueAndPlay(queueEntry.songName, repeat: queueEntry.repeat);
            }
        }

        public readonly struct QueueEntry(SongData.Name songName, float fadeVal = 1f, bool repeat = false, float targetVolume = 1f)
        {
            public readonly SongData.Name songName = songName;
            public readonly float fadeVal = fadeVal;
            public readonly bool repeat = repeat;
            public readonly float targetVolume = targetVolume * globalVolume;
        }
    }
}