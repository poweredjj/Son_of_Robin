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

        // to avoid exposing original dictionaries
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

                MessageLog.Add(debugMessage: true, text: $"Compendium - adding material: {materialName} x{materialCount}");

                if (!this.materialsBySources.ContainsKey(sourceName)) this.materialsBySources[sourceName] = [];
                this.materialsBySources[sourceName].Add(materialName);

                if (!this.acquiredMaterials.ContainsKey(materialName)) this.acquiredMaterials[materialName] = 0;
                this.acquiredMaterials[materialName] += materialCount;
            }
        }

        public void AddDestroyedSource(PieceTemplate.Name sourceName)
        {
            MessageLog.Add(debugMessage: true, text: $"Compendium - adding destroyed source: {sourceName}");

            if (!this.destroyedSources.ContainsKey(sourceName)) this.destroyedSources[sourceName] = 0;
            this.destroyedSources[sourceName]++;
        }
    }
}