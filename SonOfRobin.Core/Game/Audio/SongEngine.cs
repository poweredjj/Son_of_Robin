using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SongEngine(World world)
    {
        public enum SongCategory { Heat, Rain };

        private const int delayAfterPlay = 60 * 60;

        private readonly Random random = new();
        private readonly World world = world;
        private readonly Dictionary<SongData.Name, TimeSpan> whenCanBePlayedAgainDict = [];
        private int earliestUpdateNextCheckPossible = 0;

        private static readonly Dictionary<SongCategory, SongData.Name[]> songsByCategory = new Dictionary<SongCategory, SongData.Name[]>
        {
            { SongCategory.Rain, [ SongData.Name.Rain1, SongData.Name.Rain2 ] },
            { SongCategory.Heat, [ SongData.Name.Heat1, SongData.Name.Heat2 ] },
        };

        private static readonly HashSet<SongData.Name> allSongs = songsByCategory.Values.SelectMany(s => s).ToHashSet();

        public void Update()
        {
            if (!Sound.GlobalOn || !SongPlayer.GlobalOn || this.world.demoMode) return;

            this.TryToTurnOffASong();

            if (!SongPlayer.IsPlaying || this.world.CurrentUpdate >= this.earliestUpdateNextCheckPossible) this.TryToTurnOnASong();
        }

        private void TryToTurnOnASong()
        {
            // "foreign" songs should be allowed to play freely
            if (SongPlayer.CurrentSongName != SongData.Name.Empty && !allSongs.Contains(SongPlayer.CurrentSongName)) return;

            // more important songs should be checked first

            if (this.world.Player.sleepMode == Player.SleepMode.Awake)
            {
                if (this.world.weather.RainPercentage >= 0.3f && !songsByCategory[SongCategory.Rain].Contains(SongPlayer.CurrentSongName))
                {
                    SongData.Name songName = songsByCategory[SongCategory.Rain][this.random.Next(songsByCategory[SongCategory.Rain].Length)];
                    this.Play(songName: songName, nextPlayDelaySeconds: 60 * 15);
                }
                else if (this.world.weather.HeatPercentage == 1 && this.world.islandClock.CurrentDayNo % 2 == 0)
                {
                    SongData.Name songName = songsByCategory[SongCategory.Heat][this.random.Next(songsByCategory[SongCategory.Heat].Length)];
                    this.Play(songName: songName, nextPlayDelaySeconds: 60 * 15);
                }
            }
        }

        private void TryToTurnOffASong()
        {
            if (SongPlayer.CurrentSongName == SongData.Name.Empty) return;

            bool turnOffSong =
                (songsByCategory[SongCategory.Rain].Contains(SongPlayer.CurrentSongName) && this.world.weather.RainPercentage == 0) ||
                (songsByCategory[SongCategory.Heat].Contains(SongPlayer.CurrentSongName) && this.world.weather.HeatPercentage < 1);

            if (turnOffSong) SongPlayer.FadeOut(fadeDurationFrames: 60 * 2);
        }

        public bool Play(SongData.Name songName, int nextPlayDelaySeconds = 0, bool ignoreDelay = false)
        {
            if (!ignoreDelay && this.whenCanBePlayedAgainDict.ContainsKey(songName) && this.whenCanBePlayedAgainDict[songName] > this.world.TimePlayed) return false;

            if (SongPlayer.CurrentSongName != songName)
            {
                if (SongPlayer.CurrentSongName == SongData.Name.Empty) SongPlayer.AddToQueue(songName);
                else SongPlayer.ClearQueueFadeCurrentAndPlay(songName: songName, fadeDurationFrames: 100);

                this.whenCanBePlayedAgainDict[songName] = this.world.TimePlayed + TimeSpan.FromSeconds((nextPlayDelaySeconds + SongData.GetSong(songName).Duration.TotalSeconds) * 60);
            }

            this.earliestUpdateNextCheckPossible = this.world.CurrentUpdate + delayAfterPlay; // if a song starts playing, it should be allowed to play for a while

            return true;
        }

        public void ClearAllDelays()
        {
            this.whenCanBePlayedAgainDict.Clear();
        }
    }
}