using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            FogMoveRandomly,
            RainInitialize,
            RainFall,
            FlameBurn,

            AnimalRunForClosestWater,
            AnimalCallForHelp,
            SeaWaveMove,
        }

        public static readonly Category[] allCategories = (Category[])Enum.GetValues(typeof(Category));

        public enum Category
        { Wood, Stone, Metal, SmallPlant, Flesh, Dirt, Crystal, Indestructible }

        public static Texture2D GetTextureForCategory(Category category)
        {
            switch (category)
            {
                case Category.Wood:
                    return PieceInfo.GetTexture(PieceTemplate.Name.TreeBig);

                case Category.Stone:
                    return AnimData.framesForPkgs[AnimData.PkgName.MineralsSmall3].texture;

                case Category.Metal:
                    return PieceInfo.GetTexture(PieceTemplate.Name.Anvil);

                case Category.SmallPlant:
                    return PieceInfo.GetTexture(PieceTemplate.Name.GrassRegular);

                case Category.Flesh:
                    return AnimData.framesForPkgs[AnimData.PkgName.AnimalIcon].texture;

                case Category.Dirt:
                    return PieceInfo.GetTexture(PieceTemplate.Name.Hole);

                case Category.Crystal:
                    return PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositSmall);

                default:
                    return AnimData.framesForPkgs[AnimData.PkgName.NoAnim].texture;
            }
        }

        protected const float passiveMovementMultiplier = 100f;

        public readonly World world;
        public readonly string id;
        public readonly PieceTemplate.Name name;
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
        public readonly int[] maxMassForSize;
        private readonly int staysAfterDeath;
        public int maxAge;
        public int currentAge;
        public float bioWear;
        public float efficiency;
        public int strength;
        public int showStatBarsTillFrame;
        public float maxHitPoints;
        public readonly bool indestructible;
        public readonly byte stackSize;
        public readonly float fireAffinity;
        private float burnLevel;
        public virtual PieceStorage PieceStorage { get; protected set; }
        public BuffEngine buffEngine; // active buffs
        public List<Buff> buffList; // buff to be activated when this piece (equip, food, etc.) is used by another piece
        public readonly Yield yield;
        public readonly Yield appearDebris; // yield that is used to make debris when placing this piece
        public readonly Scheduler.TaskName boardTask;
        public readonly Scheduler.TaskName toolbarTask;
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
        private readonly bool canShrink;
        private float hitPoints;

        public BoardPiece(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, string readableName, string description, Category category, State activeState, float fireAffinity,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, bool blocksPlantGrowth = false, bool visible = true, bool ignoresCollisions = false, int destructionDelay = 0, int maxAge = 0, bool floatsOnWater = false, int generation = 0, int mass = 1, int staysAfterDeath = 800, float maxHitPoints = 1, byte stackSize = 1, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, Scheduler.TaskName toolbarTask = Scheduler.TaskName.Empty, bool canBePickedUp = false, Yield yield = null, Yield appearDebris = null, bool indestructible = false, bool rotatesWhenDropped = false, bool movesWhenDropped = true, bool serialize = true, List<Buff> buffList = null, AllowedDensity allowedDensity = null, int strength = 0, LightEngine lightEngine = null, int minDistance = 0, int maxDistance = 100, PieceSoundPack soundPack = null, bool isAffectedByWind = true, bool canShrink = false)
        {
            this.world = world;
            this.name = name;
            this.category = category;
            this.id = id;

            this.sprite = new Sprite(boardPiece: this, id: this.id, world: this.world, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, blocksPlantGrowth: blocksPlantGrowth, visible: visible, ignoresCollisions: ignoresCollisions, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, allowedDensity: allowedDensity, lightEngine: lightEngine, minDistance: minDistance, maxDistance: maxDistance, isAffectedByWind: isAffectedByWind);

            this.soundPack = soundPack == null ? new PieceSoundPack() : soundPack;
            this.soundPack.Activate(this);

            this.stackSize = stackSize;
            this.destructionDelay = destructionDelay;
            this.activeState = activeState;
            this.lastFrameSMProcessed = 0;
            this.indestructible = indestructible;
            this.maxHitPoints = maxHitPoints;
            this.HitPoints = maxHitPoints;
            this.showStatBarsTillFrame = 0;
            this.speed = speed;
            this.strength = strength;
            this.maxMassForSize = maxMassForSize;
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
            this.burnLevel = 0f;
            this.fireAffinity = fireAffinity;
            this.canShrink = canShrink;
            this.isTemporaryDecoration = false; // to be set later
        }

        public virtual bool ShowStatBars
        { get { return this.world.CurrentUpdate < this.showStatBarsTillFrame; } }

        public float HitPoints
        {
            get { return this.hitPoints; }
            set { this.hitPoints = Math.Min(Math.Max(value, 0), this.maxHitPoints); }
        }

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

        public bool IsBurning
        { get { return this.burnLevel >= 0.5f; } }

        public float BurnLevel
        {
            get
            { return this.burnLevel; }
            set
            {
                bool wasBurning = this.IsBurning;

                float valDiff = value - this.burnLevel;
                if (valDiff > 0) valDiff *= this.buffEngine != null && this.buffEngine.HasBuff(BuffEngine.BuffType.Wet) ? this.fireAffinity / 4 : this.fireAffinity;

                this.burnLevel += valDiff;
                this.burnLevel = Math.Max(this.burnLevel, 0);
                this.burnLevel = Math.Min(this.burnLevel, 1);

                if (this.burnLevel > 0) this.world.coolingManager.AddPiece(this);

                this.sprite.effectCol.RemoveEffectsOfType(effect: SonOfRobinGame.EffectBurn);
                if (this.burnLevel > 0) this.sprite.effectCol.AddEffect(new BurnInstance(intensity: this.burnLevel, boardPiece: this, framesLeft: -1));

                bool isBurning = this.IsBurning;

                if (isBurning && this.GetType() == typeof(Animal) && this.activeState != State.AnimalRunForClosestWater)
                {
                    Animal animal = (Animal)this;
                    animal.activeState = State.AnimalRunForClosestWater;
                    animal.aiData.Reset();
                }

                if (isBurning && wasBurning != isBurning)
                {
                    var reallyClosePieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 30, compareWithBottom: true);

                    bool flameFound = false;
                    foreach (BoardPiece piece in reallyClosePieces)
                    {
                        if (piece.name == PieceTemplate.Name.BurningFlame)
                        {
                            flameFound = true;
                            break;
                        }
                    }

                    if (!flameFound)
                    {
                        BoardPiece flame = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.sprite.position, templateName: PieceTemplate.Name.BurningFlame, closestFreeSpot: true);

                        int offsetY = this.IsAnimalOrPlayer ? 2 : this.sprite.gfxRect.Bottom - flame.sprite.gfxRect.Bottom + 2;

                        new Tracking(world: this.world, targetSprite: this.sprite, followingSprite: flame.sprite, offsetY: offsetY);

                        if (this.GetType() == typeof(Player))
                        {
                            flame.sprite.opacityFade.Finish();
                            Sound.QuickPlay(name: SoundData.Name.StartFireBig);
                        }
                    }

                    this.buffEngine?.RemoveEveryBuffOfType(BuffEngine.BuffType.Wet);
                }
            }
        }

        public float Mass
        {
            get { return this.mass; }
            set
            {
                this.mass = Math.Max(value, 0f);

                if (this.world == null || !this.sprite.IsInCameraRect) // size change should not be visible
                {
                    int previousSpriteSize = this.sprite.animSize;
                    this.SetSpriteSizeByMass();

                    if (previousSpriteSize != this.sprite.animSize && this.PieceStorage != null && this.GetType() == typeof(Plant))
                    {
                        Plant plant = (Plant)this;
                        plant.fruitEngine.SetAllFruitPosAgain();
                    }
                }
            }
        }

        public float HitPointsPercent
        { get { return this.HitPoints / this.maxHitPoints; } }

        private byte SpriteSize
        {
            get
            {
                if (this.maxMassForSize == null) return 0;

                for (int s = 0; s < this.maxMassForSize.Length; s++)
                {
                    if (this.Mass < this.maxMassForSize[s]) return (byte)s;
                }

                return (byte)this.maxMassForSize.Length;
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
            this.world.trackingManager.RemoveFromQueue(this);
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
                { "base_name", this.name },
                { "base_sprite", this.sprite.Serialize() }
            };

            PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.name);

            if (this.maxAge > 0) pieceData["base_maxAge"] = this.maxAge;
            if (pieceInfo.mass != this.mass) pieceData["base_mass"] = this.mass;
            if (this.buffEngine.HasAnyBuff) pieceData["base_buffEngine"] = this.buffEngine.Serialize();
            if (pieceInfo.initialActiveState != this.activeState) pieceData["base_activeState"] = this.activeState;
            if (this.PieceStorage != null) pieceData["base_pieceStorage"] = this.PieceStorage.Serialize();
            if (pieceInfo.maxHitPoints != this.maxHitPoints) pieceData["base_maxHitPoints"] = this.maxHitPoints;
            if (pieceInfo.startHitPoints != this.HitPoints) pieceData["base_hitPoints"] = this.HitPoints;
            if (pieceInfo.strength != this.strength) pieceData["base_strength"] = this.strength;
            if (pieceInfo.speed != this.speed) pieceData["base_speed"] = this.speed;
            if (this.burnLevel > 0) pieceData["base_burnLevel"] = this.burnLevel;
            var soundPackSerialized = this.soundPack.Serialize();
            if (soundPackSerialized != null) pieceData["base_soundPack"] = soundPackSerialized;
            if (this.buffList.Any()) pieceData["base_buffList"] = this.buffList;
            if (this.canBeHit != pieceInfo.canBeHit) pieceData["base_canBeHit"] = this.canBeHit;
            if (!this.alive) pieceData["base_alive"] = this.alive;
            if (this.bioWear > 0) pieceData["base_bioWear"] = this.bioWear;
            if (this.currentAge > 0) pieceData["base_currentAge"] = this.currentAge;

            return pieceData;
        }

        public virtual void Deserialize(Dictionary<string, Object> pieceData)
        {
            if (pieceData.ContainsKey("base_mass")) this.mass = (float)(double)pieceData["base_mass"];
            if (pieceData.ContainsKey("base_hitPoints")) this.HitPoints = (float)(double)pieceData["base_hitPoints"];
            if (pieceData.ContainsKey("base_speed")) this.speed = (float)(double)pieceData["base_speed"];
            if (pieceData.ContainsKey("base_strength")) this.strength = (int)(Int64)pieceData["base_strength"];
            if (pieceData.ContainsKey("base_maxHitPoints")) this.maxHitPoints = (float)(double)pieceData["base_maxHitPoints"];
            if (pieceData.ContainsKey("base_bioWear")) this.bioWear = (float)(double)pieceData["base_bioWear"];
            if (pieceData.ContainsKey("base_currentAge")) this.currentAge = (int)(Int64)pieceData["base_currentAge"];
            if (pieceData.ContainsKey("base_activeState")) this.activeState = (State)(Int64)pieceData["base_activeState"];
            if (pieceData.ContainsKey("base_maxAge")) this.maxAge = (int)(Int64)pieceData["base_maxAge"];
            if (pieceData.ContainsKey("base_pieceStorage")) this.PieceStorage = PieceStorage.Deserialize(storageData: pieceData["base_pieceStorage"], storagePiece: this);
            if (pieceData.ContainsKey("base_buffEngine")) this.buffEngine = BuffEngine.Deserialize(piece: this, buffEngineData: pieceData["base_buffEngine"]);
            if (pieceData.ContainsKey("base_buffList")) this.buffList = (List<Buff>)pieceData["base_buffList"];
            if (pieceData.ContainsKey("base_soundPack")) this.soundPack.Deserialize(pieceData["base_soundPack"]);
            if (pieceData.ContainsKey("base_canBeHit")) this.canBeHit = (bool)pieceData["base_canBeHit"];
            if (pieceData.ContainsKey("base_burnLevel")) this.burnLevel = (float)(double)pieceData["base_burnLevel"];
            this.sprite.Deserialize(pieceData["base_sprite"]);

            this.UpdateEfficiency();
            this.SetSpriteSizeByMass();

            if (pieceData.ContainsKey("base_alive") && !(bool)pieceData["base_alive"]) this.Kill();

            this.world.piecesByOldID[(string)pieceData["base_id"]] = this;
        }

        public virtual void DrawStatBar()
        {
            new StatBar(label: "", value: (int)this.HitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: true, texture: AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

            if (Preferences.debugShowStatBars && this.BurnLevel > 0) new StatBar(label: "", value: (int)(this.BurnLevel * 100f), valueMax: 100, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: true, texture: AnimData.framesForPkgs[AnimData.PkgName.Flame].texture);

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

        public void GrowOlder(int timeDelta)
        {
            this.currentAge += timeDelta;
            this.UpdateEfficiency();
        }

        private void UpdateEfficiency()
        {
            this.efficiency = Math.Max(1 - (this.currentAge / (float)this.maxAge) - this.bioWear, 0);
        }

        private void SetSpriteSizeByMass()
        {
            byte newSpriteSize = this.SpriteSize;
            if (!this.canShrink && this.sprite.animSize > newSpriteSize) return;
            this.sprite.AssignNewSize(newSpriteSize);
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
            // fruits should be destroyed, not dropped
            if (this.PieceStorage != null && this.GetType() != typeof(Plant)) this.PieceStorage.DropAllPiecesToTheGround(addMovement: true);
            this.RemoveFromStateMachines();
            this.sprite.Kill();
            if (addDestroyEvent) new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: this.staysAfterDeath, boardPiece: this);
        }

        public virtual void Destroy()
        {
            if (!this.exists) return;
            if (this.alive) this.Kill(addDestroyEvent: false);
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

                case State.AnimalRunForClosestWater:
                    {
                        this.SM_AnimalRunForClosestWater();
                        return;
                    }

                case State.AnimalCallForHelp:
                    {
                        this.SM_AnimalCallForHelp();
                        return;
                    }

                case State.SeaWaveMove:
                    {
                        this.SM_SeaWaveMove();
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

                case State.RainInitialize:
                    {
                        this.SM_RainInitialize();
                        return;
                    }

                case State.RainFall:
                    {
                        this.SM_RainFall();
                        return;
                    }

                case State.FlameBurn:
                    {
                        this.SM_FlameBurn();
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

        public void AddPassiveMovement(Vector2 movement, bool force = false)
        {
            if (!this.movesWhenDropped && !force) return;

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

        public bool GoOneStepTowardsGoal(Vector2 goalPosition, float walkSpeed, bool runFrom = false, bool splitXY = false, bool setOrientation = true, bool slowDownInWater = true, bool slowDownOnRocks = true)
        {
            if (this.sprite.position == goalPosition)
            {
                this.sprite.CharacterStand();
                return false;
            }

            float realSpeed = walkSpeed;
            if (slowDownInWater && this.sprite.IsInWater) realSpeed = Math.Max(1, walkSpeed * 0.75f);
            if (slowDownOnRocks && this.sprite.IsOnRocks) realSpeed = Math.Max(1, walkSpeed * 0.65f);

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

        public virtual void SM_RainInitialize()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_RainFall()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void SM_FlameBurn()
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