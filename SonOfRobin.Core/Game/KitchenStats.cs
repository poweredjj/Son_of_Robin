using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class KitchenStats
    {
        private Dictionary<PieceTemplate.Name, int> usedBases;
        private Dictionary<PieceTemplate.Name, int> usedBoosters;
        public int AllIngredientsCount { get; private set; }
        public int BaseCount { get; private set; }
        public int BoosterCount { get; private set; }
        public int TotalCookCount { get; private set; }
        public int IngredientNamesCount
        { get { return usedBases.Count + usedBoosters.Count; } }

        public KitchenStats()
        {
            this.usedBases = new Dictionary<PieceTemplate.Name, int>();
            this.usedBoosters = new Dictionary<PieceTemplate.Name, int>();
            this.AllIngredientsCount = 0;
            this.TotalCookCount = 0;
        }

        public void RegisterCooking(List<BoardPiece> baseList, List<BoardPiece> boosterList = null)
        {
            this.TotalCookCount++;

            foreach (BoardPiece ingredient in baseList)
            {
                this.AllIngredientsCount++;
                this.BaseCount++;
                if (!this.usedBases.ContainsKey(ingredient.name)) this.usedBases[ingredient.name] = 0;
                this.usedBases[ingredient.name]++;
            }

            if (boosterList != null)
            {
                foreach (BoardPiece ingredient in boosterList)
                {
                    this.AllIngredientsCount++;
                    this.BoosterCount++;
                    if (!this.usedBoosters.ContainsKey(ingredient.name)) this.usedBoosters[ingredient.name] = 0;
                    this.usedBoosters[ingredient.name]++;
                }
            }
        }

        public Dictionary<string, object> Serialize()
        {
            var statsData = new Dictionary<string, object>
            {
                { "AllIngredientsCount", this.AllIngredientsCount },
                { "BaseCount", this.BaseCount },
                { "BoosterCount", this.BoosterCount },
                { "TotalCookCount", this.TotalCookCount },
                { "usedBases", this.usedBases.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) }, // serialized as <int, int>, otherwise enums are serialized as strings
                { "usedBoosters", this.usedBoosters.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) }, // serialized as <int, int>, otherwise enums are serialized as strings
            };

            return statsData;
        }

        public void Deserialize(Dictionary<string, Object> statsData)
        {
            this.AllIngredientsCount = (int)(Int64)statsData["AllIngredientsCount"];
            this.BaseCount = (int)(Int64)statsData["BaseCount"];
            this.BoosterCount = (int)(Int64)statsData["BoosterCount"];
            this.TotalCookCount = (int)(Int64)statsData["TotalCookCount"];
            this.usedBases = ((Dictionary<int, int>)statsData["usedBases"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
            this.usedBoosters = ((Dictionary<int, int>)statsData["usedBoosters"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
        }
    }
}