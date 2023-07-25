using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        public BoardPiece BurningPiece { get; private set; }

        private bool burningPieceChecked; // one time check, after deserialization
        private int burningFramesLeft;

        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState,
            byte animSize = 0, string animName = "default", bool visible = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, visible: visible, activeState: activeState, lightEngine: new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: false))
        {
            // looped sound would populate all sound channels fast, so non-looped short sound is used instead
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.FireBurnShort, maxPitchVariation: 0.5f));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, maxPitchVariation: 0.5f));
            this.burningPieceChecked = false;
            this.burningFramesLeft = Random.Next(60, 40 * 60);
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            this.burningFramesLeft--;

            // attaching burningPiece

            if (this.BurningPiece == null && !this.burningPieceChecked)
            {
                // one time check after deserialization, to properly assign burningPiece again

                Sprite burningSprite = this.world.trackingManager.GetTargetSprite(this.sprite);
                if (burningSprite != null) this.BurningPiece = burningSprite.boardPiece;

                this.burningPieceChecked = true;
            }

            if (this.burningFramesLeft <= 0 && this.BurningPiece != null)
            {
                this.Destroy();
                this.BurningPiece.HeatLevel = 0.45f; // must be below minBurnLevelForFlame, otherwise glitches will happen
                return;
            }

            // checking for rain and water

            float rainPercentage = this.world.weather.RainPercentage;
            bool isRaining = this.world.weather.IsRaining;
            this.Mass -= rainPercentage * 0.01f;

            if (this.BurningPiece != null && this.BurningPiece.exists)
            {
                if (!this.BurningPiece.sprite.IsOnBoard)
                {
                    this.BurningPiece.HeatLevel = 0;
                    this.StopBurning();
                    return;
                }

                if (this.BurningPiece.sprite.IsOnBoard && this.BurningPiece.sprite.IsInWater)
                {
                    this.BurningPiece.HeatLevel = 0;
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff); // only when is put out by water
                    this.StopBurning();
                    return;
                }

                if (!this.BurningPiece.IsBurning)
                {
                    this.StopBurning();
                    return;
                }
            }

            // calculating burn values

            int affectedDistance = Math.Min(Math.Max((int)(this.Mass / 20), 25), 120);

            float baseBurnVal = Math.Max(this.Mass / 50f, 1);
            if (isRaining) baseBurnVal /= 4;

            float baseHitPointsVal = (float)baseBurnVal / 180f;

            // warming up nearby pieces

            var piecesWithinRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: affectedDistance, compareWithBottom: true);
            foreach (BoardPiece heatedPiece in piecesWithinRange)
            {
                if (heatedPiece.pieceInfo.fireAffinity == 0 || heatedPiece.sprite.IsInWater) continue;

                float distanceMultiplier = 1f - (Vector2.Distance(this.sprite.position, heatedPiece.sprite.position) / (float)affectedDistance);

                heatedPiece.HeatLevel += baseBurnVal * distanceMultiplier;

                if (heatedPiece != this.BurningPiece && heatedPiece.IsAnimalOrPlayer)
                {
                    if (!heatedPiece.IsBurning) // getting damage before burning
                    {
                        float hitPointsToSubtract = baseHitPointsVal * distanceMultiplier;
                        heatedPiece.HitPoints = Math.Max(heatedPiece.HitPoints - hitPointsToSubtract, 0);

                        if (hitPointsToSubtract > 0.1f && SonOfRobinGame.CurrentUpdate % 15 == 0 && this.world.random.Next(0, 4) == 0)
                        {
                            heatedPiece.soundPack.Play(PieceSoundPack.Action.Cry);

                            if (heatedPiece.GetType() == typeof(Player) && !this.world.solidColorManager.AnySolidColorPresent)
                            {
                                this.world.camera.AddRandomShake();
                                this.world.FlashRedOverlay();
                                new RumbleEvent(force: 0.07f, bigMotor: true, fadeInSeconds: 0.25f, durationSeconds: 0, fadeOutSeconds: 0.25f, minSecondsSinceLastRumbleBigMotor: 0.7f);
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

            if (this.BurningPiece != null && this.BurningPiece.exists)
            {
                // affecting burningPiece

                float hitPointsToTake = this.BurningPiece.GetType() == typeof(Player) ? 0.6f : Math.Max(0.05f, this.BurningPiece.maxHitPoints / 700f);
                this.BurningPiece.HitPoints -= hitPointsToTake;
                if (this.BurningPiece.pieceInfo.blocksMovement)
                {
                    ParticleEngine.TurnOn(sprite: this.BurningPiece.sprite, preset: ParticleEngine.Preset.BurnFlame, duration: 10, particlesToEmit: (int)(this.BurningPiece.HeatLevel * 2));
                    this.BurningPiece.showStatBarsTillFrame = this.world.CurrentUpdate + 600;
                }

                if (this.BurningPiece.IsAnimalOrPlayer && !this.BurningPiece.soundPack.IsPlaying(PieceSoundPack.Action.Cry))
                    this.BurningPiece.soundPack.Play(PieceSoundPack.Action.Cry);

                if (this.BurningPiece.GetType() == typeof(Player))
                {
                    if (!this.world.solidColorManager.AnySolidColorPresent)
                    {
                        this.world.camera.AddRandomShake();
                        this.world.FlashRedOverlay();
                        new RumbleEvent(force: 1f, bigMotor: true, fadeInSeconds: 0.25f, durationSeconds: 0, fadeOutSeconds: 0.25f, minSecondsSinceLastRumbleBigMotor: 0.4f);
                    }
                }

                if (isRaining) this.Mass *= 0.985f;
                else this.Mass += baseBurnVal;

                if (this.BurningPiece.HitPoints < 1)
                {
                    bool isAnimal = this.BurningPiece.GetType() == typeof(Animal);

                    if (isAnimal) this.BurningPiece.soundPack.Play(PieceSoundPack.Action.IsDestroyed);
                    Yield.DebrisType debrisType = isAnimal ? Yield.DebrisType.Blood : Yield.DebrisType.Soot;

                    this.BurningPiece.pieceInfo.Yield?.DropDebris(piece: this.BurningPiece, debrisTypeListOverride: new List<Yield.DebrisType> { debrisType });
                    this.BurningPiece.Destroy();
                    this.BurningPiece = null;
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