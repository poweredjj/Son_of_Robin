using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Player : BoardPiece
    {
        private static readonly int maxShootingPower = 90;

        public int maxFedLevel;
        public int fedLevel;
        public float maxStamina;
        public float stamina;
        private float fatigue;
        public float maxFatigue;
        public float shootingAngle;
        private int shootingPower;
        public SleepEngine sleepEngine;
        private Vector2 pointWalkTarget;

        private bool ShootingModeInputPressed
        {
            get
            {
                if (TouchInput.IsBeingTouchedInAnyWay) return Math.Abs(this.world.analogCameraCorrection.X) > 0.05f || Math.Abs(this.world.analogCameraCorrection.Y) > 0.05f;
                else return InputMapper.IsPressed(InputMapper.Action.WorldUseToolbarPiece) || Mouse.RightIsDown;
            }
        }

        public byte InvWidth
        {
            get { return this.pieceStorage.Width; }
            set
            {
                if (this.pieceStorage.Width == value) return;
                this.pieceStorage.Resize(value, this.InvHeight);
            }
        }
        public byte InvHeight
        {
            get { return this.pieceStorage.Height; }
            set
            {
                if (this.pieceStorage.Height == value) return;
                this.pieceStorage.Resize(this.InvWidth, value);
            }
        }

        public byte ToolbarWidth
        {
            get { return this.toolStorage.Width; }
            set
            {
                if (this.toolStorage.Width == value) return;
                this.toolStorage.Resize(value, this.ToolbarHeight);
            }
        }

        public byte ToolbarHeight
        {
            get { return this.toolStorage.Height; }
            set
            {
                if (this.toolStorage.Height == value) return;
                this.toolStorage.Resize(this.ToolbarWidth, value);
            }
        }

        public int wentToSleepFrame;
        public List<PieceStorage> CraftStorages { get { return new List<PieceStorage> { this.pieceStorage, this.toolStorage, this.equipStorage }; } }
        public List<PieceStorage> CraftStoragesToolbarFirst { get { return new List<PieceStorage> { this.toolStorage, this.pieceStorage, this.equipStorage }; } } // the same as above, changed order

        public StorageSlot ActiveSlot
        { get { return this.toolStorage?.lastUsedSlot; } }

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

                var nearbyPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);

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
                Vector2 centerOffset = this.GetCenterOffset();
                int offsetX = (int)centerOffset.X;
                int offsetY = (int)centerOffset.Y;

                var interestingPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);
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

                this.fatigue = Math.Min(Math.Max(value, 0), this.maxFatigue);

                if (this.IsVeryTired)
                {
                    if (!this.buffEngine.HasBuff(BuffEngine.BuffType.Tired)) this.buffEngine.AddBuff(world: this.world, buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Tired, value: 0, autoRemoveDelay: 0, isPositive: false));
                }
                else
                {
                    if (this.buffEngine.HasBuff(BuffEngine.BuffType.Tired)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Tired);
                }

                if (this.IsVeryTired) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.Tired);
                if (this.FatiguePercent < 0.2f) this.world.hintEngine.Enable(HintEngine.Type.Tired);
                if (this.FatiguePercent > 0.95f) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.VeryTired);
                if (this.FatiguePercent < 0.8f) this.world.hintEngine.Enable(HintEngine.Type.VeryTired);

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
                    if (!this.buffEngine.HasBuff(BuffEngine.BuffType.Hungry)) this.buffEngine.AddBuff(world: this.world, buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Hungry, value: 0, autoRemoveDelay: 0, isPositive: false));
                }
                else
                {
                    if (this.buffEngine.HasBuff(BuffEngine.BuffType.Hungry)) this.buffEngine.RemoveEveryBuffOfType(BuffEngine.BuffType.Hungry);
                }

                return fedPercent;
            }
        }
        public float FatiguePercent { get { return (float)this.Fatigue / (float)this.maxFatigue; } }
        public bool IsVeryTired { get { return this.FatiguePercent > 0.75f; } }
        public bool CanSleepNow { get { return this.FatiguePercent > 0.12f; } }
        public bool CanWakeNow { get { return this.FatiguePercent < 0.85f; } }

        public PieceStorage toolStorage;
        public PieceStorage equipStorage;
        public Player(World world, Vector2 position, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, byte invWidth, byte invHeight, byte toolbarWidth, byte toolbarHeight, string readableName, string description,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, mass: 50000, maxMassBySize: null, generation: generation, canBePickedUp: false, maxHitPoints: 400, fadeInAnim: false, placeAtBeachEdge: true, isShownOnMiniMap: true, readableName: readableName, description: description, yield: yield, strength: 1, category: Category.Indestructible, lightEngine: new LightEngine(size: 300, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true))
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = 300;
            this.stamina = maxStamina;
            this.maxFatigue = 2000;
            this.fatigue = 0;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.activeState = State.PlayerControlledWalking;

            this.pieceStorage = new PieceStorage(width: invWidth, height: invHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Inventory);
            this.toolStorage = new PieceStorage(width: toolbarWidth, height: toolbarHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Tools);
            this.equipStorage = new PieceStorage(width: 2, height: 2, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Equip);
            this.ConfigureEquip();
            this.shootingAngle = -100; // -100 == no real value
            this.shootingPower = 0;
            this.wentToSleepFrame = 0;

            BoardPiece handTool = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Hand, world: this.world);

            StorageSlot slotToLock = this.toolStorage.FindCorrectSlot(handTool);
            this.toolStorage.AddPiece(handTool);
            slotToLock.locked = true;
        }

        private void ConfigureEquip()
        {
            StorageSlot backpackSlot = this.equipStorage.AllSlots[0];
            backpackSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BackpackMedium };
            backpackSlot.label = "backpack";

            StorageSlot beltSlot = this.equipStorage.AllSlots[1];
            beltSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BeltMedium };
            beltSlot.label = "belt";


            foreach (StorageSlot slot in this.equipStorage.AllSlots.GetRange(2, 2))
            {
                slot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Map };
                slot.label = "accessory";
            }
        }

        public override void Kill()
        {
            if (Preferences.DebugGodMode) return;

            foreach (PieceStorage storage in this.CraftStorages)
            { storage.DropAllPiecesToTheGround(addMovement: true); }

            base.Kill();

            Scene.RemoveAllScenesOfType(typeof(TextWindow));

            this.world.colorOverlay.color = Color.DarkRed;
            this.world.colorOverlay.viewParams.Opacity = 0.75f;
            this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: 200));
            new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 3f } }, menu: null);
            new Scheduler.Task(taskName: Scheduler.TaskName.OpenMenuTemplate, turnOffInputUntilExecution: true, delay: 300, menu: null, executeHelper: new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.GameOver } });
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
            pieceData["player_sleepEngine"] = this.sleepEngine;
            pieceData["player_toolStorage"] = this.toolStorage.Serialize();
            pieceData["player_equipStorage"] = this.equipStorage.Serialize();

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.fedLevel = (int)pieceData["player_fedLevel"];
            this.maxFedLevel = (int)pieceData["player_maxFedLevel"];
            this.stamina = (float)pieceData["player_stamina"];
            this.maxStamina = (float)pieceData["player_maxStamina"];
            this.fatigue = (float)pieceData["player_fatigue"];
            this.maxFatigue = (float)pieceData["player_maxFatigue"];
            this.sleepEngine = (SleepEngine)pieceData["player_sleepEngine"];
            this.toolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], world: this.world, storagePiece: this);
            this.equipStorage = PieceStorage.Deserialize(storageData: pieceData["player_equipStorage"], world: this.world, storagePiece: this);
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

                if (this.FedPercent < 0.5f) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.Hungry);
                if (this.FedPercent < 0.2f) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.VeryHungry);
                if (this.FedPercent < 0.01f) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.Starving);
            }
            else this.hitPoints = Math.Max(this.hitPoints - 0.02f, 0);

            if (addFatigue) this.Fatigue += 0.01f;
        }

        public void AcquireEnergy(float energyAmount)
        {
            energyAmount *= this.efficiency;

            this.fedLevel = Math.Min(this.fedLevel + Convert.ToInt32(energyAmount * 2), this.maxFedLevel);
            this.Stamina = this.maxStamina;

            if (this.FedPercent > 0.8f) this.world.hintEngine.Enable(HintEngine.Type.Hungry);
            if (this.FedPercent > 0.4f) this.world.hintEngine.Enable(HintEngine.Type.VeryHungry);
            if (this.FedPercent > 0.1f) this.world.hintEngine.Enable(HintEngine.Type.Starving);
        }

        public override void SM_PlayerControlledWalking()
        {
            if (this.world.currentUpdate % 120 == 0) this.world.hintEngine.CheckForPieceHintToShow();

            this.ExpendEnergy(0.1f);
            if (!this.Walk()) this.Stamina = Math.Min(this.Stamina + 1, this.maxStamina);

            this.CheckGround();

            // highlighting pieces to interact with and corresponding interface elements

            if (this.world.inputActive) this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: true); // only to highlight pieces that will be hit

            BoardPiece pieceToInteract = this.ClosestPieceToInteract;
            if (pieceToInteract != null)
            {
                if (this.world.inputActive)
                {
                    pieceToInteract.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Green));
                    Tutorials.ShowTutorial(type: Tutorials.Type.Interact, ignoreIfShown: true, ignoreDelay: false);
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
                    Tutorials.ShowTutorial(type: Tutorials.Type.PickUp, ignoreIfShown: true, ignoreDelay: false);
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

            if (InputMapper.IsPressed(InputMapper.Action.WorldUseToolbarPiece) || Mouse.RightIsDown)
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: true, highlightOnly: false)) return;
            }

            bool pickUp = InputMapper.HasBeenPressed(InputMapper.Action.WorldPickUp) ||
                (Preferences.pointToInteract && pieceToPickUp != null && pieceToPickUp.sprite.gfxRect.Contains(this.pointWalkTarget));

            if (pickUp)
            {
                this.pointWalkTarget = Vector2.Zero; // to avoid picking up indefinitely
                this.PickUpClosestPiece(closestPiece: pieceToPickUp);
                return;
            }

            bool interact = !pickUp && (InputMapper.HasBeenPressed(InputMapper.Action.WorldInteract) ||
                (Preferences.pointToInteract && pieceToInteract != null && pieceToInteract.sprite.gfxRect.Contains(this.pointWalkTarget)));

            if (interact)
            {
                if (pieceToInteract != null)
                {
                    this.pointWalkTarget = Vector2.Zero; // to avoid interacting indefinitely
                    this.world.hintEngine.Disable(Tutorials.Type.Interact);
                    new Scheduler.Task(taskName: pieceToInteract.boardTask, delay: 0, executeHelper: pieceToInteract);
                }
            }
        }

        private bool Walk(bool setOrientation = true)
        {
            Vector2 movement = this.world.analogMovementLeftStick;
            if (movement != Vector2.Zero) this.pointWalkTarget = Vector2.Zero;

            //  bool inputActive = Input.InputActive;
            bool layoutChangedRecently = TouchInput.FramesSinceLayoutChanged < 5;

            if (movement == Vector2.Zero && Preferences.pointToWalk && !layoutChangedRecently)
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
            if (InputMapper.IsPressed(InputMapper.Action.WorldRun) && (this.Stamina > 0)) currentSpeed *= 2;

            movement *= currentSpeed;

            Vector2 goalPosition = this.sprite.position + movement;
            bool hasBeenMoved = this.GoOneStepTowardsGoal(goalPosition, splitXY: true, walkSpeed: currentSpeed, setOrientation: setOrientation);

            if (hasBeenMoved)
            {
                this.ExpendEnergy(0.2f);

                int staminaUsed = 0;

                if (this.sprite.IsInWater) staminaUsed = 1;
                if (InputMapper.IsPressed(InputMapper.Action.WorldRun)) staminaUsed += 2;

                if (staminaUsed > 0) this.Stamina = Math.Max(this.Stamina - staminaUsed, 0);
            }

            return hasBeenMoved;
        }

        private void CheckGround()
        {
            if (this.sprite.IsDeepInDangerZone) Tutorials.ShowTutorial(type: Tutorials.Type.DangerZone, ignoreDelay: true);

            if (this.sprite.IsInLava)
            {
                this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.Lava, ignoreDelay: true);
                this.hitPoints -= 1;
                if (!this.world.colorOverlay.transManager.HasAnyTransition)
                {
                    Vector2 screenShake = new Vector2(world.random.Next(-20, 20), world.random.Next(-20, 20));

                    world.transManager.AddMultipleTransitions(outTrans: true, duration: world.random.Next(4, 10), playCount: -1, replaceBaseValue: false, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.02f, paramsToChange: new Dictionary<string, float> { { "PosX", screenShake.X }, { "PosY", screenShake.Y } });

                    this.world.colorOverlay.color = Color.Red;
                    this.world.colorOverlay.viewParams.Opacity = 0f;
                    this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f));
                }
            }
        }

        public override void SM_PlayerControlledShooting()
        {
            this.ExpendEnergy(0.1f);
            this.shootingPower = Math.Min(this.shootingPower + 1, maxShootingPower);

            this.Stamina = Math.Max(this.Stamina - 1, 0);

            if (this.visualAid == null) this.visualAid = PieceTemplate.CreateOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.Crosshair);

            this.Walk(setOrientation: false);

            // shooting angle should be set once at the start
            if (this.shootingAngle == -100) this.shootingAngle = this.sprite.GetAngleFromOrientation();

            Vector2 moving = this.world.analogMovementLeftStick;
            Vector2 shooting = this.world.analogMovementRightStick;
            if (Mouse.RightIsDown) shooting = (this.world.TranslateScreenToWorldPos(Mouse.Position) - this.sprite.position) / 20;

            Vector2 directionVector = Vector2.Zero;
            if (moving != Vector2.Zero) directionVector = moving;
            if (shooting != Vector2.Zero) directionVector = shooting;

            this.visualAid.sprite.opacity = !this.ShootingModeInputPressed ? 0f : 1f;

            if (directionVector != Vector2.Zero)
            {
                this.sprite.SetOrientationByMovement(directionVector);
                Vector2 goalPosition = this.sprite.position + directionVector; // used to calculate shooting angle
                this.shootingAngle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: goalPosition);
            }

            int aidDistance = (int)(SonOfRobinGame.VirtualWidth * 0.08f);
            int aidOffsetX = (int)Math.Round(aidDistance * Math.Cos(this.shootingAngle));
            int aidOffsetY = (int)Math.Round(aidDistance * Math.Sin(this.shootingAngle));
            Vector2 aidPos = this.sprite.position + new Vector2(aidOffsetX, aidOffsetY);
            this.visualAid.sprite.SetNewPosition(aidPos);

            if (TouchInput.IsBeingTouchedInAnyWay && VirtButton.HasButtonBeenPressed(VButName.Shoot) ||
                InputMapper.HasBeenReleased(InputMapper.Action.WorldUseToolbarPiece) || Mouse.RightHasBeenReleased) // virtual button has to be checked separately here
            {
                this.UseToolbarPiece(isInShootingMode: true, buttonHeld: false);
                this.shootingPower = 0;
            }

            if (!this.ShootingModeInputPressed || !this.ActiveToolbarWeaponHasAmmo || this.sprite.CanDrownHere)
            {
                this.visualAid.Destroy();
                this.visualAid = null;
                this.world.touchLayout = TouchLayout.WorldMain;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
                this.activeState = State.PlayerControlledWalking;
                this.shootingAngle = -100;
                this.shootingPower = 0;
            }
        }

        public override void SM_PlayerControlledSleep()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip) || this.Fatigue == 0)
            {
                this.WakeUp();
                return;
            }

            this.sleepEngine.Execute(player: this);
            if (this.world.currentUpdate % 10 == 0) SonOfRobinGame.progressBar.TurnOn(curVal: (int)(this.maxFatigue - this.Fatigue), maxVal: (int)this.maxFatigue, text: "Sleeping...");
        }

        public void GoToSleep(SleepEngine sleepEngine, Vector2 zzzPos, List<BuffEngine.Buff> wakeUpBuffs)
        {
            if (!this.CanSleepNow)
            {
                new TextWindow(text: "I cannot sleep right now.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
                return;
            }

            if (this.world.currentUpdate < this.wentToSleepFrame + 60) return; // to prevent going to sleep with max fatigue and with attacking enemies around

            this.wentToSleepFrame = this.world.currentUpdate;
            this.sleepEngine = sleepEngine;
            this.buffList.AddRange(wakeUpBuffs);

            if (this.visualAid != null)
            {
                this?.visualAid.Destroy();
                this.visualAid = null;
            }

            this.visualAid = PieceTemplate.CreateOnBoard(world: world, position: zzzPos, templateName: PieceTemplate.Name.Zzz);

            if (!this.sleepEngine.canBeAttacked) this.sprite.Visible = false;
            this.world.touchLayout = TouchLayout.WorldSleep;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldSleep;
            this.sprite.CharacterStand();
            this.activeState = State.PlayerControlledSleep;

            this.world.colorOverlay.color = Color.Black;
            this.world.colorOverlay.viewParams.Opacity = 0.75f;
            this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: 20));

            new Scheduler.Task(taskName: Scheduler.TaskName.TempoFastForward, delay: 22, executeHelper: 20, turnOffInputUntilExecution: true);

            MessageLog.AddMessage(msgType: MsgType.User, message: "Going to sleep.");
        }

        public void WakeUp(bool force = false)
        {
            if (!this.CanWakeNow && !force)
            {
                new TextWindow(text: "You are too tired to wake up now.", textColor: Color.White, bgColor: Color.Red, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
                return;
            }

            SonOfRobinGame.progressBar.TurnOff();

            foreach (BuffEngine.Buff buff in this.buffList)
            { this.buffEngine.AddBuff(buff: buff, world: this.world); }
            this.buffList.Clear();

            this?.visualAid.Destroy();
            this.visualAid = null;

            this.activeState = State.PlayerControlledWalking;
            this.world.touchLayout = TouchLayout.WorldMain;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.sprite.Visible = true;

            world.updateMultiplier = 1;
            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;
            Scheduler.RemoveAllTasksOfName(Scheduler.TaskName.TempoFastForward); // to prevent fast forward, when waking up before this task was executed

            this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: true, baseParamName: "Opacity", targetVal: 0f, duration: 20, endCopyToBase: true));

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
                    throw new DivideByZeroException($"Unsupported sprite orientation - {this.sprite.orientation}.");
            }

            return new Vector2(offsetX, offsetY);
        }

        private void PickUpClosestPiece(BoardPiece closestPiece)
        {
            if (closestPiece == null) return;

            this.world.hintEngine.Disable(Tutorials.Type.PickUp);

            bool piecePickedUp = this.PickUpPiece(piece: closestPiece);
            if (piecePickedUp)
            {
                closestPiece.sprite.rotation = 0f;
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Picked up {closestPiece.readableName}.");
                this.world.hintEngine.CheckForPieceHintToShow(forcedMode: true);
            }
            else
            {
                new TextWindow(text: "My inventory is full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ShowHint, closingTaskHelper: HintEngine.Type.SmallInventory);

                MessageLog.AddMessage(msgType: MsgType.User, message: $"Inventory full - cannot pick up {closestPiece.readableName}.");
            }
        }

        public bool PickUpPiece(BoardPiece piece)
        {
            bool pieceCollected = false;

            if (piece.GetType() == typeof(Tool) || piece.GetType() == typeof(PortableLight)) pieceCollected = this.toolStorage.AddPiece(piece);
            if (!pieceCollected) pieceCollected = this.pieceStorage.AddPiece(piece);
            if (!pieceCollected) pieceCollected = this.toolStorage.AddPiece(piece);

            if (pieceCollected)
            {
                WorldEvent.RemovePieceFromQueue(world: this.world, pieceToRemove: piece);
                Tracking.RemoveFromTrackingQueue(world: this.world, pieceToRemove: piece);
                return true;
            }
            else return false;
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
                    this.world.hintEngine.ShowGeneralHint(HintEngine.Type.CantShootInWater);
                    return false;
                }

                if (activeTool.CheckForAmmo(removePiece: false) == null) return false;

                this.world.touchLayout = TouchLayout.WorldShoot;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldShoot;
                this.activeState = State.PlayerControlledShooting;
                return true;
            }

            return false;
        }

        private bool UseToolbarPiece(bool isInShootingMode, bool buttonHeld = false, bool highlightOnly = false)
        {
            if (!this.CanUseActiveToolbarPiece) return false;

            StorageSlot activeSlot = this.ActiveSlot;
            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;

            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            if (this.sprite.CanDrownHere) return false;

            if (activeToolbarPiece?.GetType() == typeof(Tool))
            {
                Tool activeTool = (Tool)activeToolbarPiece;
                if (activeTool.shootsProjectile && !isInShootingMode) return false;
            }

            var executeHelper = new Dictionary<string, Object> {
                    {"player", this},
                    {"slot", activeSlot},
                    {"toolbarPiece", activeToolbarPiece},
                    {"shootingPower", this.shootingPower},
                    {"offsetX", offsetX},
                    {"offsetY", offsetY},
                    {"buttonHeld", buttonHeld },
                    {"highlightOnly", highlightOnly },
                };

            new Scheduler.Task(taskName: activeToolbarPiece.toolbarTask, delay: 0, executeHelper: executeHelper);
            return true;
        }

    }

}
