using System;

namespace SonOfRobin
{
    [Serializable]
    public class Buff
    {
        public string id;
        public readonly bool increaseIDAtEveryUse;
        public readonly BuffEngine.BuffType type;
        public readonly bool isPositive;
        public readonly string description;
        public readonly string iconText;
        public object value;
        public readonly bool canKill;
        public readonly int autoRemoveDelay;
        public int activationFrame;
        public int endFrame;
        public readonly bool isPermanent;
        public readonly int sleepMinutesNeededForActivation;

        public Buff(BuffEngine.BuffType type, object value, int autoRemoveDelay = 0, int sleepMinutesNeededForActivation = 0, bool isPermanent = false, bool canKill = false, bool increaseIDAtEveryUse = false)
        {
            this.type = type;
            value = this.CastValueToCorrectType(value);

            this.increaseIDAtEveryUse = increaseIDAtEveryUse; // for buffs that could stack (like sleeping buffs)
            this.id = Helpers.GetUniqueHash();
            // AutoRemoveDelay should not be used for equip!
            // It should only be used for temporary buffs (food, status effects, etc.).
            this.autoRemoveDelay = autoRemoveDelay;
            this.value = value;
            this.canKill = canKill;
            this.isPermanent = isPermanent;
            this.sleepMinutesNeededForActivation = sleepMinutesNeededForActivation;
            this.isPositive = this.GetIsPositive();
            this.description = this.GetDescription();
            this.iconText = this.GetIconText();

            this.endFrame = 0; // to be assigned during activation
            this.activationFrame = 0; // to be assigned during activation
        }

        public static Buff CopyBuff(Buff buff)
        {
            // all constructor params must be repeated here
            return new Buff(type: buff.type, value: buff.value, autoRemoveDelay: buff.autoRemoveDelay, sleepMinutesNeededForActivation: buff.sleepMinutesNeededForActivation, isPermanent: buff.isPermanent, canKill: buff.canKill, increaseIDAtEveryUse: buff.increaseIDAtEveryUse);
        }

        private object CastValueToCorrectType(object value)
        {
            switch (this.type)
            {
                case BuffEngine.BuffType.InvWidth:
                    value = Helpers.CastObjectToByte(value);
                    break;

                case BuffEngine.BuffType.InvHeight:
                    value = Helpers.CastObjectToByte(value);
                    break;

                case BuffEngine.BuffType.ToolbarWidth:
                    value = Helpers.CastObjectToByte(value);
                    break;

                case BuffEngine.BuffType.ToolbarHeight:
                    value = Helpers.CastObjectToByte(value);
                    break;

                case BuffEngine.BuffType.Speed:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                case BuffEngine.BuffType.Strength:
                    value = Helpers.CastObjectToInt(value);
                    break;

                case BuffEngine.BuffType.HP:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                case BuffEngine.BuffType.MaxHP:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                case BuffEngine.BuffType.MaxStamina:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                case BuffEngine.BuffType.NotUsedKeptForCompatibility1:
                    value = Helpers.CastObjectToInt(value);
                    break;

                case BuffEngine.BuffType.RegenPoison:
                    value = Helpers.CastObjectToInt(value);
                    break;

                case BuffEngine.BuffType.Haste:
                    value = Helpers.CastObjectToInt(value);
                    break;

                case BuffEngine.BuffType.Fatigue:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                case BuffEngine.BuffType.Sprint:
                    value = Helpers.CastObjectToFloat(value);
                    break;

                default:
                    break;
            }

            return value;
        }

        public bool HadEnoughSleepForBuff(World world)
        {
            TimeSpan sleepDuration = world.islandClock.IslandDateTime - world.Player.SleepStarted;
            return sleepDuration.TotalMinutes >= this.sleepMinutesNeededForActivation;
        }

