using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public abstract class BoardPiece
    {
        public enum State : byte
        {
            Empty = 0,
            PlayerWaitForBuilding = 1,

            PlayerControlledWalking = 2,
            PlayerControlledShooting = 3,
            PlayerControlledSleep = 4,
            PlayerControlledBuilding = 5,
            PlayerControlledGhosting = 6,
            PlayerControlledByCinematic = 7,

            PlantGrowthAndReproduction = 8,

            AnimalWalkAround = 9,
            AnimalAssessSituation = 10,
            AnimalRest = 11,
            AnimalChaseTarget = 12,
            AnimalAttack = 13,
            AnimalEat = 14,
            AnimalMate = 15,
            AnimalGiveBirth = 16,
            AnimalFlee = 17,

            FireplaceBurn = 18,
            DryMeat = 30,
            FlameBurn = 25,

            ScareAnimalsAway = 19,
            PlayAmbientSound = 20,
            MapMarkerShowAndCheck = 21,
            FogMoveRandomly = 22,
            WeatherFogMoveRandomly = 23,

            AnimalRunForClosestWater = 26,
            AnimalCallForHelp = 27,
            SeaWaveMove = 28,
            EmitParticles = 29,
            HasteCloneFollowPlayer = 31,
            CaveEntranceDisappear = 32,
            EndingBoatCruise = 33,
            OscillateAroundTarget = 34,
        }

        public static readonly Category[] allCategories = (Category[])Enum.GetValues(typeof(Category));

        public enum Category
        {
            NotSet,
            Wood,
            Stone,
            Metal,
            SmallPlant,
            Flesh,
            Leather,
            Dirt,
            Crystal,
            Indestructible,
        }

        public static ImageObj GetTextureForCategory(Category category)
        {
            return category switch
            {
                Category.Wood => PieceInfo.GetImageObj(PieceTemplate.Name.TreeBig),
                Category.Stone => AnimData.GetImageObj(AnimData.PkgName.MineralsSmall3),
                Category.Metal => PieceInfo.GetImageObj(PieceTemplate.Name.Anvil),
                Category.SmallPlant => PieceInfo.GetImageObj(PieceTemplate.Name.GrassRegular),
                Category.Flesh => TextureBank.GetImageObj(textureName: TextureBank.TextureName.Animal),
                Category.Leather => AnimData.GetImageObj(AnimData.PkgName.Leather),
                Category.Dirt => PieceInfo.GetImageObj(PieceTemplate.Name.Hole),
                Category.Crystal => PieceInfo.GetImageObj(PieceTemplate.Name.CrystalDepositSmall),
                _ => AnimData.GetImageObj(AnimData.PkgName.NoAnim),
            };
        }

        protected const float passiveMovementMultiplier = 100f;
        public const float minBurnVal = 0.5f;

        public readonly World world;
        public Level level;
        public readonly int id;
        public readonly PieceTemplate.Name name;
        public Sprite sprite;
        public State activeState;
        public ActiveSoundPack activeSoundPack;
        public int lastFrameSMProcessed;
        public float speed;
        private float mass;
        public bool exists;
        public bool alive;
        public int maxAge;
        public int currentAge;
        public float bioWear;
        public float efficiency;
        public int strength;
        public int showStatBarsTillFrame;
        public float maxHitPoints;
        public readonly PieceInfo.Info pieceInfo;
        private float heatLevel;
        public virtual PieceStorage PieceStorage { get; protected set; }
        public BuffEngine buffEngine; // active buffs
        public List<Buff> buffList; // buff to be activated when this piece (equip, food, etc.) is used by another piece
        protected int passiveRotation;
        protected Vector2 passiveMovement;
        public bool rotatesWhenDropped;
        public BoardPiece visualAid;
        private BoardPiece flameLight;
        public string readableName;
        public string description;
        public bool canBeHit;
        public bool createdByPlayer;
        public bool isTemporaryDecoration;
        private float hitPoints;

        public BoardPiece(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState,
            byte animSize = 0, string animName = "default", float speed = 1, bool visible = true, int maxAge = 0, float maxHitPoints = 1, bool rotatesWhenDropped = false, List<Buff> buffList = null, int strength = 0, LightEngine lightEngine = null)
        {
            this.world = world;
            this.level = world?.ActiveLevel;
            this.name = name;
            this.id = id;
            this.pieceInfo = PieceInfo.TryToGetInfo(this.name);

            this.sprite = new Sprite(boardPiece: this, id: this.id, world: this.world, animPkgName: animPackage, animSize: animSize, animName: animName, visible: visible, allowedTerrain: allowedTerrain, lightEngine: lightEngine);

            this.activeSoundPack = new ActiveSoundPack(this);
            this.activeState = activeState;
            this.lastFrameSMProcessed = this.world != null ? this.level.stateMachineTypesManager.GetDeltaCounterForType(this.GetType()) : 0;
            this.maxHitPoints = maxHitPoints;
            this.HitPoints = maxHitPoints;
            this.showStatBarsTillFrame = 0;
            this.speed = speed;
            this.strength = strength;
            this.mass = this.pieceInfo != null ? this.pieceInfo.startingMass : 1;
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
            this.passiveMovement = Vector2.Zero;
            this.passiveRotation = 0;
            this.rotatesWhenDropped = rotatesWhenDropped;
            this.canBeHit = true;
            this.createdByPlayer = false;
            this.heatLevel = 0f;
            this.flameLight = null;
            this.isTemporaryDecoration = false; // to be set later
        }

        public bool HasPassiveMovement
        { get { return this.passiveMovement != Vector2.Zero || this.passiveRotation != 0; } }

        public virtual bool ShowStatBars
        { get { return this.world.CurrentUpdate < this.showStatBarsTillFrame; } }

        public float HitPoints
        {
            get { return this.hitPoints; }
            set { this.hitPoints = Math.Clamp(value: value, min: 0, max: this.maxHitPoints); }
        }

        public bool AreEnemiesNearby
        {
            get
            {
                var nearbyPieces = this.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 450, compareWithBottom: true);

                foreach (BoardPiece piece in nearbyPieces)
                {
                    if (piece.GetType() == typeof(Animal) && piece.alive)
                    {
                        Animal animal = (Animal)piece;
                        if (animal.Eats.Contains(this.name)) return true;
                    }
                }

                return false;
            }
        }

        public bool IsActiveFireplaceNearby
        {
            get
            {
                var nearbyPieces = this.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 450, compareWithBottom: true);

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

        public byte StackSize
        { get { return this.pieceInfo == null ? (byte)1 : this.pieceInfo.stackSize; } }

        public int DestructionDelay
        { get { return this.pieceInfo == null ? 0 : this.pieceInfo.destructionDelay; } }

        public bool IsBurning
        { get { return this.heatLevel >= minBurnVal; } }

        public float HeatLevel
        {
            get
            { return this.heatLevel; }
            set
            {
                if (this.buffEngine.HasBuff(BuffEngine.BuffType.HeatLevelLocked)) return;

                float previousHeatLevel = this.heatLevel;

                float valDiff = value - this.heatLevel;
                if (valDiff > 0) valDiff *= this.buffEngine != null && this.buffEngine.HasBuff(BuffEngine.BuffType.Wet) ? this.pieceInfo.fireAffinity / 4 : this.pieceInfo.fireAffinity;

                this.heatLevel += valDiff;
                this.heatLevel = Math.Clamp(value: this.heatLevel, min: 0, max: 1);

                this.sprite.effectCol.RemoveEffectsOfType(effect: SonOfRobinGame.EffectBurn);
                if (this.heatLevel > 0)
                {
                    this.sprite.effectCol.AddEffect(new BurnInstance(intensity: this.heatLevel, boardPiece: this, framesLeft: -1));
                    this.level.heatedPieces.Add(this);
                }

                if (this.IsBurning)
                {
                    if (this.GetType() == typeof(Animal) && this.activeState != State.AnimalRunForClosestWater)
                    {
                        Animal animal = (Animal)this;
                        animal.activeState = State.AnimalRunForClosestWater;
                        animal.aiData.Reset();
                    }

                    this.buffEngine?.RemoveEveryBuffOfType(BuffEngine.BuffType.Wet);

                    if (previousHeatLevel == 0)
                    {
                        int delay = this.world.random.Next(60 * 2, 25 * 60);
                        if (!this.sprite.BlocksMovement || this.world.HeatQueueSize > 100) delay /= 3; // to prevent from large fires

                        // SonOfRobinGame.messageLog.AddMessage(text: $"{SonOfRobinGame.CurrentUpdate} {this.readableName} cool delay {delay}.");
                        new LevelEvent(eventName: LevelEvent.EventName.StopBurning, level: this.level, delay: delay, boardPiece: this);
                    }
                }
                else
                {
                    if (this.flameLight != null)
                    {
                        new OpacityFade(sprite: this.flameLight.sprite, destOpacity: 0, duration: 80, destroyPiece: true);
                        this.flameLight = null;
                    }
                }
            }
        }

        public float Mass
        {
            get { return this.mass; }
            set
            {
                this.mass = Math.Max(value, 0f);

                if (this.world == null || !this.sprite.IsInCameraRect || Preferences.debugShowAnimSizeChangeInCamera) // size change should not be visible
                {
                    int previousSpriteSize = this.sprite.AnimSize;
                    this.SetSpriteSizeByMass();

                    if (previousSpriteSize != this.sprite.AnimSize && this.PieceStorage != null && this.GetType() == typeof(Plant))
                    {
                        Plant plant = (Plant)this;
                        plant.fruitEngine.SetAllFruitPosAgain();
                    }
                }
            }
        }

        private void SetSpriteSizeByMass()
        {
            byte newSpriteSize = this.SpriteSize;
            if (this.sprite.AnimSize == newSpriteSize) return;
            if (!this.pieceInfo.canShrink && this.sprite.AnimSize > newSpriteSize) return;

            this.sprite.AssignNewSize(newSpriteSize);
        }

        public float HitPointsPercent
        { get { return this.HitPoints / this.maxHitPoints; } }

        private byte SpriteSize
        {
            get
            {
                int[] maxMassForSize = this.pieceInfo.maxMassForSize;
                if (maxMassForSize == null) return 0;

                for (int size = 0; size < maxMassForSize.Length; size++)
                {
                    if (this.Mass < maxMassForSize[size]) return (byte)size;
                }

                return (byte)maxMassForSize.Length;
            }
        }

        public bool IsPlantMadeByPlayer
        { get { return this.createdByPlayer && this.GetType() == typeof(Plant); } }

        public int FramesSinceLastProcessed
        { get { return Math.Max(this.level.stateMachineTypesManager.GetDeltaCounterForType(this.GetType()) - this.lastFrameSMProcessed, 0); } }

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

            this.RemovePassiveMovement();
            this.sprite.RemoveFromBoard();
            this.RemoveFromPieceCount();
        }

        public void AddPlannedDestruction()
        {
            if (this.DestructionDelay == 0) return;

            // duration value "-1" should be replaced with animation duration
            new LevelEvent(eventName: LevelEvent.EventName.Destruction, level: this.level, delay: this.DestructionDelay == -1 ? this.sprite.Anim.duration - 1 : this.DestructionDelay, boardPiece: this);
        }

        public virtual Dictionary<string, Object> Serialize()
        {
            var pieceData = new Dictionary<string, Object>
            {
                { "base_id", this.id },
                { "base_name", this.name },
                { "base_sprite", this.sprite.Serialize() }
            };

            PieceInfo.Info pieceInfo = this.pieceInfo;

            if (this.maxAge > 0) pieceData["base_maxAge"] = this.maxAge;
            if (pieceInfo.startingMass != this.mass) pieceData["base_mass"] = this.mass;
            if (this.buffEngine.HasAnyBuff) pieceData["base_buffEngine"] = this.buffEngine.Serialize();
            if (pieceInfo.initialActiveState != this.activeState) pieceData["base_activeState"] = this.activeState;
            if (this.PieceStorage != null) pieceData["base_pieceStorage"] = this.PieceStorage.Serialize();
            if (pieceInfo.maxHitPoints != this.maxHitPoints) pieceData["base_maxHitPoints"] = this.maxHitPoints;
            if (pieceInfo.startHitPoints != this.HitPoints) pieceData["base_hitPoints"] = this.HitPoints;
            if (pieceInfo.strength != this.strength) pieceData["base_strength"] = this.strength;
            if (pieceInfo.speed != this.speed) pieceData["base_speed"] = this.speed;
            if (this.heatLevel > 0) pieceData["base_heatLevel"] = this.heatLevel;
            var soundPackSerialized = this.activeSoundPack.Serialize();
            if (soundPackSerialized != null) pieceData["base_activeSoundPack"] = soundPackSerialized;
            if (this.buffList.Count > 0) pieceData["base_buffList"] = this.buffList;
            if (this.canBeHit != pieceInfo.canBeHit) pieceData["base_canBeHit"] = this.canBeHit;
            if (this.createdByPlayer != pieceInfo.createdByPlayer) pieceData["base_createdByPlayer"] = this.createdByPlayer;
            if (!this.alive) pieceData["base_alive"] = this.alive;
            if (this.bioWear > 0) pieceData["base_bioWear"] = this.bioWear;
            if (this.currentAge > 0) pieceData["base_currentAge"] = this.currentAge;
            if (this.readableName != pieceInfo.readableName) pieceData["base_readableName"] = this.readableName;
            if (this.description != pieceInfo.description) pieceData["base_description"] = this.description;
            if (this.showStatBarsTillFrame > this.world.CurrentUpdate) pieceData["base_showStatBarsTillFrame"] = this.showStatBarsTillFrame;

            return pieceData;
        }

        public virtual void Deserialize(Dictionary<string, Object> pieceData)
        {
            if (pieceData.ContainsKey("base_mass")) this.mass = (float)(double)pieceData["base_mass"];
            if (pieceData.ContainsKey("base_maxHitPoints")) this.maxHitPoints = (float)(double)pieceData["base_maxHitPoints"]; // must be deserialized before base_hitPoints
            if (pieceData.ContainsKey("base_hitPoints")) this.HitPoints = (float)(double)pieceData["base_hitPoints"];
            if (pieceData.ContainsKey("base_speed")) this.speed = (float)(double)pieceData["base_speed"];
            if (pieceData.ContainsKey("base_strength")) this.strength = (int)(Int64)pieceData["base_strength"];
            if (pieceData.ContainsKey("base_bioWear")) this.bioWear = (float)(double)pieceData["base_bioWear"];
            if (pieceData.ContainsKey("base_currentAge")) this.currentAge = (int)(Int64)pieceData["base_currentAge"];
            if (pieceData.ContainsKey("base_activeState")) this.activeState = (State)(Int64)pieceData["base_activeState"];
            if (pieceData.ContainsKey("base_maxAge")) this.maxAge = (int)(Int64)pieceData["base_maxAge"];
            if (pieceData.ContainsKey("base_pieceStorage")) this.PieceStorage = PieceStorage.Deserialize(storageData: pieceData["base_pieceStorage"], storagePiece: this);
            if (pieceData.ContainsKey("base_buffEngine")) this.buffEngine = BuffEngine.Deserialize(piece: this, buffEngineData: pieceData["base_buffEngine"]);
            if (pieceData.ContainsKey("base_buffList")) this.buffList = (List<Buff>)pieceData["base_buffList"];
            if (pieceData.ContainsKey("base_activeSoundPack")) this.activeSoundPack.Deserialize(pieceData["base_activeSoundPack"]);
            if (pieceData.ContainsKey("base_canBeHit")) this.canBeHit = (bool)pieceData["base_canBeHit"];
            if (pieceData.ContainsKey("base_createdByPlayer")) this.createdByPlayer = (bool)pieceData["base_createdByPlayer"];
            if (pieceData.ContainsKey("base_heatLevel")) this.heatLevel = (float)(double)pieceData["base_heatLevel"];
            if (pieceData.ContainsKey("base_readableName")) this.readableName = (string)pieceData["base_readableName"];
            if (pieceData.ContainsKey("base_description")) this.description = (string)pieceData["base_description"];
            if (pieceData.ContainsKey("base_showStatBarsTillFrame")) this.showStatBarsTillFrame = (int)(Int64)pieceData["base_showStatBarsTillFrame"];
            this.sprite.Deserialize(pieceData["base_sprite"]);

            this.UpdateEfficiency();

            // Any code that changes AnimFrame or position should be avoided here.
            // All boardPieces must be created and deserialized first, before running these kind of operations.

            if (pieceData.ContainsKey("base_alive") && !(bool)pieceData["base_alive"])
            {
                // Kill() should not be used here, because it may cause collision errors
                this.alive = false;
                this.RemoveFromStateMachines();
            }

            this.world.piecesByIDForDeserialization[(int)(Int64)pieceData["base_id"]] = this;
        }

        public virtual void DrawStatBar()
        {
            new StatBar(label: "", value: (int)this.HitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: true, image: AnimData.GetImageObj(AnimData.PkgName.Heart));

            if (Preferences.debugShowStatBars && this.HeatLevel > 0) new StatBar(label: "", value: (int)(this.HeatLevel * 100f), valueMax: 100, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, image: AnimData.GetImageObj(AnimData.PkgName.Flame));

            StatBar.FinishThisBatch();
        }

        public void AddToPieceCount()
        {
            this.level.pieceCountByName[this.name]++;
            if (!this.level.pieceCountByClass.ContainsKey(this.GetType())) this.level.pieceCountByClass[this.GetType()] = 0;
            this.level.pieceCountByClass[this.GetType()]++;
        }

        public void RemoveFromPieceCount()
        {
            this.level.pieceCountByName[this.name]--;
            this.level.pieceCountByClass[this.GetType()]--;
        }

        public void GrowOlder(int timeDelta)
        {
            this.currentAge += timeDelta;
            this.UpdateEfficiency();
        }

        private void UpdateEfficiency()
        {
            this.efficiency = Math.Max(1 - (this.currentAge / (float)this.maxAge) - this.bioWear, 0);
        }

        public virtual void Kill(bool addDestroyEvent = true)
        {
            if (!this.alive) return;

            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Die);

            if (this.GetType() == typeof(Animal))
            {
                this.sprite.CharacterStand();
                this.sprite.rotation = (float)(Random.NextSingle() * Math.PI);
            }

            if (this.IsAnimalOrPlayer) this.rotatesWhenDropped = true; // so it can be tossed around with rotation

            if (this.visualAid != null) this.visualAid.Destroy();
            this.alive = false;
            // fruits should be destroyed, not dropped
            if (this.PieceStorage != null && this.GetType() != typeof(Plant)) this.PieceStorage.DropAllPiecesToTheGround(addMovement: true);
            this.activeState = State.Empty; // RemoveFromStateMachines() should not be called, because then passive movement would not be calculated properly
            this.sprite.Kill();
            if (addDestroyEvent) new LevelEvent(eventName: LevelEvent.EventName.Destruction, level: this.level, delay: this.pieceInfo.staysAfterDeath, boardPiece: this);
        }

        public virtual void Destroy()
        {
            if (!this.exists) return;
            if (this.alive) this.Kill(addDestroyEvent: false);
            this.activeSoundPack.StopAll(ignoredAction: PieceSoundPackTemplate.Action.IsDestroyed);
            this.sprite.Destroy();
            this.RemoveFromBoard();
            if (this.visualAid != null) this.visualAid.Destroy();
            if (this.flameLight != null) new OpacityFade(sprite: this.flameLight.sprite, destOpacity: 0, duration: 30, destroyPiece: true);
            this.exists = false;
        }

        public void AddToStateMachines()
        {
            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} adding to state machines - '{this.readableName}'.");

            if (this.GetType() == typeof(Plant)) Grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants);
            else Grid.AddToGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants);
        }

        public void RemoveFromStateMachines()
        {
            //  SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} removing from state machines - '{this.readableName}'.");

            if (this.GetType() == typeof(Plant)) Grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesPlants);
            else Grid.RemoveFromGroup(sprite: this.sprite, groupName: Cell.Group.StateMachinesNonPlants);
        }

        public static BoardPiece FindClosestPiece(Sprite sprite, List<BoardPiece> pieceList, int offsetX = 0, int offsetY = 0)
        {
            Vector2 center = sprite.position;
            if (offsetX != 0 || offsetY != 0) center += new Vector2(offsetX, offsetY);

            try
            {
                return pieceList.OrderBy(piece => Vector2.Distance(center, piece.sprite.position)).First();
            }
            catch (InvalidOperationException)
            { }

            return null;
        }

        public static BoardPiece FindClosestPiece(Sprite sprite, IEnumerable<BoardPiece> pieceList, int offsetX = 0, int offsetY = 0)
        {
            Vector2 center = sprite.position;
            if (offsetX != 0 || offsetY != 0) center += new Vector2(offsetX, offsetY);

            try
            {
                return pieceList.OrderBy(piece => Vector2.Distance(center, piece.sprite.position)).First();
            }
            catch (InvalidOperationException)
            { }

            return null;
        }

        public void DestroyContainedPlants(int delay)
        {
            if (!this.sprite.IsOnBoard) return;

            var collidingPlants = this.sprite.GetCollidingSprites(new List<Cell.Group> { Cell.Group.Visible }).Where(sprite => sprite.boardPiece.GetType() == typeof(Plant) && this.sprite.ColRect.Contains(sprite.position));

            Scheduler.ExecutionDelegate destroyAndDropDebrisDlgt = () =>
            {
                foreach (Sprite collidingPlant in collidingPlants)
                {
                    BoardPiece plantPiece = collidingPlant.boardPiece;
                    plantPiece.pieceInfo.Yield?.DropDebris(piece: plantPiece);
                    plantPiece.Destroy();
                }
            };

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: delay, executeHelper: destroyAndDropDebrisDlgt);
        }

        public void StateMachineWork()
        {
            // checking if state machine can be processed

            if (this.FramesSinceLastProcessed == 0) return; // to avoid processing the same state machine multiple times in one frame
            if (!this.exists || !this.sprite.IsOnBoard)
            {
                this.RemoveFromStateMachines();
                return;
            }

            // processing state machine

            bool passiveMovementOccured = this.ProcessPassiveMovement();
            if (passiveMovementOccured) // passive movement blocks the state machine until the movement stops
            {
                this.lastFrameSMProcessed = this.level.stateMachineTypesManager.GetDeltaCounterForType(this.GetType()); // has to be updated here, to prevent from processing passive movement multiple times
                return;
            }
            else
            {
                // resetting passive movement params
                this.passiveMovement = Vector2.Zero;
                this.passiveRotation = 0;
            }

            if (!this.level.stateMachineTypesManager.CanBeProcessed(this)) return;

            if (!this.alive)
            {
                this.RemoveFromStateMachines();
                return;
            }

            switch (this.activeState)
            {
                case State.PlayerControlledWalking:
                    this.SM_PlayerControlledWalking();
                    break;

                case State.PlayerControlledShooting:
                    this.SM_PlayerControlledShooting();
                    break;

                case State.PlayerControlledSleep:
                    this.SM_PlayerControlledSleep();
                    break;

                case State.PlantGrowthAndReproduction:
                    this.SM_GrowthAndReproduction();
                    break;

                case State.AnimalWalkAround:
                    this.SM_AnimalWalkAround();
                    break;

                case State.AnimalAssessSituation:
                    this.SM_AnimalAssessSituation();
                    break;

                case State.AnimalRest:
                    this.SM_AnimalRest();
                    break;

                case State.AnimalChaseTarget:
                    this.SM_AnimalChaseTarget();
                    break;

                case State.AnimalAttack:
                    this.SM_AnimalAttack();
                    break;

                case State.AnimalEat:
                    this.SM_AnimalEat();
                    break;

                case State.AnimalMate:
                    this.SM_AnimalMate();
                    break;

                case State.AnimalGiveBirth:
                    this.SM_AnimalGiveBirth();
                    break;

                case State.AnimalFlee:
                    this.SM_AnimalFlee();
                    break;

                case State.AnimalRunForClosestWater:
                    this.SM_AnimalRunForClosestWater();
                    break;

                case State.AnimalCallForHelp:
                    this.SM_AnimalCallForHelp();
                    break;

                case State.SeaWaveMove:
                    this.SM_SeaWaveMove();
                    break;

                case State.PlayerControlledBuilding:
                    this.SM_PlayerControlledBuilding();
                    break;

                case State.PlayerWaitForBuilding:
                    this.SM_PlayerWaitForBuilding();
                    break;

                case State.PlayerControlledGhosting:
                    this.SM_PlayerControlledGhosting();
                    break;

                case State.PlayerControlledByCinematic:
                    this.SM_PlayerControlledByCinematic();
                    break;

                case State.FireplaceBurn:
                    this.SM_FireplaceBurn();
                    break;

                case State.ScareAnimalsAway:

                    this.SM_ScarePredatorsAway();
                    break;

                case State.PlayAmbientSound:
                    this.SM_PlayAmbientSound();
                    break;

                case State.MapMarkerShowAndCheck:
                    this.SM_MapMarkerShowAndCheck();
                    break;

                case State.FogMoveRandomly:
                    this.SM_FogMoveRandomly();
                    break;

                case State.WeatherFogMoveRandomly:
                    this.SM_WeatherFogMoveRandomly();
                    break;

                case State.FlameBurn:
                    this.SM_FlameBurn();
                    break;

                case State.DryMeat:
                    this.SM_DryMeat();
                    break;

                case State.EmitParticles:
                    this.SM_EmitParticles();
                    break;

                case State.HasteCloneFollowPlayer:
                    this.SM_HasteCloneFollowPlayer();
                    break;

                case State.CaveEntranceDisappear:
                    this.SM_CaveEntranceDisappear();
                    break;

                case State.EndingBoatCruise:
                    this.SM_EndingBoatCruise();
                    break;

                case State.OscillateAroundTarget:
                    this.SM_OscillateAroundTarget();
                    break;

                case State.Empty: // this state should be removed from execution (for performance reasons)
                    this.RemoveFromStateMachines();
                    break;

                default:
                    { throw new ArgumentException($"Unsupported state - {this.activeState}."); }
            }

            this.UpdateLastFrameSMProcessed(); // updated after SM processing, to allow for proper time delta calculation
        }

        public void UpdateLastFrameSMProcessed()
        {
            this.lastFrameSMProcessed = this.level.stateMachineTypesManager.GetDeltaCounterForType(this.GetType());
        }

        public void ResetParamsForNewLevel()
        {
            this.level = this.world.ActiveLevel;
            this.sprite.currentCell = null;
            this.UpdateLastFrameSMProcessed(); // without it, state machine won't get processed correctly after moving to new level (important for passive movement)
        }

        public void ProcessHeat()
        {
            // cooling down

            if (!this.sprite.IsOnBoard)
            {
                this.heatLevel = 0; // changing the value directly
                this.level.heatedPieces.Remove(this);
                return;
            }

            bool isRaining = this.world.weather.IsRaining;

            this.HeatLevel -= isRaining ? 0.012f : 0.0028f;
            if (this.sprite.IsInWater)
            {
                if (this.IsBurning) this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOff); // only when is put out by water
                this.HeatLevel = 0;
            }

            if (this.HeatLevel == 0)
            {
                this.level.heatedPieces.Remove(this);
                return;
            }

            if (!this.IsBurning) return;

            // processing burning

            if (!this.activeSoundPack.IsPlaying(PieceSoundPackTemplate.Action.Burning)) this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Burning);

            // affecting this piece

            this.HeatLevel += isRaining ? 0.0025f : 0.005f;

            float hitPointsToTake = this.GetType() == typeof(Player) ? 0.6f : Math.Max(0.035f, this.maxHitPoints / 900f);
            this.HitPoints -= hitPointsToTake;
            if (this.pieceInfo.blocksMovement) this.showStatBarsTillFrame = this.world.CurrentUpdate + 600;

            bool isInCameraRect = this.sprite.IsInCameraRect;
            if (isInCameraRect)
            {
                int particleFrameDivider = this.pieceInfo.blocksMovement ? 1 : (int)(6f / this.HeatLevel);
                if (this.world.CurrentUpdate % particleFrameDivider == 0)
                {
                    ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.BurnFlame, duration: 1, particlesToEmit: (int)(this.HeatLevel * (this.pieceInfo.blocksMovement ? 3 : 1)));
                    ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.HeatFlame, duration: 1, particlesToEmit: 1);
                }

                if (this.IsAnimalOrPlayer && this.alive && !this.activeSoundPack.IsPlaying(PieceSoundPackTemplate.Action.Cry)) this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Cry);

                if (this.GetType() == typeof(Player))
                {
                    if (!this.world.solidColorManager.AnySolidColorPresent)
                    {
                        this.world.camera.AddRandomShake();
                        this.world.FlashRedOverlay();
                        new RumbleEvent(force: 1f, bigMotor: true, fadeInSeconds: 0.25f, durationSeconds: 0, fadeOutSeconds: 0.25f, minSecondsSinceLastRumbleBigMotor: 0.4f);
                    }
                }
            }

            if (this.HitPoints < 1)
            {
                if (isInCameraRect)
                {
                    bool isAnimal = this.GetType() == typeof(Animal);
                    if (isAnimal) this.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsDestroyed);

                    var debrisTypeList = new List<ParticleEngine.Preset>
                    {
                        isAnimal ? ParticleEngine.Preset.DebrisBlood : ParticleEngine.Preset.DebrisSoot
                    };

                    if (this.sprite.BlocksMovement) debrisTypeList.Add(ParticleEngine.Preset.SmokePuff);

                    this.pieceInfo.Yield?.DropDebris(piece: this, debrisTypeListOverride: debrisTypeList);
                }

                if (this.GetType() == typeof(Player))
                {
                    this.HeatLevel = 0;
                    this.Kill();
                }
                else this.Destroy();
                return;
            }

            // warming up nearby pieces

            float baseBurnVal = Math.Max(this.heatLevel / 100f, 0.035f);
            float baseHitPointsVal = (float)baseBurnVal / 180f;

            Rectangle heatRect = this.sprite.GfxRect;
            heatRect.Inflate(this.sprite.GfxRect.Width * 0.8f, this.sprite.GfxRect.Height * 0.8f);

            var nearbyPieces = this.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 150);
            foreach (BoardPiece heatedPiece in nearbyPieces)
            {
                if (heatedPiece.pieceInfo.fireAffinity == 0 ||
                    !heatRect.Intersects(heatedPiece.sprite.ColRect) ||
                    !heatedPiece.sprite.IsInWater ||
                    heatedPiece == this)
                {
                    continue;
                }

                heatedPiece.HeatLevel += baseBurnVal * 0.27f;

                if (heatedPiece.IsAnimalOrPlayer)
                {
                    if (!heatedPiece.IsBurning) // getting damage before burning
                    {
                        float hitPointsToSubtract = baseHitPointsVal * 0.4f;
                        heatedPiece.HitPoints = Math.Max(heatedPiece.HitPoints - hitPointsToSubtract, 0);

                        if (hitPointsToSubtract > 0.1f && SonOfRobinGame.CurrentUpdate % 15 == 0 && this.world.random.Next(4) == 0)
                        {
                            if (heatedPiece.sprite.IsInCameraRect) heatedPiece.activeSoundPack.Play(PieceSoundPackTemplate.Action.Cry);

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

            // creating and updating flameLight

            if (isInCameraRect)
            {
                if (this.flameLight == null && this.sprite.currentCell.spriteGroups[Cell.Group.LightSource].Count < Preferences.maxFlameLightsPerCell && SonOfRobinGame.fps.FPS >= 45)
                {
                    this.flameLight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.EmptyVisualEffect, closestFreeSpot: true);

                    this.flameLight.sprite.lightEngine = new LightEngine(size: 150, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: false, addedGfxRectMultiplier: 0, width: this.sprite.GfxRect.Width, height: this.sprite.GfxRect.Height);
                    this.flameLight.sprite.lightEngine.AssignSprite(this.flameLight.sprite);
                    this.flameLight.sprite.lightEngine.Activate();

                    new Tracking(level: this.level, targetSprite: this.sprite, followingSprite: flameLight.sprite);

                    this.flameLight.sprite.opacity = 0f;
                    new OpacityFade(sprite: this.flameLight.sprite, destOpacity: 1, duration: 30);
                }

                if (this.flameLight != null)
                {
                    int maxSize = Math.Max(this.sprite.GfxRect.Width, this.sprite.GfxRect.Height) * 3;
                    int minSize = Math.Max(Math.Max(this.sprite.GfxRect.Width, this.sprite.GfxRect.Height), 90);
                    minSize = Math.Min(minSize, maxSize);

                    this.flameLight.sprite.lightEngine.Size = (int)Helpers.ConvertRange(oldMin: 0.5f, oldMax: 1, newMin: minSize, newMax: maxSize, oldVal: this.HeatLevel, clampToEdges: true);
                }
            }
        }

        public void AddPassiveMovement(Vector2 movement, bool force = false)
        {
            if (!this.pieceInfo.movesWhenDropped && !force) return;

            if (this.GetType() == typeof(Player) && this.alive)
            {
                // player moved by wind could lose "connection" to a chest, resulting in a crash
                if (this.activeState == State.PlayerControlledBuilding || this.activeState == State.PlayerWaitForBuilding) return;

                float movementPower = (Math.Abs(movement.X) + Math.Abs(movement.Y)) / 3200f;
                new RumbleEvent(force: Math.Min(movementPower * 0.8f, 1), durationSeconds: movementPower * 2.0f, bigMotor: true, smallMotor: true, fadeInSeconds: 0, fadeOutSeconds: 0.34f);
            }

            // activeState should not be changed ("empty" will be removed from state machines, other states will run after the movement stops)
            this.passiveMovement += movement;

            int maxRotation = (int)(Math.Max(Math.Abs(movement.X), Math.Abs(movement.Y)) * 1);
            maxRotation = Math.Min(maxRotation, 120);

            if (this.rotatesWhenDropped) this.passiveRotation = Random.Next(-maxRotation, maxRotation);
            this.AddToStateMachines();
        }

        public void RemovePassiveMovement()
        {
            this.passiveMovement = Vector2.Zero;
            this.passiveRotation = 0;
        }

        public virtual bool ProcessPassiveMovement()
        {
            if (!this.HasPassiveMovement) return false;

            //  SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} processing passive movement for {this.readableName}.");

            if (Math.Abs(this.passiveMovement.X) < (passiveMovementMultiplier / 2f) && Math.Abs(this.passiveMovement.Y) < (passiveMovementMultiplier / 2f))
            {
                this.PlayPassiveMovementSound();

                this.passiveMovement = Vector2.Zero;
                this.passiveRotation = 0;
                if (this.sprite.IsInWater) this.sprite.rotation = 0f; // rotated sprite cannot show submerge

                return false;
            }

            if (this.sprite.Move(movement: this.passiveMovement / passiveMovementMultiplier, splitXY: false))
            {
                this.passiveMovement *= 0.94f;
                this.sprite.rotation += this.passiveRotation / 300f;
            }
            else
            {
                this.PlayPassiveMovementSound();

                this.passiveMovement *= -0.7f;// bounce off the obstacle and slow down a little
                this.passiveRotation *= -1;
            }
            return true;
        }

        private void PlayPassiveMovementSound()
        {
            if (this.sprite.IsInWater) this.activeSoundPack.Play(PieceSoundPackTemplate.Action.StepWater);
            else this.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsDropped);
        }

        public int CalculateAnimDelayFromMovement(Vector2 movement)
        {
            bool isPlayer = this.GetType() == typeof(Player);
            bool longAnim = this.sprite.Anim.frameArray.Length > 5;

            float distance = Vector2.Distance(Vector2.Zero, movement);

            return (int)Helpers.ConvertRange(oldMin: 0, oldMax: isPlayer ? 3.5 : 2.5, newMin: 0, newMax: longAnim ? 6 : 20, oldVal: distance, clampToEdges: true, reverseVal: true);
        }

        public int CalculateSoundDelayFromMovement(Vector2 movement)
        {
            bool isPlayer = this.GetType() == typeof(Player);
            float distance = Vector2.Distance(Vector2.Zero, movement);
            return (int)Helpers.ConvertRange(oldMin: 0, oldMax: isPlayer ? 3.5 : 2.5, newMin: 0, newMax: 40, oldVal: distance, clampToEdges: true, reverseVal: true);
        }

        public bool GoOneStepTowardsGoal(Vector2 goalPosition, float walkSpeed, bool runFrom = false, bool setOrientation = true, bool slowDownInWater = true, bool slowDownOnRocks = true)
        {
            float targetDistance = Vector2.Distance(this.sprite.position, goalPosition);
            if (targetDistance <= 1f)
            {
                this.sprite.CharacterStand();
                return false;
            }

            float realSpeed = walkSpeed;
            bool isInWater = this.sprite.IsInWater;
            if (slowDownInWater && isInWater) realSpeed = walkSpeed * 0.75f;
            if (slowDownOnRocks && this.sprite.IsOnRocks && !this.buffEngine.HasBuff(BuffEngine.BuffType.FastMountainWalking)) realSpeed *= 0.45f;

            double movementAngle = runFrom ?
                Helpers.GetAngleBetweenTwoPoints(start: goalPosition, end: this.sprite.position) :
                Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: goalPosition);

            realSpeed = Math.Min(realSpeed, targetDistance); // to avoid overshooting the goal

            if (realSpeed < 1f)
            {
                // movement slower than realSpeed == 1 cannot be processed, so realSpeed == 1 will be "flickered" instead
                if (this.world.CurrentUpdate % (int)(11 - (realSpeed * 10)) == 0) realSpeed = 1f;
                else return true; // "flicker" movement should still count as movement
            }

            Vector2 movement = new((float)(realSpeed * Math.Cos(movementAngle)), (float)(realSpeed * Math.Sin(movementAngle)));

            // magnifying small values to 1, while keeping false values (rounding errors bleeding to other axis) at zero
            if (Math.Abs(movement.X) < 0.5f) movement.X = 0;
            else if (Math.Abs(movement.X) > 0.5f && Math.Abs(movement.X) < 1f) movement.X = movement.X < 0 ? -1 : 1;

            if (Math.Abs(movement.Y) < 0.5f) movement.Y = 0;
            else if (Math.Abs(movement.Y) > 0.5f && Math.Abs(movement.Y) < 1f) movement.Y = movement.Y < 0 ? -1 : 1;

            bool hasBeenMoved = this.sprite.Move(movement);
            if (setOrientation) this.sprite.SetOrientationByMovement(movement: movement, orientationAngleOverride: (float)movementAngle);

            if (!hasBeenMoved && movement != Vector2.Zero)
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
                this.sprite.CharacterWalk(playSpeedDelay: this.CalculateAnimDelayFromMovement(movement: movement));

                if (this.sprite.IsInCameraRect)
                {
                    this.activeSoundPack.Play(action: this.sprite.WalkSoundAction, coolDownExtension: this.CalculateSoundDelayFromMovement(movement: movement));
                    this.world.swayManager.MakeSmallPlantsReactToStep(this.sprite);

                    if (isInWater && !this.sprite.IgnoresCollisions)
                    {
                        int particlesToEmit = (int)Helpers.ConvertRange(oldMin: 0, oldMax: Terrain.waterLevelMax, newMin: 0, newMax: 13, oldVal: Terrain.waterLevelMax - this.sprite.GetFieldValue(Terrain.Name.Height), clampToEdges: true);

                        ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterWalk, particlesToEmit: particlesToEmit, duration: 1);
                        ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterDistortWalk, particlesToEmit: particlesToEmit / 2, duration: 1);
                    }

                    if (!isInWater && this.GetType() == typeof(Player) && !this.sprite.IgnoresCollisions && this.sprite.GetExtProperty(name: ExtBoardProps.Name.BiomeSwamp))
                    {
                        ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.MudWalk, particlesToEmit: 2, duration: 1);
                    }
                }
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

        public virtual void SM_EmitParticles()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_HasteCloneFollowPlayer()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_CaveEntranceDisappear()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_EndingBoatCruise()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_OscillateAroundTarget()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_PlayAmbientSound()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_FogMoveRandomly()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_WeatherFogMoveRandomly()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_FlameBurn()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_DryMeat()
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

        public virtual void SM_AnimalRunForClosestWater()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_AnimalCallForHelp()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_SeaWaveMove()
        { throw new DivideByZeroException("This method should not be executed."); }
    }
}