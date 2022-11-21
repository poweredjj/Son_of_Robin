using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceSoundPack
    {
        public enum Action { IsDestroyed, IsHit, IsOn, IsDropped, Cry, Die, Eat, Open, Close, StepGrass, StepWater, StepSand, StepRock, StepLava, StepMud, SwimShallow, SwimDeep, HasAppeared, ArrowFly, ArrowHit, PlayerBowDraw, TurnOn, TurnOff, PlayerBowRelease, PlayerPulse, PlayerSnore, PlayerYawn, PlayerStomachGrowl, PlayerSprint, PlayerPant, PlayerSpeak, Ambient }

        private readonly Dictionary<Action, Sound> soundDict;
        private BoardPiece boardPiece;

        public PieceSoundPack()
        {
            soundDict = new Dictionary<Action, Sound>();
        }

        public object Serialize()
        {
            var playingLoopsList = new List<object>();

            foreach (var kvp in this.soundDict)
            {
                Action action = kvp.Key;
                Sound sound = kvp.Value;

                if (sound.isLooped && sound.IsPlaying) playingLoopsList.Add(action);
            }

            return playingLoopsList;
        }

        public void Deserialize(Object soundPackData)
        {
            var playingLoopsList = (List<object>)soundPackData;

            foreach (Action action in playingLoopsList)
            {
                Sound sound = this.soundDict[action];

                if (sound.isEmpty) sound.playAfterAssign = true;
                else this.Play(action);
            }
        }

        public void AddAction(Action action, Sound sound, bool replaceExisting = true)
        {
            if (sound.isEmpty) return;

            if (this.boardPiece != null && !sound.HasBoardPiece) sound.AddBoardPiece(this.boardPiece);

            if (this.soundDict.ContainsKey(action))
            {
                if (replaceExisting)
                {
                    Sound previousSound = this.soundDict[action];

                    if (previousSound.playAfterAssign || (sound.isLooped && previousSound.isLooped && previousSound.IsPlaying))
                    {
                        previousSound.Stop();
                        sound.Play();
                    }

                    this.soundDict.Remove(action);
                }
                else return;
            }

            soundDict[action] = sound;
        }

        public void RemoveAction(Action action)
        {
            if (this.soundDict.ContainsKey(action)) this.soundDict.Remove(action);
        }

        public void Activate(BoardPiece boardPieceToAssign)
        {
            if (this.boardPiece != null) throw new ArgumentException($"Cannot add boardPiece {boardPieceToAssign.name} {boardPieceToAssign.id} to sound with boardPiece already assigned {this.boardPiece.name} {this.boardPiece.id} ");

            this.boardPiece = boardPieceToAssign;

            foreach (Sound sound in soundDict.Values)
            {
                sound.AddBoardPiece(this.boardPiece);
            }

            AddAction(action: Action.IsHit, sound: new Sound(nameList: GetHitSoundNames(boardPiece), boardPiece: this.boardPiece, maxPitchVariation: 0.5f), replaceExisting: false);

            AddAction(action: Action.IsDestroyed, sound: new Sound(nameList: GetIsDestroyedNamesList(boardPiece), boardPiece: this.boardPiece, maxPitchVariation: 0.4f), replaceExisting: false);

            AddAction(action: Action.IsDropped, sound: new Sound(nameList: GetIsDroppedSoundNames(boardPiece), boardPiece: this.boardPiece, maxPitchVariation: 0.5f, cooldown: 30), replaceExisting: false);

            bool isPlayer = this.boardPiece.GetType() == typeof(Player);
            bool isAnimal = this.boardPiece.GetType() == typeof(Animal);

            if (isPlayer || isAnimal)
            {
                float volumeMultiplier = isPlayer ? 1f : 0.35f;

                AddAction(action: Action.StepGrass, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepGrass1, SoundData.Name.StepGrass2, SoundData.Name.StepGrass3, SoundData.Name.StepGrass4, SoundData.Name.StepGrass5, SoundData.Name.StepGrass6 }, boardPiece: this.boardPiece, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: isPlayer ? 1f : 0.35f, maxPitchVariation: 0.6f), replaceExisting: false);

                AddAction(action: Action.StepSand, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepSand1, SoundData.Name.StepSand2, SoundData.Name.StepSand3, SoundData.Name.StepSand4 }, boardPiece: this.boardPiece, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f), replaceExisting: false);

                AddAction(action: Action.StepMud, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepMud1, SoundData.Name.StepMud2, SoundData.Name.StepMud3, SoundData.Name.StepMud4, SoundData.Name.StepMud5, SoundData.Name.StepMud6, SoundData.Name.StepMud7 }, boardPiece: this.boardPiece, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f), replaceExisting: false);

                AddAction(action: Action.StepRock, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.StepRock1, SoundData.Name.StepRock2, SoundData.Name.StepRock3 }, boardPiece: this.boardPiece, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 0.7f, maxPitchVariation: 0.2f), replaceExisting: false);

                AddAction(action: Action.StepLava, sound: new Sound(name: SoundData.Name.StepLava, boardPiece: this.boardPiece, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.2f), replaceExisting: false);

                AddAction(action: Action.StepWater, sound: new Sound(name: SoundData.Name.StepWater, boardPiece: this.boardPiece, maxPitchVariation: 0.5f, cooldown: isPlayer ? 20 : 14, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f), replaceExisting: false);

                AddAction(action: Action.SwimShallow, sound: new Sound(name: SoundData.Name.SwimShallow, boardPiece: this.boardPiece, maxPitchVariation: 0.5f, cooldown: isPlayer ? 60 : 25, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f), replaceExisting: false);

                AddAction(action: Action.SwimDeep, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.SwimDeep1, SoundData.Name.SwimDeep2, SoundData.Name.SwimDeep3, SoundData.Name.SwimDeep4, SoundData.Name.SwimDeep5 }, boardPiece: this.boardPiece, cooldown: isPlayer ? 60 : 25, ignore3DAlways: isPlayer, volume: volumeMultiplier * 1f, maxPitchVariation: 0.6f), replaceExisting: false);
            }
        }

        public void Play(Action action, bool ignore3D = false, bool ignoreCooldown = false)
        {
            if (!this.soundDict.ContainsKey(action)) return;
            this.soundDict[action].Play(ignore3DThisPlay: ignore3D, ignoreCooldown: ignoreCooldown);
        }

        public void Stop(Action action)
        {
            if (!this.soundDict.ContainsKey(action)) return;
            this.soundDict[action].Stop();
        }

        public Sound GetSound(Action action)
        {
            return this.soundDict.ContainsKey(action) ? this.soundDict[action] : null;
        }

        public bool IsPlaying(Action action)
        {
            return this.soundDict.ContainsKey(action) ? this.soundDict[action].IsPlaying : false;
        }

        public bool IsLooped(Action action)
        {
            return this.soundDict.ContainsKey(action) ? this.soundDict[action].isLooped : false;
        }

        public void StopAll(Action ignoredAction)
        {
            foreach (var kvp in this.soundDict)
            {
                if (kvp.Key != ignoredAction) kvp.Value.Stop();
            }
        }

        private static List<SoundData.Name> GetHitSoundNames(BoardPiece boardPiece)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (boardPiece.category)
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
                    throw new DivideByZeroException($"Unsupported category - {boardPiece.category}.");
            }

            return soundNameList;
        }

        private static List<SoundData.Name> GetIsDroppedSoundNames(BoardPiece boardPiece)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (boardPiece.category)
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
                    throw new DivideByZeroException($"Unsupported category - {boardPiece.category}.");
            }

            return soundNameList;
        }

        private static List<SoundData.Name> GetIsDestroyedNamesList(BoardPiece boardPiece)
        {
            var soundNameList = new List<SoundData.Name>();

            switch (boardPiece.category)
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
                    throw new DivideByZeroException($"Unsupported category - {boardPiece.category}.");
            }

            return soundNameList;
        }
    }
}
