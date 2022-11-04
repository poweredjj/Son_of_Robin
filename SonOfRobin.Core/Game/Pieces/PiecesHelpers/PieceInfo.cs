﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class PieceInfo
    {
        private static readonly Dictionary<PieceTemplate.Name, Info> info = new Dictionary<PieceTemplate.Name, Info> { };
        public static List<Info> AllInfo { get { return info.Values.ToList(); } }
        public static bool HasBeenInitialized { get; private set; } = false;
        public class Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public readonly AllowedTerrain allowedTerrain;
            public readonly int stackSize;
            public readonly Type type;
            public readonly bool blocksMovement;
            public readonly bool convertsWhenUsed;
            public readonly PieceTemplate.Name convertsToWhenUsed;
            public readonly bool shootsProjectile;
            public bool isCarnivorous;
            public readonly BoardPiece.Category category;
            public readonly bool canBePickedUp;
            public List<BuffEngine.Buff> buffList;
            public readonly AnimFrame frame;
            public readonly Texture2D texture;
            public readonly Scheduler.TaskName toolbarTask;
            public readonly Scheduler.TaskName boardTask;
            public List<PieceTemplate.Name> eats;
            public List<PieceTemplate.Name> isEatenBy;
            public readonly bool hasFruit;
            public readonly PieceTemplate.Name fruitName;
            public PieceTemplate.Name isSpawnedBy;

            public List<string> BuffDescList
            {
                get
                {
                    var buffDescList = new List<string> { };
                    if (buffList == null) return buffDescList;

                    foreach (var buff in buffList)
                    {
                        buffDescList.Add(buff.description);
                    }
                    return buffDescList;
                }
            }

            public Info(BoardPiece piece)
            {
                this.name = piece.name;
                this.category = piece.category;
                this.allowedTerrain = piece.sprite.allowedTerrain;
                this.canBePickedUp = piece.canBePickedUp;
                this.type = piece.GetType();
                this.blocksMovement = piece.sprite.blocksMovement;
                this.readableName = piece.readableName;
                this.description = piece.description;
                this.stackSize = piece.stackSize;
                this.buffList = piece.buffList;
                this.frame = piece.sprite.frame;
                this.texture = this.frame.texture;
                this.toolbarTask = piece.toolbarTask;
                this.boardTask = piece.boardTask;
                if (piece.GetType() == typeof(Animal)) this.eats = ((Animal)piece).eats;
                this.convertsWhenUsed = false;
                if (piece.GetType() == typeof(Potion))
                {
                    this.convertsWhenUsed = true;
                    this.convertsToWhenUsed = ((Potion)piece).convertsToWhenUsed;
                }
                this.shootsProjectile = false;
                if (piece.GetType() == typeof(Tool)) this.shootsProjectile = ((Tool)piece).shootsProjectile;
                this.isEatenBy = new List<PieceTemplate.Name> { };

                this.hasFruit = false;
                if (piece.GetType() == typeof(Plant))
                {
                    Plant plant = (Plant)piece;
                    if (plant.fruitEngine != null)
                    {
                        this.fruitName = plant.fruitEngine.fruitName;
                        this.hasFruit = true;
                    }
                }
            }
        }

        public static Info GetInfo(PieceTemplate.Name pieceName)
        {
            return info[pieceName];
        }

        public static Texture2D GetTexture(PieceTemplate.Name pieceName)
        {
            // to simplify frequently used query

            return info[pieceName].texture;
        }

        public static void CreateAllInfo()
        {
            // creates one instance of every piece type - to get required info out of it

            foreach (PieceTemplate.Name name in PieceTemplate.allNames)
            {
                BoardPiece piece = PieceTemplate.Create(templateName: name, world: null);
                info[name] = new Info(piece: piece);
            }

            // getting isEatenBy data

            foreach (Info potentialPrey in info.Values)
            {
                if (potentialPrey.eats != null)
                {
                    // animal will either hunt the player or run away
                    if (!potentialPrey.eats.Contains(PieceTemplate.Name.Player)) potentialPrey.isEatenBy.Add(PieceTemplate.Name.Player);

                    foreach (Info potentialPredator in info.Values)
                    {
                        if (potentialPredator.eats != null && potentialPredator.eats.Contains(potentialPrey.name))
                        {
                            potentialPrey.isEatenBy.Add(potentialPredator.name);
                            if (potentialPrey.type == typeof(Animal)) potentialPredator.isCarnivorous = true;
                        }
                    }
                }
            }

            // getting fruitSpawner data

            foreach (Info fruitSpawnerInfo in info.Values.Where(info => info.hasFruit))
            {
                info[fruitSpawnerInfo.fruitName].isSpawnedBy = fruitSpawnerInfo.name;
            }

            HasBeenInitialized = true;
        }

        public static List<PieceTemplate.Name> GetIsEatenBy(PieceTemplate.Name name)
        {
            if (info.ContainsKey(name) && info[name].isEatenBy != null) return info[name].isEatenBy;

            return new List<PieceTemplate.Name> { };
        }

    }
}
