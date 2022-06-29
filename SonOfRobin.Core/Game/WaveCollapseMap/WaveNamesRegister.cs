using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WaveNamesRegister
    {
        private readonly Random random;
        private Dictionary<int, Dictionary<string, WaveCollapseElement>> numberedEntries;
        private Dictionary<string, WaveCollapseElement> elementsToIdentify;
        private readonly List<WaveCollapseElement> allElementsList;

        public WaveNamesRegister(Random random, List<WaveCollapseElement> allElementsList)
        {
            this.allElementsList = allElementsList;
            this.numberedEntries = new Dictionary<int, Dictionary<string, WaveCollapseElement>>();
            this.elementsToIdentify = new Dictionary<string, WaveCollapseElement>();
            this.random = random;

            foreach (WaveCollapseElement element in this.allElementsList)
            {
                this.elementsToIdentify[element.id] = element;
            }
        }

        public WaveCollapseElement GetRandomElementOfLowestNameCount()
        {
            this.IdentifyElements();
           // this.ValidateAll(); // for testing integrity only

            var countList = this.numberedEntries.Keys.OrderBy(k => k).ToList();

            foreach (int count in countList)
            {
                var candidateList = this.numberedEntries[count];

                if (candidateList.Any())
                {
                    int randomIndex = random.Next(candidateList.Count);
                    return candidateList.Values.ToList()[randomIndex];
                }
            }

            throw new ArgumentException("Cannot find any element.");
        }

        private void IdentifyElements()
        {
            if (!this.elementsToIdentify.Any()) return;

            foreach (WaveCollapseElement element in this.elementsToIdentify.Values)
            {
                if (!element.IsCollapsed)
                {
                    int allowedNameCount = element.AllowedNames.Count;
                    if (!numberedEntries.ContainsKey(allowedNameCount)) numberedEntries[allowedNameCount] = new Dictionary<string, WaveCollapseElement>();

                    numberedEntries[allowedNameCount][element.id] = element;
                }
            }

            this.elementsToIdentify.Clear();
        }

        public void MoveAllElementsToIdentifyList()
        {
            foreach (Dictionary<string, WaveCollapseElement> kvp in this.numberedEntries.Values)
            {
                foreach (WaveCollapseElement element in kvp.Values)
                {
                    this.elementsToIdentify[element.id] = element;
                }
            }

            this.numberedEntries.Clear();
        }

        public void MoveElement(WaveCollapseElement element, bool elementCollapsed)
        {
            string id = element.id;

            foreach (Dictionary<string, WaveCollapseElement> kvp in this.numberedEntries.Values)
            {
                if (kvp.Keys.Contains(id))
                {
                    kvp.Remove(id);

                    if (!elementCollapsed) this.elementsToIdentify[id] = element;
                    return;
                }
            }
        }

        private void ValidateAll()
        {
            foreach (WaveCollapseElement element in this.allElementsList)
            {
                element.ResetLastAllowedNames();
                int allowedNamesCount = element.AllowedNames.Count;

                if (element.IsCollapsed)
                {
                    foreach (Dictionary<string, WaveCollapseElement> kvp in this.numberedEntries.Values)
                    {
                        if (kvp.ContainsKey(element.id)) throw new ArgumentException($"Collapsed element ({element.Name}) {element.id} found in non-collapsed entries.");
                    }
                }
                else
                {
                    if (!this.numberedEntries[allowedNamesCount].ContainsKey(element.id)) throw new ArgumentException($"Element {element.id} allowed names count mismatch {allowedNamesCount}.");
                }
            }

        }

    }
}