        private bool GetIsPositive()
        {
            switch (this.type)
            {
                case BuffEngine.BuffType.InvWidth:
                    return (byte)this.value >= 0;

                case BuffEngine.BuffType.InvHeight:
                    return (byte)this.value >= 0;

                case BuffEngine.BuffType.ToolbarWidth:
                    return (byte)this.value >= 0;

                case BuffEngine.BuffType.ToolbarHeight:
                    return (byte)this.value >= 0;

                case BuffEngine.BuffType.Speed:
                    return (float)this.value >= 0;

                case BuffEngine.BuffType.Strength:
                    return (int)this.value >= 0;

                case BuffEngine.BuffType.HP:
                    return (float)this.value >= 0;

                case BuffEngine.BuffType.MaxHP:
                    return (float)this.value >= 0;

                case BuffEngine.BuffType.MaxStamina:
                    return (float)this.value >= 0;

                case BuffEngine.BuffType.NotUsedKeptForCompatibility1:
                    return (int)this.value >= 0;

                case BuffEngine.BuffType.RegenPoison:
                    return (int)this.value >= 0;

                case BuffEngine.BuffType.Haste:
                    return (int)this.value >= 0;

                case BuffEngine.BuffType.Fatigue:
                    return (float)this.value <= 0; // reversed, because fatigue is a "negative" stat

                case BuffEngine.BuffType.EnableMap:
                    return true;

                case BuffEngine.BuffType.Tired:
                    return false;

                case BuffEngine.BuffType.LowHP:
                    return false;

                case BuffEngine.BuffType.Hungry:
                    return false;

                case BuffEngine.BuffType.Heat:
                    return false;

                case BuffEngine.BuffType.HeatProtection:
                    return true;

                case BuffEngine.BuffType.SwampProtection:
                    return true;

                case BuffEngine.BuffType.Wet:
                    return true;

                case BuffEngine.BuffType.Sprint:
                    return true;

                case BuffEngine.BuffType.SprintCooldown:
                    return false;

                default:
                    throw new ArgumentException($"Unsupported buff type - {this.type}.");
            }
        }

        private string SignString
        { get { return $"{this.value}".StartsWith("-") ? "" : "+"; } }

        private string GetDescription()
        {
            string description;
            string sign = this.SignString;
            string duration = this.autoRemoveDelay == 0 ? "" : $" for {Math.Round(this.autoRemoveDelay / 60f)}s";

            switch (this.type)
            {
                case BuffEngine.BuffType.InvWidth:
                    description = $"Inventory width {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.InvHeight:
                    description = $"Inventory height {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.ToolbarWidth:
                    description = $"Toolbar width {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.ToolbarHeight:
                    description = $"Toolbar height {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.Speed:
                    description = $"Speed {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.Strength:
                    description = $"Strength {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.HP:
                    description = $"Health {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.MaxHP:
                    description = $"Max health {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.MaxStamina:
                    description = $"Max stamina {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.EnableMap:
                    description = "Shows map of visited places.";
                    break;

                case BuffEngine.BuffType.Tired:
                    description = "Decreases multiple stats.";
                    break;

                case BuffEngine.BuffType.LowHP:
                    description = "Low health.";
                    break;

                case BuffEngine.BuffType.Hungry:
                    description = "Hungry.";
                    break;

                case BuffEngine.BuffType.Heat:
                    description = "Heat - gets tired easily.";
                    break;

                case BuffEngine.BuffType.HeatProtection:
                    description = "Heat protection.";
                    break;

                case BuffEngine.BuffType.SwampProtection:
                    description = "Swamp poison protection.";
                    break;

                case BuffEngine.BuffType.Wet:
                    description = "Wet.";
                    break;

                case BuffEngine.BuffType.NotUsedKeptForCompatibility1:
                    description = $"Light source {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.RegenPoison:
                    description = this.isPositive ? $"Regen {sign}{this.value}{duration}." : $"Poison {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.Haste:
                    description = $"Haste {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.Fatigue:
                    description = $"Fatigue {sign}{this.value}.";
                    break;

                case BuffEngine.BuffType.Sprint:
                    description = $"Sprint {sign}{this.value}{duration}.";
                    break;

                case BuffEngine.BuffType.SprintCooldown:
                    description = $"Cannot sprint{duration}.";
                    break;

                default:
                    throw new ArgumentException($"Unsupported buff type - {this.type}.");
            }

            if (this.sleepMinutesNeededForActivation > 0) description = $"After sleeping for {Math.Round((float)((float)this.sleepMinutesNeededForActivation / 60), 1)} hours: {Helpers.FirstCharToLowerCase(description)}";

            return description;
        }

