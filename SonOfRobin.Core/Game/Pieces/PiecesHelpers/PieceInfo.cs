using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class PieceInfo
    {
        private static readonly Dictionary<PieceTemplate.Name, Info> info = new Dictionary<PieceTemplate.Name, Info>();
        public static readonly Dictionary<Type, List<PieceTemplate.Name>> namesForType = new Dictionary<Type, List<PieceTemplate.Name>>();
        public static bool HasBeenInitialized { get; private set; } = false;

        public class Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public readonly AllowedTerrain allowedTerrain;
            public readonly int stackSize;
            public readonly Type type;
            public readonly bool serialize;
            public readonly bool blocksMovement;
            public readonly bool convertsWhenUsed;
            public readonly PieceTemplate.Name convertsToWhenUsed;
            public readonly bool shootsProjectile;
            public readonly Equipment.EquipType equipType;
            public bool isCarnivorous;
            public readonly BoardPiece.Category category;
            public readonly bool canBePickedUp;
            public readonly List<Buff> buffList;
            public readonly AnimFrame frame;
            public readonly Texture2D texture;
            public readonly Scheduler.TaskName toolbarTask;
            public readonly Scheduler.TaskName boardTask;
            public List<PieceTemplate.Name> eats;
            public List<PieceTemplate.Name> isEatenBy;
            public List<PieceTemplate.Name> combinesWith;
            public readonly bool hasFruit;
            public readonly float massTakenMultiplier;
            public readonly float maxHitPoints;
            public readonly bool toolIndestructible;
            public readonly int toolRange;
            public readonly float startHitPoints;
            public readonly BoardPiece.State initialActiveState;
            public readonly int strength;
            public readonly float mass;
            public readonly bool canBeHit;
            public readonly float speed;
            public readonly Color color;
            public readonly float opacity;
            public readonly LightEngine lightEngine;
            public readonly ParticleEngine particleEngine;
            public readonly string animName;
            public readonly byte animSize;
            public readonly PieceTemplate.Name fruitName;
            public PieceTemplate.Name isSpawnedBy;
            public Dictionary<BoardPiece.Category, float> strengthMultiplierByCategory;
            public readonly float cookerFoodMassMultiplier;

            public Info(BoardPiece piece)
            {
                this.animSize = piece.sprite.AnimSize;
                this.mass = piece.Mass;

                if (piece.maxMassForSize != null) piece.Mass = piece.maxMassForSize.Last(); // to show frame of biggest size
                this.frame = piece.sprite.AnimFrame;
                piece.Mass = this.mass; // reverting to original mass

                this.name = piece.name;
                this.category = piece.category;
                this.allowedTerrain = piece.sprite.allowedTerrain;
                this.canBePickedUp = piece.canBePickedUp;
                this.maxHitPoints = piece.maxHitPoints;
                this.startHitPoints = piece.HitPoints;
                this.strength = piece.strength;
                this.speed = piece.speed;
                this.type = piece.GetType();
                this.serialize = piece.serialize;
                this.initialActiveState = piece.activeState;
                this.blocksMovement = piece.sprite.blocksMovement;
                this.readableName = piece.readableName;
                this.description = piece.description;
                this.stackSize = piece.stackSize;
                this.buffList = piece.buffList;
                this.texture = this.frame.texture;
                this.toolbarTask = piece.toolbarTask;
                this.boardTask = piece.boardTask;
                this.canBeHit = piece.canBeHit;
                this.color = piece.sprite.color;
                this.opacity = piece.sprite.opacity;
                this.lightEngine = piece.sprite.lightEngine;
                this.particleEngine = piece.sprite.particleEngine;
                this.animName = piece.sprite.AnimName;
                if (piece.GetType() == typeof(Animal)) this.eats = ((Animal)piece).Eats;
                this.equipType = piece.GetType() == typeof(Equipment) ? ((Equipment)piece).equipType : Equipment.EquipType.None;
                this.cookerFoodMassMultiplier = piece.GetType() == typeof(Cooker) ? ((Cooker)piece).foodMassMultiplier : 0f;

                this.convertsWhenUsed = false;
                if (piece.GetType() == typeof(Potion))
                {
                    this.convertsWhenUsed = true;
                    this.convertsToWhenUsed = ((Potion)piece).convertsToWhenUsed;
                }

                this.shootsProjectile = false;
                this.toolIndestructible = false;
                this.toolRange = 0;
                if (piece.GetType() == typeof(Tool))
                {
                    Tool tool = (Tool)piece;
                    this.shootsProjectile = tool.shootsProjectile;
                    this.strengthMultiplierByCategory = tool.multiplierByCategory;
                    this.toolIndestructible = tool.indestructible;
                    this.toolRange = tool.range;
                }

                if (piece.GetType() == typeof(Shelter))
                {
                    Shelter shelter = (Shelter)piece;
                    SleepEngine sleepEngine = shelter.sleepEngine;
                    this.buffList.AddRange(sleepEngine.wakeUpBuffs);
                }

                if (piece.GetType() == typeof(Projectile))
                {
                    // "emulating" tool multiplier list
                    this.strengthMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, ((Projectile)piece).baseHitPower / 4 } };
                }

                this.isEatenBy = new List<PieceTemplate.Name> { };

                this.combinesWith = PieceCombiner.CombinesWith(this.name);

                this.hasFruit = false;
                this.massTakenMultiplier = 1;
                if (piece.GetType() == typeof(Plant))
                {
                    Plant plant = (Plant)piece;
                    if (plant.fruitEngine != null)
                    {
                        this.fruitName = plant.fruitEngine.fruitName;
                        this.hasFruit = true;
                    }
                    this.massTakenMultiplier = plant.massTakenMultiplier;
                }
            }

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
        }

        public static List<Info> AllInfo
        { get { return info.Values.ToList(); } }

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
                    if (!ContainsPlayer(potentialPrey.eats)) potentialPrey.isEatenBy.AddRange(PieceInfo.GetPlayerNames());

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

            // getting names for type data

            foreach (Info currentInfo in info.Values)
            {
                if (!namesForType.ContainsKey(currentInfo.type)) namesForType[currentInfo.type] = new List<PieceTemplate.Name>();
                namesForType[currentInfo.type].Add(currentInfo.name);
            }

            HasBeenInitialized = true;
        }

        public static List<PieceTemplate.Name> GetIsEatenBy(PieceTemplate.Name name)
        {
            if (info.ContainsKey(name) && info[name].isEatenBy != null) return info[name].isEatenBy;

            return new List<PieceTemplate.Name> { };
        }

        public static bool IsPlayer(PieceTemplate.Name pieceName)
        {
            return info[pieceName].type == typeof(Player) && pieceName != PieceTemplate.Name.PlayerGhost;
        }

        public static bool ContainsPlayer(List<PieceTemplate.Name> pieceNameList)
        {
            foreach (PieceTemplate.Name pieceName in pieceNameList)
            {
                if (IsPlayer(pieceName)) return true;
            }
            return false;
        }

        public static List<PieceTemplate.Name> GetPlayerNames()
        {
            var playerNameList = new List<PieceTemplate.Name>();
            foreach (Info currentInfo in AllInfo)
            {
                if (IsPlayer(currentInfo.name)) playerNameList.Add(currentInfo.name);
            }

            return playerNameList;
        }

        public static List<PieceTemplate.Name> GetNamesForEquipType(Equipment.EquipType equipType)
        {
            var equipNameList = new List<PieceTemplate.Name>();
            foreach (Info currentInfo in AllInfo)
            {
                if (currentInfo.equipType == equipType) equipNameList.Add(currentInfo.name);
            }

            return equipNameList;
        }

        public static List<InfoWindow.TextEntry> GetCategoryAffinityTextEntryList(PieceTemplate.Name pieceName, float scale = 1f)
        {
            var entryList = new List<InfoWindow.TextEntry>();

            var multiplierByCategory = info[pieceName].strengthMultiplierByCategory;
            if (multiplierByCategory == null) return entryList;

            string text = "|     ";
            var imageList = new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Biceps].texture };

            foreach (BoardPiece.Category category in BoardPiece.allCategories) // allCategories is used to keep the same order for every tool
            {
                if (multiplierByCategory.ContainsKey(category) && multiplierByCategory[category] > 0)
                {
                    float multiplier = multiplierByCategory[category];

                    text += $"| {multiplier}   ";
                    imageList.Add(BoardPiece.GetTextureForCategory(category));
                }
            }

            entryList.Add(new InfoWindow.TextEntry(text: text, scale: scale, imageList: imageList, color: Color.White));

            return entryList;
        }

        public static List<InfoWindow.TextEntry> GetCombinesWithTextEntryList(PieceTemplate.Name pieceName, float scale = 1f)
        {
            var entryList = new List<InfoWindow.TextEntry>();

            var combinesWith = info[pieceName].combinesWith;
            if (!combinesWith.Any()) return entryList;

            string text = "Combines with: ";
            var imageList = new List<Texture2D>();

            foreach (PieceTemplate.Name combineName in combinesWith)
            {
                text += "| ";
                imageList.Add(GetTexture(combineName));
            }

            entryList.Add(new InfoWindow.TextEntry(text: text, scale: scale, imageList: imageList, color: Color.White));

            return entryList;
        }
    }
}