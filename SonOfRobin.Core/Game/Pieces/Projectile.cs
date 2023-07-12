using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Projectile : BoardPiece
    {
        public enum TargetCategory
        { Wood, Stone, Metal, SmallPlant, Animal }

        public readonly int baseHitPower;
        private int realHitPower;
        private bool shootMode; // true == shoot (can damage target), false == bounce (can't damage anything)
        private readonly bool canBeStuck;
        private readonly bool isBurning;

        public Projectile(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int baseHitPower, int maxHitPoints, byte stackSize, bool canBeStuck, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, Yield yield = null, bool rotatesWhenDropped = true, List<Buff> buffList = null, bool isBurning = false, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, stackSize: stackSize, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, buffList: buffList, activeState: State.Empty, lightEngine: lightEngine)
        {
            this.canBeStuck = canBeStuck;
            this.isBurning = isBurning;
            this.baseHitPower = baseHitPower;
            this.realHitPower = 0; // calculated each shooting time
            this.shootMode = false;

            this.soundPack.AddAction(action: PieceSoundPack.Action.ArrowFly, sound: new Sound(name: SoundData.Name.ArrowFly, maxPitchVariation: 0.3f));
            this.soundPack.AddAction(action: PieceSoundPack.Action.ArrowHit, sound: new Sound(name: SoundData.Name.ArrowHit, maxPitchVariation: 0.3f));
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropArrow, maxPitchVariation: 0.3f, cooldown: 20));
        }

        public void GetThrown(Vector2 startPosition, Vector2 movement, float hitPowerMultiplier, int shootingPower)
        {
            this.realHitPower = (int)(this.baseHitPower * hitPowerMultiplier);
            this.shootMode = true;

            bool shotPossible = this.PlaceOnBoard(randomPlacement: false, position: startPosition, precisePlacement: true, addPlannedDestruction: true);
            if (!shotPossible)
            {
                this.world.Player.ToolStorage.AddPiece(this);
                return;
            }

            this.world.Player.soundPack.Play(PieceSoundPack.Action.PlayerBowRelease);
            new RumbleEvent(force: 0.3f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.22f);

            float angle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: this.sprite.position + movement);
            this.sprite.rotation = angle + (float)(Math.PI * 2f * 0.375f);

            float distanceMultiplier = Math.Max(shootingPower / 50f, 0.4f);
            movement *= distanceMultiplier;
            this.AddPassiveMovement(movement: movement);
            if (this.isBurning)
            {
                this.sprite.AssignNewName("burning");
                if (this.sprite.lightEngine != null) this.sprite.lightEngine.Activate();
            }

            this.soundPack.Play(PieceSoundPack.Action.ArrowFly);

            this.world.HintEngine.Disable(Tutorials.Type.ShootProjectile);
            this.world.HintEngine.Disable(PieceHint.Type.AnimalNegative);
            this.world.HintEngine.Disable(PieceHint.Type.AnimalBow);
        }

        public override bool ProcessPassiveMovement()
        {
            if (!this.shootMode) return base.ProcessPassiveMovement();
            if (this.passiveMovement == Vector2.Zero) return false;

            Vector2 movement = this.passiveMovement / passiveMovementMultiplier;
            bool attachedToTarget = false;

            if (!this.sprite.Move(movement: movement, additionalMoveType: Sprite.AdditionalMoveType.Half))
            {
                this.soundPack.Stop(PieceSoundPack.Action.ArrowFly);

                List<Sprite> collidingSpritesList = this.sprite.GetCollidingSpritesAtPosition(positionToCheck: this.sprite.position + movement, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColMovement });
                var collidingPiecesList = collidingSpritesList.Select(s => s.boardPiece);

                var collidingAnimals = collidingPiecesList.Where(piece => piece.GetType() == typeof(Animal)).ToList();
                BoardPiece closestAnimal = FindClosestPiece(sprite: this.sprite, pieceList: collidingAnimals, offsetX: 0, offsetY: 0);

                if (closestAnimal != null)
                {
                    Animal animal = (Animal)closestAnimal;
                    Tool.HitTarget(attacker: this.world.Player, target: animal, hitPower: this.realHitPower, targetPushMultiplier: 0.06f, buffList: this.buffList);

                    if (!this.pieceInfo.toolIndestructible && !this.isBurning) // buring arrows should not be destroyed before exploding
                    {
                        this.HitPoints = Math.Max(0, this.HitPoints - this.world.random.Next(10, 35));
                        this.showStatBarsTillFrame = world.CurrentUpdate + 1200;
                        if (this.HitPoints == 0)
                        {
                            this.Destroy();
                            return true;
                        }
                    }

                    if (this.canBeStuck && this.exists)
                    {
                        Tracking tracking = new Tracking(world: this.world, targetSprite: animal.sprite, followingSprite: this.sprite, turnOffDelay: this.world.random.Next(200, 2000), bounceWhenRemoved: true);
                        if (tracking.isCorrect)
                        {
                            this.showStatBarsTillFrame = 0;
                            attachedToTarget = true;
                        }
                    }
                }
                else // target is not animal
                {
                    if (!this.pieceInfo.toolIndestructible && !this.isBurning) // buring arrows should not be destroyed before exploding
                    {
                        this.HitPoints = Math.Max(0, this.HitPoints - this.world.random.Next(0, 15));
                        this.showStatBarsTillFrame = world.CurrentUpdate + 1200;
                        if (this.HitPoints == 0)
                        {
                            PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Attack);
                            this.Destroy();
                            return true;
                        }
                    }
                }

                if (this.shootMode && this.isBurning) this.Explode();

                this.shootMode = false;

                if (attachedToTarget || (this.canBeStuck && this.world.random.Next(0, 2) == 1))
                {
                    this.soundPack.Play(PieceSoundPack.Action.ArrowHit);
                    this.passiveMovement = Vector2.Zero;
                    this.passiveRotation = 0;
                }
                else
                {
                    this.soundPack.Play(PieceSoundPack.Action.IsDropped);
                    this.passiveMovement *= -0.5f;
                    this.passiveMovement.X += (float)((this.world.random.NextDouble() - 0.5f) * 500f);
                    this.passiveMovement.Y += (float)((this.world.random.NextDouble() - 0.5f) * 500f);
                    this.passiveRotation = this.world.random.Next(-70, 70);
                }
            }

            if (this.exists && Math.Abs(this.passiveMovement.X) < (passiveMovementMultiplier * 4.5f) && Math.Abs(this.passiveMovement.Y) < (passiveMovementMultiplier * 4.5f))
            {
                this.soundPack.Stop(PieceSoundPack.Action.ArrowFly);
                this.shootMode = false;
                this.passiveMovement = Vector2.Zero;
                if (this.sprite.IsInWater) this.Destroy();
                if (this.isBurning) this.Explode();
                else this.soundPack.Play(PieceSoundPack.Action.IsDropped);
                return false;
            }

            this.passiveMovement *= 0.98f;
            return true;
        }

        private void Explode()
        {
            var piecesToHeat = new List<BoardPiece>();

            var piecesWithinSmallRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 70, compareWithBottom: true);
            var ignoredTypesList = new List<Type> { typeof(Player), typeof(Debris), typeof(Flame) };

            foreach (BoardPiece piece in piecesWithinSmallRange)
            {
                if (!ignoredTypesList.Contains(piece.GetType())) piecesToHeat.Add(piece);
            }

            var piecesWithinLargeRange = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 180, compareWithBottom: false);
            var piecesAffectedInLargeRange = new List<PieceTemplate.Name> { PieceTemplate.Name.SwampGas };

            foreach (BoardPiece piece in piecesWithinLargeRange)
            {
                if (piecesAffectedInLargeRange.Contains(piece.name) && !piecesToHeat.Contains(piece)) piecesToHeat.Add(piece);
            }

            foreach (BoardPiece piece in piecesToHeat)
            {
                piece.BurnLevel += this.baseHitPower * 12;
            }

            PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.sprite.position, templateName: PieceTemplate.Name.Explosion, closestFreeSpot: true);
            this.Destroy();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["projectile_realHitPower"] = this.realHitPower;
            pieceData["projectile_shootMode"] = this.shootMode;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.realHitPower = (int)(Int64)pieceData["projectile_realHitPower"];
            this.shootMode = (bool)pieceData["projectile_shootMode"];
        }
    }
}