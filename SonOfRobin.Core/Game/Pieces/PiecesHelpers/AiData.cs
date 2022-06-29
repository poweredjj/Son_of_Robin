using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace SonOfRobin
{
    [Serializable]
    public struct AiData
    // contains ai working data
    {

        private static Vector2 nullPosition = new Vector2(-1, -1);
        private List<int> coordinates;
        [NonSerialized]
        private Vector2 position;

        public List<int> Coordinates
        {
            get { return coordinates; }
            set
            {
                coordinates = value;
                UpdatePosition();
            }
        }
        public Vector2 Position { get { return position; } }
        public bool dontStop;
        public short timeLeft;

        public AiData(char notUsedVariable = 'a')
        {
            this.position = nullPosition;
            this.coordinates = null;
            this.dontStop = false;
            this.timeLeft = 0;
        }
        public void Reset()
        {
            this.position = nullPosition;
            this.coordinates = null;
            this.dontStop = false;
            this.timeLeft = 0;
        }

        public void UpdatePosition()
        {
            if (this.coordinates != null) this.position = new Vector2(coordinates[0], coordinates[1]);        
        }

        public void SetCoordinates(List<int> corrdinates)
        {
            this.Coordinates = corrdinates;
        }
        public void SetDontStop(bool dontStop)
        {
            this.dontStop = dontStop;
        }
        public void SetTimeLeft(short timeLeft)
        {
            this.timeLeft = timeLeft;
        }
    }
}