        public string PotionText
        {
            get
            {
                switch (this.type)
                {
                    case BuffEngine.BuffType.Speed:
                        return this.isPositive ? "speed" : "slowdown";

                    case BuffEngine.BuffType.Strength:
                        return this.isPositive ? "strength" : "weakness";

                    case BuffEngine.BuffType.HP:
                        return "health";

                    case BuffEngine.BuffType.MaxHP:
                        return "max health";

                    case BuffEngine.BuffType.MaxStamina:
                        return "stamina";

                    case BuffEngine.BuffType.HeatProtection:
                        return "heat protection";

                    case BuffEngine.BuffType.SwampProtection:
                        return "swamp protection";

                    case BuffEngine.BuffType.Wet:
                        return null;

                    case BuffEngine.BuffType.NotUsedKeptForCompatibility1:
                        return null;

                    case BuffEngine.BuffType.RegenPoison:
                        return this.isPositive ? "regen" : "poison";

                    case BuffEngine.BuffType.Haste:
                        return "haste";

                    case BuffEngine.BuffType.Fatigue:
                        return this.isPositive ? "rest" : "fatigue";

                    default:
                        return "this buff has no potionText defined";
                }
            }
        }

        private string GetIconText()
        {
            string sign = this.SignString;

            switch (this.type)
            {
                case BuffEngine.BuffType.InvWidth:
                    return null;

                case BuffEngine.BuffType.InvHeight:
                    return null;

                case BuffEngine.BuffType.ToolbarWidth:
                    return null;

                case BuffEngine.BuffType.ToolbarHeight:
                    return null;

                case BuffEngine.BuffType.EnableMap:
                    return null;

                case BuffEngine.BuffType.Speed:
                    return $"SPD\n{sign}{this.value}";

                case BuffEngine.BuffType.Strength:
                    return $"STR\n{sign}{this.value}";

                case BuffEngine.BuffType.HP:
                    return $"HP\n{sign}{this.value}";

                case BuffEngine.BuffType.MaxHP:
                    return $"MAX HP\n{sign}{this.value}";

                case BuffEngine.BuffType.MaxStamina:
                    return $"STAMINA\n{sign}{this.value}";

                case BuffEngine.BuffType.Tired:
                    return "TIRED";

                case BuffEngine.BuffType.LowHP:
                    return "LOW\nHEALTH";

                case BuffEngine.BuffType.Hungry:
                    return "HUNGRY";

                case BuffEngine.BuffType.Heat:
                    return "HEAT";

                case BuffEngine.BuffType.HeatProtection:
                    return "HEAT\nPROTECT";

                case BuffEngine.BuffType.SwampProtection:
                    return "SWAMP\nPROTECT";

                case BuffEngine.BuffType.Wet:
                    return "WET";

                case BuffEngine.BuffType.NotUsedKeptForCompatibility1:
                    return $"LIGHT\n{sign}{this.value}";

                case BuffEngine.BuffType.RegenPoison:
                    return this.isPositive ? $"REGEN\n{sign}{this.value}" : $"POISON\n{sign}{this.value}";

                case BuffEngine.BuffType.Haste:
                    return $"HASTE\n{sign}{this.value}";

                case BuffEngine.BuffType.Fatigue:
                    return $"FATIGUE\n{sign}{this.value}";

                case BuffEngine.BuffType.Sprint:
                    return $"SPRINT\n{sign}{this.value}";

                case BuffEngine.BuffType.SprintCooldown:
                    return "CANNOT\nSPRINT";

                default:
                    throw new ArgumentException($"Unsupported buff type - {this.type}.");
            }
        }
    }
}