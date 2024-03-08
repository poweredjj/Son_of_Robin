using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SongEngine(World world)
    {
        private const int delayAfterPlay = 60 * 60;

        private readonly Random random = new();
        private readonly World world = world;
        private readonly Dictionary<SongData.Name, TimeSpan> whenCanBePlayedAgainDict = [];
        private int earliestUpdateNextCheckPossible = 0;

        private static readonly SongData.Name[] rainSongs = [SongData.Name.Rain1, SongData.Name.Rain2];
        private static readonly SongData.Name[] desertSongs = [SongData.Name.Desert1, SongData.Name.Desert2];

        public void Update()
        {
            if (!Sound.GlobalOn || !SongPlayer.GlobalOn || this.world.demoMode) return;

            this.TryToTurnOffASong();

            if (!SongPlayer.IsPlaying || this.world.CurrentUpdate >= this.earliestUpdateNextCheckPossible) this.TryToTurnOnASong();
        }

        private void TryToTurnOnASong()
        {
            // more important songs should be placed first

            if (this.world.Player.sleepMode == Player.SleepMode.Awake)
            {
                if (this.world.weather.RainPercentage >= 0.3f && !rainSongs.Contains(SongPlayer.CurrentSongName))
                {
                    SongData.Name songName = rainSongs[this.random.Next(rainSongs.Length)];
                    this.Play(songName: songName, nextPlayDelaySeconds: 60 * 15);
                }
                else if (this.world.weather.HeatPercentage == 1 && this.world.islandClock.CurrentDayNo % 2 == 0)
                {
                    SongData.Name songName = desertSongs[this.random.Next(desertSongs.Length)];
                    this.Play(songName: songName, nextPlayDelaySeconds: 60 * 15);
                }
            }
        }

        private void TryToTurnOffASong()
        {
            bool turnOffSong =
                (rainSongs.Contains(SongPlayer.CurrentSongName) && this.world.weather.RainPercentage == 0) ||
                (desertSongs.Contains(SongPlayer.CurrentSongName) && this.world.weather.HeatPercentage < 1);

            if (turnOffSong) SongPlayer.FadeOut();
        }

        public bool Play(SongData.Name songName, int nextPlayDelaySeconds = 0, bool ignoreDelay = false)
        {
            if (!ignoreDelay && this.whenCanBePlayedAgainDict.ContainsKey(songName) && this.whenCanBePlayedAgainDict[songName] > this.world.TimePlayed) return false;

            if (SongPlayer.CurrentSongName != songName)
            {
                if (SongPlayer.CurrentSongName == SongData.Name.Empty) SongPlayer.AddToQueue(songName);
                else SongPlayer.ClearQueueFadeCurrentAndPlay(songName);

                this.whenCanBePlayedAgainDict[songName] = this.world.TimePlayed + TimeSpan.FromSeconds(nextPlayDelaySeconds + SongData.GetSong(songName).Duration.TotalSeconds * 60);
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