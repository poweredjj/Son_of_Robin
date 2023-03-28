using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class GridTemplate
    {
        private const float currentVersion = 1.25f;
        private const string headerName = "_template_header.json";
        public const int demoWorldSeed = 777777;

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

        public GridTemplate(int seed, int width, int height, int cellWidth, int cellHeight, int resDivider, float version = currentVersion, string templatePath = "")
        {
            this.seed = seed;
            this.width = width;
            this.height = height;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.resDivider = resDivider;
            this.version = version;
            this.CreatedDate = DateTime.Now;

            this.templatePath = templatePath == "" ? this.CheckCreateFolder() : templatePath;
            this.headerPath = Path.Combine(this.templatePath, headerName);

            this.SaveHeader();
        }

        public bool IsObsolete
        { get { return this.version != currentVersion; } }

        public static bool CorrectTemplatesExist
        { get { return CorrectTemplates.Any(); } }

        public static bool CorrectNonDemoTemplatesExist
        { get { return CorrectNonDemoTemplates.Any(); } }

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

        public static List<GridTemplate> CorrectNonDemoTemplates
        {
            get { return CorrectTemplates.Where(template => template.seed != demoWorldSeed).ToList(); }
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
            FileReaderWriter.Save(path: this.headerPath, savedObj: this.Serialize());
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

            return new GridTemplate(seed: seed, width: width, height: height, cellWidth: cellWidth, cellHeight: cellHeight, resDivider: resDivider, version: version, templatePath: templatePath);
        }
    }
}