using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public abstract class BoardPiece
    {
        public enum State
        {
            Empty,

            PlayerControlledWalking,

            PlantGrowthAndReproduction,

            AnimalWalkAround,
            AnimalAssessSituation,
            AnimalRest,
            AnimalChaseTarget,
            AnimalAttack,
            AnimalEat,
            AnimalMate,
            AnimalGiveBirth,
            AnimalFlee,
        }

        private static int idCounter;
        public readonly World world;
        public readonly string id;
        public readonly PieceTemplate.Name name;
        public Sprite sprite;
        public State activeState;
        public float speed;
        private float mass;
        public readonly float startingMass;
        public bool exists;
        public bool alive;
        public int generation;
        private readonly Dictionary<byte, int> maxMassBySize;
        private readonly int staysAfterDeath;
        public int maxAge;
        public int currentAge;
        public float bioWear;
        public float efficiency;
        public float hitPoints;
        public int showHitPointsTillFrame;
        public readonly float maxHitPoints;
        public readonly byte stackSize;
        public PieceStorage pieceStorage;
        public readonly Yield yield;
        public Scheduler.ActionName boardAction;
        public Scheduler.ActionName toolbarAction;
        public readonly bool canBePickedUp;
        private static readonly float passiveMovementMultiplier = 100f;
        private Vector2 passiveMovement;
        private readonly int destructionDelay;

        public float Mass
        {
            get { return this.mass; }
            set
            {
                this.mass = value;
                int previousSpriteSize = this.sprite.animSize;
                this.SetSpriteSizeByMass();
                if (previousSpriteSize != this.sprite.animSize && this.pieceStorage != null && this.GetType() == typeof(Plant))
                {
                    Plant plant = (Plant)this;
                    plant.SetAllFruitPosAgain();
                }
            }
        }

        private byte SpriteSize
        {
            get
            {
                try
                {
                    foreach (var kvp in this.maxMassBySize)
                    {
                        if (this.Mass < kvp.Value)
                        {
                            this.sprite.AssignNewSize(kvp.Key);
                            return kvp.Key;
                        }
                    }
                }
                catch (NullReferenceException) { }
                return 0;
            }
        }

        public BoardPiece(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, bool visible = true, ushort minDistance = 0, ushort maxDistance = 100, bool ignoresCollisions = false, int destructionDelay = 0, int maxAge = 0, bool floatsOnWater = false, bool checksFullCollisions = false, int generation = 0, int mass = 1, int staysAfterDeath = 800, float maxHitPoints = 1, byte stackSize = 1, byte storageWidth = 0, byte storageHeight = 0, Scheduler.ActionName boardAction = Scheduler.ActionName.Empty, Scheduler.ActionName toolbarAction = Scheduler.ActionName.Empty, bool canBePickedUp = false, Yield yield = null)
        {
            this.world = world;
            this.name = name;
            this.id = $"{idCounter}_{this.name}_{this.world.random.Next(0, 1000000)}";
            this.sprite = new Sprite(boardPiece: this, id: this.id, position: position, world: this.world, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, visible: visible, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, allowedFields: allowedFields, floatsOnWater: floatsOnWater, checksFullCollisions: checksFullCollisions);

            if (!this.sprite.placedCorrectly) return;

            AddToPieceCount();
            idCounter++;
            this.destructionDelay = destructionDelay;
            this.activeState = State.Empty;
            this.maxHitPoints = maxHitPoints;
            this.hitPoints = maxHitPoints;
            this.showHitPointsTillFrame = 0;
            this.speed = speed;
            this.maxMassBySize = maxMassBySize;
            this.mass = mass;
            this.startingMass = mass;
            this.staysAfterDeath = staysAfterDeath + this.world.random.Next(0, 300);
            this.generation = generation;
            this.exists = true;
            this.alive = true;
            this.maxAge = (maxAge == 0) ? 0 : this.world.random.Next(Convert.ToInt32(maxAge * 0.4), Convert.ToInt32(maxAge * 1.6));
            this.currentAge = 0;
            this.bioWear = 0; // 0 - 1 valid range
            this.efficiency = 1; // 0 - 1 valid range
            this.stackSize = stackSize;
            this.pieceStorage = storageWidth > 0 && storageHeight > 0 ? new PieceStorage(width: storageWidth, height: storageHeight, world: this.world, storagePiece: this, storageType: this.name == PieceTemplate.Name.Player ? PieceStorage.StorageType.Inventory : PieceStorage.StorageType.Chest) : null;
            this.boardAction = boardAction;
            this.toolbarAction = toolbarAction;
            this.passiveMovement = Vector2.Zero;
            this.canBePickedUp = canBePickedUp;
            this.yield = yield;
            if (this.yield != null) this.yield.AddPiece(this);

            this.AddPlannedDestruction();
            this.AddToStateMachines();
        }

        public void AddPlannedDestruction()
        {
            if (destructionDelay == 0) return;

            // duration value "-1" should be replaced with animation duration
            new PlannedDestruction(world: this.world, delay: (this.destructionDelay == -1) ? this.sprite.GetAnimDuration() - 1 : this.destructionDelay, boardPiece: this);
        }

        public virtual Dictionary<string, Object> Serialize()
        {
            var pieceData = new Dictionary<string, Object>
            {
                {"base_old_id", this.id}, // will be changed after loading, used to identify piece in other contexts
                {"base_name", this.name},
                {"base_hitPoints", this.hitPoints},
                {"base_showHitPointsTillFrame", this.showHitPointsTillFrame},
                {"base_mass", this.mass},
                {"base_alive", this.alive},
                {"base_maxAge", this.maxAge},
                {"base_bioWear", this.bioWear},
                {"base_efficiency", this.efficiency},
                {"base_activeState", this.activeState},
                {"base_pieceStorage", this.pieceStorage},
                {"base_boardAction", this.boardAction},
                {"base_toolbarAction", this.toolbarAction},
                {"base_passiveMovementX", this.passiveMovement.X},
                {"base_passiveMovementY", this.passiveMovement.Y},
            };

            if (this.pieceStorage != null) pieceData["base_pieceStorage"] = this.pieceStorage.Serialize();

            this.sprite.Serialize(pieceData);

            return pieceData;
        }

        public virtual void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.mass = (float)pieceData["base_mass"];
            this.hitPoints = (float)pieceData["base_hitPoints"];
            this.showHitPointsTillFrame = (int)pieceData["base_showHitPointsTillFrame"];
            this.bioWear = (float)pieceData["base_bioWear"];
            this.efficiency = (float)pieceData["base_efficiency"];
            this.activeState = (State)pieceData["base_activeState"];
            this.maxAge = (int)pieceData["base_maxAge"];
            this.pieceStorage = PieceStorage.Deserialize(storageData: pieceData["base_pieceStorage"], world: this.world, storagePiece: this);
            this.boardAction = (Scheduler.ActionName)pieceData["base_boardAction"];
            this.toolbarAction = (Scheduler.ActionName)pieceData["base_toolbarAction"];
            this.passiveMovement = new Vector2((float)pieceData["base_passiveMovementX"], (float)pieceData["base_passiveMovementY"]);

            this.sprite.Deserialize(pieceData);

            if (!(bool)pieceData["base_alive"]) this.Kill();
        }

        public virtual void DrawStatBar()
        {
            int posX = this.sprite.gfxRect.Center.X;
            int posY = this.sprite.gfxRect.Bottom;

            new StatBar(label: "", value: (int)this.hitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY);

            StatBar.FinishThisBatch();
        }

        public void AddToPieceCount()
        {
            this.world.pieceCountByName[this.name]++;
            if (!this.world.pieceCountByClass.ContainsKey(this.GetType())) this.world.pieceCountByClass[this.GetType()] = 0;
            this.world.pieceCountByClass[this.GetType()]++;
        }

        public void RemoveFromPieceCount()
        {
            this.world.pieceCountByName[this.name]--;
            this.world.pieceCountByClass[this.GetType()]--;
        }

        public void GrowOlder()
        {
            this.currentAge++;
            this.efficiency = Math.Max(1 - (this.currentAge / (float)this.maxAge) - this.bioWear, 0);
        }

        private void SetSpriteSizeByMass()
        { this.sprite.AssignNewSize(SpriteSize); }

        public virtual void Kill()
        {
            if (!this.alive) return;

            this.alive = false;
            if (this.pieceStorage != null) this.pieceStorage.DropAllPiecesToTheGround(addMovement: true);
            this.RemoveFromStateMachines();
            this.sprite.Kill();
            new PlannedDestruction(world: this.world, delay: this.staysAfterDeath, boardPiece: this);
        }

        public void Destroy()
        {
            if (!this.exists) return;
            if (this.alive) this.Kill();

            this.sprite.Destroy();
            this.exists = false;
            this.RemoveFromPieceCount();
        }


        public void AddToStateMachines()
        {
            if (this.GetType() == typeof(Plant))
            { this.world.grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants); }
            else
            { this.world.grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants); }
        }

        public void RemoveFromStateMachines()
        {
            if (this.GetType() == typeof(Plant))
            {
                this.world.grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants);
            }
            else
            { this.world.grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants); }
        }

        public static BoardPiece FindClosestPiece(Sprite sprite, List<BoardPiece> pieceList, int offsetX = 0, int offsetY = 0)
        {
            Vector2 center = sprite.position;
            if (offsetX != 0 || offsetY != 0) center += new Vector2(offsetX, offsetY);

            try
            {
                pieceList = pieceList.OrderBy(piece => Vector2.Distance(center, piece.sprite.position)).ToList();
                return pieceList[0];
            }
            catch (ArgumentOutOfRangeException)
            { }

            return null;
        }


        public void StateMachineWork()
        {
            if (this.ProcessPassiveMovement()) return; // passive movement blocks the state machine until the movement stops

            if (!this.alive)
            {
                this.RemoveFromStateMachines();
                return;
            }

            switch (this.activeState)
            {
                case State.PlayerControlledWalking:
                    {
                        this.SM_PlayerControlledWalking();
                        return;
                    }

                case State.PlantGrowthAndReproduction:
                    {
                        this.SM_GrowthAndReproduction();
                        return;
                    }

                case State.AnimalWalkAround:
                    {
                        this.SM_AnimalWalkAround();
                        return;
                    }

                case State.AnimalAssessSituation:
                    {
                        this.SM_AnimalAssessSituation();
                        return;
                    }

                case State.AnimalRest:
                    {
                        this.SM_AnimalRest();
                        return;
                    }

                case State.AnimalChaseTarget:
                    {
                        this.SM_AnimalChaseTarget();
                        return;
                    }

                case State.AnimalAttack:
                    {
                        this.SM_AnimalAttack();
                        return;
                    }

                case State.AnimalEat:
                    {
                        this.SM_AnimalEat();
                        return;
                    }

                case State.AnimalMate:
                    {
                        this.SM_AnimalMate();
                        return;
                    }

                case State.AnimalGiveBirth:
                    {
                        this.SM_AnimalGiveBirth();
                        return;
                    }

                case State.AnimalFlee:
                    {
                        this.SM_AnimalFlee();
                        return;
                    }

                case State.Empty: // this state should be removed from execution (for performance reasons)
                    {
                        this.RemoveFromStateMachines();
                        return;
                    }
                default:
                    { throw new DivideByZeroException($"Unsupported state - {this.activeState}."); }
            }
        }

        public void AddPassiveMovement(Vector2 movement)
        {
            // activeState should not be changed ("empty" will be removed from state machines, other states will run after the movement stops)
            this.passiveMovement += movement;
            this.AddToStateMachines();
        }

        public bool ProcessPassiveMovement()
        {
            if (this.passiveMovement == Vector2.Zero) return false;
            if (this.passiveMovement.X < (passiveMovementMultiplier / 2f) && this.passiveMovement.Y < (passiveMovementMultiplier / 2f))
            {
                this.passiveMovement = Vector2.Zero;
                return false;
            }

            if (this.sprite.Move(movement: this.passiveMovement / passiveMovementMultiplier))
            { this.passiveMovement *= 0.94f; }
            else
            {
                this.passiveMovement *= -0.7f;// bounce off the obstacle and slow down a little
            }
            return true;
        }



        public virtual void SM_PlayerControlledWalking()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_GrowthAndReproduction()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalWalkAround()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalAssessSituation()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalRest()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalChaseTarget()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalAttack()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalEat()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalMate()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalGiveBirth()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalFlee()
        { throw new DivideByZeroException("This method should not be executed."); }


        public bool GoOneStepTowardsGoal(Vector2 goalPosition, float walkSpeed, bool runFrom = false, bool splitXY = false)
        {
            if (this.sprite.position.X == goalPosition.X && this.sprite.position.Y == goalPosition.Y)
            {
                this.sprite.CharacterStand();
                return false;
            }

            float realSpeed = walkSpeed;
            if (this.sprite.IsInWater) realSpeed = Math.Max(1, this.speed * 0.75f);

            Vector2 positionDifference = goalPosition - this.sprite.position;

            if (runFrom) positionDifference = new Vector2(-positionDifference.X, -positionDifference.Y);

            // calculating "raw" direction (not taking speed into account)
            Vector2 movement = new Vector2(
                Math.Max(Math.Min(positionDifference.X, 1), -1),
                 Math.Max(Math.Min(positionDifference.Y, 1), -1));

            movement = new Vector2(movement.X * realSpeed, movement.Y * realSpeed);

            // trying not to "overshoot" the goal
            if (Math.Abs(positionDifference.X) < Math.Abs(movement.X)) movement.X = positionDifference.X;
            if (Math.Abs(positionDifference.Y) < Math.Abs(movement.Y)) movement.Y = positionDifference.Y;

            if (Math.Abs(movement.X) > Math.Abs(movement.Y))
            { this.sprite.orientation = (movement.X < 0) ? Sprite.Orientation.left : Sprite.Orientation.right; }
            else
            { this.sprite.orientation = (movement.Y < 0) ? Sprite.Orientation.up : Sprite.Orientation.down; }

            bool hasBeenMoved = this.sprite.Move(movement);

            if (splitXY && !hasBeenMoved && movement.X != 0 && movement.Y != 0)
            {
                Vector2[] splitMoves = { new Vector2(0, movement.Y), new Vector2(movement.X, 0) };
                foreach (Vector2 splitMove in splitMoves)
                {
                    if (!(splitMove.X == 0 && splitMove.Y == 0))
                    {
                        hasBeenMoved = this.sprite.Move(splitMove);
                        if (hasBeenMoved) break;
                    }
                }
            }

            if (hasBeenMoved)
            { this.sprite.CharacterWalk(); }
            else
            { this.sprite.CharacterStand(); }

            return hasBeenMoved;
        }

    }
}
