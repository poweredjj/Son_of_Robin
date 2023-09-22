using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public readonly struct PieceSoundPackTemplate
    {
        public enum Action : byte
        {
            IsDestroyed = 0,
            IsHit = 1,
            IsOn = 2,
            IsDropped = 3,
            Cry = 4,
            Die = 5,
            Eat = 6,
            Open = 7,
            Close = 8,
            StepGrass = 9,
            StepWater = 10,
            StepSand = 11,
            StepRock = 12,
            StepLava = 13,
            StepMud = 14,
            SwimShallow = 15,
            SwimDeep = 16,
            HasAppeared = 17,
            ArrowFly = 18,
            ArrowHit = 19,
            PlayerBowDraw = 20,
            TurnOn = 21,
            TurnOff = 22,
            PlayerBowRelease = 23,
            PlayerPulse = 24,
            PlayerSnore = 25,
            PlayerYawn = 26,
            PlayerStomachGrowl = 27,
            PlayerSprint = 28,
            PlayerPant = 29,
            PlayerSpeak = 30,
            Ambient = 31,
            PlayerJump = 32,
            Burning = 33,
            BurnEnd = 34,
        }

        private readonly Dictionary<Action, Sound> staticSoundDict;

        public PieceSoundPackTemplate(PieceInfo.Info pieceInfo)
        {
            this.staticSoundDict = new Dictionary<Action, Sound>();

            BoardPiece.Category category = pieceInfo.category;
            bool isPlayer = pieceInfo.type == typeof(Player);
            bool isAnimal = pieceInfo.type == typeof(Animal);

            // looped sound would rapidly populate all sound channels, so non-looped short sound is used instead
            this.AddAction(action: Action.Burning, sound: new Sound(name: SoundData.Name.FireBurnShort, maxPitchVariation: 0.5f));
            this.AddAction(action: Action.BurnEnd, sound: new Sound(name: SoundData.Name.EndFire, maxPitchVariation: 0.5f));
            this.AddAction(action: Action.IsHit, sound: new Sound(nameList: GetHitSoundNames(category), maxPitchVariation: 0.5f));
            this.AddAction(action: Action.IsDestroyed, sound: new Sound(nameList: GetIsDestroyedNamesList(category), maxPitchVariation: 0.4f));
            this.AddAction(action: Action.IsDropped, sound: new Sound(nameList: GetIsDroppedSoundNames(category), maxPitchVariation: 0.25f, cooldown: 30));

            if (isPlayer || isAnimal)
            {
                float volumeMultiplier = isPlayer ? 1f : 0.35f;

                this.AddAction(action: Action.StepGrass, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepGrass1, SoundData.Name.StepGrass2, SoundData.Name.StepGrass3, SoundData.Name.StepGrass4, SoundData.Name.StepGrass5, SoundData.Name.StepGrass6 }, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: isPlayer ? 1f : 0.35f, maxPitchVariation: 0.6f));

                this.AddAction(action: Action.StepSand, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepSand1, SoundData.Name.StepSand2, SoundData.Name.StepSand3, SoundData.Name.StepSand4 }, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f));

                this.AddAction(action: Action.StepMud, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepMud1, SoundData.Name.StepMud2, SoundData.Name.StepMud3, SoundData.Name.StepMud4, SoundData.Name.StepMud5, SoundData.Name.StepMud6, SoundData.Name.StepMud7 }, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f));

                this.AddAction(action: Action.StepRock, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepRock1, SoundData.Name.StepRock2, SoundData.Name.StepRock3 }, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 0.7f, maxPitchVariation: 0.2f));

                this.AddAction(action: Action.StepLava, sound: new Sound(name: SoundData.Name.StepLava, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f));

                this.AddAction(action: Action.StepWater, sound: new Sound(name: SoundData.Name.StepWater, maxPitchVariation: 0.5f, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f));

                this.AddAction(action: Action.SwimShallow, sound: new Sound(name: SoundData.Name.SwimShallow, maxPitchVariation: 0.5f, cooldown: isPlayer ? 60 : 25, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f));

                this.AddAction(action: Action.SwimDeep, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.SwimDeep1, SoundData.Name.SwimDeep2, SoundData.Name.SwimDeep3, SoundData.Name.SwimDeep4, SoundData.Name.SwimDeep5 }, cooldown: isPlayer ? 60 : 25, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.6f));
            }
            else if (pieceInfo.type == typeof(AlchemyLab))
            {
                this.AddAction(action: Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
                this.AddAction(action: Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));
                this.AddAction(action: Action.IsOn, sound: new Sound(name: SoundData.Name.BoilingPotionLoop, isLooped: true));
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.BoilingPotion, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(Container))
            {
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.ChestOpen));
                this.AddAction(action: Action.Close, sound: new Sound(name: SoundData.Name.ChestClose));
            }
            else if (pieceInfo.type == typeof(Cooker))
            {
                this.AddAction(action: Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
                this.AddAction(action: Action.TurnOff, sound: new Sound(name: SoundData.Name.SteamHit));
            }
            else if (pieceInfo.type == typeof(Fireplace))
            {
                this.AddAction(action: Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
                this.AddAction(action: Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
                this.AddAction(action: Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.StoneMove2, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(MeatHarvestingWorkshop))
            {
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.KnifeSharpen, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(MeatDryingRack))
            {
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.HitSmallPlant2, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(Totem))
            {
                this.AddAction(action: Action.Open, sound: new Sound(name: SoundData.Name.Rattle, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(PortableLight))
            {
                this.AddAction(action: Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireSmall, ignore3DAlways: true));
                this.AddAction(action: Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, ignore3DAlways: true));
                this.AddAction(action: Action.IsOn, sound: new Sound(name: SoundData.Name.Torch, maxPitchVariation: 0.5f, isLooped: true, ignore3DAlways: true));
            }
            else if (pieceInfo.type == typeof(Projectile))
            {
                this.AddAction(action: Action.ArrowFly, sound: new Sound(name: SoundData.Name.ArrowFly, maxPitchVariation: 0.3f));
                this.AddAction(action: Action.ArrowHit, sound: new Sound(name: SoundData.Name.ArrowHit, maxPitchVariation: 0.3f));
                this.AddAction(action: Action.IsDropped, sound: new Sound(name: SoundData.Name.DropArrow, maxPitchVariation: 0.3f, cooldown: 20));
            }
        }

        public void AddAction(Action action, Sound sound, bool replaceExisting = false)
        {
            if (sound.isEmpty) return;

            if (this.staticSoundDict.ContainsKey(action))
            {
                if (replaceExisting) this.staticSoundDict.Remove(action);
                else return;
            }

            this.staticSoundDict[action] = sound;
        }

        public void RemoveAction(Action action)
        {
            if (this.staticSoundDict.ContainsKey(action)) this.staticSoundDict.Remove(action);
        }

        public Sound GetSound(Action action)
        {
            return this.staticSoundDict.ContainsKey(action) ? this.staticSoundDict[action] : null;
        }

        public bool IsLooped(Action action)
        {
            return this.staticSoundDict.ContainsKey(action) ? this.staticSoundDict[action].isLooped : false;
        }

        private static List<SoundData.Name> GetHitSoundNames(BoardPiece.Category category)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (category)
            {
                case BoardPiece.Category.Wood:
                    soundNameList.Add(SoundData.Name.HitWood);
                    break;

                case BoardPiece.Category.Stone:
                    soundNameList.Add(SoundData.Name.HitRock1);
                    soundNameList.Add(SoundData.Name.HitRock2);
                    soundNameList.Add(SoundData.Name.HitRock3);
                    soundNameList.Add(SoundData.Name.HitRock4);
                    soundNameList.Add(SoundData.Name.HitRock5);
                    soundNameList.Add(SoundData.Name.HitRock6);
                    break;

                case BoardPiece.Category.Metal:
                    soundNameList.Add(SoundData.Name.HitMetal);
                    break;

                case BoardPiece.Category.SmallPlant:
                    soundNameList.Add(SoundData.Name.HitSmallPlant1);
                    soundNameList.Add(SoundData.Name.HitSmallPlant2);
                    soundNameList.Add(SoundData.Name.HitSmallPlant3);
                    break;

                case BoardPiece.Category.Flesh:
                    soundNameList.Add(SoundData.Name.HitFlesh1);
                    soundNameList.Add(SoundData.Name.HitFlesh2);
                    break;

                case BoardPiece.Category.Leather:
                    soundNameList.Add(SoundData.Name.HitLeather);
                    break;

                case BoardPiece.Category.Dirt:
                    soundNameList.Add(SoundData.Name.Dig1);
                    soundNameList.Add(SoundData.Name.Dig2);
                    soundNameList.Add(SoundData.Name.Dig3);
                    break;

                case BoardPiece.Category.Crystal:
                    soundNameList.Add(SoundData.Name.HitCrystal);
                    break;

                case BoardPiece.Category.Indestructible:
                    soundNameList.Add(SoundData.Name.Empty);
                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            return soundNameList;
        }

        private static List<SoundData.Name> GetIsDestroyedNamesList(BoardPiece.Category category)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (category)
            {
                case BoardPiece.Category.Wood:
                    soundNameList.Add(SoundData.Name.DestroyWood);
                    break;

                case BoardPiece.Category.Stone:
                    soundNameList.Add(SoundData.Name.DestroyRock1);
                    soundNameList.Add(SoundData.Name.DestroyRock2);
                    break;

                case BoardPiece.Category.Metal:
                    soundNameList.Add(SoundData.Name.DestroyMetal);
                    break;

                case BoardPiece.Category.SmallPlant:
                    soundNameList.Add(SoundData.Name.DestroySmallPlant1);
                    soundNameList.Add(SoundData.Name.DestroySmallPlant2);
                    soundNameList.Add(SoundData.Name.DestroySmallPlant3);
                    soundNameList.Add(SoundData.Name.DestroySmallPlant4);
                    soundNameList.Add(SoundData.Name.DestroySmallPlant5);
                    soundNameList.Add(SoundData.Name.DestroySmallPlant6);
                    break;

                case BoardPiece.Category.Flesh:
                    soundNameList.Add(SoundData.Name.DestroyFlesh1);
                    soundNameList.Add(SoundData.Name.DestroyFlesh2);
                    break;

                case BoardPiece.Category.Leather:
                    soundNameList.Add(SoundData.Name.DestroyLeather);
                    break;

                case BoardPiece.Category.Dirt:
                    soundNameList.Add(SoundData.Name.Dig4);
                    break;

                case BoardPiece.Category.Crystal:
                    soundNameList.Add(SoundData.Name.DestroyCrystal);
                    break;

                case BoardPiece.Category.Indestructible:
                    soundNameList.Add(SoundData.Name.Empty);
                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            return soundNameList;
        }

        private static List<SoundData.Name> GetIsDroppedSoundNames(BoardPiece.Category category)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (category)
            {
                case BoardPiece.Category.Wood:
                    soundNameList.Add(SoundData.Name.ChestClose);
                    break;

                case BoardPiece.Category.Stone:
                    soundNameList.Add(SoundData.Name.StepRock1);
                    break;

                case BoardPiece.Category.Metal:
                    soundNameList.Add(SoundData.Name.HitMetal);
                    break;

                case BoardPiece.Category.SmallPlant:
                    soundNameList.Add(SoundData.Name.DropPlant);
                    break;

                case BoardPiece.Category.Flesh:
                    soundNameList.Add(SoundData.Name.DropGeneric);
                    break;

                case BoardPiece.Category.Leather:
                    soundNameList.Add(SoundData.Name.DropLeather);
                    break;

                case BoardPiece.Category.Dirt:
                    soundNameList.Add(SoundData.Name.DropSand);
                    break;

                case BoardPiece.Category.Crystal:
                    soundNameList.Add(SoundData.Name.DropCrystal);
                    break;

                case BoardPiece.Category.Indestructible:
                    soundNameList.Add(SoundData.Name.DropGeneric);
                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            return soundNameList;
        }
    }
}