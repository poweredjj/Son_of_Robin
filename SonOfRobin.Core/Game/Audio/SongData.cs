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
            Beginning,
            Rain1,
            Rain2,
            Heat1,
            Heat2,
            Night,
        }

        public static readonly Dictionary<Name, string> songFilenamesDict = new()
        {
            { Name.Title, "fscm-productions-flowers" },
            { Name.Beginning, "alexander-nakarada-tam-lin" },
            { Name.Rain1, "alex-productions-rain-on-the-window" },
            { Name.Rain2, "purrple-cat-lost-and-found" },
            { Name.Heat1, "alex-productions-desert" },
            { Name.Heat2, "hayden-folker-the-red-desert" },
            { Name.Night, "alexander-nakarada-night-of-mystery" },
        };

        public static readonly Dictionary<Name, string> songDescriptionsDict = new()
        {
            { Name.Title, "Flowers - FSCM Productions" },
            { Name.Beginning, "Tam Lin - Alexander Nakarada (CreatorChords)" },
            { Name.Rain1, "Rain On The Window - Alex-Productions" },
            { Name.Rain2, "Lost And Found - Purrple Cat" },
            { Name.Heat1, "Desert - Alex-Productions" },
            { Name.Heat2, "The Red Desert - Hayden Folker" },
            { Name.Night, "Night Of Mystery - Alexander Nakarada" },
        };
    }
}