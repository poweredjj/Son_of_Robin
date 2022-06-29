using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public struct SaveInfo
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
                return $"{SaveDateString} time played: {ElapsedTimeString} seed: {this.seed} {this.width}x{this.height}{autoSaveString}";
            }
        }
        public SaveInfo(string folderName)
        {
            this.folderName = folderName;

            this.fullPath = Path.Combine(SonOfRobinGame.saveGamesPath, folderName);

            string headerPath = Path.Combine(this.fullPath, "header.sav");

            var headerData = (Dictionary<string, Object>)LoaderSaver.Load(path: headerPath);

            this.saveIsCorrect = false;
            this.saveDate = DateTime.FromOADate(0d);
            this.autoSave = false;
            this.seed = -1;
            this.width = -1;
            this.height = -1;
            this.currentUpdate = -1;
            this.elapsedTime = TimeSpan.FromSeconds(0);

            if (headerData != null && headerData.ContainsKey("saveVersion"))
            {
                float saveVersion = (float)headerData["saveVersion"];
                if (saveVersion == SaveManager.saveVersion)
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

    public class SaveManager
    {
        public readonly static float saveVersion = 1.228f;

        public static bool AnySavesExist
        { get { return Directory.GetDirectories(SonOfRobinGame.saveGamesPath).ToList().Count > 0; } }
        public static List<SaveInfo> CorrectSaves
        {
            get
            {
                var correctSaves = new List<SaveInfo> { };

                var saveFolders = Directory.GetDirectories(SonOfRobinGame.saveGamesPath);

                foreach (string folder in saveFolders)
                {
                    var saveInfo = new SaveInfo(Path.GetFileName(folder));
                    if (saveInfo.saveIsCorrect) correctSaves.Add(saveInfo);
                }

                correctSaves = correctSaves.OrderByDescending(s => s.saveDate).ToList();
                return correctSaves;
            }
        }

        public static List<SaveInfo> IncorrectSaves
        {
            get
            {
                var incorrectSaves = new List<SaveInfo> { };

                var saveFolders = Directory.GetDirectories(SonOfRobinGame.saveGamesPath);

                foreach (string folder in saveFolders)
                {
                    var saveInfo = new SaveInfo(Path.GetFileName(folder));
                    if (!saveInfo.saveIsCorrect) incorrectSaves.Add(saveInfo);
                }

                return incorrectSaves;
            }
        }

        public static string NewSaveSlotName
        {
            get
            {
                int newSaveSlotNo = 1;
                foreach (SaveInfo saveInfo in CorrectSaves)
                {
                    try
                    {
                        int saveSlotNo = Convert.ToInt32(saveInfo.folderName);
                        if (saveSlotNo >= newSaveSlotNo) newSaveSlotNo = saveSlotNo + 1;
                    }
                    catch (Exception)
                    { }
                }
                return Convert.ToString(newSaveSlotNo);
            }
        }

        public static void DeleteObsoleteSaves()
        {
            foreach (SaveInfo saveInfo in IncorrectSaves)
            {
                saveInfo.Delete();
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Deleted obsolete save '{saveInfo.folderName}'.", color: Color.White);
            }
        }

    }
}
