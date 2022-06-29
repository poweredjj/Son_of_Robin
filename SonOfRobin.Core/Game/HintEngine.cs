using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    public class HintEngine
    {
        public enum Type { Tired, }
        public List<Type> disabledHints = new List<Type> { };
        public readonly World world;

        public HintEngine(World world)
        {
            this.world = world;

        }

        public void Show(Type type)
        {
            if (this.disabledHints.Contains(type)) return;

            switch (type)
            {
                case Type.Tired:
                    this.DisableType(type: type, delay: 0);

                    new TextWindow(text: "I'm tired...", textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true);

                    break;

                default:
                    { throw new DivideByZeroException($"Unsupported hint type - {type}."); }
            }

        }

        private void DisableType(Type type, int delay = 0)
        {
            if (!this.disabledHints.Contains(type)) this.disabledHints.Add(type);

            if (delay != 0) new WorldEvent(eventName: WorldEvent.EventName.RestoreHint, delay: delay, world: this.world, boardPiece: null, eventHelper: type);
        }

        public void EnableType(Type type)
        {
            this.disabledHints.Remove(type);
        }



    }
}
