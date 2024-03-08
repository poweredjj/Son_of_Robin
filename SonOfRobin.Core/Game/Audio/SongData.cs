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
            Rain1,
            Rain2,
            Desert1,
            Desert2,
        }

        public static readonly Dictionary<Name, string> songFilenamesDict = new()
        {
            { Name.Title, "fscm-productions-flowers" },
            { Name.Rain1, "alex-productions-rain-on-the-window" },
            { Name.Rain2, "purrple-cat-lost-and-found" },
            { Name.Desert1, "alex-productions-desert" },
            { Name.Desert2, "hayden-folker-the-red-desert" },
        };

        public static readonly Dictionary<Name, string> songDescriptionsDict = new()
        {
            { Name.Title, "Flowers - FSCM Productions" },
            { Name.Rain1, "Rain On The Window - Alex-Productions" },
            { Name.Rain2, "Lost And Found - Purrple Cat" },
            { Name.Desert1, "Desert - Alex-Productions" },
            { Name.Desert2, "The Red Desert - Hayden Folker" },
        };
    }
}