using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SongData
    {
        private static readonly Dictionary<Name, Song> songsDict = [];

        public static Song GetSong(Name songName)
        {
            if (!songsDict.ContainsKey(songName))
            {
                MessageLog.Add(debugMessage: true, text: $"Loading song: {songName}");
                songsDict[songName] = SonOfRobinGame.ContentMgr.Load<Song>($"music/{songFilenamesDict[songName]}");
            }
            return songsDict[songName];
        }

        public enum Name : byte
        {
            Empty,
            Title,
        }

        public static readonly Dictionary<Name, string> songFilenamesDict = new()
        {
            { Name.Title, "fscm-productions-flowers" },
        };
    }
}