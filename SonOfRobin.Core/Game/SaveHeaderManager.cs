using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class SaveHeaderManager
    {
        public static readonly float saveVersion = 1.479f;

        public static bool AnySavesExist
        { get { return Directory.GetDirectories(SonOfRobinGame.saveGamesPath).ToList().Count > 0; } }

        public static List<SaveHeaderInfo> CorrectSaves
        {
            get
            {
                var correctSaves = new List<SaveHeaderInfo> { };

                var saveFolders = Directory.GetDirectories(SonOfRobinGame.saveGamesPath);

                foreach (string folder in saveFolders)
                {
                    var saveInfo = new SaveHeaderInfo(Path.GetFileName(folder));
                    if (saveInfo.saveIsCorrect) correctSaves.Add(saveInfo);
                }

                correctSaves = correctSaves.OrderByDescending(s => s.saveDate).ToList();
                return correctSaves;
            }
        }

        public static List<SaveHeaderInfo> IncorrectSaves
        {
            get
            {
                var incorrectSaves = new List<SaveHeaderInfo> { };

                var saveFolders = Directory.GetDirectories(SonOfRobinGame.saveGamesPath);

                foreach (string folder in saveFolders)
                {
                    var saveInfo = new SaveHeaderInfo(Path.GetFileName(folder));
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
                foreach (SaveHeaderInfo saveInfo in CorrectSaves)
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
            List<SaveHeaderInfo> incorrectSaves = IncorrectSaves;

            if (!incorrectSaves.Any())
            {
                new TextWindow(text: "No obsolete saves were found.", textColor: Color.White, bgColor: Color.Blue, useTransition: true, animate: true);
                return;
            }

            foreach (SaveHeaderInfo saveInfo in incorrectSaves)
            {
                saveInfo.Delete();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Deleted obsolete save '{saveInfo.folderName}'.", color: Color.White);
            }

            MessageLog.AddMessage(msgType: MsgType.User, message: $"Deleted obsolete saves ({incorrectSaves.Count}).", color: Color.White);
        }
    }
}