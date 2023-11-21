using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class GridTemplate
    {
        private const float currentVersion = 1.32f;
        private const string headerName = "_template_header.json";
        public const int demoWorldSeed = 777777;
        private static Point properCellSize = new(1, 1);
        private static Point ProperCellSize
        {
            get
            {
                if (properCellSize.X == 1 && properCellSize.Y == 1) properCellSize = CalculateCellSize();
                return properCellSize;
            }
        }

        public readonly int seed;
        public readonly int width;
        public readonly int height;
        private readonly int cellWidth;
        private readonly int cellHeight;
        public readonly int resDivider;

        private readonly float version;
        public readonly string templatePath;
        private readonly string headerPath;
        public DateTime CreatedDate { get; private set; }

        public GridTemplate(int seed, int width, int height, int cellWidth, int cellHeight, int resDivider, DateTime createdDate, float version = currentVersion, string templatePath = "", bool saveHeader = false)
        {
            this.seed = seed;
            this.width = width;
            this.height = height;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.resDivider = resDivider;
            this.version = version;
            this.CreatedDate = createdDate;

            this.templatePath = templatePath == "" ? this.CheckCreateFolder() : templatePath;
            this.headerPath = Path.Combine(this.templatePath, headerName);

            if (saveHeader) this.SaveHeader();
        }

        public bool IsObsolete
        { get { return this.version != currentVersion; } }

        public bool HasCorrectCellSize
        { get { return this.cellWidth != currentVersion; } }

        public static bool CorrectTemplatesExist
        { get { return CorrectTemplates.Count > 0; } }

        public static bool CorrectNonDemoTemplatesOfProperSizeExist
        { get { return CorrectNonDemoTemplatesOfProperSize.Count > 0; } }

        public static List<GridTemplate> CorrectTemplates
        {
            get
            {
                var templatePaths = Directory.GetDirectories(SonOfRobinGame.worldTemplatesPath);
                var correctTemplates = new List<GridTemplate> { };

                foreach (string templatePath in templatePaths)
                {
                    GridTemplate gridTemplate = LoadHeader(templatePath);
                    if (gridTemplate == null || gridTemplate.IsObsolete) continue;
                    else correctTemplates.Add(gridTemplate);
                }

                return correctTemplates;
            }
        }

        public static List<GridTemplate> CorrectNonDemoTemplatesOfProperSize
        {
            get
            {
                Point cellSize = ProperCellSize;
                return CorrectTemplates.Where(template => template.seed != demoWorldSeed && cellSize.X == template.cellWidth && cellSize.Y == template.cellHeight).ToList();
            }
        }

        private bool Equals(GridTemplate gridToCompare)
        {
            return
                this.seed == gridToCompare.seed &&
                this.width == gridToCompare.width &&
                this.height == gridToCompare.height &&
                this.cellWidth == gridToCompare.cellWidth &&
                this.cellHeight == gridToCompare.cellHeight &&
                this.resDivider == gridToCompare.resDivider &&
                this.version == gridToCompare.version;
        }

        public static GridTemplate LoadHeader(string templatePath)
        {
            string checkedHeaderPath = Path.Combine(templatePath, headerName);

            var headerData = FileReaderWriter.Load(path: checkedHeaderPath);
            if (headerData == null) return null;

            GridTemplate gridTemplate = Deserialize(headerData);
            return gridTemplate;
        }

        private void SaveHeader()
        {
            FileReaderWriter.Save(path: this.headerPath, savedObj: this.Serialize(), compress: false);
        }

        private string CheckCreateFolder()
        {
            // trying to use existing templates

            foreach (GridTemplate template in CorrectTemplates)
            {
                if (this.Equals(template)) return template.templatePath;
            }

            // skipping obsolete templates and creating a new one

            string templatePath;
            int counter = 0;

            while (true)
            {
                string folderName = $"seed_{this.seed}_{this.width}x{this.height}_{this.resDivider}_{counter}";
                templatePath = Path.Combine(SonOfRobinGame.worldTemplatesPath, folderName);

                if (!Directory.Exists(templatePath))
                {
                    Directory.CreateDirectory(templatePath);
                    break;
                }

                counter++;
            }

            return templatePath;
        }

        public static void DeleteObsolete()
        {
            var templatePaths = Directory.GetDirectories(SonOfRobinGame.worldTemplatesPath);
            var existingWorlds = Scene.GetAllScenesOfType(typeof(World)).Select(worldScene => (World)worldScene);
            var correctSaves = SaveHeaderManager.CorrectSaves;
            var pathsToKeep = new List<string>();

            foreach (string templatePath in templatePaths)
            {
                GridTemplate gridTemplate = LoadHeader(templatePath);
                if (gridTemplate == null || gridTemplate.IsObsolete) continue;

                if (gridTemplate.CreatedDate.Date == DateTime.Today) // templates created today should not be deleted
                {
                    pathsToKeep.Add(templatePath);
                    continue;
                }

                if (correctSaves.Where(saveHeader =>
                saveHeader.seed == gridTemplate.seed &&
                saveHeader.width == gridTemplate.width &&
                saveHeader.height == gridTemplate.height
                ).Any())
                {
                    pathsToKeep.Add(templatePath);
                    continue;
                }

                if (existingWorlds.Where(currentWorld =>
                currentWorld.seed == gridTemplate.seed &&
                currentWorld.IslandLevel.width == gridTemplate.width &&
                currentWorld.IslandLevel.height == gridTemplate.height
                ).Any())
                {
                    pathsToKeep.Add(templatePath);
                    continue;
                }
            }

            List<string> pathsToDelete = templatePaths.Where(path => !pathsToKeep.Contains(path)).ToList();
            foreach (string templatePathToDelete in pathsToDelete)
            {
                try
                { Directory.Delete(path: templatePathToDelete, recursive: true); }
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
            }
        }

        public static Point CalculateCellSize()
        {
            Vector2 maxFrameSize = CalculateMaxFrameSize();
            int cellWidth = (int)(maxFrameSize.X * 1.1);
            int cellHeight = (int)(maxFrameSize.Y * 1.1);

            cellWidth = (int)Math.Ceiling(cellWidth / 2d) * 2;
            cellHeight = (int)Math.Ceiling(cellHeight / 2d) * 2;

            return new Point(cellWidth, cellHeight);
        }

        private static Vector2 CalculateMaxFrameSize()
        {
            int frameMaxWidth = 0;
            int frameMaxHeight = 0;

            foreach (var kvp in AnimData.frameListById)
            {
                foreach (AnimFrame frame in kvp.Value)
                {
                    if (frame.ignoreWhenCalculatingMaxSize) continue;

                    int scaledWidth = (int)(frame.gfxWidth * frame.scale);
                    int scaledHeight = (int)(frame.gfxHeight * frame.scale);

                    if (scaledWidth > frameMaxWidth)
                    {
                        frameMaxWidth = scaledWidth;
                        // SonOfRobinGame.messageLog.AddMessage(text: $"frameMaxWidth {frameMaxWidth}: {kvp.Key}");
                    }
                    if (scaledHeight > frameMaxHeight)
                    {
                        frameMaxHeight = scaledHeight;
                        // SonOfRobinGame.messageLog.AddMessage(text: $"frameMaxHeight {frameMaxHeight}: {kvp.Key}");
                    }
                }
            }

            return new Vector2(frameMaxWidth, frameMaxHeight);
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> templateDict = new Dictionary<string, object>
            {
                { "seed", this.seed },
                { "width", this.width },
                { "height", this.height },
                { "cellWidth", this.cellWidth },
                { "cellHeight", this.cellHeight },
                { "resDivider", this.resDivider },
                { "version", this.version },
                { "templatePath", this.templatePath },
                { "createdDate", DateTime.Now },
            };

            return templateDict;
        }

        public static GridTemplate Deserialize(Object templateData)
        {
            var templateDict = (Dictionary<string, Object>)templateData;

            int seed = (int)(Int64)templateDict["seed"];
            int width = (int)(Int64)templateDict["width"];
            int height = (int)(Int64)templateDict["height"];
            int cellWidth = (int)(Int64)templateDict["cellWidth"];
            int cellHeight = (int)(Int64)templateDict["cellHeight"];
            int resDivider = (int)(Int64)templateDict["resDivider"];
            float version = (float)(double)templateDict["version"];
            string templatePath = (string)templateDict["templatePath"];
            DateTime createdDate = templateDict.ContainsKey("createdDate") ? (DateTime)templateDict["createdDate"] : DateTime.Now;

            return new GridTemplate(seed: seed, width: width, height: height, cellWidth: cellWidth, cellHeight: cellHeight, resDivider: resDivider, createdDate: createdDate, version: version, templatePath: templatePath);
        }
    }
}