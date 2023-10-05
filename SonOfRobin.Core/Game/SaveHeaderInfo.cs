using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class SaveHeaderInfo
    {
        public class ScreenshotTexture
        {
            // needed to properly dispose texture, when no longer referenced

            private readonly string pngPath;
            private Texture2D texture;

            public ScreenshotTexture(string pngPath)
            {
                this.pngPath = pngPath;
                this.texture = null;
            }

            public Texture2D Texture
            {
                get
                {
                    if (this.texture != null && !this.texture.IsDisposed) return this.texture;
                    this.texture = GfxConverter.LoadTextureFromPNG(this.pngPath);
                    return this.texture;
                }
            }

            ~ScreenshotTexture()
            {
                this.texture?.Dispose();
            }
        }

        public readonly bool saveIsCorrect;
        public readonly bool saveIsObsolete;
        public readonly bool saveIsCorrupted;
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
        private ScreenshotTexture screenshot;

        public SaveHeaderInfo(string folderName)
        {
            this.folderName = folderName;

            this.fullPath = Path.Combine(SonOfRobinGame.saveGamesPath, folderName);

            string headerPath = Path.Combine(this.fullPath, LoaderSaver.headerName);
            var headerData = (Dictionary<string, Object>)FileReaderWriter.Load(path: headerPath);

            this.saveIsCorrect = false;
            this.saveIsObsolete = true;
            this.saveIsCorrupted = false;
            this.saveVersion = 0;
            this.saveDate = DateTime.FromOADate(0d);
            this.seed = -1;
            this.width = -1;
            this.height = -1;
            this.frozenClock = null;
            this.timePlayed = TimeSpan.FromSeconds(0);
            this.playerName = PieceTemplate.Name.Empty;
            this.screenshot = new ScreenshotTexture(Path.Combine(this.fullPath, LoaderSaver.screenshotName));

            if (!this.folderName.StartsWith(LoaderSaver.tempPrefix) && headerData != null && headerData.ContainsKey("saveVersion"))
            {
                this.saveIsCorrect = true;
                this.saveVersion = (float)(double)headerData["saveVersion"];
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

                if (this.saveIsCorrect)
                {
                    var necessaryFileList = new List<string> { LoaderSaver.hintsName, LoaderSaver.trackingName, LoaderSaver.eventsName, LoaderSaver.weatherName, LoaderSaver.gridName, $"{LoaderSaver.piecesPrefix}0.json" };

                    var saveFileNames = Directory.GetFiles(this.fullPath).Select(path => Path.GetFileName(path).Replace(".gzip", ""));

                    foreach (string fileName in necessaryFileList)
                    {
                        if (!saveFileNames.Contains(fileName))
                        {
                            this.saveIsCorrupted = true;
                            break;
                        }
                    }
                }
            }
        }

        public Texture2D Screenshot
        {
            get
            {
                return this.screenshot.Texture;
            }
        }

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
                if (!this.saveIsObsolete && this.saveIsCorrupted) fullDescription += "   CORRUPTED";

                return fullDescription;
            }
        }

        public string AdditionalInfo
        {
            get
            {
                string additionalInfo = $"seed: {String.Format("{0:0000}", this.seed)}   {this.width}x{this.height}";
                if (this.saveIsObsolete) additionalInfo += "\nTHIS SAVE IS INCOMPATIBLE WITH CURRENT GAME VERSION";
                if (!this.saveIsObsolete && this.saveIsCorrupted) additionalInfo += "\nTHIS SAVE IS CORRUPTED";

                return additionalInfo;
            }
        }

        public List<Texture2D> AddInfoTextureList
        {
            get
            {
                var textureList = new List<Texture2D> { PieceInfo.GetTexture(playerName) };
                return textureList;
            }
        }

        public void Delete()
        {
            Directory.Delete(path: this.fullPath, recursive: true);
        }
    }
}