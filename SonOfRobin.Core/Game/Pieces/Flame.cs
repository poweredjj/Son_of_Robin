using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        private BoardPiece burningPiece;
        private bool burningPieceChecked; // one time check, after deserialization
        private int burningFramesLeft;

        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, int[] maxMassForSize = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: 0f, lightEngine: new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: false), mass: 60, canShrink: true)
        {
            // looped sound would populate all sound channels fast, so non-looped short sound is used instead
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.FireBurnShort, maxPitchVariation: 0.5f));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, maxPitchVariation: 0.5f));
            this.burningPieceChecked = false;
            this.burningFramesLeft = Random.Next(60, 25 * 60);
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            this.burningFramesLeft--;

            // attaching burningPiece

            if (this.burningPiece == null && !this.burningPieceChecked)
            {
                // one time check after deserialization, to properly assign burningPiece again

                Sprite burningSprite = this.world.trackingManager.GetTargetSprite(this.sprite);
                if (burningSprite != null) this.burningPiece = burningSprite.boardPiece;

                this.burningPieceChecked = true;
            }

            if (this.burningFramesLeft <= 0 && this.burningPiece != null)
            {
                this.burningPiece = null;
                this.Mass /= 3;
            }

            // checking for rain and water

            float rainPercentage = this.world.weather.RainPercentage;
            bool isRaining = this.world.weather.IsRaining;
            this.Mass -= rainPercentage * 0.01f;

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                if (!this.burningPiece.sprite.IsOnBoard)
                {
                    this.burningPiece.BurnLevel = 0;
                    this.StopBurning();
                    return;
                }

                if (this.burningPiece.sprite.IsOnBoard && this.burningPiece.sprite.IsInWater)
                {
                    this.burningPiece.BurnLevel = 0;
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff); // only when is put out by water
                    this.StopBurning();
                    return;
                }

                if (!this.burningPiece.IsBurning)
                {
                    this.StopBurning();
                    return;
                }
            }

            // calculating burn values

            int affectedDistance = Math.Min(Math.Max((int)(this.Mass / 20), 25), 110);

            float baseBurnVal = Math.Max(this.Mass / 60f, 1);
            if (isRaining) baseBurnVal /= 4;

            float baseHitPointsVal = (float)baseBurnVal / 180f;

            // warming up nearby pieces

            var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: affectedDistance, compareWithBottom: true);
            foreach (BoardPiece heatedPiece in piecesWithinRange)
            {
                if (heatedPiece.fireAffinity == 0 || heatedPiece.sprite.IsInWater) continue;

                float distanceMultiplier = 1f - (Vector2.Distance(this.sprite.position, heatedPiece.sprite.position) / (float)affectedDistance);

                heatedPiece.BurnLevel += baseBurnVal * distanceMultiplier;

                if (heatedPiece != this.burningPiece && heatedPiece.IsAnimalOrPlayer)
                {
                    if (!heatedPiece.IsBurning) // getting damage before burning
                    {
                        float hitPointsToSubtract = baseHitPointsVal * distanceMultiplier;
                        heatedPiece.hitPoints = Math.Max(heatedPiece.hitPoints - hitPointsToSubtract, 0);

                        if (hitPointsToSubtract > 0.1f && SonOfRobinGame.CurrentUpdate % 15 == 0 && this.world.random.Next(0, 4) == 0)
                        {
                            heatedPiece.soundPack.Play(PieceSoundPack.Action.Cry);

                            if (heatedPiece.GetType() == typeof(Player) && !this.world.solidColorManager.AnySolidColorPresent)
                            {
                                this.world.ShakeScreen();
                                this.world.FlashRedOverlay();
                            }
                        }

                        if (heatedPiece.GetType() == typeof(Animal))
                        {
                            Animal animal = (Animal)heatedPiece;
                            animal.target = this;
                            animal.aiData.Reset();
                            animal.activeState = State.AnimalFlee;
                        }
                    }
                }
            }

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                // affecting burningPiece

                float hitPointsToTake = this.burningPiece.GetType() == typeof(Player) ? 0.6f : Math.Min(0.25f, this.burningPiece.maxHitPoints / 150);

                this.burningPiece.hitPoints = Math.Max(this.burningPiece.hitPoints - hitPointsToTake, 0);

                if (this.burningPiece.IsAnimalOrPlayer && SonOfRobinGame.CurrentUpdate % 40 == 0) this.burningPiece.soundPack.Play(PieceSoundPack.Action.Cry);

                if (this.burningPiece.GetType() == typeof(Player))
                {
                    if (!this.world.solidColorManager.AnySolidColorPresent)
                    {
                        this.world.ShakeScreen();
                        this.world.FlashRedOverlay();
                    }
                }

                if (isRaining) this.Mass *= 0.985f;
                else this.Mass += baseBurnVal;

                if (this.burningPiece.hitPoints == 0)
                {
                    bool isAnimal = this.burningPiece.GetType() == typeof(Animal);

                    if (isAnimal) this.burningPiece.soundPack.Play(PieceSoundPack.Action.IsDestroyed);
                    Yield.DebrisType debrisType = isAnimal ? Yield.DebrisType.Blood : Yield.DebrisType.Soot;

                    this.burningPiece.yield?.DropDebris(debrisTypeListOverride: new List<Yield.DebrisType> { debrisType });
                    this.burningPiece.Destroy();
                    this.burningPiece = null;
                }
            }
            else
            {
                // cooling down, if no burningPiece is present
                this.Mass *= 0.991f;
            }

            if (this.Mass <= 10) this.StopBurning(); // must be less than starting mass

            // updating lightEngine

            this.sprite.lightEngine.Size = Math.Max(affectedDistance * 5, 50);
            if (!this.sprite.lightEngine.IsActive && this.sprite.currentCell.spriteGroups[Cell.Group.LightSource].Values.Count < 2)
            {
                this.sprite.lightEngine.Activate();
            }
        }

        private void StopBurning(bool playSound = false)
        {
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            if (playSound || this.world.weather.IsRaining) this.soundPack.Play(PieceSoundPack.Action.TurnOff);
            new OpacityFade(sprite: this.sprite, destOpacity: 0f, duration: 20, destroyPiece: true);
            this.RemoveFromStateMachines();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["flame_burningFramesLeft"] = this.burningFramesLeft;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            if (pieceData.ContainsKey("flame_burningFramesLeft")) this.burningFramesLeft = (int)(Int64)pieceData["flame_burningFramesLeft"];
        }
    }
}