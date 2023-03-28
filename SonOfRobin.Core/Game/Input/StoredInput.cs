using Microsoft.Xna.Framework.Input;
using System;

namespace SonOfRobin
{
    [Serializable]
    public class StoredInput
    {
        public static readonly StoredInput empty = new StoredInput();

        public enum Type
        { Empty, Key, Button, VirtButton, LeftStick, Analog }

        public readonly Type type;
        private readonly Keys key;
        private readonly Buttons button;
        private readonly VButName virtButton;
        private readonly InputMapper.AnalogType analog;

        public StoredInput()
        {
            this.type = Type.Empty;
        }

        public StoredInput(Keys key)
        {
            this.type = Type.Key;
            this.key = key;
        }

        public StoredInput(Buttons button)
        {
            this.type = Type.Button;
            this.button = button;
        }

        public StoredInput(VButName virtButton)
        {
            this.type = Type.VirtButton;
            this.virtButton = virtButton;
        }

        public StoredInput(InputMapper.AnalogType analog)
        {
            this.type = Type.Analog;
            this.analog = analog;
        }

        public Keys Key
        {
            get
            {
                if (this.type != Type.Key) throw new ArgumentException($"Wrong type for key - '{this.type}'.");
                return this.key;
            }
        }

        public Buttons Button
        {
            get
            {
                if (this.type != Type.Button) throw new ArgumentException($"Wrong type for button - '{this.type}'.");
                return this.button;
            }
        }

        public VButName VirtButton
        {
            get
            {
                if (this.type != Type.VirtButton) throw new ArgumentException($"Wrong type for virtButton - '{this.type}'.");
                return this.virtButton;
            }
        }

        public InputMapper.AnalogType Analog
        {
            get
            {
                if (this.type != Type.Analog) throw new ArgumentException($"Wrong type for analog - '{this.type}'.");
                return this.analog;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            StoredInput other = (StoredInput)obj;
            if (this.type != other.type) return false;

            switch (this.type)
            {
                case Type.Key:
                    return this.key == other.key;

                case Type.Button:
                    return this.button == other.button;

                case Type.VirtButton:
                    return this.virtButton == other.virtButton;

                case Type.Analog:
                    return this.analog == other.analog;

                case Type.Empty:
                    return true;

                default:
                    throw new ArgumentException($"Unsupported type - '{type}'.");
            }
        }
    }
}