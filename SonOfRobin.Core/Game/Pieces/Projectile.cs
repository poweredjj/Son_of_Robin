using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Projectile : BoardPiece
    {
        public enum TargetCategory { Wood, Stone, Metal, SmallPlant, Animal }

        private readonly int baseHitPower;
        private int realHitPower;
        private bool shootMode; // true == shoot (can damage target), false == bounce (can't damage anything)
        private readonly bool canBeStuck;

        public Projectile(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, int baseHitPower, int maxHitPoints, byte stackSize, bool canBeStuck, string readableName, string description,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool indestructible = false, Yield yield = null, bool rotatesWhenDropped = true, bool fadeInAnim = false, List<BuffEngine.Buff> buffList = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, stackSize: stackSize, indestructible: indestructible, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, readableName: readableName, description: description, category: Category.Indestructible, buffList: buffList, activeState: State.Empty)
        {
            this.canBeStuck = canBeStuck;
            this.baseHitPower = baseHitPower;
            this.realHitPower = 0; // calculated each shooting time
            this.shootMode = false;
        }

        public void GetThrown(Vector2 startPosition, Vector2 movement, float hitPowerMultiplier, int shootingPower)
        {
            this.realHitPower = (int)(this.baseHitPower * hitPowerMultiplier);
            this.shootMode = true;

            bool shotPossible = this.PlaceOnBoard(position: startPosition, precisePlacement: true);
            if (!shotPossible)
            {
                this.world.player.toolStorage.AddPiece(this);
                return;
            }

            float angle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: this.sprite.position + movement);
            this.sprite.rotation = angle + (float)(Math.PI * 2f * 0.375f);

            float distanceMultiplier = Math.Max(shootingPower / 50f, 0.4f);
            movement *= distanceMultiplier;
            this.AddPassiveMovement(movement: movement);

            this.world.hintEngine.Disable(Tutorials.Type.ShootProjectile);
            this.world.hintEngine.Disable(PieceHint.Type.AnimalNegative);
            if (this.name == PieceTemplate.Name.ArrowWood || this.name == PieceTemplate.Name.ArrowIron) this.world.hintEngine.Disable(PieceHint.Type.AnimalBow);
        }

        public override bool ProcessPassiveMovement()
        {
            if (!this.shootMode) return base.ProcessPassiveMovement();
            if (this.passiveMovement == Vector2.Zero) return false;

            Vector2 movement = this.passiveMovement / passiveMovementMultiplier;
            bool attachedToTarget = false;

            if (!this.sprite.Move(movement: movement, additionalMoveType: Sprite.AdditionalMoveType.Half))
            {
                List<Sprite> collidingSpritesList = this.sprite.GetCollidingSpritesAtPosition(positionToCheck: this.sprite.position + movement);
                List<BoardPiece> collidingPiecesList = collidingSpritesList.Select(s => s.boardPiece).ToList();

                var collidingAnimals = collidingPiecesList.Where(piece => piece.GetType() == typeof(Animal)).ToList();
                BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: collidingAnimals, offsetX: 0, offsetY: 0);

                if (closestPiece?.GetType() == typeof(Animal))
                {
                    Animal animal = (Animal)closestPiece;
                    Tool.HitTarget(attacker: this.world.player, target: animal, hitPower: this.realHitPower, targetPushMultiplier: 0.06f, buffList: this.buffList);

                    if (!this.indestructible)
                    {
                        this.hitPoints = Math.Max(0, this.hitPoints - this.world.random.Next(10, 35));
                        this.showStatBarsTillFrame = world.currentUpdate + 1200;
                        if (this.hitPoints == 0)
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
                else
                {
                    if (!this.indestructible)
                    {
                        this.hitPoints = Math.Max(0, this.hitPoints - this.world.random.Next(0, 15));
                        this.showStatBarsTillFrame = world.currentUpdate + 1200;
                        if (this.hitPoints == 0)
                        {
                            PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Attack);
                            this.Destroy();
                            return true;
                        }
                    }
                }

                this.shootMode = false;

                if (attachedToTarget || (this.canBeStuck && this.world.random.Next(0, 2) == 1))
                {
                    this.passiveMovement = Vector2.Zero;
                    this.passiveRotation = 0;
                }
                else
                {
                    this.passiveMovement *= -0.5f;
                    this.passiveMovement.X += (float)((this.world.random.NextDouble() - 0.5f) * 500f);
                    this.passiveMovement.Y += (float)((this.world.random.NextDouble() - 0.5f) * 500f);
                    this.passiveRotation = this.world.random.Next(-70, 70);
                }

            }

            if (Math.Abs(this.passiveMovement.X) < (passiveMovementMultiplier * 4.5f) && Math.Abs(this.passiveMovement.Y) < (passiveMovementMultiplier * 4.5f))
            {
                this.shootMode = false;
                this.passiveMovement = Vector2.Zero;
                if (this.sprite.IsInWater) this.Destroy();
                return false;
            }

            this.passiveMovement *= 0.98f;
            return true;
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
            this.realHitPower = (int)pieceData["projectile_realHitPower"];
            this.shootMode = (bool)pieceData["projectile_shootMode"];
        }

    }
}