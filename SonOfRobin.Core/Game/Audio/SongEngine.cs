using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SongEngine(World world)
    {
        private readonly Random random = new();
        private readonly World world = world;
        private readonly Dictionary<SongData.Name, TimeSpan> whenCanBePlayedAgainDict = [];

        private static readonly SongData.Name[] rainSongs = [SongData.Name.Rain1, SongData.Name.Rain2];

        public void Update()
        {
            if (!Sound.GlobalOn || !SongPlayer.GlobalOn || this.world.demoMode) return;

            if (this.world.Player.sleepMode == Player.SleepMode.Awake)
            {
                if (this.world.weather.RainPercentage >= 0.3f)
                {
                    SongData.Name songName = rainSongs[this.random.Next(rainSongs.Length)];
                    this.Play(songName: songName, nextPlayDelaySeconds: 60 * 15);
                }
            }
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

            return true;
        }

        public void ClearAllDelays()
        {
            this.whenCanBePlayedAgainDict.Clear();
        }
    }
}