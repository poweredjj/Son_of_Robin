using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WaveCollapseMap
    {
        public enum Name
        {
            Empty, Error,
            WaterDeep, WaterMedium, WaterShallow, Sand, Dirt, Grass, MountainsLow, MountainsMedium, MountainsHigh, Lava,
        }

        public static readonly List<Name> allNames = Enum.GetValues(typeof(Name)).Cast<Name>().ToList();

        public static readonly Dictionary<Name, List<Name>> allAllowedCombinations = new Dictionary<Name, List<Name>>
        {
            { Name.WaterDeep, new List<Name> { Name.WaterMedium } },
            { Name.Sand, new List<Name> { Name.Dirt, Name.WaterShallow, Name.WaterMedium } },
            { Name.Dirt, new List<Name> { Name.Grass, Name.MountainsLow } },
            { Name.Grass, new List<Name> { Name.Sand, Name.Dirt, Name.MountainsLow } },
            { Name.MountainsLow, new List<Name> { Name.Dirt, Name.Grass, Name.MountainsMedium } },
            { Name.MountainsMedium, new List<Name> { Name.MountainsLow, Name.MountainsHigh, Name.Lava } },
            { Name.MountainsHigh, new List<Name> { Name.MountainsMedium, Name.Lava } },
            { Name.Lava, new List<Name> { Name.MountainsMedium, Name.MountainsHigh } },
        };

        public static readonly Dictionary<Name, float> allProbabilities = new Dictionary<Name, float> {
            // modifier for random() - 1f is 100%, 0f is 0%
            { Name.WaterDeep, 0.4f },
            { Name.WaterMedium, 0.4f },
            { Name.WaterShallow, 0.4f },
            { Name.Sand, 0.01f },
            { Name.Dirt, 0.05f },
            { Name.Grass, 0.2f },
            { Name.MountainsLow, 0.15f },
            { Name.MountainsMedium, 0.1f },
            { Name.MountainsHigh, 0.1f },
            { Name.Lava, 0.01f },
        };

        private readonly Dictionary<Name, Texture2D> debugTextures;
        private readonly DateTime creationTime;
        private TimeSpan processTime;
        public readonly List<Name> namesToUse;
        public readonly Dictionary<Name, List<Name>> allowedNamesByName;

        public readonly Dictionary<Name, float> allowedPercentageByName;
        public readonly WaveCollapseElement[,] waveBoard;
        private readonly List<WaveCollapseElement> allElements;
        private readonly Dictionary<string, WaveCollapseElement> elementsToCollapse;
        public readonly int allElementsCount;
        private readonly Dictionary<Name, int> elementCountByName;

        public readonly int width;
        public readonly int height;
        public readonly int seed;
        public readonly Random random;
        public readonly WaveNamesRegister waveNamesRegister;

        private readonly bool showProgressBar;
        public readonly bool correctErrors;

        public bool IsFullyCollapsed { get { return !this.elementsToCollapse.Any(); } }

        public WaveCollapseElement RandomUncollapsedElementWithUncollapsedNeighbours
        {
            get
            {
                if (!this.elementsToCollapse.Any()) return null;

                for (int i = 0; i < 500; i++)
                {
                    WaveCollapseElement candidate = this.elementsToCollapse.ElementAt(this.random.Next(0, this.elementsToCollapse.Count)).Value;
                    if (candidate.HasUncollapsedNeighbours) return candidate;
                }

                return null;
            }
        }

        public WaveCollapseMap(int width, int height, int seed = -1, bool addBorder = false, Name borderName = Name.Empty, List<Name> nameWhiteList = null, List<Name> nameBlackList = null, bool correctErrors = true, Dictionary<Name, float> limitedPercentageByName = null, bool showProgressBar = false)
        {
            if (!addBorder) borderName = Name.Empty; // to ensure initial value

            this.creationTime = DateTime.Now;
            this.correctErrors = correctErrors;

            this.width = width;
            this.height = height;
            this.seed = seed == -1 ? new Random().Next(9999) : seed;
            this.random = new Random(this.seed);
            this.showProgressBar = showProgressBar;

            this.namesToUse = this.GetNamesToUse(nameWhiteList: nameWhiteList, nameBlackList: nameBlackList, borderName: borderName);
            this.allElementsCount = this.width * this.height;
            this.elementCountByName = new Dictionary<Name, int>();
            foreach (Name name in this.namesToUse) this.elementCountByName[name] = 0;
            this.elementCountByName[Name.Error] = 0;

            this.allowedPercentageByName = this.GetAllowedPercentageByName(limitedPercentageByName);
            this.allowedNamesByName = this.GetAllowedNamesByName();
            this.debugTextures = this.GetDebugTextures();

            this.waveBoard = new WaveCollapseElement[width, height];
            this.allElements = this.CreateAllElements();
            this.waveNamesRegister = new WaveNamesRegister(random: this.random, allElementsList: this.allElements);

            this.elementsToCollapse = new Dictionary<string, WaveCollapseElement>();
            foreach (WaveCollapseElement element in this.allElements)
            {
                if (!element.IsCollapsed) this.elementsToCollapse[element.id] = element;
            }
            if (addBorder) this.AddBorder(borderName);

            this.CalculateNeighboursForAllElements();

            if (this.showProgressBar) this.ShowProgressBar();
        }

        private Dictionary<Name, float> GetAllowedPercentageByName(Dictionary<Name, float> limitedPercentageByName)
        {
            if (limitedPercentageByName == null) limitedPercentageByName = new Dictionary<Name, float>();
            var newAllowedPercentageDict = new Dictionary<Name, float>();

            foreach (Name name in this.namesToUse)
            {
                float allowedPercentage = limitedPercentageByName.ContainsKey(name) ? limitedPercentageByName[name] : 1f;
                newAllowedPercentageDict[name] = allowedPercentage;

                if (allowedPercentage > 1f) throw new ArgumentException($"AllowedPercentage for {name} is greater than 1.");
            }

            foreach (Name name in limitedPercentageByName.Keys)
            {
                if (!this.namesToUse.Contains(name)) throw new ArgumentException($"Limited percentage - name {name} is not present in namesToUse.");
            }

            newAllowedPercentageDict[Name.Error] = 1f;

            return newAllowedPercentageDict;
        }

        public bool IsPercentageExceeded(Name name)
        {
            return this.GetElementPercentage(name) >= this.allowedPercentageByName[name];
        }

        public float GetElementPercentage(Name name)
        {
            return (float)this.GetElementCount(name) / (float)this.allElementsCount;
        }

        public int GetElementCount(Name name)
        {
            return this.elementCountByName[name];
        }

        public void AddToElementCount(Name name)
        {
            this.elementCountByName[name]++;
            if (this.IsPercentageExceeded(name))
            {
                if (this.namesToUse.Contains(name))
                {
                    // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Percentage ({this.GetElementPercentage(name)}) exceeded for {name}.");

                    this.namesToUse.Remove(name);
                    this.allowedNamesByName.Remove(name);

                    foreach (Name nameToCheck in this.allowedNamesByName.Keys.ToList())
                    {
                        List<Name> nameList = this.allowedNamesByName[nameToCheck];
                        if (nameList.Contains(name)) nameList.Remove(name);
                    }

                    this.waveNamesRegister.MoveAllElementsToIdentifyList();

                    foreach (WaveCollapseElement element in this.elementsToCollapse.Values)
                    {
                        element.ResetLastAllowedNames();
                    }
                }
            }
        }

        public void RemoveFromElementCount(Name name)
        {
            // Will not restore allowed names, that were removed, when allowed percentage was exceeded.

            this.elementCountByName[name]--;
        }

        private List<Name> GetNamesToUse(List<Name> nameWhiteList, List<Name> nameBlackList, Name borderName)
        {
            if (nameWhiteList == null && nameBlackList == null) return allNames.Where(n => n != Name.Empty && n != Name.Error).ToList();

            if (nameWhiteList != null && nameBlackList != null) throw new ArgumentException("Whitelist and blacklist cannot both be active.");
            if (borderName != Name.Empty && nameWhiteList != null && !nameWhiteList.Contains(borderName)) throw new ArgumentException($"Whitelist doesn't contain borderName {borderName}.");
            if (borderName != Name.Empty && nameBlackList != null && nameBlackList.Contains(borderName)) throw new ArgumentException($"Blacklist contains borderName {borderName}.");

            if (nameWhiteList != null)
            {
                return nameWhiteList.Where(n => n != Name.Empty && n != Name.Error).ToList();
            }
            else // blacklist != null
            {
                return allNames.Where(n => n != Name.Empty && n != Name.Error && !nameBlackList.Contains(n)).ToList();
            }
        }

        public void CollapseRandomElements(int count, List<Name> nameList, int nearbyElementCount, bool collapseNeighbours = true)
        {
            for (int i = 0; i < count; i++)
            {
                var mainElement = this.RandomUncollapsedElementWithUncollapsedNeighbours;
                if (mainElement != null)
                {
                    Name randomName = nameList[this.random.Next(nameList.Count)];

                    var nearbyElementList = new List<WaveCollapseElement> { mainElement };

                    for (int e = 0; e < nearbyElementCount; e++)
                    {
                        for (int j = 0; j < 20; j++) // number of tries
                        {
                            WaveCollapseElement randomElement = nearbyElementList[this.random.Next(0, nearbyElementList.Count)];
                            WaveCollapseElement randomNeighbour = randomElement.Neighbours[this.random.Next(0, randomElement.Neighbours.Count)];
                            if (randomNeighbour.HasUncollapsedNeighbours)
                            {
                                nearbyElementList.Add(randomNeighbour);
                                break;
                            }
                        }
                    }

                    foreach (WaveCollapseElement element in nearbyElementList)
                    {
                        if (!element.IsCollapsed) element.ForceCollapse(name: randomName);
                    }

                    if (collapseNeighbours)
                    {
                        foreach (WaveCollapseElement element in nearbyElementList)
                        {
                            element.CollapseNeighbours();
                        }
                    }

                    // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Collapsing element ({randomElement.x},{randomElement.y}) to {randomName}.", color: Color.Cyan);
                }
                else return;
            }
        }

        public Dictionary<Name, List<Name>> GetAllowedNamesByName()
        {
            var allowedNamesByNames = new Dictionary<Name, List<Name>>();

            foreach (Name name in this.namesToUse)
            {
                allowedNamesByNames[name] = new List<Name>();
            }

            foreach (var kvp in allAllowedCombinations)
            {
                Name mainName = kvp.Key;
                List<Name> allowedNames = kvp.Value;

                if (!this.namesToUse.Contains(mainName)) continue;

                foreach (Name allowedName in allowedNames)
                {
                    if (!allowedNamesByNames[mainName].Contains(allowedName)) allowedNamesByNames[mainName].Add(allowedName);
                    if (!allowedNamesByNames[mainName].Contains(mainName)) allowedNamesByNames[mainName].Add(mainName);

                    if (!this.namesToUse.Contains(allowedName)) continue;
                    if (!allowedNamesByNames[allowedName].Contains(mainName)) allowedNamesByNames[allowedName].Add(mainName);
                    if (!allowedNamesByNames[allowedName].Contains(allowedName)) allowedNamesByNames[allowedName].Add(allowedName);
                }
            }

            foreach (Name name in this.namesToUse)
            {
                if (!allowedNamesByNames[name].Any()) throw new ArgumentException($"No allowedNamesCount defined for {name}.");
            }

            return allowedNamesByNames;
        }

        private List<WaveCollapseElement> CreateAllElements()
        {
            List<WaveCollapseElement> allElements = new List<WaveCollapseElement>();

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    WaveCollapseElement newElement = new WaveCollapseElement(x: x, y: y, map: this, random: this.random);

                    this.waveBoard[x, y] = newElement;
                    allElements.Add(newElement);
                }
            }

            return allElements;
        }

        private void CalculateNeighboursForAllElements()
        {
            foreach (WaveCollapseElement element in this.allElements)
            {
                Vector2 basePos = new Vector2(element.x, element.y);

                List<Vector2> destPosList = new List<Vector2> {
                    new Vector2(basePos.X - 1, basePos.Y),
                    new Vector2(basePos.X + 1, basePos.Y),
                    new Vector2(basePos.X, basePos.Y - 1),
                    new Vector2(basePos.X, basePos.Y + 1)};

                List<WaveCollapseElement> neighbours = new List<WaveCollapseElement>();

                foreach (Vector2 destPos in destPosList)
                {
                    if (destPos.X >= 0 && destPos.X < this.width && destPos.Y >= 0 && destPos.Y < this.height) neighbours.Add(this.waveBoard[(int)destPos.X, (int)destPos.Y]);
                }

                element.SetNeighbours(neighbours);
            }
        }

        private void AddBorder(Name borderName)
        {
            if (!this.namesToUse.Contains(borderName)) throw new ArgumentException($"BorderName '{borderName}' is not contained in allNames.");

            var borderElemList = new List<WaveCollapseElement>();

            foreach (int x in new List<int> { 0, this.width - 1 })
            {
                for (int y = 0; y < this.height; y++)
                {
                    WaveCollapseElement element = this.waveBoard[x, y];
                    if (!borderElemList.Contains(element)) borderElemList.Add(element);
                }
            }

            foreach (int y in new List<int> { 0, this.height - 1 })
            {
                for (int x = 0; x < this.width; x++)
                {
                    WaveCollapseElement element = this.waveBoard[x, y];
                    if (!borderElemList.Contains(element)) borderElemList.Add(element);
                }
            }

            foreach (WaveCollapseElement element in borderElemList)
            {
                element.ForceCollapse(borderName);
            }
        }

        public bool ForceCollapse(int x, int y, Name name)
        {
            if (!this.namesToUse.Contains(name)) throw new ArgumentException($"Name '{name}' is not contained in allNames.");

            WaveCollapseElement element = this.waveBoard[x, y];

            if (element.IsCollapsed) return false;

            element.ForceCollapse(name);
            return true;
        }
        private void ShowProgressBar()
        {
            int completeAmount = this.allElementsCount - this.elementsToCollapse.Count;

            TimeSpan timeLeft = CalculateTimeLeft(startTime: this.creationTime, completeAmount: completeAmount, totalAmount: this.allElementsCount);
            string timeLeftString = TimeSpanToString(timeLeft + TimeSpan.FromSeconds(1));

            string seedText = String.Format("{0:0000}", this.seed);
            string message = $"preparing map\nseed {seedText}\n{this.width} x {this.height}\ngenerating terrain {timeLeftString}";

            SonOfRobinGame.progressBar.TurnOn(curVal: completeAmount, maxVal: this.allElementsCount, text: message);
        }

        private static TimeSpan CalculateTimeLeft(DateTime startTime, int completeAmount, int totalAmount)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            try
            {
                TimeSpan timePerCell = TimeSpan.FromSeconds(elapsedTime.TotalSeconds / completeAmount);
                TimeSpan timeLeft = TimeSpan.FromSeconds(timePerCell.TotalSeconds * (totalAmount - completeAmount));

                return timeLeft;
            }
            catch (OverflowException)
            { return TimeSpan.FromMinutes(30); }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = timeSpan.ToString("ss");
            else if (timeSpan < TimeSpan.FromHours(1)) timeLeftString = timeSpan.ToString("mm\\:ss");
            else if (timeSpan < TimeSpan.FromDays(1)) timeLeftString = timeSpan.ToString("hh\\:mm\\:ss");
            else timeLeftString = timeSpan.ToString("dd\\:hh\\:mm\\:ss");

            return timeLeftString;
        }

        public void ProcessNextBatchInteractively(int elementsPerFrame)
        {
            SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the collapse process

            int elementsLeftToCollapse = elementsPerFrame;

            while (true)
            {
                this.ProcessOneCollapseCycle();
                elementsLeftToCollapse--;
                if (this.IsFullyCollapsed || elementsLeftToCollapse <= 0) break;
            }

            if (this.showProgressBar)
            {
                if (this.IsFullyCollapsed) SonOfRobinGame.progressBar.TurnOff();
                else this.ShowProgressBar();
            }
        }

        public void ProcessOneCollapseCycle()
        {
            if (this.IsFullyCollapsed) return;

            bool randomElement = this.random.Next(0, 5000) == 0;

            if (randomElement)
            {
                WaveCollapseElement elementToCollapse = this.RandomUncollapsedElementWithUncollapsedNeighbours;
                if (elementToCollapse == null) return;

                elementToCollapse.Collapse();
                foreach (WaveCollapseElement neighbour in elementToCollapse.Neighbours)
                {
                    if (!neighbour.IsCollapsed) neighbour.Collapse();
                }
            }
            else
            {
                WaveCollapseElement elementToCollapse = this.waveNamesRegister.GetRandomElementOfLowestNameCount();
                elementToCollapse.Collapse();
            }

            if (this.IsFullyCollapsed)
            {
                this.processTime = DateTime.Now - this.creationTime;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Wave collapse time ({this.width}x{this.height}): {this.processTime:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

                if (this.correctErrors) this.CorrectElementsWithError();
                this.Validate();

                SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;
            }
        }


        private void CorrectElementsWithError()
        {
            var elementsWithError = this.allElements.Where(element => element.Name == Name.Error).ToList();
            if (!elementsWithError.Any()) return;

            while (true)
            {
                int randomIndex = random.Next(elementsWithError.Count);
                WaveCollapseElement element = elementsWithError[randomIndex];

                bool elementCorrected = element.CorrectError();
                if (elementCorrected) elementsWithError.RemoveAt(randomIndex);

                if (!elementsWithError.Any()) return;
            }
        }

        public void RemoveCollapsedElement(WaveCollapseElement element)
        {
            this.elementsToCollapse.Remove(element.id);
            this.waveNamesRegister.MoveElement(element: element, elementCollapsed: true);
        }

        private void Validate()
        {
            foreach (WaveCollapseElement element in this.allElements)
            {
                if (!element.IsCollapsed) throw new ArgumentException($"Found not collapsed element at {element.x},{element.y}.");
            }
        }

        public Texture2D GetElementTexture(int x, int y)
        {
            return this.debugTextures[this.waveBoard[x, y].Name];
        }

        private Dictionary<Name, Texture2D> GetDebugTextures()
        {
            var textures = new Dictionary<Name, Texture2D>();

            foreach (Name name in allNames)
            {
                Texture2D texture;

                switch (name)
                {
                    case Name.Empty:
                        texture = SonOfRobinGame.whiteRectangle;
                        break;

                    case Name.Error:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.Exclamation].texture;
                        break;

                    case Name.WaterDeep:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileWaterDeep].texture;
                        break;

                    case Name.WaterMedium:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileWaterMedium].texture;
                        break;

                    case Name.WaterShallow:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileWaterShallow].texture;
                        break;

                    case Name.Sand:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileSand].texture;
                        break;

                    case Name.Dirt:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileDirt].texture;
                        break;

                    case Name.Grass:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileGrass].texture;
                        break;

                    case Name.MountainsLow:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileMountainLow].texture;
                        break;

                    case Name.MountainsMedium:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileMountainMedium].texture;
                        break;

                    case Name.MountainsHigh:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileMountainHigh].texture;
                        break;

                    case Name.Lava:
                        texture = AnimData.framesForPkgs[AnimData.PkgName.TileLava].texture;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported name '{name}'.");
                }

                textures[name] = texture;
            }

            return textures;
        }

    }
}
