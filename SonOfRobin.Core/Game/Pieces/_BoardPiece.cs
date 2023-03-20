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
            PlayerWaitForBuilding,

            PlayerControlledWalking,
            PlayerControlledShooting,
            PlayerControlledSleep,
            PlayerControlledBuilding,
            PlayerControlledGhosting,
            PlayerControlledByCinematic,

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

            FireplaceBurn,

            ScareAnimalsAway,
            PlayAmbientSound,
            MapMarkerShowAndCheck,
            FogMoveRandomly
        }

        public enum Category
        { Wood, Stone, Metal, SmallPlant, Flesh, Dirt, Crystal, Indestructible }

        protected const float passiveMovementMultiplier = 100f;

        public readonly World world;
        public readonly string id;
        public readonly PieceTemplate.Name name;
        public readonly bool female;
        public readonly Category category;
        public Sprite sprite;
        public State activeState;
        public PieceSoundPack soundPack;
        public int lastFrameSMProcessed;
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
        public int strength;
        public int showStatBarsTillFrame;
        public float maxHitPoints;
        public readonly bool indestructible;
        public readonly byte stackSize;
        public PieceStorage PieceStorage { get; protected set; }
        public BuffEngine buffEngine; // active buffs
        public List<Buff> buffList; // buff to be activated when this piece (equip, food, etc.) is used by another piece
        public readonly Yield yield;
        public readonly Yield appearDebris; // yield that is used to make debris when placing this piece
        public Scheduler.TaskName boardTask;
        public Scheduler.TaskName toolbarTask;
        public readonly bool canBePickedUp;
        protected Vector2 passiveMovement;
        protected int passiveRotation;
        public bool rotatesWhenDropped;
        public readonly bool movesWhenDropped;
        private readonly int destructionDelay;
        public BoardPiece visualAid;
        public readonly string readableName;
        public readonly string description;
        public readonly bool serialize;
        public bool canBeHit;
        public bool isTemporaryDecoration;

        public BoardPiece(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, Dictionary<byte, int> maxMassBySize, string readableName, string description, Category category, State activeState,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, bool blocksPlantGrowth = false, bool visible = true, bool ignoresCollisions = false, int destructionDelay = 0, int maxAge = 0, bool floatsOnWater = false, int generation = 0, int mass = 1, int staysAfterDeath = 800, float maxHitPoints = 1, byte stackSize = 1, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, Scheduler.TaskName toolbarTask = Scheduler.TaskName.Empty, bool canBePickedUp = false, Yield yield = null, Yield appearDebris = null, bool indestructible = false, bool rotatesWhenDropped = false, bool movesWhenDropped = true, bool fadeInAnim = false, bool serialize = true, List<Buff> buffList = null, AllowedDensity allowedDensity = null, int strength = 0, LightEngine lightEngine = null, int minDistance = 0, int maxDistance = 100, PieceSoundPack soundPack = null, bool female = false)
        {
            this.world = world;
            this.name = name;
            this.female = female;
            this.category = category;
            this.id = id;

            this.sprite = new Sprite(boardPiece: this, id: this.id, world: this.world, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, blocksPlantGrowth: blocksPlantGrowth, visible: visible, ignoresCollisions: ignoresCollisions, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, fadeInAnim: fadeInAnim, allowedDensity: allowedDensity, lightEngine: lightEngine, minDistance: minDistance, maxDistance: maxDistance);

            this.soundPack = soundPack == null ? new PieceSoundPack() : soundPack;
            this.soundPack.Activate(this);

            this.stackSize = stackSize;
            this.destructionDelay = destructionDelay;
            this.activeState = activeState;
            this.lastFrameSMProcessed = 0;
            this.indestructible = indestructible;
            this.maxHitPoints = maxHitPoints;
            this.hitPoints = maxHitPoints;
            this.showStatBarsTillFrame = 0;
            this.speed = speed;
            this.strength = strength;
            this.maxMassBySize = maxMassBySize;
            this.mass = mass;
            this.startingMass = mass;
            this.staysAfterDeath = staysAfterDeath + Random.Next(0, 300);
            this.generation = generation;
            this.exists = true;
            this.alive = true;
            this.maxAge = maxAge == 0 ? 0 : Random.Next((int)(maxAge * 0.4), (int)(maxAge * 1.6));
            this.currentAge = 0;
            this.bioWear = 0; // 0 - 1 valid range
            this.efficiency = 1; // 0 - 1 valid range
            this.readableName = readableName;
            this.description = description;
            this.PieceStorage = null; // updated in child classes - if needed
            this.buffEngine = new BuffEngine(piece: this);
            this.buffList = buffList == null ? new List<Buff> { } : buffList;
            this.boardTask = boardTask;
            this.toolbarTask = toolbarTask;
            this.passiveMovement = Vector2.Zero;
            this.passiveRotation = 0;
            this.rotatesWhenDropped = rotatesWhenDropped;
            this.movesWhenDropped = movesWhenDropped;
            this.canBePickedUp = canBePickedUp;
            this.serialize = serialize;
            this.yield = yield;
            this.appearDebris = appearDebris;
            if (this.yield == null && Yield.antiCraftRecipes.ContainsKey(this.name)) this.yield = Yield.antiCraftRecipes[this.name].ConvertToYield();
            if (this.yield != null) this.yield.AddPiece(this);
            if (this.appearDebris != null) this.appearDebris.AddPiece(this);
            this.canBeHit = true;
            this.isTemporaryDecoration = false; // to be set later
        }

        public virtual bool ShowStatBars
        { get { return this.world.CurrentUpdate < this.showStatBarsTillFrame; } }

        public bool AreEnemiesNearby
        {
            get
            {
                var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 450, compareWithBottom: true);

                foreach (BoardPiece piece in nearbyPieces)
                {
                    if (piece.GetType() == typeof(Animal) && piece.alive)
                    {
                        Animal animal = (Animal)piece;
                        if (animal.eats.Contains(this.name)) return true;
                    }
                }

                return false;
            }
        }

        public bool IsActiveFireplaceNearby
        {
            get
            {
                var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 450, compareWithBottom: true);

                foreach (BoardPiece piece in nearbyPieces)
                {
                    if (piece.GetType() == typeof(Fireplace))
                    {
                        Fireplace fireplace = (Fireplace)piece;
                        if (fireplace.IsOn) return true;
                    }
                }

                return false;
            }
        }

        public bool IsAnimalOrPlayer
        { get { return this.GetType() == typeof(Animal) || this.GetType() == typeof(Player); } }

        public bool IsPlantOrFruit
        { get { return this.GetType() == typeof(Plant) || this.GetType() == typeof(Fruit); } }

        public bool IsEquippedByPlayer
        {
            get
            {
                if (this.world.Player == null) return false;
                return this.world.Player.EquipStorage.ContainsThisPieceID(this.id);
            }
        }

        public bool IsOnPlayersToolbar
        {
            get
            {
                if (this.world.Player == null) return false;
                return this.world.Player.ToolStorage.ContainsThisPieceID(this.id);
            }
        }

        public float Mass
        {
            get { return this.mass; }
            set
            {
                this.mass = value;
                int previousSpriteSize = this.sprite.animSize;
                this.SetSpriteSizeByMass();
                if (previousSpriteSize != this.sprite.animSize && this.PieceStorage != null && this.GetType() == typeof(Plant))
                {
                    Plant plant = (Plant)this;
                    plant.fruitEngine.SetAllFruitPosAgain();
                }
            }
        }

        public float HitPointsPercent
        { get { return this.hitPoints / this.maxHitPoints; } }

        private byte SpriteSize
        {
            get
            {
                if (this.maxMassBySize == null) return 0;

                foreach (var kvp in this.maxMassBySize)
                {
                    if (this.Mass < kvp.Value) return kvp.Key;
                }
                return 0;
            }
        }

        public static Random Random
        {
            get
            {
                World world = World.GetTopWorld();
                return world == null ? SonOfRobinGame.random : world.random;
            }
        }

        public bool PlaceOnBoard(bool randomPlacement, Vector2 position, bool addPlannedDestruction = true, bool addToStateMachines = true, bool ignoreCollisions = false, bool precisePlacement = false, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false)
        {
            if (!this.sprite.PlaceOnBoard(randomPlacement: randomPlacement, position: position, ignoreCollisions: ignoreCollisions, precisePlacement: precisePlacement, closestFreeSpot: closestFreeSpot, minDistanceOverride: minDistanceOverride, maxDistanceOverride: maxDistanceOverride, ignoreDensity: ignoreDensity)) return false;

            if (addPlannedDestruction) this.AddPlannedDestruction();
            if (addToStateMachines) this.AddToStateMachines();
            this.AddToPieceCount();

            return true;
        }

        public void RemoveFromBoard()
        {
            if (!this.sprite.IsOnBoard) return;

            this.sprite.RemoveFromBoard();
            WorldEvent.RemovePieceFromQueue(world: this.world, pieceToRemove: this);
            Tracking.RemoveFromTrackingQueue(world: this.world, pieceToRemove: this);
            this.RemoveFromPieceCount();
        }

        public void AddPlannedDestruction()
        {
            if (this.destructionDelay == 0) return;

            // duration value "-1" should be replaced with animation duration
            new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: this.destructionDelay == -1 ? this.sprite.GetAnimDuration() - 1 : this.destructionDelay, boardPiece: this);
        }

        public virtual Dictionary<string, Object> Serialize()
        {
            var pieceData = new Dictionary<string, Object>
            {
                { "base_id", this.id },
                { "base_female", this.female },
                { "base_name", this.name },
                { "base_speed", this.speed },
                { "base_hitPoints", this.hitPoints },
                { "base_strength", this.strength },
                { "base_maxHitPoints", this.maxHitPoints },
                { "base_showStatBarsTillFrame", this.showStatBarsTillFrame },
                { "base_mass", this.mass },
                { "base_alive", this.alive },
                { "base_maxAge", this.maxAge },
                { "base_bioWear", this.bioWear },
                { "base_efficiency", this.efficiency },
                { "base_activeState", this.activeState },
                { "base_pieceStorage", this.PieceStorage },
                { "base_boardTask", this.boardTask },
                { "base_toolbarTask", this.toolbarTask },
                { "base_passiveMovementX", this.passiveMovement.X },
                { "base_passiveMovementY", this.passiveMovement.Y },
                { "base_passiveRotation", this.passiveRotation },
                { "base_buffEngine", this.buffEngine.Serialize() },
                { "base_buffList", this.buffList },
                { "base_soundPack", this.soundPack.Serialize() },
                { "base_canBeHit", this.canBeHit },
            };

            if (this.PieceStorage != null) pieceData["base_pieceStorage"] = this.PieceStorage.Serialize();
            this.sprite.Serialize(pieceData);

            return pieceData;
        }

        public virtual void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.mass = (float)pieceData["base_mass"];
            this.hitPoints = (float)pieceData["base_hitPoints"];
            this.speed = (float)pieceData["base_speed"];
            this.strength = (int)pieceData["base_strength"];
            this.maxHitPoints = (float)pieceData["base_maxHitPoints"];
            this.showStatBarsTillFrame = (int)pieceData["base_showStatBarsTillFrame"];
            this.bioWear = (float)pieceData["base_bioWear"];
            this.efficiency = (float)pieceData["base_efficiency"];
            this.activeState = (State)pieceData["base_activeState"];
            this.maxAge = (int)pieceData["base_maxAge"];
            this.PieceStorage = PieceStorage.Deserialize(storageData: pieceData["base_pieceStorage"], world: this.world, storagePiece: this);
            this.boardTask = (Scheduler.TaskName)pieceData["base_boardTask"];
            this.toolbarTask = (Scheduler.TaskName)pieceData["base_toolbarTask"];
            this.passiveMovement = new Vector2((float)pieceData["base_passiveMovementX"], (float)pieceData["base_passiveMovementY"]);
            this.passiveRotation = (int)pieceData["base_passiveRotation"];
            this.buffEngine = BuffEngine.Deserialize(piece: this, buffEngineData: pieceData["base_buffEngine"]);
            this.buffList = (List<Buff>)pieceData["base_buffList"];
            this.soundPack.Deserialize(pieceData["base_soundPack"]);
            this.canBeHit = (bool)pieceData["base_canBeHit"];

            this.sprite.Deserialize(pieceData);

            if (!(bool)pieceData["base_alive"]) this.Kill();
        }

        public virtual void DrawStatBar()
        {
            new StatBar(label: "", value: (int)this.hitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: true, texture: AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

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

        private void SetSpriteSizeByMass(bool growOnly = false)
        {
            byte newSpriteSize = this.SpriteSize;
            if (growOnly && this.sprite.animSize > newSpriteSize) return;
            this.sprite.AssignNewSize(this.SpriteSize);
        }

        public virtual void Kill(bool addDestroyEvent = true)
        {
            if (!this.alive) return;

            this.soundPack.Play(PieceSoundPack.Action.Die);

            if (this.IsAnimalOrPlayer)
            {
                this.rotatesWhenDropped = true; // so it can be tossed around with rotation
                this.sprite.rotation = (float)(Random.NextDouble() * Math.PI);
            }
            if (this.visualAid != null) this.visualAid.Destroy();
            this.alive = false;
            if (this.PieceStorage != null) this.PieceStorage.DropAllPiecesToTheGround(addMovement: true);
            this.RemoveFromStateMachines();
            this.sprite.Kill();
            if (addDestroyEvent) new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: this.staysAfterDeath, boardPiece: this);
        }

        public void Destroy()
        {
            if (!this.exists) return;
            if (this.alive) this.Kill(addDestroyEvent: false);
            if (PieceInfo.IsPlayer(this.name)) this.yield.DropFinalPieces();
            this.soundPack.StopAll(ignoredAction: PieceSoundPack.Action.IsDestroyed);
            this.RemoveFromBoard();
            if (this.visualAid != null) this.visualAid.Destroy();
            this.exists = false;
        }

        public void AddToStateMachines()
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} adding to state machines - '{this.readableName}'.");

            if (this.GetType() == typeof(Plant)) this.world.Grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants);
            else this.world.Grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants);
        }

        public void RemoveFromStateMachines()
        {
            //  MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} removing from state machines - '{this.readableName}'.");

            if (this.GetType() == typeof(Plant)) this.world.Grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants);
            else this.world.Grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants);
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
            // checking if state machine can be processed

            if (this.lastFrameSMProcessed == this.world.CurrentUpdate) return; // to avoid processing the same state machine multiple times in one frame
            if (!this.exists || !this.sprite.IsOnBoard)
            {
                this.RemoveFromStateMachines();
                return;
            }

            // processing state machine

            this.lastFrameSMProcessed = this.world.CurrentUpdate;

            if (this.ProcessPassiveMovement()) return; // passive movement blocks the state machine until the movement stops

            if (!this.world.stateMachineTypesManager.CanBeProcessed(this)) return;

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

                case State.PlayerControlledShooting:
                    {
                        this.SM_PlayerControlledShooting();
                        return;
                    }

                case State.PlayerControlledSleep:
                    {
                        this.SM_PlayerControlledSleep();
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

                case State.PlayerControlledBuilding:
                    {
                        this.SM_PlayerControlledBuilding();
                        return;
                    }

                case State.PlayerWaitForBuilding:
                    {
                        this.SM_PlayerWaitForBuilding();
                        return;
                    }

                case State.PlayerControlledGhosting:
                    {
                        this.SM_PlayerControlledGhosting();
                        return;
                    }

                case State.PlayerControlledByCinematic:
                    {
                        this.SM_PlayerControlledByCinematic();
                        return;
                    }

                case State.FireplaceBurn:
                    {
                        this.SM_FireplaceBurn();
                        return;
                    }

                case State.ScareAnimalsAway:
                    {
                        this.SM_ScarePredatorsAway();
                        return;
                    }

                case State.PlayAmbientSound:
                    {
                        this.SM_PlayAmbientSound();
                        return;
                    }

                case State.MapMarkerShowAndCheck:
                    {
                        this.SM_MapMarkerShowAndCheck();
                        return;
                    }

                case State.FogMoveRandomly:
                    {
                        this.SM_FogMoveRandomly();
                        return;
                    }

                case State.Empty: // this state should be removed from execution (for performance reasons)
                    {
                        this.RemoveFromStateMachines();
                        return;
                    }
                default:
                    { throw new ArgumentException($"Unsupported state - {this.activeState}."); }
            }
        }

        public void AddPassiveMovement(Vector2 movement)
        {
            if (!this.movesWhenDropped) return;

            // activeState should not be changed ("empty" will be removed from state machines, other states will run after the movement stops)
            this.passiveMovement += movement;

            int maxRotation = (int)(Math.Max(Math.Abs(movement.X), Math.Abs(movement.Y)) * 1);
            maxRotation = Math.Min(maxRotation, 120);

            if (this.rotatesWhenDropped) this.passiveRotation = Random.Next(-maxRotation, maxRotation);
            this.AddToStateMachines();
        }

        public virtual bool ProcessPassiveMovement()
        {
            if (this.passiveMovement == Vector2.Zero && this.passiveRotation == 0) return false;

            //  MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} processing passive movement for {this.readableName}.");

            if (Math.Abs(this.passiveMovement.X) < (passiveMovementMultiplier / 2f) && Math.Abs(this.passiveMovement.Y) < (passiveMovementMultiplier / 2f))
            {
                this.soundPack.Play(PieceSoundPack.Action.IsDropped);

                this.passiveMovement = Vector2.Zero;
                this.passiveRotation = 0;
                if (this.sprite.IsInWater) this.sprite.rotation = 0f; // rotated sprite cannot show submerge

                return false;
            }

            if (this.sprite.Move(movement: this.passiveMovement / passiveMovementMultiplier))
            {
                this.passiveMovement *= 0.94f;
                this.sprite.rotation += this.passiveRotation / 300f;
            }
            else
            {
                this.soundPack.Play(PieceSoundPack.Action.IsDropped);

                this.passiveMovement *= -0.7f;// bounce off the obstacle and slow down a little
                this.passiveRotation *= -1;
            }
            return true;
        }

        public bool GoOneStepTowardsGoal(Vector2 goalPosition, float walkSpeed, bool runFrom = false, bool splitXY = false, bool setOrientation = true, bool slowDownInWater = true)
        {
            if (this.sprite.position == goalPosition)
            {
                this.sprite.CharacterStand();
                return false;
            }

            float realSpeed = walkSpeed;
            if (this.sprite.IsInWater && slowDownInWater) realSpeed = Math.Max(1, walkSpeed * 0.75f);

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

            if (setOrientation) this.sprite.SetOrientationByMovement(movement);
            bool hasBeenMoved = this.sprite.Move(movement);

            if (!hasBeenMoved && splitXY && movement != Vector2.Zero)
            {
                Vector2[] splitMoves = { new Vector2(0, movement.Y), new Vector2(movement.X, 0) };
                foreach (Vector2 splitMove in splitMoves)
                {
                    if (splitMove != Vector2.Zero)
                    {
                        if (this.sprite.Move(splitMove))
                        {
                            hasBeenMoved = true;
                            break;
                        }
                    }
                }
            }

            if (hasBeenMoved)
            {
                this.sprite.CharacterWalk();
                this.soundPack.Play(action: this.sprite.WalkSoundAction);
                if (this.sprite.IsInCameraRect) this.world.swayManager.MakeSmallPlantsReactToStep(this.sprite);
            }
            else this.sprite.CharacterStand();

            return hasBeenMoved;
        }

        public virtual void SM_PlayerWaitForBuilding()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledWalking()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledShooting()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledSleep()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledBuilding()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledGhosting()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayerControlledByCinematic()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_FireplaceBurn()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_ScarePredatorsAway()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayAmbientSound()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_FogMoveRandomly()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_MapMarkerShowAndCheck()
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
    }
}