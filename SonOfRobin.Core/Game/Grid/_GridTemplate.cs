using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    [Serializable]
    public class GridTemplate
    {
        private static readonly float currentVersion = 1.14f;
        private static readonly string headerName = "_template_header.dat";

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

        public bool IsObsolete { get { return this.version != currentVersion; } }
        public static bool CorrectTemplatesExist { get { return CorrectTemplates.Count > 0; } }
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

        public GridTemplate(int seed, int width, int height, int cellWidth, int cellHeight, int resDivider)
        {
            this.seed = seed;
            this.width = width;
            this.height = height;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.resDivider = resDivider;
            this.version = currentVersion;
            this.CreatedDate = DateTime.Now;

            this.templatePath = this.CheckCreateFolder();
            this.headerPath = Path.Combine(this.templatePath, headerName);

            this.SaveHeader();
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

            GridTemplate headerData = (GridTemplate)FileReaderWriter.Load(path: checkedHeaderPath);
            return headerData;
        }

        private void SaveHeader()
        {
            FileReaderWriter.Save(path: this.headerPath, savedObj: this);
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

    }
}
