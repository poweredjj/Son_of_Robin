using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class StoredInput
    {
        public enum Type : byte
        {
            Key = 0,
            Button = 1,
            Analog = 2,
        }

        public readonly Type type;
        private readonly Keys key;
        private readonly Buttons button;
        private readonly InputMapper.AnalogType analog;

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

                case Type.Analog:
                    return this.analog == other.analog;

                default:
                    throw new ArgumentException($"Unsupported type - '{type}'.");
            }
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17; // Start with a prime number as the initial hash

                // Multiply the current hash by a prime number, and add the hash code of the enum value for the Type property.
                hash = hash * 23 + this.type.GetHashCode();

                // Use a switch statement to get the hash code of the appropriate field, based on the value of the Type property.
                switch (this.type)
                {
                    case Type.Key:
                        // Multiply the current hash by a prime number, and add the hash code of the Key field.
                        hash = hash * 23 + this.key.GetHashCode();
                        break;

                    case Type.Button:
                        // Multiply the current hash by a prime number, and add the hash code of the Button field.
                        hash = hash * 23 + this.button.GetHashCode();
                        break;

                    case Type.Analog:
                        // Multiply the current hash by a prime number, and add the hash code of the Analog field.
                        hash = hash * 23 + this.analog.GetHashCode();
                        break;

                    default:
                        throw new ArgumentException($"Unsupported type - '{this.type}'.");
                }

                return hash;
            }
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> inputData = new Dictionary<string, object>();

            inputData["type"] = this.type;

            switch (this.type)
            {
                case Type.Key:
                    inputData["key"] = this.key;
                    break;

                case Type.Button:
                    inputData["button"] = this.button;
                    break;

                case Type.Analog:
                    inputData["analog"] = this.analog;
                    break;

                default:
                    throw new ArgumentException($"Unsupported type - '{type}'.");
            }

            return inputData;
        }

        public static StoredInput Deserialize(Object inputData)
        {
            if (inputData == null) return null;
            var inputDict = (Dictionary<string, Object>)inputData;

            Type type = (Type)(Int64)inputDict["type"];
            switch (type)
            {
                case Type.Key:
                    return new StoredInput((Keys)(Int64)inputDict["key"]);

                case Type.Button:
                    return new StoredInput((Buttons)(Int64)inputDict["button"]);

                case Type.Analog:
                    return new StoredInput((InputMapper.AnalogType)(Int64)inputDict["analog"]);

                default:
                    throw new ArgumentException($"Unsupported type - '{type}'.");
            }
        }
    }
}