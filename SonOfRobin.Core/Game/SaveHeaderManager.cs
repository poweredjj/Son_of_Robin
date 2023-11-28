using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class SaveHeaderManager
    {
        public static readonly float saveVersion = 1.507f;

        public static bool AnySavesExist
        { get { return Directory.GetDirectories(SonOfRobinGame.saveGamesPath).Length > 0; } }

        public static List<SaveHeaderInfo> CorrectSaves
        {
            get
            {
                var correctSaves = new List<SaveHeaderInfo> { };

                var saveFolders = Directory.GetDirectories(SonOfRobinGame.saveGamesPath);
                foreach (string folder in saveFolders)
                {
                    SaveHeaderInfo saveInfo = new(folderName: Path.GetFileName(folder));
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
                    var saveInfo = new SaveHeaderInfo(folderName: Path.GetFileName(folder));
                    if (!saveInfo.saveIsCorrect || saveInfo.saveIsObsolete || saveInfo.saveIsCorrupted) incorrectSaves.Add(saveInfo);
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

        public static void DeleteIncompatibleSaves()
        {
            List<SaveHeaderInfo> incorrectSaves = IncorrectSaves;

            if (incorrectSaves.Count == 0)
            {
                new TextWindow(text: "No incompatible saves were found.", textColor: Color.White, bgColor: Color.Blue, useTransition: true, animate: true);
                return;
            }

            foreach (SaveHeaderInfo saveInfo in incorrectSaves)
            {
                saveInfo.Delete();
                MessageLog.Add(debugMessage: true, text: $"Deleted obsolete save '{saveInfo.folderName}'.", textColor: Color.White);
            }

            MessageLog.Add(text: $"Deleted obsolete saves ({incorrectSaves.Count}).", textColor: Color.White);
        }
    }
}