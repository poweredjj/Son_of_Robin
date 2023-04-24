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

        public int maxFedLevel;
        public int fedLevel;
        public float maxStamina;
        public float stamina;
        public float cookingSkill;
        public float maxFatigue;
        private float fatigue;
        public int craftLevel;
        public float ShootingAngle { get; private set; }
        private int shootingPower;
        private SleepEngine sleepEngine;
        public List<Vector2> LastSteps { get; private set; }

        public Vector2 pointWalkTarget;
        public Craft.Recipe recipeToBuild;
        public BoardPiece simulatedPieceToBuild;
        public int buildDurationForOneFrame;
        public float buildFatigueForOneFrame;
        public PieceStorage ToolStorage { get; private set; }
        public PieceStorage EquipStorage { get; private set; }

        public Player(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool female, int strength, float speed, float maxStamina, float maxHitPoints, float maxFatigue, int craftLevel, float cookingSkill, byte invWidth, byte invHeight, byte toolbarWidth, byte toolbarHeight, float fireAffinity,
            byte animSize = 0, string animName = "default", bool blocksMovement = true, bool ignoresCollisions = false, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int minDistance = 0, int maxDistance = 100, LightEngine lightEngine = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, mass: 50000, maxMassForSize: null, generation: generation, canBePickedUp: false, maxHitPoints: maxHitPoints, readableName: readableName, description: description, yield: yield, strength: strength, category: Category.Flesh, lightEngine: lightEngine, ignoresCollisions: ignoresCollisions, minDistance: minDistance, maxDistance: maxDistance, activeState: activeState, soundPack: soundPack, female: female, isAffectedByWind: false, fireAffinity: fireAffinity)
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = maxStamina;
            this.stamina = maxStamina;
            this.maxFatigue = maxFatigue;
            this.fatigue = 0;
            this.cookingSkill = cookingSkill;
            this.craftLevel = craftLevel;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.LastSteps = new List<Vector2>();

            var allowedToolbarPieces = new List<PieceTemplate.Name> { PieceTemplate.Name.LanternFrame }; // indivitual cases, that will not be added below

            if (PieceInfo.HasBeenInitialized)
            {
                List<Type> typeList = new List<Type> { typeof(Tool), typeof(PortableLight), typeof(Projectile) };

                foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
                {
                    PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                    if (pieceInfo.toolbarTask == Scheduler.TaskName.GetEaten ||
                        pieceInfo.toolbarTask == Scheduler.TaskName.GetDrinked ||
                        typeList.Contains(pieceInfo.type)) allowedToolbarPieces.Add(pieceName);
                }
            }
            else allowedToolbarPieces.Add(PieceTemplate.Name.Hand);

            this.PieceStorage = new PieceStorage(width: invWidth, height: invHeight, storagePiece: this, storageType: PieceStorage.StorageType.Inventory);
            this.ToolStorage = new PieceStorage(width: toolbarWidth, height: toolbarHeight, storagePiece: this, storageType: PieceStorage.StorageType.Tools, allowedPieceNames: allowedToolbarPieces);
            this.EquipStorage = new PieceStorage(width: 3, height: 3, storagePiece: this, storageType: PieceStorage.StorageType.Equip);
            this.ConfigureEquip();
            this.ShootingAngle = -100; // -100 == no real value
            this.shootingPower = 0;
            this.wentToSleepFrame = 0;
            this.sleepingInsideShelter = false;
            this.sleepMode = SleepMode.Awake;
            this.recipeToBuild = null;
            this.simulatedPieceToBuild = null;

            BoardPiece handTool = PieceTemplate.Create(templateName: PieceTemplate.Name.Hand, world: this.world);

            StorageSlot handSlot = this.ToolStorage.FindCorrectSlot(handTool);
            this.ToolStorage.AddPiece(handTool);
            handSlot.locked = true;
        }

        public override bool ShowStatBars
        { get { return true; } }

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

        public int wentToSleepFrame;
        public bool sleepingInsideShelter;
        public SleepMode sleepMode;

        public List<PieceStorage> CraftStorages
        { get { return new List<PieceStorage> { this.PieceStorage, this.ToolStorage, this.EquipStorage }; } }

        public List<PieceStorage> CraftStoragesToolbarFirst
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
                interestingPieces = interestingPieces.Where(piece => piece.canBePickedUp).ToList();
                if (interestingPieces.Count == 0) return null;

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
                if (fatigueDifference > 0 && this.buffEngine.HasBuff(BuffEngine.BuffType.Heat)) fatigueDifference *= 2;

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
                float fedPercent = (float)this.fedLevel / (float)this.maxFedLevel;

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

            StorageSlot headSlot = this.EquipStorage.GetSlot(1, 0);
            headSlot.locked = false;
            headSlot.hidden = false;
            headSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.HatSimple };
            headSlot.label = "head";

            StorageSlot chestSlot = this.EquipStorage.GetSlot(1, 1);
            chestSlot.locked = false;
            chestSlot.hidden = false;
            chestSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Dungarees };
            chestSlot.label = "chest";

            StorageSlot legsSlot = this.EquipStorage.GetSlot(1, 2);
            legsSlot.locked = false;
            legsSlot.hidden = false;
            legsSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BootsProtective };
            legsSlot.label = "legs";

            StorageSlot backpackSlot = this.EquipStorage.GetSlot(0, 1);
            backpackSlot.locked = false;
            backpackSlot.hidden = false;
            backpackSlot.allowedPieceNames = new List<PieceTemplate.Name>();
            backpackSlot.label = "backpack";

            foreach (PieceInfo.Info info in PieceInfo.AllInfo)
            {
                foreach (Buff buff in info.buffList)
                {
                    if (buff.type == BuffEngine.BuffType.InvWidth || buff.type == BuffEngine.BuffType.InvHeight)
                    {
                        backpackSlot.allowedPieceNames.Add(info.name);
                        break;
                    }
                }
            }

            StorageSlot beltSlot = this.EquipStorage.GetSlot(2, 1);
            beltSlot.locked = false;
            beltSlot.hidden = false;
            beltSlot.allowedPieceNames = new List<PieceTemplate.Name>();
            beltSlot.label = "belt";

            foreach (PieceInfo.Info info in PieceInfo.AllInfo)
            {
                foreach (Buff buff in info.buffList)
                {
                    if (buff.type == BuffEngine.BuffType.ToolbarWidth || buff.type == BuffEngine.BuffType.ToolbarHeight)
                    {
                        beltSlot.allowedPieceNames.Add(info.name);
                        break;
                    }
                }
            }

            var accessorySlotsList = new List<StorageSlot> { this.EquipStorage.GetSlot(0, 0), this.EquipStorage.GetSlot(2, 0) };

            foreach (StorageSlot accessorySlot in accessorySlotsList)
            {
                accessorySlot.locked = false;
                accessorySlot.hidden = false;
                accessorySlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Map };
                accessorySlot.label = "accessory";
            }
        }

        public override void Kill(bool addDestroyEvent = true)
        {
            if (Preferences.DebugGodMode || !PieceInfo.IsPlayer(this.name)) return;

            this.ToolStorage.DropAllPiecesToTheGround(addMovement: true); // only ToolStorage pieces should fall to the ground
            foreach (PieceStorage storage in this.CraftStorages)
            {
                foreach (StorageSlot slot in storage.OccupiedSlots)
                {
                    storage.RemoveAllPiecesFromSlot(slot: slot); // the rest should be destroyed
                }
            }

            if (this.buffEngine.HasBuff(BuffEngine.BuffType.LowHP)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.LowHP);

            base.Kill();

            Scene.RemoveAllScenesOfType(typeof(TextWindow));

            SolidColor redOverlay = new SolidColor(color: Color.DarkRed, viewOpacity: 0.65f);
            this.world.solidColorManager.Add(redOverlay);

            Sound.QuickPlay(SoundData.Name.GameOver);

            new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 3f } });
            new Scheduler.Task(taskName: Scheduler.TaskName.OpenMenuTemplate, turnOffInputUntilExecution: true, delay: 300, executeHelper: new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.GameOver } });
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
            pieceData["player_cookingSkill"] = this.cookingSkill;
            pieceData["player_craftLevel"] = this.craftLevel;
            pieceData["player_sleepEngine"] = this.sleepEngine;
            pieceData["player_toolStorage"] = this.ToolStorage.Serialize();
            pieceData["player_equipStorage"] = this.EquipStorage.Serialize();
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
            this.cookingSkill = (float)(double)pieceData["player_cookingSkill"];
            this.craftLevel = (int)(Int64)pieceData["player_craftLevel"];
            this.sleepEngine = (SleepEngine)pieceData["player_sleepEngine"];
            this.ToolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], storagePiece: this);
            this.EquipStorage = PieceStorage.Deserialize(storageData: pieceData["player_equipStorage"], storagePiece: this);
            if (pieceData.ContainsKey("player_LastSteps"))
            {
                List<Point> lastStepsPointList = (List<Point>)pieceData["player_LastSteps"];
                this.LastSteps = lastStepsPointList.Select(p => new Vector2(p.X, p.Y)).ToList();
            }
            this.RefreshAllowedPiecesForStorages();
        }

        public void RefreshAllowedPiecesForStorages() // for old saves compatibility, to ensure that all storages have the right allowedPieceNames
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
            else this.hitPoints = Math.Max(this.hitPoints - 0.02f, 0);

            if (addFatigue) this.Fatigue += 0.01f;
        }

        public void AcquireEnergy(float energyAmount)
        {
            energyAmount *= this.efficiency;

            this.fedLevel = Math.Min(this.fedLevel + Convert.ToInt32(energyAmount * 2), this.maxFedLevel);
            this.Stamina = this.maxStamina;

            if (this.FedPercent > 0.8f) this.world.HintEngine.Enable(HintEngine.Type.Hungry);
            if (this.FedPercent > 0.4f) this.world.HintEngine.Enable(HintEngine.Type.VeryHungry);
            if (this.FedPercent > 0.1f) this.world.HintEngine.Enable(HintEngine.Type.Starving);
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
            this.Walk(slowDownInWater: false);
        }

        public override void SM_PlayerControlledByCinematic()
        {
            if (this.pointWalkTarget != Vector2.Zero)
            {
                this.GoOneStepTowardsGoal(this.pointWalkTarget, splitXY: true, walkSpeed: this.speed);
                if (Vector2.Distance(this.sprite.position, this.pointWalkTarget) < 2f)
                {
                    this.sprite.CharacterStand();
                    this.pointWalkTarget = Vector2.Zero;
                }
            }
        }

        public override void SM_PlayerControlledWalking()
        {
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

                if (this.pointWalkTarget != Vector2.Zero)
                {
                    movement = this.pointWalkTarget - this.sprite.position;

                    if (Math.Abs(movement.X) < 3) movement.X = 0; // to avoid animation flickering
                    if (Math.Abs(movement.Y) < 3) movement.Y = 0; // to avoid animation flickering

                    if (movement.X == 0 && movement.Y == 0) this.pointWalkTarget = Vector2.Zero;
                }
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
                this.hitPoints -= 0.5f;
                this.BurnLevel += 0.12f;
                this.soundPack.Play(PieceSoundPack.Action.StepLava);
            }

            bool gotHitWithLightning = this.world.weather.LightningJustStruck && this.sprite.IsInWater && this.world.random.Next(2) == 0;

            if (gotHitWithLightning)
            {
                this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lightning, ignoreDelay: true);
                this.hitPoints -= 80;
                Sound.QuickPlay(SoundData.Name.ElectricShock);
            }

            if (isOnLava || gotHitWithLightning)
            {
                if (!this.world.solidColorManager.AnySolidColorPresent)
                {
                    this.soundPack.Play(PieceSoundPack.Action.Cry);
                    this.world.ShakeScreen();
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
            if (Mouse.RightIsDown) shooting = (this.world.TranslateScreenToWorldPos(Mouse.Position) - this.sprite.position) / 20;

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

                var optionList = new List<object>();

                optionList.Add(new Dictionary<string, object> { { "label", "go out" }, { "taskName", Scheduler.TaskName.ForceWakeUp }, { "executeHelper", this } });

                if (this.world.islandClock.CurrentPartOfDay == IslandClock.PartOfDay.Night) optionList.Add(new Dictionary<string, object> { { "label", "wait until morning" }, { "taskName", Scheduler.TaskName.WaitUntilMorning }, { "executeHelper", this } });
                optionList.Add(new Dictionary<string, object> { { "label", "wait indefinitely" }, { "taskName", Scheduler.TaskName.Empty }, { "executeHelper", null } });

                var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "You are fully rested." }, { "customOptionList", optionList } };

                new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);

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

        public void GoToSleep(SleepEngine sleepEngine, Vector2 zzzPos, List<Buff> wakeUpBuffs)
        {
            if (this.world.CurrentUpdate < this.wentToSleepFrame + 60) return; // to prevent going to sleep with max fatigue and with attacking enemies around

            this.wentToSleepFrame = this.world.CurrentUpdate;
            this.sleepingInsideShelter = !sleepEngine.canBeAttacked;
            this.sleepMode = SleepMode.Sleep;
            this.sleepEngine = sleepEngine;
            this.world.islandClock.multiplier = this.sleepEngine.islandClockMultiplier;
            this.buffList.AddRange(wakeUpBuffs);

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

            foreach (Buff buff in this.buffList)
            { this.buffEngine.AddBuff(buff: buff, world: this.world); }
            this.buffList.Clear();

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
    }
}