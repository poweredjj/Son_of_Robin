using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        private BoardPiece burningPiece;
        private bool targetSpriteChecked; // one time check, after deserialization

        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, int[] maxMassForSize = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: 0f, lightEngine: new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: false), mass: 60)
        {
            // looped sound would populate all sound channels fast, so non-looped short sound is used instead
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.FireBurnShort, maxPitchVariation: 0.5f));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, maxPitchVariation: 0.5f));
            this.targetSpriteChecked = false;
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            // attaching burningPiece

            if (this.burningPiece == null && !this.targetSpriteChecked)
            {
                // one time check after deserialization, to properly assign burningPiece again

                Sprite burningSprite = Tracking.GetTargetSprite(world: this.world, followingSprite: this.sprite);
                if (burningSprite != null) this.burningPiece = burningSprite.boardPiece;

                this.targetSpriteChecked = true;
            }

            // checking for rain and water

            float rainPercentage = this.world.weather.RainPercentage;
            this.Mass -= rainPercentage * rainPercentage * 0.01f;

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                if (this.burningPiece.sprite.IsInWater)
                {
                    this.burningPiece.BurnLevel = 0;
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff); // only when is put out by water
                    this.StopBurning();
                    return;
                }

                if (rainPercentage > 0)
                {
                    this.burningPiece.BurnLevel -= rainPercentage * 0.05f;
                    if (!this.burningPiece.IsBurning)
                    {
                        this.StopBurning();
                        return;
                    }
                }
            }

            // calculating burn values

            int affectedDistance = Math.Min(Math.Max((int)(this.Mass / 20), 25), 110);

            float baseBurnVal = Math.Max(this.Mass / 60f, 1);
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
                            animal.aiData.Reset(animal);
                            animal.activeState = State.AnimalFlee;
                        }
                    }
                }
            }

            if (this.burningPiece != null && this.burningPiece.exists)
            {
                // affecting burningPiece

                this.burningPiece.hitPoints = Math.Max(this.burningPiece.hitPoints - baseHitPointsVal, 0);

                if (this.burningPiece.IsAnimalOrPlayer && SonOfRobinGame.CurrentUpdate % 40 == 0) this.burningPiece.soundPack.Play(PieceSoundPack.Action.Cry);

                if (this.burningPiece.GetType() == typeof(Player))
                {
                    if (!this.world.solidColorManager.AnySolidColorPresent)
                    {
                        this.world.ShakeScreen();
                        this.world.FlashRedOverlay();
                    }
                }

                this.Mass += baseBurnVal;
                if (this.burningPiece.hitPoints == 0)
                {
                    bool isAnimal = this.burningPiece.GetType() == typeof(Animal);

                    Yield.DebrisType debrisType = isAnimal ? Yield.DebrisType.Blood : Yield.DebrisType.Soot;
                    if (isAnimal) this.burningPiece.soundPack.Play(PieceSoundPack.Action.Die);

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
            if (!this.sprite.lightEngine.IsActive) this.sprite.lightEngine.Activate();
        }

        private void StopBurning()
        {
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            if (this.burningPiece != null) new WorldEvent(eventName: WorldEvent.EventName.CoolDownAfterBurning, world: this.world, delay: 10 * 60, boardPiece: this.burningPiece, eventHelper: this.burningPiece.BurnLevel);
            new OpacityFade(sprite: this.sprite, destOpacity: 0f, duration: 20, destroyPiece: true);
            this.RemoveFromStateMachines();
        }
    }
}