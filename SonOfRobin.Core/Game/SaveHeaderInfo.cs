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
        private readonly int seed;
        private readonly int width;
        private readonly int height;
        private readonly int currentUpdate;
        private readonly TimeSpan elapsedTime;
        public readonly DateTime saveDate;
        private string ElapsedTimeString { get { return this.elapsedTime.ToString("hh\\:mm"); } }
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

                if (SonOfRobinGame.platform == Platform.Mobile) return $"{SaveDateString} time played: {ElapsedTimeString}{autoSaveString} seed: {String.Format("{0:0000}", this.seed)} {this.width}x{this.height}";
                else return $"{SaveDateString}   time played: {ElapsedTimeString}{autoSaveString}";
            }
        }

        public string AdditionalInfo
        { get { return $"seed: {String.Format("{0:0000}", this.seed)}   {this.width}x{this.height}"; } }
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
            this.currentUpdate = -1;
            this.elapsedTime = TimeSpan.FromSeconds(0);

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
                    this.currentUpdate = (int)headerData["currentUpdate"];
                    this.elapsedTime = TimeSpan.FromMilliseconds(this.currentUpdate * 16.67);
                }
            }
        }

        public void Delete()
        { Directory.Delete(path: this.fullPath, recursive: true); }
    }

}
