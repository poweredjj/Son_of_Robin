using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Player : BoardPiece
    {
        public enum SleepMode : byte
        {
            Awake,
            Sleep,
            WaitMorning,
            WaitIndefinitely,
        };

        public enum SkillName : byte
        {
            Hunter = 0,
            Plunderer = 1,
            Crafter = 2,
            Survivalist = 3,
            Fashionista = 4,
            Maintainer = 5,
            Hoarder = 6,
        }

        public static readonly SkillName[] allSkillNames = (SkillName[])Enum.GetValues(typeof(SkillName));

        public static readonly Dictionary<SkillName, string> skillDescriptions = new()
        {
            { SkillName.Hunter, "harvest meat in the wild" },
            { SkillName.Plunderer, "more item drops" },
            { SkillName.Crafter, "faster, easier crafting" },
            { SkillName.Survivalist, "hunger resistance" },
            { SkillName.Fashionista, "extra accessory slot" },
            { SkillName.Maintainer, "tools last longer" },
            { SkillName.Hoarder, "can carry more items" },
        };

        public static readonly Dictionary<SkillName, PieceTemplate.Name> skillTextures = new()
        {
            { SkillName.Hunter, PieceTemplate.Name.MeatRawPrime },
            { SkillName.Plunderer, PieceTemplate.Name.ChestTreasureBig },
            { SkillName.Crafter, PieceTemplate.Name.WorkshopMaster },
            { SkillName.Survivalist, PieceTemplate.Name.Burger },
            { SkillName.Fashionista, PieceTemplate.Name.GlassesVelvet },
            { SkillName.Maintainer, PieceTemplate.Name.AxeIron },
            { SkillName.Hoarder, PieceTemplate.Name.BackpackMedium },
        };

        private const int maxShootingPower = 90;
        public const int maxLastStepsCount = 100;
        public const int maxCraftLevel = 5;

        public float maxFedLevel;
        public float fedLevel;
        public float maxFatigue;
        private float fatigue;
        public SkillName Skill { get; private set; }
        public int CraftLevel { get; private set; }
        public int CookLevel { get; private set; }
        public int BrewLevel { get; private set; }
        public int HarvestLevel { get; private set; }
        public float ShootingAngle { get; private set; }
        private int shootingPower;
        private SleepEngine sleepEngine;
        public List<Vector2> LastSteps { get; private set; }
        private Vector2 previousStepPos; // used to calculate distanceWalked only
        private float distanceWalked;

        public Vector2 pointWalkTarget;
        public Craft.Recipe recipeToBuild;
        public BoardPiece simulatedPieceToBuild;
        public int buildDurationForOneFrame;
        public float buildFatigueForOneFrame;
        public DateTime SleepStarted { get; private set; }
        public bool sleepingInsideShelter;
        public SleepMode sleepMode;
        public PieceStorage ToolStorage { get; private set; }
        public PieceStorage EquipStorage { get; private set; }
        public PieceStorage GlobalChestStorage { get; private set; } // one storage shared across all crystal chests

        public Player(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState,
            byte animSize = 0, string animName = "default") :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, speed: 3, name: name, allowedTerrain: allowedTerrain, maxHitPoints: 400, readableName: readableName, description: description, strength: 1, activeState: activeState)
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxFatigue = 2000f;
            this.fatigue = 0;
            this.Skill = Preferences.newWorldStartingSkill;
            this.CraftLevel = 1;
            this.CookLevel = 1;
            this.BrewLevel = 1;
            this.HarvestLevel = 1;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.LastSteps = new List<Vector2>();
            this.previousStepPos = new Vector2(-100, -100); // initial value, to be changed later
            this.distanceWalked = 0;
            this.pointWalkTarget = Vector2.Zero;

            var allowedToolbarPieces = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.LanternEmpty }; // indivitual cases, that will not be added below

            if (PieceInfo.HasBeenInitialized)
            {
                var typeList = new List<Type> { typeof(Tool), typeof(PortableLight), typeof(Projectile), typeof(Seed) };

                foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
                {
                    PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                    if (pieceInfo.toolbarTask == Scheduler.TaskName.GetEaten ||
                        pieceInfo.toolbarTask == Scheduler.TaskName.GetDrinked ||
                        typeList.Contains(pieceInfo.type))
                    {
                        bool poisonFound = false;
                        foreach (Buff buff in pieceInfo.buffList)
                        {
                            if (buff.type == BuffEngine.BuffType.RegenPoison && !buff.isPositive)
                            {
                                poisonFound = true;
                                break;
                            }
                        }

                        if (!poisonFound) allowedToolbarPieces.Add(pieceName);
                    }
                }
            }
            else allowedToolbarPieces.Add(PieceTemplate.Name.KnifeSimple);

            this.PieceStorage = new PieceStorage(width: 4, height: this.Skill == SkillName.Hoarder ? (byte)3 : (byte)2, storagePiece: this, storageType: PieceStorage.StorageType.Inventory);
            this.ToolStorage = new PieceStorage(width: 3, height: 1, storagePiece: this, storageType: PieceStorage.StorageType.Tools, allowedPieceNames: allowedToolbarPieces);
            this.EquipStorage = new PieceStorage(width: 3, height: 3, storagePiece: this, storageType: PieceStorage.StorageType.Equip);
            this.GlobalChestStorage = new PieceStorage(width: 8, height: 6, storagePiece: this, storageType: PieceStorage.StorageType.Chest);
            this.ConfigureEquip();
            this.ShootingAngle = -100; // -100 == no real value
            this.shootingPower = 0;
            this.SleepStarted = new DateTime(1900, 1, 1);
            this.sleepingInsideShelter = false;
            this.sleepMode = SleepMode.Awake;
            this.recipeToBuild = null;
            this.simulatedPieceToBuild = null;

            BoardPiece knifeTool = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.KnifeSimple, world: this.world);
            StorageSlot knifeSlot = this.ToolStorage.FindCorrectSlot(knifeTool);
            this.ToolStorage.AddPiece(knifeTool);
            knifeSlot.locked = true;
        }

        public override bool ShowStatBars
        { get { return true; } }

        public float DistanceWalkedKilometers
        { get { return (float)Math.Round(this.distanceWalked / 5000, 2); } }

        private bool ShootingModeInputPressed
        {
            get
            {
                if (InputMapper.IsPressed(InputMapper.Action.WorldUseToolbarPiece)) return true;
                if (!TouchInput.IsBeingTouchedInAnyWay) return false;

                float cameraCorrectionTiltPower = Vector2.Distance(InputMapper.Analog(InputMapper.Action.WorldCameraMove), Vector2.Zero);
                return cameraCorrectionTiltPower > 0.05f;
            }
        }

        public byte InvWidth
        {
            get { return this.PieceStorage.Width; }
            set
            {
                if (this.PieceStorage.Width == value) return;
                this.PieceStorage.Resize(value, this.InvHeight);
            }
        }

        public byte InvHeight
        {
            get { return this.PieceStorage.Height; }
            set
            {
                if (this.PieceStorage.Height == value) return;
                this.PieceStorage.Resize(this.InvWidth, value);
            }
        }

        public byte ToolbarWidth
        {
            get { return this.ToolStorage.Width; }
            set
            {
                if (this.ToolStorage.Width == value) return;
                this.ToolStorage.Resize(value, this.ToolbarHeight);
            }
        }

        public byte ToolbarHeight
        {
            get { return this.ToolStorage.Height; }
            set
            {
                if (this.ToolStorage.Height == value) return;
                this.ToolStorage.Resize(this.ToolbarWidth, value);
            }
        }

        public bool ResourcefulCrafter
        { get { return this.CraftLevel >= 3; } }

        public List<PieceStorage> GetCraftStoragesToTakeFrom(bool showCraftMarker)
        {
            var craftStorages = new List<PieceStorage> { this.PieceStorage, this.ToolStorage, this.EquipStorage };

            if (this.ResourcefulCrafter)
            {
                var chestNames = new List<PieceTemplate.Name> { PieceTemplate.Name.ChestWooden, PieceTemplate.Name.ChestStone, PieceTemplate.Name.ChestIron, PieceTemplate.Name.ChestCrystal };

                var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 90 * this.CraftLevel, compareWithBottom: true);
                var chestPieces = nearbyPieces.Where(piece => piece.GetType() == typeof(Container) && chestNames.Contains(piece.name));

                foreach (BoardPiece chestPiece in chestPieces)
                {
                    if (!craftStorages.Contains(chestPiece.PieceStorage)) craftStorages.Add(chestPiece.PieceStorage); // checking to avoid adding shared chest multiple times
                }

                if (showCraftMarker)
                {
                    foreach (BoardPiece chestPiece in chestPieces)
                    {
                        if (chestPiece.visualAid == null || !chestPiece.visualAid.exists)
                        {
                            BoardPiece usedChestMarker = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: chestPiece.sprite.position, templateName: PieceTemplate.Name.BubbleCraftGreen);

                            new Tracking(world: world, targetSprite: chestPiece.sprite, followingSprite: usedChestMarker.sprite, targetYAlign: YAlign.Top, targetXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, offsetX: 0, offsetY: 5);

                            new WorldEvent(eventName: WorldEvent.EventName.FadeOutSprite, delay: 40, world: world, boardPiece: usedChestMarker, eventHelper: 20);
                        }
                    }
                }
            }

            return craftStorages;
        }

        public List<PieceStorage> CraftStoragesToPutInto
        { get { return new List<PieceStorage> { this.ToolStorage, this.PieceStorage }; } } // the same as above, changed order

        public StorageSlot ActiveSlot
        {
            get
            {
                if (this.ToolStorage == null) return null;
                Point lastUsedSlotPos = this.ToolStorage.LastUsedSlotPos;
                if (lastUsedSlotPos.X == -1 && lastUsedSlotPos.Y == -1) return null;

                return this.ToolStorage.GetSlot(lastUsedSlotPos.X, lastUsedSlotPos.Y);
            }
        }

        public BoardPiece ActiveToolbarPiece
        { get { return this.ActiveSlot?.TopPiece; } }

        private bool CanUseActiveToolbarPiece
        {
            get
            {
                BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;
                return activeToolbarPiece != null && activeToolbarPiece.pieceInfo.toolbarTask != Scheduler.TaskName.Empty;
            }
        }

        public bool CanSeeAnything
        {
            get
            {
                bool canSeeAnything = this.world.islandClock.CurrentPartOfDay != IslandClock.PartOfDay.Night || this.world.Player.sprite.IsInLightSourceRange;
                if (!canSeeAnything) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.TooDarkToSeeAnything, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
                return canSeeAnything;
            }
        }

        private bool ActiveToolbarWeaponHasAmmo // applies only to shooting weapons
        {
            get
            {
                BoardPiece piece = this.ActiveToolbarPiece;
                if (piece == null) return false;
                if (piece.GetType() != typeof(Tool)) return false;

                Tool activeTool = (Tool)piece;
                if (!activeTool.pieceInfo.toolShootsProjectile) return false;

                return activeTool.CheckForAmmo(removePiece: false) != null;
            }
        }

        private BoardPiece ClosestPieceToInteract
        {
            get
            {
                Rectangle focusRect = this.GetFocusRect();

                var spritesForRect = world.Grid.GetSpritesForRect(groupName: Cell.Group.Visible, rectangle: focusRect, addPadding: true);
                if (spritesForRect.Count == 0) return null;

                var piecesToInteract = new List<BoardPiece>();
                foreach (Sprite sprite in spritesForRect)
                {
                    if (sprite.boardPiece.pieceInfo.boardTask != Scheduler.TaskName.Empty && !sprite.boardPiece.IsBurning && sprite.boardPiece != this)
                    {
                        piecesToInteract.Add(sprite.boardPiece);
                    }
                }
                if (piecesToInteract.Count == 0) return null;

                Vector2 focusRectCenter = new(focusRect.Center.X, focusRect.Center.Y);
                return piecesToInteract.OrderBy(piece => Vector2.Distance(focusRectCenter, piece.sprite.position)).First();
            }
        }

        private BoardPiece ClosestPieceToPickUp
        {
            get
            {
                if (!this.CanSeeAnything) return null;

                Rectangle focusRect = this.GetFocusRect();

                var spritesForRect = this.world.Grid.GetSpritesForRect(groupName: Cell.Group.Visible, rectangle: focusRect, addPadding: true);
                if (spritesForRect.Count == 0) return null;

                var piecesToPickUp = new List<BoardPiece>();
                foreach (Sprite sprite in spritesForRect)
                {
                    if (sprite.boardPiece.pieceInfo.canBePickedUp && !sprite.boardPiece.IsBurning && (sprite.boardPiece.GetType() != typeof(Animal) || !sprite.boardPiece.alive))
                    {
                        piecesToPickUp.Add(sprite.boardPiece);
                    }
                }
                if (piecesToPickUp.Count == 0) return null;

                Vector2 focusRectCenter = new(focusRect.Center.X, focusRect.Center.Y);
                return piecesToPickUp.OrderBy(piece => Vector2.Distance(this.sprite.position, piece.sprite.position)).First();
            }
        }

        public float GetFinalFatigueValue(float fatigueVal)
        {
            if (fatigueVal > 0 && this.buffEngine.HasBuff(BuffEngine.BuffType.Heat)) fatigueVal *= 2;
            return fatigueVal;
        }

        public float Fatigue
        {
            get { return this.fatigue; }
            set
            {
                if (Preferences.DebugGodMode) return;

                float fatigueDifference = value - this.fatigue;
                fatigueDifference = this.GetFinalFatigueValue(fatigueDifference);

                this.fatigue = Math.Clamp(value: this.fatigue + fatigueDifference, min: 0, max: this.maxFatigue);

                if (this.IsVeryTired)
                {
                    if (!this.buffEngine.HasBuff(BuffEngine.BuffType.Tired)) this.buffEngine.AddBuff(world: this.world, buff: new Buff(type: BuffEngine.BuffType.Tired, value: 0, autoRemoveDelay: 0));
                }
                else
                {
                    if (this.buffEngine.HasBuff(BuffEngine.BuffType.Tired)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Tired);
                }

                if (this.IsStartingToGetTired) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.HowToSave, world: this.world, ignoreDelay: true);

                if (this.IsVeryTired) this.world.HintEngine.ShowGeneralHint(HintEngine.Type.Tired);

                if (this.FatiguePercent < 0.2f) this.world.HintEngine.Enable(HintEngine.Type.Tired);
                if (this.FatiguePercent > 0.95f) this.world.HintEngine.ShowGeneralHint(HintEngine.Type.VeryTired);
                if (this.FatiguePercent < 0.8f) this.world.HintEngine.Enable(HintEngine.Type.VeryTired);

                if (this.fatigue == this.maxFatigue) new Scheduler.Task(taskName: Scheduler.TaskName.SleepOutside, delay: 0, executeHelper: null);
            }
        }

        public float FedPercent
        {
            get
            {
                float fedPercent = this.ConvertFedLevelToFedPercent(this.fedLevel);

                if (fedPercent < 0.2f)
                {
                    if (!this.buffEngine.HasBuff(BuffEngine.BuffType.Hungry)) this.buffEngine.AddBuff(world: this.world, buff: new Buff(type: BuffEngine.BuffType.Hungry, value: 0, autoRemoveDelay: 0));
                }
                else
                {
                    if (this.buffEngine.HasBuff(BuffEngine.BuffType.Hungry)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Hungry);
                }

                return fedPercent;
            }
        }

        public float FatiguePercent
        { get { return (float)this.Fatigue / (float)this.maxFatigue; } }

        public bool IsStartingToGetTired
        { get { return this.FatiguePercent > 0.45f; } }

        public bool IsVeryTired
        { get { return this.FatiguePercent > 0.75f; } }

        private bool HasLowHP
        { get { return this.HitPointsPercent < 0.15f; } }

        public bool CanWakeNow
        { get { return this.sleepingInsideShelter || this.FatiguePercent < 0.85f; } }

        private void ConfigureEquip()
        {
            foreach (StorageSlot slot in this.EquipStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            var equipTypeBySlotCoords = new Dictionary<Point, Equipment.EquipType> {
                { new Point(1,0), Equipment.EquipType.Head },
                { new Point(1,1), Equipment.EquipType.Chest },
                { new Point(1,2), Equipment.EquipType.Legs },
                { new Point(0,1), Equipment.EquipType.Backpack },
                { new Point(2,1), Equipment.EquipType.Belt },
                { new Point(0,0), Equipment.EquipType.Accessory },
                { new Point(2,0), Equipment.EquipType.Accessory },
                { new Point(0,2), Equipment.EquipType.Map },
            };

            if (this.Skill == SkillName.Fashionista) equipTypeBySlotCoords[new Point(2, 2)] = Equipment.EquipType.Accessory;

            foreach (var kvp in equipTypeBySlotCoords)
            {
                Point slotCoords = kvp.Key;
                Equipment.EquipType equipType = kvp.Value;

                StorageSlot slot = this.EquipStorage.GetSlot(slotCoords.X, slotCoords.Y);
                slot.locked = false;
                slot.hidden = false;
                slot.allowedPieceNames = PieceInfo.GetNamesForEquipType(equipType);
                slot.label = equipType.ToString().ToLower();
            }
        }

        public override void Kill(bool addDestroyEvent = true)
        {
            if (Preferences.DebugGodMode || !PieceInfo.IsPlayer(this.name)) return;

            this.ToolStorage.DropAllPiecesToTheGround(addMovement: true); // only ToolStorage pieces should fall to the ground
            foreach (PieceStorage storage in this.CraftStoragesToPutInto)
            {
                foreach (StorageSlot slot in storage.OccupiedSlots)
                {
                    storage.RemoveAllPiecesFromSlot(slot: slot); // the rest should be destroyed
                }
            }

            if (this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.LowHP);

            base.Kill(addDestroyEvent);

            Scene.RemoveAllScenesOfType(typeof(TextWindow));

            SolidColor redOverlay = new SolidColor(color: Color.DarkRed, viewOpacity: 0.65f);
            this.world.solidColorManager.Add(redOverlay);

            Sound.QuickPlay(SoundData.Name.GameOver);

            new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 3f } });
            new Scheduler.Task(taskName: Scheduler.TaskName.OpenMenuTemplate, turnOffInputUntilExecution: true, delay: 300, executeHelper: new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.GameOver } });
        }

        public override void Destroy()
        {
            if (!this.exists) return;
            this.pieceInfo.Yield.DropFinalPieces(piece: this);
            base.Destroy();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["player_fedLevel"] = this.fedLevel;
            pieceData["player_maxFedLevel"] = this.maxFedLevel;
            pieceData["player_fatigue"] = this.fatigue;
            pieceData["player_maxFatigue"] = this.maxFatigue;
            pieceData["player_Skill"] = this.Skill;
            pieceData["player_CraftLevel"] = this.CraftLevel;
            pieceData["player_CookLevel"] = this.CookLevel;
            pieceData["player_BrewLevel"] = this.BrewLevel;
            pieceData["player_HarvestLevel"] = this.HarvestLevel;
            pieceData["player_distanceWalked"] = this.distanceWalked;
            pieceData["player_ToolStorage"] = this.ToolStorage.Serialize();
            pieceData["player_EquipStorage"] = this.EquipStorage.Serialize();
            pieceData["player_GlobalChestStorage"] = this.GlobalChestStorage.Serialize();
            pieceData["player_LastSteps"] = this.LastSteps.Select(s => new Point((int)s.X, (int)s.Y)).ToList();

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.fedLevel = (float)(double)pieceData["player_fedLevel"];
            this.maxFedLevel = (float)(double)pieceData["player_maxFedLevel"];
            this.fatigue = (float)(double)pieceData["player_fatigue"];
            this.maxFatigue = (float)(double)pieceData["player_maxFatigue"];
            this.Skill = (SkillName)(Int64)pieceData["player_Skill"];
            this.CraftLevel = (int)(Int64)pieceData["player_CraftLevel"];
            this.CookLevel = (int)(Int64)pieceData["player_CookLevel"];
            this.BrewLevel = (int)(Int64)pieceData["player_BrewLevel"];
            this.HarvestLevel = (int)(Int64)pieceData["player_HarvestLevel"];
            this.distanceWalked = (float)(double)pieceData["player_distanceWalked"];
            this.ToolStorage = PieceStorage.Deserialize(storageData: pieceData["player_ToolStorage"], storagePiece: this);
            this.EquipStorage = PieceStorage.Deserialize(storageData: pieceData["player_EquipStorage"], storagePiece: this);
            this.GlobalChestStorage = PieceStorage.Deserialize(storageData: pieceData["player_GlobalChestStorage"], storagePiece: this);
            List<Point> lastStepsPointList = (List<Point>)pieceData["player_LastSteps"];
            this.LastSteps = lastStepsPointList.Select(p => new Vector2(p.X, p.Y)).ToList();

            this.RefreshAllowedPiecesForStorages();
        }

        public void RefreshAllowedPiecesForStorages() // for older saves compatibility, to ensure that all storages have the right allowedPieceNames
        {
            Player tempPlayer = (Player)PieceTemplate.CreatePiece(templateName: this.name, world: this.world);

            for (int x = 0; x < this.EquipStorage.Width; x++)
            {
                for (int y = 0; y < this.EquipStorage.Height; y++)
                {
                    this.EquipStorage.GetSlot(x, y).allowedPieceNames = tempPlayer.EquipStorage.GetSlot(x, y).allowedPieceNames;
                }
            }

            {
                var pieceStorageAllowedNames = tempPlayer.PieceStorage.AllowedPieceNames;
                this.PieceStorage.AssignAllowedPieceNames(pieceStorageAllowedNames);
                foreach (StorageSlot slot in this.PieceStorage.AllSlots)
                {
                    slot.allowedPieceNames = pieceStorageAllowedNames;
                }
            }

            {
                var toolStorageAllowedNames = tempPlayer.ToolStorage.AllowedPieceNames;
                this.ToolStorage.AssignAllowedPieceNames(toolStorageAllowedNames);
                foreach (StorageSlot slot in this.ToolStorage.AllSlots)
                {
                    slot.allowedPieceNames = toolStorageAllowedNames;
                }
            }
        }

        public override void DrawStatBar()
        {
            // the rest of stat bars are drawn in PlayerPanel scene

            if (this.activeState == State.PlayerControlledShooting)
            {
                new StatBar(width: 80, height: 5, label: "power", value: (int)this.shootingPower, valueMax: (int)maxShootingPower, colorMin: new Color(220, 255, 0), colorMax: new Color(255, 0, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom);
                StatBar.FinishThisBatch();
            }
        }

        public void ExpendEnergy(float energyAmount, bool addFatigue)
        {
            if (Preferences.DebugGodMode) return;

            if (this.fedLevel > 0)
            {
                float hungerVal = energyAmount * 0.6f;
                if (this.Skill == SkillName.Survivalist) hungerVal *= 0.7f;
                // MessageLog.AddMessage( message: $"{SonOfRobinGame.CurrentUpdate} hungerVal {hungerVal}");

                this.fedLevel = Math.Max(this.fedLevel - hungerVal, 0);

                if (this.FedPercent < 0.5f) this.world.HintEngine.ShowGeneralHint(HintEngine.Type.Hungry);
                if (this.FedPercent < 0.2f) this.world.HintEngine.ShowGeneralHint(HintEngine.Type.VeryHungry);
                if (this.FedPercent < 0.01f) this.world.HintEngine.ShowGeneralHint(HintEngine.Type.Starving);
            }
            else this.HitPoints = Math.Max(this.HitPoints - 0.02f, 0);

            if (addFatigue) this.Fatigue += 0.01f;
        }

        public void AcquireEnergy(float energyAmount)
        {
            this.fedLevel = Math.Min(this.fedLevel + this.ConvertEnergyAmountToFedLevel(energyAmount), this.maxFedLevel);

            if (this.FedPercent > 0.8f) this.world.HintEngine.Enable(HintEngine.Type.Hungry);
            if (this.FedPercent > 0.4f) this.world.HintEngine.Enable(HintEngine.Type.VeryHungry);
            if (this.FedPercent > 0.1f) this.world.HintEngine.Enable(HintEngine.Type.Starving);
        }

        public float ConvertMassToEnergyAmount(float mass)
        {
            return mass * 15f;
        }

        private int ConvertEnergyAmountToFedLevel(float energyAmount)
        {
            return (int)energyAmount * 2;
        }

        private float ConvertFedLevelToFedPercent(float fedLevel)
        {
            return (float)fedLevel / (float)this.maxFedLevel;
        }

        public float ConvertMassToFedPercent(float mass)
        {
            float energyAmount = this.ConvertMassToEnergyAmount(mass);
            int fedLevel = this.ConvertEnergyAmountToFedLevel(energyAmount);
            return this.ConvertFedLevelToFedPercent(fedLevel);
        }

        public void UpdateDistanceWalked()
        {
            if (this.previousStepPos.X != -100) this.distanceWalked += Vector2.Distance(this.previousStepPos, this.sprite.position);
            this.previousStepPos = this.sprite.position;
        }

        public void UpdateLastSteps()
        {
            if (this.world.MapEnabled && (this.LastSteps.Count == 0 || Vector2.Distance(this.sprite.position, this.LastSteps.Last()) > 180))
            {
                this.LastSteps.Add(this.sprite.position);
                if (this.LastSteps.Count > maxLastStepsCount) this.LastSteps.RemoveAt(0);
            }
        }

        public override void SM_PlayerControlledBuilding()
        {
            this.RemovePassiveMovement(); // to ensure that player will not move

            Vector2 analogMovementLeftStick = InputMapper.Analog(InputMapper.Action.WorldWalk);
            Vector2 newPos = this.simulatedPieceToBuild.sprite.position;

            if (analogMovementLeftStick != Vector2.Zero) newPos += analogMovementLeftStick;
            else
            {
                if (Preferences.PointToWalk)
                {
                    foreach (TouchLocation touch in TouchInput.TouchPanelState)
                    {
                        if (touch.State == TouchLocationState.Moved && !TouchInput.IsPointActivatingAnyTouchInterface(touch.Position))
                        {
                            Vector2 worldTouchPos = this.world.TranslateScreenToWorldPos(touch.Position);
                            newPos = worldTouchPos;
                            break;
                        }
                    }
                }
            }

            int pieceSize = Math.Max(this.simulatedPieceToBuild.sprite.AnimFrame.colWidth, this.simulatedPieceToBuild.sprite.AnimFrame.colHeight);

            float buildDistance = Vector2.Distance(this.sprite.position, newPos);

            if (buildDistance <= 80 + pieceSize) this.simulatedPieceToBuild.sprite.SetNewPosition(newPos: newPos, ignoreCollisions: true);

            bool canBuildHere = !this.simulatedPieceToBuild.sprite.CheckForCollision(ignoreDensity: false);
            if (canBuildHere)
            {
                VirtButton.ButtonHighlightOnNextFrame(VButName.Confirm);
                ControlTips.TipHighlightOnNextFrame(tipName: "place");
            }

            Color color = canBuildHere ? Color.Green : Color.Red;

            Sprite simulatedPieceSprite = this.simulatedPieceToBuild.sprite;

            simulatedPieceSprite.effectCol.AddEffect(new ColorizeInstance(color: color, priority: 0));
            simulatedPieceSprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.White, drawFill: false, borderThickness: (int)(1f / simulatedPieceSprite.AnimFrame.scale), textureSize: simulatedPieceSprite.AnimFrame.textureSize, priority: 1));

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                this.world.ExitBuildMode(restoreCraftMenu: true);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm))
            {
                if (canBuildHere) this.world.BuildPiece();
                else new TextWindow(text: $"|  {Helpers.FirstCharToUpperCase(this.simulatedPieceToBuild.readableName)} can't be placed here.", imageList: new List<Texture2D> { simulatedPieceSprite.AnimFrame.texture }, textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, checkForDuplicate: true, autoClose: false, inputType: Scene.InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, startingSound: SoundData.Name.Error);
            }
        }

        public override void SM_PlayerWaitForBuilding()
        {
            // needs to be updated from the outside

            this.RemovePassiveMovement(); // to ensure that player will not move

            this.world.islandClock.Advance(amount: this.buildDurationForOneFrame, ignorePause: true);
            this.Fatigue += this.buildFatigueForOneFrame;
            world.Player.Fatigue = Math.Min(world.Player.Fatigue, world.Player.maxFatigue - 20); // to avoid falling asleep just after crafting

            new RumbleEvent(force: 0.08f, smallMotor: true, fadeInSeconds: 0.25f, durationSeconds: 0, fadeOutSeconds: 0.25f, minSecondsSinceLastRumbleSmallMotor: 0.51f);
            new RumbleEvent(force: 0.85f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.15f, minSecondsSinceLastRumbleBigMotor: 0.35f);
        }

        public override void SM_PlayerControlledGhosting()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldPauseMenu))
            {
                this.world.OpenMenu(templateName: MenuTemplate.Name.Pause);
                return;
            }

            this.Walk(slowDownInWater: false, slowDownOnRocks: false);
        }

        public override void SM_PlayerControlledByCinematic()
        {
            if (this.pointWalkTarget != Vector2.Zero)
            {
                this.GoOneStepTowardsGoal(this.pointWalkTarget, walkSpeed: this.speed, slowDownInWater: true, slowDownOnRocks: true);
                if (this.PointWalkTargetReached)
                {
                    this.sprite.CharacterStand();
                    this.pointWalkTarget = Vector2.Zero;
                }
            }
        }

        private bool PointWalkTargetReached
        { get { return this.pointWalkTarget == Vector2.Zero || Vector2.Distance(this.sprite.position, this.pointWalkTarget) <= 1f * this.speed; } }

        public override void SM_PlayerControlledWalking()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldPauseMenu))
            {
                this.world.OpenMenu(templateName: MenuTemplate.Name.Pause);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldFieldCraft))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CraftField);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldInventory) || this.world.playerPanel.IsCounterActivatedByTouch)
            {
                Inventory.SetLayout(Inventory.LayoutType.Inventory, player: this);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldMapToggle))
            {
                this.world.ToggleMapMode();
                return;
            }

            if (!this.buffEngine.HasBuff(BuffEngine.BuffType.Sprint) && !this.buffEngine.HasBuff(BuffEngine.BuffType.SprintCooldown))
            {
                VirtButton.ButtonHighlightOnNextFrame(VButName.Sprint);
                ControlTips.TipHighlightOnNextFrame(tipName: "sprint");
                if (InputMapper.HasBeenPressed(InputMapper.Action.WorldSprintToggle))
                {
                    int sprintDuration = 60 * 3;

                    if (this.buffEngine.HasBuff(BuffEngine.BuffType.ExtendSprintDuration))
                    {
                        Buff mergedExtendedSprintBuffs = BuffEngine.MergeMultipleSameTypeBuffs(this.buffEngine.GetBuffsOfType(BuffEngine.BuffType.ExtendSprintDuration));
                        int sprintExtension = (int)mergedExtendedSprintBuffs.value;
                        sprintDuration += sprintExtension;
                    }

                    this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Sprint, autoRemoveDelay: sprintDuration, value: 2f), world: this.world);
                }
            }

            if (this.world.CurrentUpdate % 121 == 0) this.world.HintEngine.CheckForPieceHintToShow();

            this.ExpendEnergy(energyAmount: 1.0f, addFatigue: false);
            this.Walk();

            this.CheckGround();
            this.CheckLowHP();

            // highlighting pieces to interact with and corresponding interface elements

            if (this.world.inputActive) this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: true); // only to highlight pieces that will be hit

            BoardPiece pieceToInteract = this.ClosestPieceToInteract;
            if (pieceToInteract != null)
            {
                if (this.world.inputActive)
                {
                    Texture2D interactTexture = InputMapper.GetTexture(InputMapper.Action.WorldInteract);

                    if (Input.CurrentControlType == Input.ControlType.Touch && pieceToInteract.pieceInfo.interactVirtButtonName != TextureBank.TextureName.Empty)
                    {
                        interactTexture = TextureBank.GetTexture(pieceToInteract.pieceInfo.interactVirtButtonName);
                        VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.Interact, texture: interactTexture);
                    }

                    FieldTip.AddUpdateTip(world: this.world, texture: interactTexture, targetSprite: pieceToInteract.sprite, alignment: FieldTip.Alignment.Center);

                    pieceToInteract.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Green, priority: 1));
                    Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Interact, world: this.world);
                    VirtButton.ButtonHighlightOnNextFrame(VButName.Interact);
                    ControlTips.TipHighlightOnNextFrame(tipName: "interact");
                }
            }

            BoardPiece pieceToPickUp = this.ClosestPieceToPickUp;
            if (pieceToPickUp != null)
            {
                if (this.world.inputActive)
                {
                    Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.PickUp, world: this.world);
                    pieceToPickUp.sprite.effectCol.AddEffect(new ColorizeInstance(color: new Color(13, 118, 163), priority: 0));
                    pieceToPickUp.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.White, drawFill: false, borderThickness: (int)(2f * (1f / pieceToPickUp.sprite.AnimFrame.scale)), textureSize: pieceToPickUp.sprite.AnimFrame.textureSize, priority: 1));
                    VirtButton.ButtonHighlightOnNextFrame(VButName.PickUp);
                    ControlTips.TipHighlightOnNextFrame(tipName: "pick up");
                    FieldTip.AddUpdateTip(world: this.world, texture: InputMapper.GetTexture(InputMapper.Action.WorldPickUp), targetSprite: pieceToPickUp.sprite, alignment: this.sprite.position.Y > pieceToPickUp.sprite.position.Y ? FieldTip.Alignment.TopOut : FieldTip.Alignment.BottomOut);
                }
            }

            // checking pressed buttons

            if (this.ShootingModeInputPressed)
            {
                if (this.TryToEnterShootingMode()) return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldUseToolbarPiece) || Mouse.RightHasBeenPressed)
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: false)) return;
            }

            if (InputMapper.IsPressed(InputMapper.Action.WorldUseToolbarPiece))
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: true, highlightOnly: false)) return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldHighlightPickups)) Preferences.pickupsHighlighted = !Preferences.pickupsHighlighted;

            bool pickUp = pieceToPickUp != null && (InputMapper.HasBeenPressed(InputMapper.Action.WorldPickUp) ||
                (Preferences.PointToInteract && pieceToPickUp.sprite.GfxRect.Contains(this.pointWalkTarget)));

            if (pickUp)
            {
                this.pointWalkTarget = Vector2.Zero; // to avoid picking up indefinitely
                this.PickUpClosestPiece(closestPiece: pieceToPickUp);
                return;
            }

            bool interact = !pickUp && pieceToInteract != null &&
                (InputMapper.HasBeenPressed(InputMapper.Action.WorldInteract) ||
                (Preferences.PointToInteract &&
                !TouchInput.IsBeingTouchedInAnyWay && // to avoid activating multiple times while holding touch
                pieceToInteract.sprite.GfxRect.Intersects(new Rectangle(x: (int)this.pointWalkTarget.X - 1, y: (int)this.pointWalkTarget.Y - 1, width: 2, height: 2))));

            if (interact)
            {
                this.pointWalkTarget = Vector2.Zero; // to avoid interacting indefinitely
                this.world.HintEngine.Disable(Tutorials.Type.Interact);
                new Scheduler.Task(taskName: pieceToInteract.pieceInfo.boardTask, delay: 0, executeHelper: pieceToInteract);
            }
        }

        private bool Walk(bool setOrientation = true, bool slowDownInWater = true, bool slowDownOnRocks = true)
        {
            Vector2 analogWalk = InputMapper.Analog(InputMapper.Action.WorldWalk);
            if (analogWalk != Vector2.Zero) this.pointWalkTarget = Vector2.Zero;

            bool layoutChangedRecently = TouchInput.FramesSinceLayoutChanged < 5;

            if (Preferences.PointToWalk && analogWalk == Vector2.Zero && !layoutChangedRecently)
            {
                foreach (TouchLocation touch in TouchInput.TouchPanelState)
                {
                    if (!TouchInput.IsPointActivatingAnyTouchInterface(touch.Position) &&
                        (this.activeState != State.PlayerControlledShooting || touch.State == TouchLocationState.Released))
                    {
                        Vector2 worldTouchPos = this.world.TranslateScreenToWorldPos(touch.Position);

                        this.pointWalkTarget = worldTouchPos;
                        break;
                    }
                }

                if (this.PointWalkTargetReached) this.pointWalkTarget = Vector2.Zero;
            }

            if (analogWalk == Vector2.Zero && this.pointWalkTarget == Vector2.Zero)
            {
                this.sprite.CharacterStand();
                return false;
            }

            // var crosshairForPointTarget = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.pointWalkTarget, templateName: PieceTemplate.Name.Crosshair); // for testing
            // crosshairForPointTarget.sprite.color = Color.Cyan; // for testing
            // new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: 1, boardPiece: crosshairForPointTarget); // for testing

            var currentSpeed = this.IsVeryTired ? this.speed / 2f : this.speed;

            Vector2 goalPosition = this.sprite.position;
            if (analogWalk != Vector2.Zero)
            {
                float analogWalkTiltPower = Preferences.alwaysRun ? 1f : Math.Min(Vector2.Distance(analogWalk, Vector2.Zero), 1f);
                currentSpeed *= analogWalkTiltPower;
                goalPosition += analogWalk * 250f; // should always be out of reach

                // MessageLog.AddMessage( message: $"{SonOfRobinGame.CurrentUpdate} vector {Math.Round(analogWalk.X, 1)},{Math.Round(analogWalk.Y, 1)} power {analogWalkTiltPower} speed {currentSpeed}");
            }
            else goalPosition = this.pointWalkTarget;

            // var crosshairForGoal = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: goalPosition, templateName: PieceTemplate.Name.Crosshair); // for testing
            // crosshairForGoal.sprite.color = Color.Violet; // for testing
            // new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: 1, boardPiece: crosshairForGoal); // for testing

            bool hasBeenMoved = this.GoOneStepTowardsGoal(goalPosition, walkSpeed: currentSpeed, setOrientation: setOrientation, slowDownInWater: slowDownInWater, slowDownOnRocks: slowDownOnRocks);
            if (hasBeenMoved)
            {
                this.ExpendEnergy(energyAmount: 0.4f, addFatigue: true);

                if (this.sprite.IsInWater && slowDownInWater)
                {
                    float submergePower = (float)Helpers.ConvertRange(oldMin: 0, oldMax: Terrain.waterLevelMax, newMin: 0, newMax: 0.45, oldVal: Terrain.waterLevelMax - this.sprite.GetFieldValue(Terrain.Name.Height), clampToEdges: true);

                    new RumbleEvent(force: submergePower, smallMotor: true, fadeInSeconds: 0.16f, durationSeconds: 0f, fadeOutSeconds: 0.16f, minSecondsSinceLastRumbleSmallMotor: 0.52f);
                    new RumbleEvent(force: submergePower, bigMotor: true, fadeInSeconds: 0.06f, durationSeconds: 0.15f, fadeOutSeconds: 0.06f, minSecondsSinceLastRumbleBigMotor: 0.62f);
                }

                if (this.sprite.IsOnRocks && slowDownOnRocks)
                {
                    float randomAddedForce = (float)this.world.random.NextSingle() * 0.16f;
                    float randomAddedDelay = (float)this.world.random.NextSingle() * 0.21f;
                    new RumbleEvent(force: 0.04f + randomAddedForce, smallMotor: true, fadeInSeconds: 0f, durationSeconds: 0f, fadeOutSeconds: 0.12f, minSecondsSinceLastRumbleSmallMotor: 0.18f + randomAddedDelay);
                }
            }
            else
            {
                this.pointWalkTarget = Vector2.Zero;
            }

            return hasBeenMoved;
        }

        private void CheckGround()
        {
            bool isInWater = this.sprite.IsInWater;
            bool isRaining = this.world.weather.IsRaining;

            if (this.world.CurrentUpdate % 65 == 0)
            {
                if (this.world.islandClock.CurrentPartOfDay == IslandClock.PartOfDay.Noon && this.world.weather.SunVisibility >= 0.8f && !isRaining)
                {
                    this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Heat, value: null), world: this.world);
                }
                else this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Heat);

                if (this.buffEngine.HasBuff(BuffEngine.BuffType.Heat)) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Heat, world: this.world, ignoreDelay: true);
            }

            if (isInWater || isRaining) this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Wet, value: null, autoRemoveDelay: 40 * 60), world: this.world);

            if (isRaining)
            {
                {
                    float addedForce = this.world.weather.RainPercentage * 0.1f;
                    float delaySeconds = 1f - (this.world.weather.RainPercentage * 0.8f);
                    float randomAddedDelay = (float)this.world.random.NextSingle() * 0.21f;
                    new RumbleEvent(force: 0.14f + addedForce, smallMotor: true, fadeInSeconds: 0f, durationSeconds: 0f, fadeOutSeconds: 0.08f, minSecondsSinceLastRumbleSmallMotor: delaySeconds + randomAddedDelay);
                }
                {
                    float addedForce = this.world.weather.RainPercentage * 0.1f;
                    float delaySeconds = 1f - (this.world.weather.RainPercentage * 0.4f);
                    float randomAddedDelay = (float)this.world.random.NextSingle() * 0.21f;
                    new RumbleEvent(force: 0.11f + addedForce, bigMotor: true, fadeInSeconds: 0f, durationSeconds: 0f, fadeOutSeconds: 0.08f, minSecondsSinceLastRumbleBigMotor: delaySeconds + randomAddedDelay);
                }
            }

            if (this.sprite.IsInBiome)
            {
                // put detailed biome checks here

                if (this.sprite.GetExtProperty(name: ExtBoardProps.Name.BiomeSwamp))
                {
                    if (!this.buffEngine.HasBuff(BuffEngine.BuffType.SwampProtection))
                    {
                        Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.SwampPoison, world: this.world, ignoreDelay: true);

                        if (!this.buffEngine.HasBuff(buffType: BuffEngine.BuffType.RegenPoison, isPositive: false))
                        {
                            Sound.QuickPlay(name: SoundData.Name.SplashMud);

                            this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.RegenPoison, value: -35, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true), world: this.world);
                        }
                    }
                }
            }

            bool isOnLava = this.sprite.IsOnLava;
            if (isOnLava)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lava, ignoreDelay: true);
                this.HitPoints -= 0.5f;
                this.HeatLevel += 0.12f;
                this.activeSoundPack.Play(PieceSoundPackTemplate.Action.StepLava);
                new RumbleEvent(force: 1f, bigMotor: true, smallMotor: true, fadeInSeconds: 0, durationSeconds: 1f / 60f, fadeOutSeconds: 0);
            }

            bool struckWithLightning = this.world.weather.LightningJustStruck && isInWater && this.world.random.Next(2) == 0;
            if (struckWithLightning)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lightning, ignoreDelay: true);
                this.HitPoints -= 80;
                Sound.QuickPlay(SoundData.Name.ElectricShock);
                new RumbleEvent(force: 1f, smallMotor: true, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0.45f);
            }

            if (isOnLava || struckWithLightning)
            {
                if (!this.world.solidColorManager.AnySolidColorPresent)
                {
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Cry);
                    this.world.camera.AddRandomShake();
                    this.world.FlashRedOverlay();
                }
            }

            if (this.world.MapEnabled && this.CanSeeAnything)
            {
                NamedLocations.Location location = this.world.Grid.namedLocations.PlayerLocation;
                if (location != null && !location.hasBeenDiscovered) this.world.Grid.namedLocations.ProcessDiscovery();
            }
        }

        public void CheckLowHP()
        {
            if (this.HasLowHP)
            {
                if (!this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.AddBuff(world: this.world, buff: new Buff(type: BuffEngine.BuffType.LowHP, value: 0, autoRemoveDelay: 0));
                new RumbleEvent(force: 0.3f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.065f, fadeOutSeconds: 0.065f, minSecondsSinceLastRumbleBigMotor: 1f);
            }
            else
            {
                if (this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.LowHP);
            }
        }

        public override void SM_PlayerControlledShooting()
        {
            this.ExpendEnergy(energyAmount: 1.4f, addFatigue: false);
            this.CheckGround();
            this.CheckLowHP();

            VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonBow));

            this.shootingPower = Math.Min(this.shootingPower + 1, maxShootingPower);
            if (this.shootingPower < maxShootingPower) new RumbleEvent(force: (float)this.shootingPower / maxShootingPower * 0.1f, smallMotor: true, fadeInSeconds: 0, durationSeconds: 1f / 60f, fadeOutSeconds: 0);

            if (this.visualAid == null) this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Crosshair);

            this.Walk(setOrientation: false);

            // shooting angle should be set once at the start
            if (this.ShootingAngle == -100) this.ShootingAngle = this.sprite.OrientationAngle;

            Vector2 moving = InputMapper.Analog(InputMapper.Action.WorldWalk);
            Vector2 shooting = InputMapper.Analog(InputMapper.Action.WorldCameraMove);

            // right mouse button will be released when shooting and should be taken into account, to set the angle correctly
            if (Mouse.RightIsDown || Mouse.RightHasBeenReleased) shooting = (this.world.TranslateScreenToWorldPos(Mouse.Position) - this.sprite.position) / 20;

            Vector2 directionVector = Vector2.Zero;
            if (moving != Vector2.Zero) directionVector = moving;
            if (shooting != Vector2.Zero) directionVector = shooting;

            if (directionVector != Vector2.Zero)
            {
                this.sprite.SetOrientationByMovement(movement: directionVector, orientationAngleOverride: this.ShootingAngle);
                Vector2 goalPosition = this.sprite.position + directionVector; // used to calculate shooting angle
                this.ShootingAngle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: goalPosition);
            }

            int aidDistance = (int)(SonOfRobinGame.VirtualWidth * 0.08f);

            Vector2 aidOffset = new Vector2(aidDistance * (float)Math.Cos(this.ShootingAngle), aidDistance * (float)Math.Sin(this.ShootingAngle));
            Vector2 aidPos = this.sprite.position + aidOffset;
            this.visualAid.sprite.SetNewPosition(aidPos);

            if (VirtButton.HasButtonBeenPressed(VButName.Shoot) || // virtual button has to be checked separately here
                InputMapper.HasBeenReleased(InputMapper.Action.WorldUseToolbarPiece) ||
                Mouse.RightHasBeenReleased)
            {
                this.UseToolbarPiece(isInShootingMode: true, buttonHeld: false);
                this.shootingPower = 0;
                this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.PlayerBowDraw);
            }

            if (!this.ShootingModeInputPressed || !this.ActiveToolbarWeaponHasAmmo || this.sprite.CanDrownHere)
            {
                this.visualAid.Destroy();
                this.visualAid = null;
                this.world.touchLayout = TouchLayout.WorldMain;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
                this.activeState = State.PlayerControlledWalking;
                this.ShootingAngle = -100;
                this.shootingPower = 0;
                this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.PlayerBowDraw);
            }
        }

        public override void SM_PlayerControlledSleep()
        {
            this.CheckLowHP();

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                this.WakeUp();
                return;
            }

            if (this.sleepMode == SleepMode.WaitMorning && this.world.islandClock.CurrentPartOfDay == IslandClock.PartOfDay.Morning)
            {
                new TextWindow(text: "The morning has came.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound, blocksUpdatesBelow: true);

                this.WakeUp();
                return;
            }

            if (this.sleepMode == SleepMode.Sleep && this.FatiguePercent <= this.sleepEngine.minFatiguePercentPossibleToGet)
            {
                this.sleepMode = SleepMode.WaitIndefinitely;
                this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.PlayerSnore);

                this.visualAid.Destroy();
                this.visualAid = null;

                if (this.sleepEngine.waitingAfterSleepPossible)
                {
                    var optionList = new List<object>();

                    optionList.Add(new Dictionary<string, object> { { "label", "go out" }, { "taskName", Scheduler.TaskName.ForceWakeUp }, { "executeHelper", this } });

                    if (this.world.islandClock.CurrentPartOfDay != IslandClock.PartOfDay.Morning) optionList.Add(new Dictionary<string, object> { { "label", "wait until morning" }, { "taskName", Scheduler.TaskName.WaitUntilMorning }, { "executeHelper", this } });

                    optionList.Add(new Dictionary<string, object> { { "label", "wait indefinitely" }, { "taskName", Scheduler.TaskName.Empty }, { "executeHelper", null } });

                    var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "You are fully rested." }, { "customOptionList", optionList } };

                    new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                }
                else
                {
                    this.WakeUp();

                    new TextWindow(text: this.sleepEngine.minFatiguePercentPossibleToGet == 0 ? "You are fully rested." : "You wake up, unable to sleep any longer.", textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: true, blocksUpdatesBelow: true);
                }

                return;
            }

            string sleepModeText = "Sleeping...";
            if (this.sleepMode == SleepMode.WaitMorning) sleepModeText = "Waiting until morning...";
            if (this.sleepMode == SleepMode.WaitIndefinitely) sleepModeText = "Waiting indefinitely...";

            sleepModeText += $"\n{Helpers.KeepTextLineBelowGivenLength(LoadingTips.GetTip(), 46)}";

            this.sleepEngine.Execute(player: this);

            switch (this.sleepMode)
            {
                case SleepMode.Sleep:
                    SonOfRobinGame.SmallProgressBar.TurnOn(curVal: (int)(this.maxFatigue - this.Fatigue), maxVal: (int)this.maxFatigue, text: sleepModeText, addTransition: false, addNumbers: false);
                    break;

                case SleepMode.WaitMorning:
                    TimeSpan maxWaitingTime = TimeSpan.FromHours(9); // should match the timespan between night and morning
                    TimeSpan timeUntilMorning = world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning);
                    if (timeUntilMorning > maxWaitingTime) timeUntilMorning = maxWaitingTime;

                    SonOfRobinGame.SmallProgressBar.TurnOn(curVal: (int)(maxWaitingTime.TotalMinutes - timeUntilMorning.TotalMinutes), maxVal: (int)maxWaitingTime.TotalMinutes, text: sleepModeText, addNumbers: false);

                    break;

                case SleepMode.WaitIndefinitely:
                    SonOfRobinGame.SmallProgressBar.TurnOn(newPosX: 0, newPosY: 0, centerHoriz: true, centerVert: true, addTransition: false,
                        entryList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "Waiting...", color: Color.White) });
                    break;

                default:
                    throw new ArgumentException($"Unsupported sleepMode - {this.sleepMode}.");
            }
        }

        public void GoToSleep(SleepEngine sleepEngine, Vector2 zzzPos, bool checkIfSleepIsPossible)
        {
            if (checkIfSleepIsPossible && sleepEngine.minFatiguePercentPossibleToGet > 0 && this.FatiguePercent - sleepEngine.minFatiguePercentPossibleToGet < 0.05f)
            {
                new TextWindow(text: "I'm not tired enough to sleep here.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound); ;
                return;
            }

            this.SleepStarted = this.world.islandClock.IslandDateTime;
            this.sleepingInsideShelter = !sleepEngine.canBeAttacked;
            this.sleepMode = SleepMode.Sleep;
            this.sleepEngine = sleepEngine;
            LoadingTips.ChangeTip();

            if (this.visualAid != null) this.visualAid.Destroy();
            this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: zzzPos, templateName: PieceTemplate.Name.Zzz);

            if (!this.sleepEngine.canBeAttacked)
            {
                this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.RegenPoison); // to remove poison inside shelter
                this.sprite.Visible = false;
            }
            this.world.touchLayout = TouchLayout.WorldSleep;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldSleep;
            this.sprite.CharacterStand();
            this.activeState = State.PlayerControlledSleep;
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerSnore);

            new Scheduler.Task(taskName: Scheduler.TaskName.TempoFastForward, delay: 0, executeHelper: this.sleepEngine.updateMultiplier);

            MessageLog.AddMessage(message: "Going to sleep.");
        }

        public void WakeUp(bool force = false)
        {
            if (!this.CanWakeNow && !force)
            {
                new TextWindow(text: "You are too tired to wake up now.", textColor: Color.White, bgColor: Color.Red, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 8, priority: 1);
                return;
            }

            SonOfRobinGame.SmallProgressBar.TurnOff();

            bool showBadSleepHint = false;
            foreach (Buff buff in this.sleepEngine.wakeUpBuffs)
            {
                if (!this.sleepEngine.canBeAttacked &&
                    !buff.isPositive &&
                    buff.HadEnoughSleepForBuff(this.world)) showBadSleepHint = true;

                this.buffEngine.AddBuff(buff: buff, world: this.world);
            }

            if (this.visualAid != null) this.visualAid.Destroy();
            this.visualAid = null;

            this.activeState = State.PlayerControlledWalking;
            this.world.touchLayout = TouchLayout.WorldMain;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.sprite.Visible = true;
            this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.PlayerSnore);
            this.sleepMode = SleepMode.Awake;

            this.world.updateMultiplier = 1;
            SonOfRobinGame.Game.IsFixedTimeStep = Preferences.FrameSkip;
            Scheduler.RemoveAllTasksOfName(Scheduler.TaskName.TempoFastForward); // to prevent fast forward, when waking up before this task was executed

            MessageLog.AddMessage(debugMessage: true, message: "Waking up.");

            if (showBadSleepHint) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BadSleep, ignoreDelay: true);
        }

        public Rectangle GetFocusRect(int inflateX = 0, int inflateY = 0)
        {
            Rectangle focusRect = new Rectangle(x: (int)(this.sprite.position.X - 1), y: (int)(this.sprite.position.Y - 1), width: 1, height: 1);
            focusRect.Inflate(24 + inflateX, 24 + inflateY);

            Point focusCenterOffset = new Point(
                (int)Math.Round(((focusRect.Width / 2) + (inflateX / 2)) * Math.Cos(this.sprite.OrientationAngle)),
                (int)Math.Round(((focusRect.Height / 2) + (inflateY / 2)) * Math.Sin(this.sprite.OrientationAngle)));

            focusRect.X += focusCenterOffset.X;
            focusRect.Y += focusCenterOffset.Y;

            return focusRect;
        }

        private void PickUpClosestPiece(BoardPiece closestPiece)
        {
            if (closestPiece == null) return;

            this.world.HintEngine.Disable(Tutorials.Type.PickUp);

            bool piecePickedUp = this.PickUpPiece(piece: closestPiece);
            if (piecePickedUp)
            {
                Sound.QuickPlay(name: SoundData.Name.PickUpItem, volume: 0.6f);
                new RumbleEvent(force: 0.32f, durationSeconds: 0, smallMotor: true, fadeInSeconds: 0.065f, fadeOutSeconds: 0.065f);

                closestPiece.sprite.rotation = 0f;
                closestPiece.HeatLevel = 0f;
                if (closestPiece.GetType() == typeof(Animal)) closestPiece.HitPoints = closestPiece.maxHitPoints; // to prevent from showing health bar

                MessageLog.AddMessage(message: $"Picked up {closestPiece.readableName}.");
                this.world.HintEngine.CheckForPieceHintToShow(ignorePlayerState: true, newOwnedPieceNameToCheck: closestPiece.name);
            }
            else
            {
                new TextWindow(text: "My inventory is full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ShowHint, closingTaskHelper: HintEngine.Type.SmallInventory, animSound: this.world.DialogueSound);

                MessageLog.AddMessage(message: $"Inventory full - cannot pick up {closestPiece.readableName}.", avoidDuplicates: true);
            }
        }

        public bool PickUpPiece(BoardPiece piece)
        {
            bool pieceCollected = this.ToolStorage.AddPiece(piece);
            if (!pieceCollected) pieceCollected = this.PieceStorage.AddPiece(piece);

            return pieceCollected;
        }

        private bool TryToEnterShootingMode()
        {
            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;

            if (activeToolbarPiece?.GetType() != typeof(Tool)) return false;

            Tool activeTool = (Tool)activeToolbarPiece;
            if (activeTool.pieceInfo.toolShootsProjectile)
            {
                if (this.sprite.CanDrownHere)
                {
                    this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CantUseToolsInWater, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.AnimFrame.texture);

                    return false;
                }

                if (activeTool.CheckForAmmo(removePiece: false) == null)
                {
                    new TextWindow(text: $"No ammo for | {activeToolbarPiece.readableName}.", imageList: new List<Texture2D> { activeToolbarPiece.sprite.AnimFrame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);

                    return false;
                }

                this.world.touchLayout = TouchLayout.WorldShoot;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldShoot;
                this.activeState = State.PlayerControlledShooting;
                this.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerBowDraw);
                return true;
            }

            return false;
        }

        private bool UseToolbarPiece(bool isInShootingMode, bool buttonHeld = false, bool highlightOnly = false)
        {
            if (!this.CanUseActiveToolbarPiece) return false;

            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;

            if (!this.CanSeeAnything &&
                activeToolbarPiece?.GetType() != typeof(PortableLight) &&
                activeToolbarPiece?.pieceInfo.toolbarTask != Scheduler.TaskName.GetDrinked &&
                activeToolbarPiece?.pieceInfo.toolbarTask != Scheduler.TaskName.GetEaten)
            {
                if (!highlightOnly && !buttonHeld) // buttonHeld check is needed in case of destroying the only (plant) light source
                {
                    this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.TooDarkToUseTools, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.AnimFrame.texture);
                }
                return false;
            }

            if (!highlightOnly && this.sprite.CanDrownHere)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CantUseToolsInWater, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.AnimFrame.texture);

                return false;
            }

            if (activeToolbarPiece?.GetType() == typeof(Tool))
            {
                Tool activeTool = (Tool)activeToolbarPiece;
                if (activeTool.pieceInfo.toolShootsProjectile && !isInShootingMode)
                {
                    VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonBow));

                    if (highlightOnly && activeTool.CheckForAmmo(removePiece: false) != null)
                    {
                        VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                        ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                    }

                    return false;
                }
            }

            var executeHelper = new Dictionary<string, Object> {
                    { "player", this },
                    { "slot", this.ActiveSlot },
                    { "toolbarPiece", activeToolbarPiece },
                    { "shootingPower", this.shootingPower },
                    { "buttonHeld", buttonHeld },
                    { "highlightOnly", highlightOnly },
                };

            new Scheduler.Task(taskName: activeToolbarPiece.pieceInfo.toolbarTask, delay: 0, executeHelper: executeHelper);
            return true;
        }

        public void HarvestMeatInTheField(StorageSlot slot)
        {
            BoardPiece animalPiece = slot.GetAllPieces(remove: true)[0];
            var meatPieces = animalPiece.pieceInfo.Yield.GetAllPieces(piece: animalPiece);

            foreach (BoardPiece meatPiece in meatPieces)
            {
                if (slot.CanFitThisPiece(meatPiece)) slot.AddPiece(meatPiece);
                else slot.storage.AddPiece(piece: meatPiece, dropIfDoesNotFit: true);
            }

            this.world.meatHarvestStats.RegisterMeatHarvest(animalPiece: animalPiece, obtainedBasePieces: meatPieces, obtainedBonusPieces: new List<BoardPiece>());
            this.CheckForMeatHarvestingLevelUp();

            Sound.QuickPlay(SoundData.Name.KnifeSharpen);

            new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 15, executeHelper: SoundData.Name.HitFlesh1);

            if (meatPieces.Count == 0)
            {
                Scheduler.Task hintTask = new HintMessage(text: String.Join("\n", "Could not harvest anything..."), boxType: HintMessage.BoxType.Dialogue, delay: 0, blockInput: false).ConvertToTask();
                hintTask.Execute();
            }
            else if (meatPieces.Count >= 2)
            {
                new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 20, executeHelper: SoundData.Name.DestroyFlesh2);
                new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 30, executeHelper: SoundData.Name.DropMeat3);
            }
        }

        public bool CheckForCookLevelUp()
        {
            var levelUpData = new Dictionary<int, Dictionary<string, int>>
            {
                // { 2, new Dictionary<string, int> { { "minTotalCookCount", 1 }, { "minIngredientNamesCount", 1 }, { "minAllIngredientsCount", 1 } } }, // for testing
                { 2, new Dictionary<string, int> { { "minTotalCookCount", 3 }, { "minIngredientNamesCount", 2 }, { "minAllIngredientsCount", 3 } } },
                { 3, new Dictionary<string, int> { { "minTotalCookCount", 23 }, { "minIngredientNamesCount", 4 }, { "minAllIngredientsCount", 23 } } },
                { 4, new Dictionary<string, int> { { "minTotalCookCount", 123 }, { "minIngredientNamesCount", 6 }, { "minAllIngredientsCount", 263 } } },
                { 5, new Dictionary<string, int> { { "minTotalCookCount", 223 }, { "minIngredientNamesCount", 9 }, { "minAllIngredientsCount", 300 } } },
            };

            int nextLevel = this.CookLevel + 1;
            if (!levelUpData.ContainsKey(nextLevel)) return false;

            var levelDict = levelUpData[nextLevel];

            int minTotalCookCount = levelDict["minTotalCookCount"];
            int minIngredientNamesCount = levelDict["minIngredientNamesCount"];
            int minAllIngredientsCount = levelDict["minAllIngredientsCount"];

            bool levelUp =
                this.world.cookStats.TotalCookCount >= minTotalCookCount &&
                this.world.cookStats.IngredientNamesCount >= minIngredientNamesCount &&
                this.world.cookStats.AllIngredientsCount >= minAllIngredientsCount;

            if (levelUp)
            {
                bool levelMaster = nextLevel == levelUpData.Keys.Max();
                // levelMaster = true; // for testing

                string newLevelName = levelMaster ? "master |" : $"{nextLevel}";

                var imageList = new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.MealStandard].texture };
                if (levelMaster) imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Star].texture);

                new TextWindow(text: $"| Cooking level up!\n       Level {this.CookLevel} -> {newLevelName}", imageList: imageList, textColor: levelMaster ? Color.PaleGoldenrod : Color.White, bgColor: levelMaster ? Color.DarkGoldenrod : Color.DodgerBlue, useTransition: true, animate: true, blocksUpdatesBelow: true, blockInputDuration: 100, priority: 1, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1);

                this.CookLevel = nextLevel;

                Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.CookLevels, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return levelUp;
        }

        public bool CheckForAlchemyLevelUp()
        {
            var levelUpData = new Dictionary<int, Dictionary<string, int>>
            {
                //{ 2, new Dictionary<string, int> { { "minTotalCookCount", 1 }, { "minIngredientNamesCount", 1 }, { "minAllIngredientsCount", 1 } } }, // for testing
                { 2, new Dictionary<string, int> { { "minTotalCookCount", 3 }, { "minIngredientNamesCount", 2 }, { "minAllIngredientsCount", 6 } } },
                { 3, new Dictionary<string, int> { { "minTotalCookCount", 13 }, { "minIngredientNamesCount", 7 }, { "minAllIngredientsCount", 36 } } },
                { 4, new Dictionary<string, int> { { "minTotalCookCount", 63 }, { "minIngredientNamesCount", 12 }, { "minAllIngredientsCount", 166 } } },
                { 5, new Dictionary<string, int> { { "minTotalCookCount", 153 }, { "minIngredientNamesCount", 15 }, { "minAllIngredientsCount", 346 } } },
            };

            int nextLevel = this.BrewLevel + 1;
            if (!levelUpData.ContainsKey(nextLevel)) return false;

            var levelDict = levelUpData[nextLevel];

            int minTotalCookCount = levelDict["minTotalCookCount"];
            int minIngredientNamesCount = levelDict["minIngredientNamesCount"];
            int minAllIngredientsCount = levelDict["minAllIngredientsCount"];

            bool levelUp =
                this.world.brewStats.TotalCookCount >= minTotalCookCount &&
                this.world.brewStats.IngredientNamesCount >= minIngredientNamesCount &&
                this.world.brewStats.AllIngredientsCount >= minAllIngredientsCount;

            if (levelUp)
            {
                bool levelMaster = nextLevel == levelUpData.Keys.Max();
                // levelMaster = true; // for testing

                string newLevelName = levelMaster ? "master |" : $"{nextLevel}";

                var imageList = new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture };
                if (levelMaster) imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Star].texture);

                new TextWindow(text: $"| Alchemy level up!\n       Level {this.BrewLevel} -> {newLevelName}", imageList: imageList, textColor: levelMaster ? Color.PaleGoldenrod : Color.White, bgColor: levelMaster ? Color.DarkGoldenrod : Color.DodgerBlue, useTransition: true, animate: true, blocksUpdatesBelow: true, blockInputDuration: 100, priority: 1, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1);

                this.BrewLevel = nextLevel;

                Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.BrewLevels, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return levelUp;
        }

        public bool CheckForMeatHarvestingLevelUp()
        {
            var levelUpData = new Dictionary<int, Dictionary<string, int>>
            {
                // { 2, new Dictionary<string, int> { { "minAnimalNames", 1 }, { "minTotalCount", 1 }} }, // for testing
                { 2, new Dictionary<string, int> { { "minAnimalNames", 1 }, { "minTotalCount", 5 }} },
                { 3, new Dictionary<string, int> { { "minAnimalNames", 2 }, { "minTotalCount", 15 }} },
                { 4, new Dictionary<string, int> { { "minAnimalNames", 3 }, { "minTotalCount", 50 }} },
                { 5, new Dictionary<string, int> { { "minAnimalNames", 4 }, { "minTotalCount", 250 }} }
            };

            int nextLevel = this.HarvestLevel + 1;
            if (!levelUpData.ContainsKey(nextLevel)) return false;

            var levelDict = levelUpData[nextLevel];

            int minAnimalNames = levelDict["minAnimalNames"];
            int minTotalCount = levelDict["minTotalCount"];

            bool levelUp =
                this.world.meatHarvestStats.TotalHarvestCount >= minAnimalNames &&
                this.world.meatHarvestStats.AnimalSpeciesHarvested >= minTotalCount;

            if (levelUp)
            {
                bool levelMaster = nextLevel == levelUpData.Keys.Max();
                // levelMaster = true; // for testing

                string newLevelName = levelMaster ? "master |" : $"{nextLevel}";

                var imageList = new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.MeatRawPrime].texture };
                if (levelMaster) imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Star].texture);

                new TextWindow(text: $"| Meat harvesting level up!\n       Level {this.HarvestLevel} -> {newLevelName}", imageList: imageList, textColor: levelMaster ? Color.PaleGoldenrod : Color.White, bgColor: levelMaster ? Color.DarkGoldenrod : Color.DodgerBlue, useTransition: true, animate: true, blocksUpdatesBelow: true, blockInputDuration: 100, priority: 1, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1);

                this.HarvestLevel = nextLevel;

                Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.MeatHarvestLevels, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return levelUp;
        }

        public bool CheckForCraftLevelUp()
        {
            var levelUpData = new Dictionary<int, Dictionary<string, int>>
            {
                //{ 2, new Dictionary<string, int> { { "minUniqueRecipesCraftedTotal", 1 }, { "minTotalNoOfCrafts", 2 } } }, // for testing
                { 2, new Dictionary<string, int> { { "minUniqueRecipesCraftedTotal", 6 }, { "minTotalNoOfCrafts", 10 } } },
                { 3, new Dictionary<string, int> { { "minUniqueRecipesCraftedTotal", 15 }, { "minTotalNoOfCrafts", 30 } } },
                { 4, new Dictionary<string, int> { { "minUniqueRecipesCraftedTotal", 30 }, { "minTotalNoOfCrafts", 150 } } },
                { 5, new Dictionary<string, int> { { "minUniqueRecipesCraftedTotal", 60 }, { "minTotalNoOfCrafts", 350 } } },
            };

            if (levelUpData.Keys.Max() != maxCraftLevel) throw new ArgumentException($"LevelUpData {levelUpData.Keys.Max()} does not match maxCraftLevel {maxCraftLevel}.");

            int nextLevel = this.CraftLevel + 1;
            if (!levelUpData.ContainsKey(nextLevel)) return false;

            var levelDict = levelUpData[nextLevel];

            int minUniqueRecipesCraftedTotal = levelDict["minUniqueRecipesCraftedTotal"];
            int minTotalNoOfCrafts = levelDict["minTotalNoOfCrafts"];

            bool levelUp =
                this.world.craftStats.UniqueRecipesCraftedTotal >= minUniqueRecipesCraftedTotal &&
                this.world.craftStats.TotalNoOfCrafts >= minTotalNoOfCrafts;

            if (levelUp) this.CraftLevel = nextLevel;

            return levelUp;
        }
    }
}