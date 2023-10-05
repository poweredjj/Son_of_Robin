using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class SaveHeaderInfo
    {
        private static readonly Dictionary<string, Texture2D> loadedScreenshots = new Dictionary<string, Texture2D>();

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
                string pngPath = Path.Combine(this.fullPath, LoaderSaver.screenshotName);

                // disposing old screenshot (in case png has been updated)
                if (loadedScreenshots.ContainsKey(pngPath) && loadedScreenshots[pngPath] != null) loadedScreenshots[pngPath].Dispose();

                Texture2D screenshot = GfxConverter.LoadTextureFromPNG(Path.Combine(this.fullPath, LoaderSaver.screenshotName));
                loadedScreenshots[pngPath] = screenshot;
                if (screenshot != null) new Scheduler.Task(taskName: Scheduler.TaskName.DisposeSaveScreenshotsIfNoMenuPresent, delay: 60 * 3);

                return screenshot;
            }
        }

        public List<InfoWindow.TextEntry> ScreenshotTextEntryList // TextEntry is not nullable, so a list is used
        {
            get
            {
                var infoTextList = new List<InfoWindow.TextEntry>();

                Texture2D screenshot = this.Screenshot;
                if (screenshot != null) infoTextList.Add(new InfoWindow.TextEntry(text: $"|", imageList: new List<Texture2D> { screenshot }, color: Color.White, scale: 7f));

                return infoTextList;
            }
        }

        public static void DisposeScreenshots()
        {
            int disposedScreenshots = 0;
            foreach (Texture2D screenshot in loadedScreenshots.Values)
            {
                if (screenshot != null && !screenshot.IsDisposed)
                {
                    screenshot.Dispose();
                    disposedScreenshots++;
                }
            }

            loadedScreenshots.Clear();
            if (disposedScreenshots > 0) MessageLog.Add(debugMessage: true, text: $"screenshots disposed ({disposedScreenshots})");
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