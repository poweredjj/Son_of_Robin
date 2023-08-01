﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeatHarvestStats
    {
        private Dictionary<PieceTemplate.Name, int> harvestedAnimalCountByName;
        private Dictionary<PieceTemplate.Name, int> obtainedBasePieceCountByName;
        private Dictionary<PieceTemplate.Name, int> obtainedBonusPieceCountByName;

        public int TotalHarvestCount
        { get { return this.harvestedAnimalCountByName.Values.Sum(); } }

        public int AnimalSpeciesHarvested
        { get { return this.harvestedAnimalCountByName.Count; } }

        public int ObtainedBasePieceCount
        { get { return this.obtainedBasePieceCountByName.Values.Sum(); } }

        public int ObtainedBonusPieceCount
        { get { return this.obtainedBonusPieceCountByName.Values.Sum(); } }

        public int ObtainedTotalPieceCount
        { get { return this.ObtainedBasePieceCount + this.ObtainedBonusPieceCount; } }

        public Dictionary<PieceTemplate.Name, int> HarvestedAnimalCountByName
        { get { return this.harvestedAnimalCountByName.ToDictionary(entry => entry.Key, entry => entry.Value); } }

        public Dictionary<PieceTemplate.Name, int> ObtainedBasePieceCountByName
        { get { return this.obtainedBasePieceCountByName.ToDictionary(entry => entry.Key, entry => entry.Value); } }

        public Dictionary<PieceTemplate.Name, int> ObtainedBonusPieceCountByName
        { get { return this.obtainedBonusPieceCountByName.ToDictionary(entry => entry.Key, entry => entry.Value); } }

        public MeatHarvestStats()
        {
            this.harvestedAnimalCountByName = new Dictionary<PieceTemplate.Name, int>();
            this.obtainedBasePieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            this.obtainedBonusPieceCountByName = new Dictionary<PieceTemplate.Name, int>();
        }

        public void RegisterMeatHarvest(BoardPiece animalPiece, List<BoardPiece> obtainedBasePieces, List<BoardPiece> obtainedBonusPieces)
        {
            if (!this.harvestedAnimalCountByName.ContainsKey(animalPiece.name)) this.harvestedAnimalCountByName[animalPiece.name] = 0;
            this.harvestedAnimalCountByName[animalPiece.name]++;

            foreach (BoardPiece obtainedPiece in obtainedBasePieces)
            {
                if (!this.obtainedBasePieceCountByName.ContainsKey(obtainedPiece.name)) this.obtainedBasePieceCountByName[obtainedPiece.name] = 0;
                this.obtainedBasePieceCountByName[obtainedPiece.name]++;
            }

            foreach (BoardPiece obtainedPiece in obtainedBonusPieces)
            {
                if (!this.obtainedBonusPieceCountByName.ContainsKey(obtainedPiece.name)) this.obtainedBonusPieceCountByName[obtainedPiece.name] = 0;
                this.obtainedBonusPieceCountByName[obtainedPiece.name]++;
            }
        }

        public Dictionary<string, object> Serialize()
        {
            var statsData = new Dictionary<string, object>
            {
                // serialized as <int, int>, otherwise enums are serialized as strings
                { "harvestedAnimalCountByName", this.harvestedAnimalCountByName.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) },
                { "obtainedBasePieceCountByName", this.obtainedBasePieceCountByName.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) },
                { "obtainedBonusPieceCountByName", this.obtainedBonusPieceCountByName.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) },
            };

            return statsData;
        }

        public void Deserialize(Dictionary<string, Object> statsData)
        {
            this.harvestedAnimalCountByName = ((Dictionary<int, int>)statsData["harvestedAnimalCountByName"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
            this.obtainedBasePieceCountByName = ((Dictionary<int, int>)statsData["obtainedBasePieceCountByName"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
            this.obtainedBonusPieceCountByName = ((Dictionary<int, int>)statsData["obtainedBonusPieceCountByName"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
        }
    }
}