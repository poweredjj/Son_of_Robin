using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public struct SaveHeaderInfo
    {
        public readonly bool saveIsCorrect;
        public readonly bool saveIsObsolete;
        public readonly float saveVersion;
        public readonly string folderName;
        public readonly string fullPath;
        public readonly int seed;
        public readonly int width;
        public readonly int height;
        private readonly IslandClock frozenClock;
        private readonly TimeSpan timePlayed;
        public readonly DateTime saveDate;
        public readonly PieceTemplate.Name playerName;
        private string ElapsedTimeString { get { return string.Format("{0:D2}:{1:D2}", (int)Math.Floor(this.timePlayed.TotalHours), this.timePlayed.Minutes); } }

        private string SaveDateString
        {
            get
            {
                if (this.saveDate.Date == DateTime.Today) return this.saveDate.ToString("TODAY HH:mm");
                else if (this.saveDate.Date == DateTime.Today - TimeSpan.FromDays(1)) return this.saveDate.ToString("YESTERDAY HH:mm");
                else return this.saveDate.ToString("yyyy/MM/dd HH:mm");
            }
        }

        public string FullDescription
        {
            get
            {
                string fullDescription = SonOfRobinGame.platform == Platform.Mobile ?
                    $"{SaveDateString} time played: {this.ElapsedTimeString} day {this.frozenClock.CurrentDayNo} seed: {String.Format("{0:0000}", this.seed)} {this.width}x{this.height}" : $"{SaveDateString}   time played: {this.ElapsedTimeString}   day {this.frozenClock.CurrentDayNo}";

                if (this.saveIsObsolete) fullDescription += "   INCOMPATIBLE";

                return fullDescription;
            }
        }

        public string AdditionalInfo
        {
            get
            {
                string additionalInfo = $"seed: {String.Format("{0:0000}", this.seed)}   {this.width}x{this.height}";
                if (this.saveIsObsolete) additionalInfo += "\nTHIS SAVE IS INCOMPATIBLE WITH CURRENT GAME VERSION";

                return additionalInfo;
            }
        }

        public Texture2D AddInfoTexture
        { get { return PieceInfo.GetTexture(playerName); } }

        public SaveHeaderInfo(string folderName)
        {
            this.folderName = folderName;

            this.fullPath = Path.Combine(SonOfRobinGame.saveGamesPath, folderName);

            string headerPath = Path.Combine(this.fullPath, "header.json");
            var headerData = (Dictionary<string, Object>)FileReaderWriter.Load(path: headerPath);

            this.saveIsCorrect = false;
            this.saveIsObsolete = true;
            this.saveVersion = 0;
            this.saveDate = DateTime.FromOADate(0d);
            this.seed = -1;
            this.width = -1;
            this.height = -1;
            this.frozenClock = null;
            this.timePlayed = TimeSpan.FromSeconds(0);
            this.playerName = PieceTemplate.Name.Empty;

            if (!this.folderName.StartsWith(LoaderSaver.tempPrefix) && headerData != null && headerData.ContainsKey("saveVersion"))
            {
                this.saveVersion = (float)(double)headerData["saveVersion"];

                this.saveIsCorrect = true;
                this.saveIsObsolete = this.saveVersion != SaveHeaderManager.saveVersion;

                try
                {
                    this.saveDate = (DateTime)headerData["realDateTime"];
                    this.seed = (int)(Int64)headerData["seed"];
                    this.width = (int)(Int64)headerData["width"];
                    this.height = (int)(Int64)headerData["height"];
                    this.frozenClock = new IslandClock(elapsedUpdates: (int)(Int64)headerData["clockTimeElapsed"]);
                    this.timePlayed = TimeSpan.Parse((string)headerData["TimePlayed"]);
                    this.playerName = (PieceTemplate.Name)(Int64)headerData["playerName"];
                }
                catch (KeyNotFoundException) { this.saveIsCorrect = false; }
            }
        }

        public void Delete()
        {
            Directory.Delete(path: this.fullPath, recursive: true);
        }
    }
}