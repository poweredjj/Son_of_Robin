using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Compendium
    {
        private readonly Dictionary<PieceTemplate.Name, HashSet<PieceTemplate.Name>> materialsBySources;
        private readonly Dictionary<PieceTemplate.Name, int> destroyedSources;
        private readonly Dictionary<PieceTemplate.Name, int> acquiredMaterials;

        public static readonly HashSet<PieceTemplate.Name> excludedMaterialNames = new() { PieceTemplate.Name.Hole, PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsMossySmall, PieceTemplate.Name.TreeStump, PieceTemplate.Name.JarTreasureRich, PieceTemplate.Name.JarTreasurePoor, PieceTemplate.Name.JarBroken, PieceTemplate.Name.CrystalDepositSmall };

        // to avoid exposing original dictionaries
        public bool AnyDestroyedSources { get { return this.destroyedSources.Count > 0; } }

        public int DestroyedSourcesCount { get { return this.destroyedSources.Select(kvp => (int)kvp.Value).Sum(); } }
        public int AcquiredMaterialsCount { get { return this.acquiredMaterials.Select(kvp => (int)kvp.Value).Sum(); } }
        public Dictionary<PieceTemplate.Name, int> DestroyedSources { get { return this.destroyedSources.ToDictionary(entry => entry.Key, entry => entry.Value); } }
        public Dictionary<PieceTemplate.Name, int> AcquiredMaterials { get { return this.acquiredMaterials.ToDictionary(entry => entry.Key, entry => entry.Value); } }
        public Dictionary<PieceTemplate.Name, HashSet<PieceTemplate.Name>> MaterialsBySources { get { return this.materialsBySources.ToDictionary(entry => entry.Key, entry => entry.Value); } }

        public Compendium()
        {
            this.materialsBySources = [];
            this.destroyedSources = [];
            this.acquiredMaterials = [];
        }

        public Compendium(Dictionary<string, Object> compendiumData)
        {
            // deserialization

            this.destroyedSources = ((Dictionary<int, int>)compendiumData["destroyedSources"])
                .ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);

            this.acquiredMaterials = ((Dictionary<int, int>)compendiumData["acquiredMaterials"])
                .ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);

            this.materialsBySources = [];

            foreach (var kvp in (Dictionary<int, List<int>>)compendiumData["materialsBySourcesAsInts"])
            {
                PieceTemplate.Name sourceName = (PieceTemplate.Name)kvp.Key;

                this.materialsBySources[sourceName] = [];

                foreach (int materialNameInt in kvp.Value)
                {
                    this.materialsBySources[sourceName].Add((PieceTemplate.Name)materialNameInt);
                }
            }
        }

        public Dictionary<string, object> Serialize()
        {
            var materialsBySourcesAsInts = new Dictionary<int, List<int>>();

            foreach (var kvp in this.materialsBySources)
            {
                PieceTemplate.Name sourceName = kvp.Key;
                HashSet<PieceTemplate.Name> materials = kvp.Value;

                materialsBySourcesAsInts[(int)sourceName] = [];
                foreach (PieceTemplate.Name materialName in materials)
                {
                    materialsBySourcesAsInts[(int)sourceName].Add((int)materialName);
                }
            }

            var compendiumData = new Dictionary<string, object>
            {
                // serialized as <int, int>, otherwise enums are serialized as strings
                { "materialsBySourcesAsInts", materialsBySourcesAsInts },
                { "acquiredMaterials", this.acquiredMaterials.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) },
                { "destroyedSources", this.destroyedSources.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) },
            };

            return compendiumData;
        }

        public void AddMaterialsForSource(PieceTemplate.Name sourceName, List<BoardPiece> materialsList)
        {
            var materialCountByName = new Dictionary<PieceTemplate.Name, int>();

            foreach (BoardPiece material in materialsList)
            {
                if (!materialCountByName.ContainsKey(material.name)) materialCountByName[material.name] = 0;
                materialCountByName[material.name]++;
            }

            this.AddMaterialsForSource(sourceName: sourceName, materialCountByName: materialCountByName);
        }

        public void AddMaterialsForSource(PieceTemplate.Name sourceName, Dictionary<PieceTemplate.Name, int> materialCountByName)
        {
            if (!this.materialsBySources.ContainsKey(sourceName)) this.materialsBySources[sourceName] = [];

            foreach (var kvp in materialCountByName)
            {
                PieceTemplate.Name materialName = kvp.Key;
                int materialCount = kvp.Value;

                // MessageLog.Add(debugMessage: true, text: $"Compendium - adding material: {materialName} x{materialCount}");

                if (!this.materialsBySources.ContainsKey(sourceName)) this.materialsBySources[sourceName] = [];
                this.materialsBySources[sourceName].Add(materialName);

                if (!this.acquiredMaterials.ContainsKey(materialName)) this.acquiredMaterials[materialName] = 0;
                this.acquiredMaterials[materialName] += materialCount;
            }
        }

        public void AddDestroyedSource(PieceTemplate.Name sourceName)
        {
            // MessageLog.Add(debugMessage: true, text: $"Compendium - adding destroyed source: {sourceName}");

            if (!this.destroyedSources.ContainsKey(sourceName)) this.destroyedSources[sourceName] = 0;
            this.destroyedSources[sourceName]++;
        }

        public void CreateEntriesForDestroyedSources(Menu menu)
        {
            this.CreateMenuEntriesForSummary(menu: menu, color: new Color(105, 20, 201), collectionToShow: this.destroyedSources, header: "Objects destroyed");
        }

        public void CreateEntriesForAcquiredMaterials(Menu menu)
        {
            this.CreateMenuEntriesForSummary(menu: menu, color: new Color(47, 30, 148), collectionToShow: this.acquiredMaterials.Where(kvp => !excludedMaterialNames.Contains(kvp.Key)).ToDictionary(), header: "Materials acquired");
        }

        private void CreateMenuEntriesForSummary(Menu menu, Color color, Dictionary<PieceTemplate.Name, int> collectionToShow, string header)
        {
            if (collectionToShow.Count == 0) return;

            int entriesPerPage = 15;
            int pageCounter = 1;
            int pieceCounter = 0;

            var textLines = new List<string>();
            var imageList = new List<ImageObj>();

            bool showPageCounter = collectionToShow.Count > entriesPerPage;

            foreach (var kvp in collectionToShow)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                int pieceCount = kvp.Value;

                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                textLines.Add($"|  x{pieceCount}  {pieceInfo.secretName}");
                imageList.Add(pieceInfo.imageObj);

                pieceCounter++;
                if (pieceCounter >= entriesPerPage || pieceName == collectionToShow.Last().Key)
                {
                    string fullHeader = showPageCounter ? $"{header} - page {pageCounter}" : $"{header}";

                    var infoTextList = new List<InfoWindow.TextEntry>
                    {
                        new InfoWindow.TextEntry(text: fullHeader, color: Color.White, scale: 1f),
                        new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList.ToList(), color: Color.White, scale: 1f, minMarkerWidthMultiplier: 1.4f, imageAlignX: Helpers.AlignX.Left)
                    };

                    string nameString = showPageCounter ? $"{header.ToLower()} - page {pageCounter}" : $"{header.ToLower()}";
                    Invoker invoker = new Invoker(menu: menu, name: nameString, taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);

                    invoker.bgColor = color;

                    pageCounter++;
                    pieceCounter = 0;
                    textLines.Clear();
                    imageList.Clear();
                }
            }
        }
    }
}