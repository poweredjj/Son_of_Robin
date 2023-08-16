using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class NamedLocations
    {
        public class Location
        {
            public readonly string name;
            public readonly Rectangle areaRect;
            public readonly Rectangle textRect;
            public bool HasBeenDiscovered { get; private set; }

            public Location(string name, Rectangle areaRect)
            {
                this.name = name;
                this.areaRect = areaRect;
                this.textRect = areaRect;
                this.textRect.Inflate(-areaRect.Width / 6, -areaRect.Height / 6);
                this.HasBeenDiscovered = false;
            }

            public void SetAsDiscovered()
            {
                this.HasBeenDiscovered = true;
            }
        }

        private readonly List<Location> locationList;

        public IEnumerable DiscoveredLocations { get { return this.locationList.Where(location => location.HasBeenDiscovered); } }

        public NamedLocations()
        {
            this.locationList = new List<Location>();
        }

        public void GenerateLocations()
        {
            // TODO add code

            this.locationList.Add(new Location(name: "test location", areaRect: new Rectangle(1000, 1000, 500, 500)));
            this.locationList[0].SetAsDiscovered();
        }

        public Object Serialize()
        {
            var locationData = new List<int>();

            int locationNo = 0;
            foreach (Location location in this.locationList)
            {
                if (location.HasBeenDiscovered) locationData.Add(locationNo);
                locationNo++;
            }

            return locationData;
        }

        public void Deserialize(Object locationData)
        {
            foreach (int locationNo in (List<int>)locationData)
            {
                this.locationList[locationNo].SetAsDiscovered();
            }
        }
    }
}