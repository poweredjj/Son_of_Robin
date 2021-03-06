using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public struct SaveHeaderInfo
    {
        public readonly bool saveIsCorrect;
        public readonly string folderName;
        public readonly string fullPath;
        public bool autoSave;
        public readonly int seed;
        public readonly int width;
        public readonly int height;
        private readonly IslandClock frozenClock;
        private readonly TimeSpan timePlayed;
        public readonly DateTime saveDate;
        public readonly bool playerFemale;
        private string ElapsedTimeString { get { return this.timePlayed.ToString("hh\\:mm"); } }
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
                string autoSaveString = this.autoSave ? " autosave" : "";

                if (SonOfRobinGame.platform == Platform.Mobile)
                    return $"{SaveDateString} time played: {this.ElapsedTimeString}{autoSaveString} day {this.frozenClock.CurrentDayNo} seed: {String.Format("{0:0000}", this.seed)} {this.width}x{this.height}";
                else return $"{SaveDateString}   time played: {this.ElapsedTimeString}{autoSaveString}   day {this.frozenClock.CurrentDayNo}";
            }
        }

        public string AdditionalInfo
        { get { return $"seed: {String.Format("{0:0000}", this.seed)}   {this.width}x{this.height}"; } }

        public Texture2D AddInfoTexture
        { get { return this.playerFemale ? AnimData.framesForPkgs[AnimData.PkgName.PlayerFemale].texture : AnimData.framesForPkgs[AnimData.PkgName.PlayerMale].texture; } }
        public SaveHeaderInfo(string folderName)
        {
            this.folderName = folderName;

            this.fullPath = Path.Combine(SonOfRobinGame.saveGamesPath, folderName);

            string headerPath = Path.Combine(this.fullPath, "header.sav");

            var headerData = (Dictionary<string, Object>)FileReaderWriter.Load(path: headerPath);

            this.saveIsCorrect = false;
            this.saveDate = DateTime.FromOADate(0d);
            this.autoSave = false;
            this.seed = -1;
            this.width = -1;
            this.height = -1;
            this.frozenClock = null;
            this.timePlayed = TimeSpan.FromSeconds(0);
            this.playerFemale = false;

            if (!this.folderName.StartsWith(LoaderSaver.tempPrefix) && headerData != null && headerData.ContainsKey("saveVersion"))
            {
                float saveVersion = (float)headerData["saveVersion"];
                if (saveVersion == SaveHeaderManager.saveVersion)
                {
                    this.saveIsCorrect = true;
                    this.saveDate = (DateTime)headerData["realDateTime"];
                    this.autoSave = this.folderName == "0";
                    this.seed = (int)headerData["seed"];
                    this.width = (int)headerData["width"];
                    this.height = (int)headerData["height"];
                    this.frozenClock = new IslandClock(elapsedUpdates: (int)headerData["clockTimeElapsed"]);
                    this.timePlayed = (TimeSpan)headerData["TimePlayed"];
                    this.playerFemale = (bool)headerData["playerFemale"];
                }
            }
        }
        public void Delete()
        { Directory.Delete(path: this.fullPath, recursive: true); }
    }

}
