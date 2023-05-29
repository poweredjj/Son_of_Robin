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
        public enum SleepMode
        { Awake, Sleep, WaitMorning, WaitIndefinitely };

        private const int maxShootingPower = 90;
        public const int maxLastStepsCount = 100;
        public const int maxCraftLevel = 5;
        public const int heatFatigueMultiplier = 2;

        public int maxFedLevel;
        public int fedLevel;
        public float maxStamina;
        public float stamina;
        public float maxFatigue;
        private float fatigue;
        public int CraftLevel { get; private set; }
        public int CookLevel { get; private set; }
        public int BrewLevel { get; private set; }
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

        public Player(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, int strength, float speed, float maxStamina, float maxHitPoints, float maxFatigue, int craftLevel, byte invWidth, byte invHeight, byte toolbarWidth, byte toolbarHeight, float fireAffinity,
            byte animSize = 0, string animName = "default", bool blocksMovement = true, bool ignoresCollisions = false, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int minDistance = 0, int maxDistance = 100, PieceSoundPack soundPack = null, int cookLevel = 1, int brewLevel = 1) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, mass: 50000, maxMassForSize: null, generation: generation, canBePickedUp: false, maxHitPoints: maxHitPoints, readableName: readableName, description: description, yield: yield, strength: strength, category: Category.Flesh, ignoresCollisions: ignoresCollisions, minDistance: minDistance, maxDistance: maxDistance, activeState: activeState, soundPack: soundPack, isAffectedByWind: false, fireAffinity: fireAffinity)
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = maxStamina;
            this.stamina = maxStamina;
            this.maxFatigue = maxFatigue;
            this.fatigue = 0;
            this.CraftLevel = craftLevel;
            this.CookLevel = cookLevel;
            this.BrewLevel = brewLevel;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.LastSteps = new List<Vector2>();
            this.previousStepPos = new Vector2(-100, -100); // initial value, to be changed later
            this.distanceWalked = 0;

            var allowedToolbarPieces = new List<PieceTemplate.Name> { PieceTemplate.Name.LanternEmpty }; // indivitual cases, that will not be added below

            if (PieceInfo.HasBeenInitialized)
            {
                List<Type> typeList = new List<Type> { typeof(Tool), typeof(PortableLight), typeof(Projectile), typeof(Seed) };

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
                            if (!buff.isPositive && buff.type == BuffEngine.BuffType.RegenPoison)
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

            this.PieceStorage = new PieceStorage(width: invWidth, height: invHeight, storagePiece: this, storageType: PieceStorage.StorageType.Inventory);
            this.ToolStorage = new PieceStorage(width: toolbarWidth, height: toolbarHeight, storagePiece: this, storageType: PieceStorage.StorageType.Tools, allowedPieceNames: allowedToolbarPieces);
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

            BoardPiece handTool = PieceTemplate.Create(templateName: PieceTemplate.Name.KnifeSimple, world: this.world);

            StorageSlot handSlot = this.ToolStorage.FindCorrectSlot(handTool);
            this.ToolStorage.AddPiece(handTool);
            handSlot.locked = true;
        }

        public override bool ShowStatBars
        { get { return true; } }

        public float DistanceWalkedKilometers
        { get { return (float)Math.Round(this.distanceWalked / 5000, 2); } }

        private bool ShootingModeInputPressed
        {
            get
            {
                return
                    InputMapper.IsPressed(InputMapper.Action.WorldUseToolbarPiece) ||
                    (TouchInput.IsBeingTouchedInAnyWay && (Math.Abs(this.world.analogCameraCorrection.X) > 0.05f ||
                    Math.Abs(this.world.analogCameraCorrection.Y) > 0.05f));
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
        { get { return new List<PieceStorage> { this.ToolStorage, this.PieceStorage, this.EquipStorage }; } } // the same as above, changed order

        public StorageSlot ActiveSlot
        { get { return this.ToolStorage?.lastUsedSlot; } }

        public BoardPiece ActiveToolbarPiece
        { get { return this.ActiveSlot?.TopPiece; } }

        private bool CanUseActiveToolbarPiece
        {
            get
            {
                BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;
                return activeToolbarPiece != null && activeToolbarPiece.toolbarTask != Scheduler.TaskName.Empty;
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
                if (!activeTool.shootsProjectile) return false;

                return activeTool.CheckForAmmo(removePiece: false) != null;
            }
        }

        private BoardPiece ClosestPieceToInteract
        {
            get
            {
                Vector2 centerOffset = this.GetCenterOffset();
                int offsetX = (int)centerOffset.X;
                int offsetY = (int)centerOffset.Y;

                var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);

                var interestingPieces = nearbyPieces.Where(piece => piece.boardTask != Scheduler.TaskName.Empty).ToList();
                if (interestingPieces.Count > 0)
                {
                    BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: interestingPieces, offsetX: offsetX, offsetY: offsetY);
                    return closestPiece;
                }
                else return null;
            }
        }

        private BoardPiece ClosestPieceToPickUp
        {
            get
            {
                if (!this.CanSeeAnything) return null;

                Vector2 centerOffset = this.GetCenterOffset();
                int offsetX = (int)centerOffset.X;
                int offsetY = (int)centerOffset.Y;

                var interestingPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);
                interestingPieces = interestingPieces.Where(piece => piece.canBePickedUp);
                if (!interestingPieces.Any()) return null;

                BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: interestingPieces, offsetX: offsetX, offsetY: offsetY);
                return closestPiece;
            }
        }

        public float Fatigue
        {
            get { return this.fatigue; }
            set
            {
                if (Preferences.DebugGodMode) return;

                float fatigueDifference = value - this.fatigue;
                if (fatigueDifference > 0 && this.buffEngine.HasBuff(BuffEngine.BuffType.Heat)) fatigueDifference *= heatFatigueMultiplier;

                this.fatigue = Math.Min(Math.Max(this.fatigue + fatigueDifference, 0), this.maxFatigue);

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

        public float Stamina
        {
            get { return this.stamina; }
            set
            {
                float staminaDifference = value - this.stamina;
                this.stamina = value;
                if (staminaDifference < 0) this.Fatigue -= staminaDifference / 10f;
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
            };

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
            this.yield.DropFinalPieces();
            base.Destroy();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["player_fedLevel"] = this.fedLevel;
            pieceData["player_maxFedLevel"] = this.maxFedLevel;
            pieceData["player_stamina"] = this.stamina;
            pieceData["player_maxStamina"] = this.maxStamina;
            pieceData["player_fatigue"] = this.fatigue;
            pieceData["player_maxFatigue"] = this.maxFatigue;
            pieceData["player_craftLevel"] = this.CraftLevel;
            pieceData["player_cookLevel"] = this.CookLevel;
            pieceData["player_brewLevel"] = this.BrewLevel;
            pieceData["player_distanceWalked"] = this.distanceWalked;
            pieceData["player_toolStorage"] = this.ToolStorage.Serialize();
            pieceData["player_equipStorage"] = this.EquipStorage.Serialize();
            pieceData["player_globalChestStorage"] = this.GlobalChestStorage.Serialize();
            pieceData["player_LastSteps"] = this.LastSteps.Select(s => new Point((int)s.X, (int)s.Y)).ToList();

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.fedLevel = (int)(Int64)pieceData["player_fedLevel"];
            this.maxFedLevel = (int)(Int64)pieceData["player_maxFedLevel"];
            this.stamina = (float)(double)pieceData["player_stamina"];
            this.maxStamina = (float)(double)pieceData["player_maxStamina"];
            this.fatigue = (float)(double)pieceData["player_fatigue"];
            this.maxFatigue = (float)(double)pieceData["player_maxFatigue"];
            this.CraftLevel = (int)(Int64)pieceData["player_craftLevel"];
            this.CookLevel = (int)(Int64)pieceData["player_cookLevel"];
            this.BrewLevel = (int)(Int64)pieceData["player_brewLevel"];
            this.distanceWalked = (float)(double)pieceData["player_distanceWalked"];
            this.ToolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], storagePiece: this);
            this.EquipStorage = PieceStorage.Deserialize(storageData: pieceData["player_equipStorage"], storagePiece: this);
            this.GlobalChestStorage = PieceStorage.Deserialize(storageData: pieceData["player_globalChestStorage"], storagePiece: this);
            List<Point> lastStepsPointList = (List<Point>)pieceData["player_LastSteps"];
            this.LastSteps = lastStepsPointList.Select(p => new Vector2(p.X, p.Y)).ToList();
            this.RefreshAllowedPiecesForStorages();
        }

        public void RefreshAllowedPiecesForStorages() // for older saves compatibility, to ensure that all storages have the right allowedPieceNames
        {
            Player tempPlayer = (Player)PieceTemplate.Create(templateName: this.name, world: this.world);

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
                new StatBar(width: 80, height: 5, label: "power", value: (int)this.shootingPower, valueMax: (int)maxShootingPower, colorMin: new Color(220, 255, 0), colorMax: new Color(255, 0, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom);
                StatBar.FinishThisBatch();
            }
        }

        public void ExpendEnergy(float energyAmount, bool addFatigue = true)
        {
            if (Preferences.DebugGodMode) return;

            if (this.fedLevel > 0)
            {
                this.fedLevel = Convert.ToInt32(Math.Max(this.fedLevel - Math.Max(energyAmount / 2, 1), 0));

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
            this.Stamina = this.maxStamina;

            if (this.FedPercent > 0.8f) this.world.HintEngine.Enable(HintEngine.Type.Hungry);
            if (this.FedPercent > 0.4f) this.world.HintEngine.Enable(HintEngine.Type.VeryHungry);
            if (this.FedPercent > 0.1f) this.world.HintEngine.Enable(HintEngine.Type.Starving);
        }

        public float ConvertMassToEnergyAmount(float mass)
        {
            return mass * 40f;
        }

        private int ConvertEnergyAmountToFedLevel(float energyAmount)
        {
            return (int)energyAmount * 2;
        }

        private float ConvertFedLevelToFedPercent(int fedLevel)
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
            if (this.world.MapEnabled && (!this.LastSteps.Any() || Vector2.Distance(this.sprite.position, this.LastSteps.Last()) > 180))
            {
                this.LastSteps.Add(this.sprite.position);
                if (this.LastSteps.Count > maxLastStepsCount) this.LastSteps.RemoveAt(0);
            }
        }

        public override void SM_PlayerControlledBuilding()
        {
            Vector2 newPos = this.simulatedPieceToBuild.sprite.position + this.world.analogMovementLeftStick;

            if (this.world.analogMovementLeftStick != Vector2.Zero) newPos += this.world.analogMovementLeftStick;
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

            int pieceSize = Math.Max(this.simulatedPieceToBuild.sprite.frame.colWidth, this.simulatedPieceToBuild.sprite.frame.colHeight);

            float buildDistance = Vector2.Distance(this.sprite.position, newPos);
            if (buildDistance <= 200 + pieceSize) this.simulatedPieceToBuild.sprite.SetNewPosition(newPos: newPos, ignoreCollisions: true);

            bool canBuildHere = buildDistance <= 80 + pieceSize && !this.simulatedPieceToBuild.sprite.CheckForCollision(ignoreDensity: true);
            if (canBuildHere)
            {
                VirtButton.ButtonHighlightOnNextFrame(VButName.Confirm);
                ControlTips.TipHighlightOnNextFrame(tipName: "place");
            }

            Color color = canBuildHere ? Color.Green : Color.Red;

            this.simulatedPieceToBuild.sprite.effectCol.AddEffect(new ColorizeInstance(color: color));
            this.simulatedPieceToBuild.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.White, textureSize: this.simulatedPieceToBuild.sprite.frame.textureSize, priority: 0));

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                this.world.ExitBuildMode(restoreCraftMenu: true);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm))
            {
                if (canBuildHere) this.world.BuildPiece();
                else new TextWindow(text: $"|  {Helpers.FirstCharToUpperCase(this.simulatedPieceToBuild.readableName)} can't be placed here.", imageList: new List<Texture2D> { this.simulatedPieceToBuild.sprite.frame.texture }, textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, checkForDuplicate: true, autoClose: false, inputType: Scene.InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, startingSound: SoundData.Name.Error);
            }
        }

        public override void SM_PlayerWaitForBuilding()
        {
            // needs to be updated from the outside

            this.world.islandClock.Advance(amount: this.buildDurationForOneFrame, ignorePause: true);
            this.Fatigue += this.buildFatigueForOneFrame;
            world.Player.Fatigue = Math.Min(world.Player.Fatigue, world.Player.maxFatigue - 20); // to avoid falling asleep just after crafting
        }

        public override void SM_PlayerControlledGhosting()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldPauseMenu))
            {
                this.world.OpenPauseMenu();
                return;
            }

            this.Walk(slowDownInWater: false);
        }

        public override void SM_PlayerControlledByCinematic()
        {
            if (this.pointWalkTarget != Vector2.Zero)
            {
                this.GoOneStepTowardsGoal(this.pointWalkTarget, splitXY: true, walkSpeed: this.speed);
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
                this.world.OpenPauseMenu();
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
                if (InputMapper.HasBeenPressed(InputMapper.Action.WorldSprintToggle)) this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Sprint, autoRemoveDelay: 3 * 60, value: 2f), world: this.world);
            }

            if (this.world.CurrentUpdate % 121 == 0) this.world.HintEngine.CheckForPieceHintToShow();

            this.ExpendEnergy(0.1f);
            if (!this.Walk()) this.Stamina = Math.Min(this.Stamina + 1, this.maxStamina);

            this.CheckGround();
            this.CheckLowHP();

            // highlighting pieces to interact with and corresponding interface elements

            if (this.world.inputActive) this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: true); // only to highlight pieces that will be hit

            BoardPiece pieceToInteract = this.ClosestPieceToInteract;
            if (pieceToInteract != null)
            {
                if (this.world.inputActive)
                {
                    pieceToInteract.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Green));
                    Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Interact, world: this.world);
                    VirtButton.ButtonHighlightOnNextFrame(VButName.Interact);
                    ControlTips.TipHighlightOnNextFrame(tipName: "interact");
                    FieldTip.AddUpdateTip(world: this.world, texture: InputMapper.GetTexture(InputMapper.Action.WorldInteract), targetSprite: pieceToInteract.sprite, alignment: FieldTip.Alignment.LeftIn);
                }
            }

            BoardPiece pieceToPickUp = this.ClosestPieceToPickUp;
            if (pieceToPickUp != null)
            {
                if (this.world.inputActive)
                {
                    Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.PickUp, world: this.world);
                    pieceToPickUp.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.DodgerBlue));
                    pieceToPickUp.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.White, textureSize: pieceToPickUp.sprite.frame.textureSize, priority: 0));
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

            bool pickUp = InputMapper.HasBeenPressed(InputMapper.Action.WorldPickUp) ||
                (Preferences.PointToInteract && pieceToPickUp != null && pieceToPickUp.sprite.gfxRect.Contains(this.pointWalkTarget));

            if (pickUp)
            {
                this.pointWalkTarget = Vector2.Zero; // to avoid picking up indefinitely
                this.PickUpClosestPiece(closestPiece: pieceToPickUp);
                return;
            }

            bool interact = !pickUp && (InputMapper.HasBeenPressed(InputMapper.Action.WorldInteract) ||
                (Preferences.PointToInteract && pieceToInteract != null && pieceToInteract.sprite.gfxRect.Contains(this.pointWalkTarget)));

            if (interact)
            {
                if (pieceToInteract != null)
                {
                    this.pointWalkTarget = Vector2.Zero; // to avoid interacting indefinitely
                    this.world.HintEngine.Disable(Tutorials.Type.Interact);
                    new Scheduler.Task(taskName: pieceToInteract.boardTask, delay: 0, executeHelper: pieceToInteract);
                }
            }
        }

        private bool Walk(bool setOrientation = true, bool slowDownInWater = true)
        {
            Vector2 movement = this.world.analogMovementLeftStick;
            if (movement != Vector2.Zero) this.pointWalkTarget = Vector2.Zero;

            bool layoutChangedRecently = TouchInput.FramesSinceLayoutChanged < 5;

            if (movement == Vector2.Zero && Preferences.PointToWalk && !layoutChangedRecently)
            {
                foreach (TouchLocation touch in TouchInput.TouchPanelState)
                {
                    if (touch.State == TouchLocationState.Pressed && !TouchInput.IsPointActivatingAnyTouchInterface(touch.Position))
                    {
                        Vector2 worldTouchPos = this.world.TranslateScreenToWorldPos(touch.Position);
                        //  var crosshair = PieceTemplate.CreateOnBoard(world: world, position: worldTouchPos, templateName: PieceTemplate.Name.Crosshair); // for testing
                        //  new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: 60, boardPiece: crosshair); // for testing

                        this.pointWalkTarget = worldTouchPos;
                        break;
                    }
                }

                if (!this.PointWalkTargetReached)
                {
                    movement = this.pointWalkTarget - this.sprite.position;

                    if (Math.Abs(movement.X) < 4) movement.X = 0; // to avoid animation flickering
                    if (Math.Abs(movement.Y) < 4) movement.Y = 0; // to avoid animation flickering

                    if (movement.X == 0 && movement.Y == 0) this.pointWalkTarget = Vector2.Zero;
                }
                else this.pointWalkTarget = Vector2.Zero;
            }
            else this.pointWalkTarget = Vector2.Zero;

            if (movement == Vector2.Zero)
            {
                this.sprite.CharacterStand();
                return false;
            }

            var currentSpeed = this.IsVeryTired ? this.speed / 2f : this.speed;

            movement *= currentSpeed;

            Vector2 goalPosition = this.sprite.position + movement;
            bool hasBeenMoved = this.GoOneStepTowardsGoal(goalPosition, splitXY: true, walkSpeed: currentSpeed, setOrientation: setOrientation, slowDownInWater: slowDownInWater);

            if (hasBeenMoved)
            {
                this.ExpendEnergy(0.2f);

                int staminaUsed = 0;

                if (this.sprite.IsInWater) staminaUsed = 1;
                if (staminaUsed > 0) this.Stamina = Math.Max(this.Stamina - staminaUsed, 0);
            }

            return hasBeenMoved;
        }

        private void CheckGround()
        {
            // adding and removing heat
            if (this.world.CurrentUpdate % 65 == 0)
            {
                if (this.world.islandClock.CurrentPartOfDay == IslandClock.PartOfDay.Noon && this.world.weather.SunVisibility >= 0.8f && !this.world.weather.IsRaining)
                {
                    this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Heat, value: null), world: this.world);
                }
                else this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Heat);

                if (this.buffEngine.HasBuff(BuffEngine.BuffType.Heat)) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Heat, world: this.world, ignoreDelay: true);
            }

            if (this.sprite.IsInWater || this.world.weather.IsRaining) this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.Wet, value: null, autoRemoveDelay: 40 * 60), world: this.world);

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
                            Sound.QuickPlay(name: SoundData.Name.SplashMud, volume: 1f);

                            this.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.RegenPoison, value: -70, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true), world: this.world);
                        }
                    }
                }
            }

            bool isOnLava = this.sprite.IsOnLava;
            if (isOnLava)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lava, ignoreDelay: true);
                this.HitPoints -= 0.5f;
                this.BurnLevel += 0.12f;
                this.soundPack.Play(PieceSoundPack.Action.StepLava);
            }

            bool gotHitWithLightning = this.world.weather.LightningJustStruck && this.sprite.IsInWater && this.world.random.Next(2) == 0;

            if (gotHitWithLightning)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lightning, ignoreDelay: true);
                this.HitPoints -= 80;
                Sound.QuickPlay(SoundData.Name.ElectricShock);
            }

            if (isOnLava || gotHitWithLightning)
            {
                if (!this.world.solidColorManager.AnySolidColorPresent)
                {
                    this.soundPack.Play(PieceSoundPack.Action.Cry);
                    this.world.camera.AddRandomShake();
                    this.world.FlashRedOverlay();
                }
            }
        }

        public void CheckLowHP()
        {
            if (this.HitPointsPercent < 0.15f)
            {
                if (!this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.AddBuff(world: this.world, buff: new Buff(type: BuffEngine.BuffType.LowHP, value: 0, autoRemoveDelay: 0));
            }
            else
            {
                if (this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.LowHP);
            }
        }

        public override void SM_PlayerControlledShooting()
        {
            this.ExpendEnergy(0.1f);
            this.CheckGround();
            this.CheckLowHP();

            this.shootingPower = Math.Min(this.shootingPower + 1, maxShootingPower);

            this.Stamina = Math.Max(this.Stamina - 1, 0);

            if (this.visualAid == null) this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Crosshair);

            this.Walk(setOrientation: false);

            // shooting angle should be set once at the start
            if (this.ShootingAngle == -100) this.ShootingAngle = this.sprite.GetAngleFromOrientation();

            Vector2 moving = this.world.analogMovementLeftStick;
            Vector2 shooting = this.world.analogMovementRightStick;

            // right mouse button will be released when shooting and should be taken into account, to set the angle correctly
            if (Mouse.RightIsDown || Mouse.RightHasBeenReleased) shooting = (this.world.TranslateScreenToWorldPos(Mouse.Position) - this.sprite.position) / 20;

            Vector2 directionVector = Vector2.Zero;
            if (moving != Vector2.Zero) directionVector = moving;
            if (shooting != Vector2.Zero) directionVector = shooting;

            if (directionVector != Vector2.Zero)
            {
                this.sprite.SetOrientationByMovement(directionVector);
                Vector2 goalPosition = this.sprite.position + directionVector; // used to calculate shooting angle
                this.ShootingAngle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: goalPosition);
            }

            int aidDistance = (int)(SonOfRobinGame.VirtualWidth * 0.08f);
            int aidOffsetX = (int)Math.Round(aidDistance * Math.Cos(this.ShootingAngle));
            int aidOffsetY = (int)Math.Round(aidDistance * Math.Sin(this.ShootingAngle));
            Vector2 aidPos = this.sprite.position + new Vector2(aidOffsetX, aidOffsetY);
            this.visualAid.sprite.SetNewPosition(aidPos);

            if (VirtButton.HasButtonBeenPressed(VButName.Shoot) || // virtual button has to be checked separately here
                InputMapper.HasBeenReleased(InputMapper.Action.WorldUseToolbarPiece) ||
                Mouse.RightHasBeenReleased)
            {
                this.UseToolbarPiece(isInShootingMode: true, buttonHeld: false);
                this.shootingPower = 0;
                this.soundPack.Stop(PieceSoundPack.Action.PlayerBowDraw);
                this.soundPack.Play(PieceSoundPack.Action.PlayerBowDraw);
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
                this.soundPack.Stop(PieceSoundPack.Action.PlayerBowDraw);
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

            if (this.sleepMode == SleepMode.Sleep && this.Fatigue == 0)
            {
                this.sleepMode = SleepMode.WaitIndefinitely;
                this.soundPack.Stop(PieceSoundPack.Action.PlayerSnore);
                this.world.islandClock.multiplier *= 3; // to speed up waiting

                this.visualAid.Destroy();
                this.visualAid = null;

                if (this.sleepEngine.waitingAfterSleepPossible)
                {
                    var optionList = new List<object>();

                    optionList.Add(new Dictionary<string, object> { { "label", "go out" }, { "taskName", Scheduler.TaskName.ForceWakeUp }, { "executeHelper", this } });

                    if (this.world.islandClock.CurrentPartOfDay == IslandClock.PartOfDay.Night) optionList.Add(new Dictionary<string, object> { { "label", "wait until morning" }, { "taskName", Scheduler.TaskName.WaitUntilMorning }, { "executeHelper", this } });

                    optionList.Add(new Dictionary<string, object> { { "label", "wait indefinitely" }, { "taskName", Scheduler.TaskName.Empty }, { "executeHelper", null } });

                    var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "You are fully rested." }, { "customOptionList", optionList } };

                    new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                }
                else
                {
                    this.WakeUp();

                    new TextWindow(text: "You are fully rested.", textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: true, blocksUpdatesBelow: true);
                }

                return;
            }

            string sleepModeText = "Sleeping...";
            if (this.sleepMode == SleepMode.WaitMorning) sleepModeText = "Waiting until morning...";
            if (this.sleepMode == SleepMode.WaitIndefinitely) sleepModeText = "Waiting indefinitely...";

            this.sleepEngine.Execute(player: this);
            if (this.world.CurrentUpdate % 10 == 0)
            {
                switch (this.sleepMode)
                {
                    case SleepMode.Sleep:
                        SonOfRobinGame.SmallProgressBar.TurnOn(curVal: (int)(this.maxFatigue - this.Fatigue), maxVal: (int)this.maxFatigue, text: sleepModeText, addTransition: false, addNumbers: false);
                        break;

                    case SleepMode.WaitMorning:
                        TimeSpan maxWaitingTime = TimeSpan.FromHours(9); // should match the timespan between night and morning
                        TimeSpan timeUntilMorning = world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning);

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
        }

        public void GoToSleep(SleepEngine sleepEngine, Vector2 zzzPos)
        {
            this.SleepStarted = this.world.islandClock.IslandDateTime;
            this.sleepingInsideShelter = !sleepEngine.canBeAttacked;
            this.sleepMode = SleepMode.Sleep;
            this.sleepEngine = sleepEngine;
            this.world.islandClock.multiplier = this.sleepEngine.islandClockMultiplier;

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
            this.soundPack.Play(PieceSoundPack.Action.PlayerSnore);

            new Scheduler.Task(taskName: Scheduler.TaskName.TempoFastForward, delay: 0, executeHelper: 8);

            MessageLog.AddMessage(msgType: MsgType.User, message: "Going to sleep.");
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
                if (!buff.isPositive &&
                    !this.world.HintEngine.shownGeneralHints.Contains(HintEngine.Type.BadSleep) &&
                    buff.HadEnoughSleepForBuff(this.world)) showBadSleepHint = true;

                this.buffEngine.AddBuff(buff: buff, world: this.world);
            }

            if (this.visualAid != null) this.visualAid.Destroy();
            this.visualAid = null;

            this.world.islandClock.multiplier = 1;
            this.activeState = State.PlayerControlledWalking;
            this.world.touchLayout = TouchLayout.WorldMain;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.sprite.Visible = true;
            this.soundPack.Stop(PieceSoundPack.Action.PlayerSnore);
            this.sleepMode = SleepMode.Awake;

            this.world.updateMultiplier = 1;
            SonOfRobinGame.Game.IsFixedTimeStep = Preferences.FrameSkip;
            Scheduler.RemoveAllTasksOfName(Scheduler.TaskName.TempoFastForward); // to prevent fast forward, when waking up before this task was executed

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Waking up.");

            if (showBadSleepHint) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BadSleep, ignoreDelay: true);
        }

        private Vector2 GetCenterOffset()
        {
            int offsetX = 0;
            int offsetY = 0;
            int offset = 20;

            switch (this.sprite.orientation)
            {
                case Sprite.Orientation.left:
                    offsetX -= offset;
                    break;

                case Sprite.Orientation.right:
                    offsetX += offset;
                    break;

                case Sprite.Orientation.up:
                    offsetY -= offset;
                    break;

                case Sprite.Orientation.down:
                    offsetY += offset;
                    break;

                default:
                    throw new ArgumentException($"Unsupported sprite orientation - {this.sprite.orientation}.");
            }

            return new Vector2(offsetX, offsetY);
        }

        private void PickUpClosestPiece(BoardPiece closestPiece)
        {
            if (closestPiece == null) return;

            this.world.HintEngine.Disable(Tutorials.Type.PickUp);

            bool piecePickedUp = this.PickUpPiece(piece: closestPiece);
            if (piecePickedUp)
            {
                Sound.QuickPlay(name: SoundData.Name.PickUpItem, volume: 0.6f);

                closestPiece.sprite.rotation = 0f;
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Picked up {closestPiece.readableName}.");
                this.world.HintEngine.CheckForPieceHintToShow(newOwnedPieceNameToCheck: closestPiece.name);
            }
            else
            {
                new TextWindow(text: "My inventory is full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ShowHint, closingTaskHelper: HintEngine.Type.SmallInventory, animSound: this.world.DialogueSound);

                MessageLog.AddMessage(msgType: MsgType.User, message: $"Inventory full - cannot pick up {closestPiece.readableName}.");
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
            if (activeTool.shootsProjectile)
            {
                if (this.sprite.CanDrownHere)
                {
                    this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CantUseToolsInWater, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.frame.texture);

                    return false;
                }

                if (activeTool.CheckForAmmo(removePiece: false) == null)
                {
                    new TextWindow(text: $"No ammo for | {activeToolbarPiece.readableName}.", imageList: new List<Texture2D> { activeToolbarPiece.sprite.frame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);

                    return false;
                }

                this.world.touchLayout = TouchLayout.WorldShoot;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldShoot;
                this.activeState = State.PlayerControlledShooting;
                this.soundPack.Play(PieceSoundPack.Action.PlayerBowDraw);
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
                activeToolbarPiece?.toolbarTask != Scheduler.TaskName.GetDrinked &&
                activeToolbarPiece?.toolbarTask != Scheduler.TaskName.GetEaten)
            {
                if (!highlightOnly) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.TooDarkToUseTools, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.frame.texture);
                return false;
            }

            if (!highlightOnly && this.sprite.CanDrownHere)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CantUseToolsInWater, ignoreDelay: true, text: activeToolbarPiece.readableName, texture: activeToolbarPiece.sprite.frame.texture);

                return false;
            }

            if (activeToolbarPiece?.GetType() == typeof(Tool))
            {
                Tool activeTool = (Tool)activeToolbarPiece;
                if (activeTool.shootsProjectile && !isInShootingMode)
                {
                    if (highlightOnly && activeTool.CheckForAmmo(removePiece: false) != null)
                    {
                        VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                        ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                    }

                    return false;
                }
            }

            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            var executeHelper = new Dictionary<string, Object> {
                    {"player", this},
                    {"slot", this.ActiveSlot},
                    {"toolbarPiece", activeToolbarPiece},
                    {"shootingPower", this.shootingPower},
                    {"offsetX", offsetX},
                    {"offsetY", offsetY},
                    {"buttonHeld", buttonHeld},
                    {"highlightOnly", highlightOnly},
                };

            new Scheduler.Task(taskName: activeToolbarPiece.toolbarTask, delay: 0, executeHelper: executeHelper);
            return true;
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
                if (levelMaster) imageList.Add(PieceInfo.GetInfo(PieceTemplate.Name.DebrisStar).texture);

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
                if (levelMaster) imageList.Add(PieceInfo.GetInfo(PieceTemplate.Name.DebrisStar).texture);

                new TextWindow(text: $"| Alchemy level up!\n       Level {this.BrewLevel} -> {newLevelName}", imageList: imageList, textColor: levelMaster ? Color.PaleGoldenrod : Color.White, bgColor: levelMaster ? Color.DarkGoldenrod : Color.DodgerBlue, useTransition: true, animate: true, blocksUpdatesBelow: true, blockInputDuration: 100, priority: 1, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1);

                this.BrewLevel = nextLevel;

                Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.BrewLevels, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
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