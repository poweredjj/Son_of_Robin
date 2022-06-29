using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WaveCollapseElement
    {
        public readonly string id;
        public readonly int x;
        public readonly int y;
        private readonly WaveCollapseMap map;
        private readonly Random random;
        public List<WaveCollapseElement> Neighbours { get { return neighbours; } }
        private readonly List<WaveCollapseElement> neighbours;
        public List<string> NeighbourIDList { get { return this.neighbours.Select(n => n.id).ToList(); } }

        public bool HasUncollapsedNeighbours
        {
            get
            {
                foreach (WaveCollapseElement neighbour in this.Neighbours)
                {
                    if (neighbour.IsCollapsed) return false;
                }
                return true;
            }
        }

        public WaveCollapseMap.Name Name { get { return name; } }
        private WaveCollapseMap.Name name;

        private List<WaveCollapseMap.Name> lastAllowedNames;

        public bool IsCollapsed { get { return this.isCollapsed; } }
        private bool isCollapsed;

        public WaveCollapseElement(int x, int y, WaveCollapseMap map, Random random)
        {
            this.id = Helpers.GetUniqueHash();
            this.x = x;
            this.y = y;
            this.map = map;
            this.random = random;

            this.name = WaveCollapseMap.Name.Empty;
            this.isCollapsed = false;
            this.neighbours = new List<WaveCollapseElement>();
        }

        public void SetNeighbours(List<WaveCollapseElement> neighboursList)
        {
            if (this.neighbours.Any()) throw new ArgumentException("Neighbours list has already been set.");

            this.neighbours.AddRange(neighboursList);
        }

        public List<WaveCollapseMap.Name> AllowedNames
        {
            get
            {
                this.RefreshLastAllowedNames();
                return this.lastAllowedNames;
            }
        }

        public void RefreshLastAllowedNames()
        {
            if (this.lastAllowedNames != null) return;

            var possibleNames = this.map.namesToUse.ToList();

            var collapsedNeighbours = this.neighbours.Where(neighbour => neighbour.isCollapsed);
            if (collapsedNeighbours.Count() == 0)
            {
                this.lastAllowedNames = possibleNames;
                return;
            }

            var neighboursNames = new List<WaveCollapseMap.Name>();
            foreach (WaveCollapseElement neighbour in collapsedNeighbours)
            {
                if (!neighboursNames.Contains(neighbour.name) && neighbour.name != WaveCollapseMap.Name.Error) neighboursNames.Add(neighbour.name);
            }

            var namesToDelete = new List<WaveCollapseMap.Name>();
            foreach (WaveCollapseMap.Name neighbourName in neighboursNames)
            {
                foreach (WaveCollapseMap.Name nameToCheck in possibleNames)
                {
                    if (this.map.allowedNamesByName.ContainsKey(neighbourName) &&
                        !this.map.allowedNamesByName[neighbourName].Contains(nameToCheck) &&
                        !namesToDelete.Contains(nameToCheck))

                        namesToDelete.Add(nameToCheck);
                }
            }

            this.lastAllowedNames = possibleNames.Where(name => !namesToDelete.Contains(name)).ToList();
        }

        public void ForceCollapse(WaveCollapseMap.Name name)
        {
            if (this.IsCollapsed) throw new ArgumentException($"This element ({this.x},{this.y}) has been already collapsed - {this.name}.");
            if (!this.map.namesToUse.Contains(name)) throw new ArgumentException($"Name {name} is not contained in namesToUse.");

            this.name = name;
            this.map.AddToElementCount(name);
            this.isCollapsed = true;
            this.map.RemoveCollapsedElement(this);

            foreach (WaveCollapseElement element in this.neighbours)
            {
                element.ResetLastAllowedNamesAndMoveInRegister();
            }
        }

        public void CollapseNeighbours()
        {
            foreach (WaveCollapseElement neighbour in this.neighbours)
            {
                if (neighbour.IsCollapsed) continue;

                neighbour.Collapse();
            }
        }

        public void Collapse()
        {
            if (this.IsCollapsed) throw new ArgumentException($"This element ({this.x},{this.y}) has already been collapsed - {this.name}.");

            var allowedNames = this.AllowedNames;

            if (allowedNames.Any())
            {
                double maxProbability = 0f;
                WaveCollapseMap.Name maxProbabilityName = WaveCollapseMap.Name.Error;

                foreach (WaveCollapseMap.Name name in allowedNames)
                {
                    double calculatedProbability = this.random.NextDouble() * WaveCollapseMap.allProbabilities[name];
                    if (this.map.GetElementPercentage(name) < 0.03f) calculatedProbability *= 5d;
                    if (calculatedProbability > maxProbability)
                    {
                        maxProbability = calculatedProbability;
                        maxProbabilityName = name;
                    }
                }

                this.name = maxProbabilityName;
            }
            else this.name = WaveCollapseMap.Name.Error;

            this.map.AddToElementCount(this.name);

            this.isCollapsed = true;
            this.map.RemoveCollapsedElement(this);

            foreach (WaveCollapseElement element in this.neighbours)
            {
                element.ResetLastAllowedNamesAndMoveInRegister();
            }

            if (this.name == WaveCollapseMap.Name.Error && this.map.correctErrors) this.CorrectError();
        }
        public void ResetLastAllowedNames()
        {
            this.lastAllowedNames = null;
        }

        public void ResetLastAllowedNamesAndMoveInRegister()
        {
            this.lastAllowedNames = null;
            this.map.waveNamesRegister.MoveElement(element: this, elementCollapsed: false);
        }

        public bool CorrectError()
        {
            if (this.name != WaveCollapseMap.Name.Error) return true;

            var namesToChooseFrom = this.neighbours.Select(n => n.name).Where(n => n != WaveCollapseMap.Name.Error && n != WaveCollapseMap.Name.Empty).ToList();
            if (!namesToChooseFrom.Any()) return false;

            this.map.RemoveFromElementCount(this.name);

            this.name = namesToChooseFrom[this.random.Next(namesToChooseFrom.Count)];
            this.map.AddToElementCount(this.name);

            foreach (WaveCollapseElement element in this.neighbours)
            {
                element.ResetLastAllowedNamesAndMoveInRegister();
            }

            return true;
        }
    }
}
