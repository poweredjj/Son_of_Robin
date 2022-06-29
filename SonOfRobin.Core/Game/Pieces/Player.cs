using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        public float fatigue;
        public float maxFatigue;
        public float shootingAngle;
        private int shootingPower;
        public SleepEngine sleepEngine;
        public byte invWidth;
        public byte invHeight;
        public byte toolbarWidth;
        public byte toolbarHeight;
        public int wentToSleepFrame;

        public BoardPiece ActiveToolbarPiece
        { get { return this.toolStorage?.lastUsedSlot?.TopPiece; } }

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

                if (this.IsVeryTired) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.Tired);
                if (this.FatiguePercent < 0.2f) this.world.hintEngine.Enable(HintEngine.Type.Tired);

                if (this.FatiguePercent > 0.95f) this.world.hintEngine.ShowGeneralHint(HintEngine.Type.VeryTired);
                if (this.FatiguePercent < 0.8f) this.world.hintEngine.Enable(HintEngine.Type.VeryTired);

                if (this.fatigue == this.maxFatigue) new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.SleepOutside, delay: 0, executeHelper: null);
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
        public float FedPercent { get { return (float)this.fedLevel / (float)this.maxFedLevel; } }
        public float FatiguePercent { get { return (float)this.Fatigue / (float)this.maxFatigue; } }
        public bool IsVeryTired { get { return this.FatiguePercent > 0.75f; } }
        public bool CanSleepNow { get { return this.FatiguePercent > 0.12f; } }
        public bool CanWakeNow { get { return this.FatiguePercent < 0.85f; } }

        public PieceStorage toolStorage;
        public PieceStorage equipStorage;
        public Player(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, byte invWidth, byte invHeight, byte toolbarWidth, byte toolbarHeight, string readableName, string description,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, mass: 50000, maxMassBySize: null, generation: generation, canBePickedUp: false, maxHitPoints: 400, fadeInAnim: false, placeAtBeachEdge: true, isShownOnMiniMap: true, readableName: readableName, description: description, yield: yield, strength: 1)
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = 300;
            this.stamina = maxStamina;
            this.maxFatigue = 2000;
            this.fatigue = 0;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.activeState = State.PlayerControlledWalking;
            this.invWidth = invWidth;
            this.invHeight = invHeight;
            this.toolbarWidth = toolbarWidth;
            this.toolbarHeight = toolbarHeight;
            this.pieceStorage = new PieceStorage(width: this.invWidth, height: this.invHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Inventory);
            this.toolStorage = new PieceStorage(width: this.toolbarWidth, height: this.toolbarHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Tools);
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

            base.Kill();

            this.world.colorOverlay.color = Color.DarkRed;
            this.world.colorOverlay.viewParams.Opacity = 0.75f;
            this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: 200));
            new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, turnOffInput: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 3f } }, menu: null);
            new Scheduler.Task(taskName: Scheduler.TaskName.OpenGameOverMenu, turnOffInput: true, delay: 300, menu: null, executeHelper: null);
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["player_toolStorage"] = this.toolStorage.Serialize();
            pieceData["player_equipStorage"] = this.equipStorage.Serialize();
            pieceData["player_fedLevel"] = this.fedLevel;
            pieceData["player_maxFedLevel"] = this.maxFedLevel;
            pieceData["player_stamina"] = this.stamina;
            pieceData["player_maxStamina"] = this.maxStamina;
            pieceData["player_fatigue"] = this.fatigue;
            pieceData["player_maxFatigue"] = this.maxFatigue;
            pieceData["player_sleepEngine"] = this.sleepEngine;
            pieceData["player_invWidth"] = this.invWidth;
            pieceData["player_invHeight"] = this.invHeight;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.toolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], world: this.world, storagePiece: this);
            this.equipStorage = PieceStorage.Deserialize(storageData: pieceData["player_equipStorage"], world: this.world, storagePiece: this);
            this.fedLevel = (int)pieceData["player_fedLevel"];
            this.maxFedLevel = (int)pieceData["player_maxFedLevel"];
            this.stamina = (float)pieceData["player_stamina"];
            this.maxStamina = (float)pieceData["player_maxStamina"];
            this.fatigue = (float)pieceData["player_fatigue"];
            this.maxFatigue = (float)pieceData["player_maxFatigue"];
            this.sleepEngine = (SleepEngine)pieceData["player_sleepEngine"];
            this.invWidth = (byte)pieceData["player_invWidth"];
            this.invHeight = (byte)pieceData["player_invHeight"];
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

            this.hitPoints = Math.Min(this.hitPoints + (energyAmount * 2), this.maxHitPoints);
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

            // highlighting pieces to interact with and corresponding interface elements

            if (this.world.inputActive) this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: true); // only to highlight pieces that will be hit

            BoardPiece pieceToInteract = this.ClosestPieceToInteract;
            if (pieceToInteract != null)
            {
                pieceToInteract.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Green));

                if (this.world.inputActive)
                {
                    Tutorials.ShowTutorial(type: Tutorials.Type.Interact, ignoreIfShown: true, ignoreDelay: false);
                    if (SonOfRobinGame.platform == Platform.Mobile) VirtButton.ButtonHighlightOnNextFrame(VButName.Interact);
                    ControlTips.TipHighlightOnNextFrame(tipName: "interact");
                }
            }

            BoardPiece pieceToPickUp = this.ClosestPieceToPickUp;
            if (pieceToPickUp != null)
            {
                if (this.world.inputActive) Tutorials.ShowTutorial(type: Tutorials.Type.PickUp, ignoreIfShown: true, ignoreDelay: false);
                pieceToPickUp.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.DodgerBlue));
                pieceToPickUp.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.White, textureSize: pieceToPickUp.sprite.frame.originalTextureSize, priority: 0));

                if (this.world.inputActive)
                {
                    if (SonOfRobinGame.platform == Platform.Mobile) VirtButton.ButtonHighlightOnNextFrame(VButName.PickUp);
                    ControlTips.TipHighlightOnNextFrame(tipName: "pick up");
                }
            }

            // checking pressed buttons

            if (this.world.actionKeyList.Contains(World.ActionKeys.ShootingMode))
            {
                if (this.TryToEnterShootingMode()) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPiecePress))
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false, highlightOnly: false)) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPieceHold))
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: true, highlightOnly: false)) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.PickUp))
            {
                this.PickUpClosestPiece(closestPiece: pieceToPickUp);
                return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.Interact))
            {
                if (pieceToInteract != null)
                {
                    this.world.hintEngine.Disable(Tutorials.Type.Interact);
                    new Scheduler.Task(menu: null, taskName: pieceToInteract.boardTask, delay: 0, executeHelper: pieceToInteract);
                }
            }
        }

        private bool Walk(bool setOrientation = true)
        {
            if (!this.world.ActionKeysContainDirection && this.world.analogMovementLeftStick == Vector2.Zero)
            {
                this.sprite.CharacterStand();
                return false;
            }

            Vector2 movement = this.CalculateMovementFromInput();

            var currentSpeed = this.IsVeryTired ? this.speed / 2f : this.speed;
            if (this.world.actionKeyList.Contains(World.ActionKeys.Run))
            {
                if (this.Stamina > 0)
                {
                    currentSpeed *= 2;
                }
            }

            movement *= currentSpeed;

            Vector2 goalPosition = this.sprite.position + movement;
            bool hasBeenMoved = this.GoOneStepTowardsGoal(goalPosition, splitXY: true, walkSpeed: currentSpeed, setOrientation: setOrientation);

            if (hasBeenMoved)
            {
                this.ExpendEnergy(0.2f);

                int staminaUsed = 0;

                if (this.sprite.IsInWater) staminaUsed = 1;
                if (this.world.actionKeyList.Contains(World.ActionKeys.Run)) staminaUsed += 2;

                if (staminaUsed > 0) this.Stamina = Math.Max(this.Stamina - staminaUsed, 0);
            }

            return hasBeenMoved;
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

            Vector2 moving = this.CalculateMovementFromInput();
            Vector2 shooting = this.world.analogMovementRightStick;

            Vector2 directionVector = Vector2.Zero;
            if (moving != Vector2.Zero) directionVector = moving;
            if (shooting != Vector2.Zero) directionVector = shooting;

            this.visualAid.sprite.opacity = shooting == Vector2.Zero && !Keyboard.IsPressed(Keys.Space) ? 0f : 1f;

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

            bool shootPressed = this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPiecePress) ||
                VirtButton.HasButtonBeenPressed(VButName.Shoot) ||
                Keyboard.HasBeenReleased(Keys.Space);

            if (shootPressed)
            {
                this.UseToolbarPiece(isInShootingMode: true, buttonHeld: false);
                this.shootingPower = 0;
            }

            bool cancelPressed = !this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPieceHold) && !this.world.actionKeyList.Contains(World.ActionKeys.ShootingMode) && !Keyboard.IsPressed(Keys.Space);

            if (cancelPressed || !this.ActiveToolbarWeaponHasAmmo || this.sprite.CanDrownHere)
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
            var sleepInterruptActions = new List<World.ActionKeys> { World.ActionKeys.Interact, World.ActionKeys.Run, World.ActionKeys.PickUp, World.ActionKeys.PickUp, World.ActionKeys.MapToggle };

            bool wakeUp = false;

            foreach (World.ActionKeys interruptAction in sleepInterruptActions)
            {
                if (this.world.actionKeyList.Contains(interruptAction)) wakeUp = true;
            }

            if (this.Fatigue == 0) wakeUp = true;

            if (wakeUp)
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

            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoFastForward, delay: 22, executeHelper: 20, turnOffInput: true);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: "Going to sleep.");
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

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Waking up.");
        }

        private Vector2 CalculateMovementFromInput(bool horizontalPriority = true)
        {
            Vector2 movement = new Vector2(0f, 0f);

            if (this.world.analogMovementLeftStick == Vector2.Zero)
            {
                // movement using keyboard and gamepad buttons

                // X axis movement is bigger than Y, to ensure horizontal orientation priority
                Dictionary<World.ActionKeys, Vector2> movementByDirection;

                if (horizontalPriority)
                {
                    movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                        {World.ActionKeys.Up, new Vector2(0f, -500f)},
                        {World.ActionKeys.Down, new Vector2(0f, 500f)},
                        {World.ActionKeys.Left, new Vector2(-500f, 0f)},
                        {World.ActionKeys.Right, new Vector2(500f, 0f)},
                        };
                }
                else
                {
                    movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                        {World.ActionKeys.Up, new Vector2(0f, -500f)},
                        {World.ActionKeys.Down, new Vector2(0f, 500f)},
                        {World.ActionKeys.Left, new Vector2(-500f, 0f)},
                        {World.ActionKeys.Right, new Vector2(500f, 0f)},
                        };
                }

                foreach (var kvp in movementByDirection)
                { if (this.world.actionKeyList.Contains(kvp.Key)) movement += kvp.Value; }
            }
            else
            {
                // analog movement
                movement = this.world.analogMovementLeftStick;
            }

            //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"movement {movement.X},{movement.Y}", color: Color.Orange); // for testing

            return movement;
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

            Tutorials.ShowTutorial(type: Tutorials.Type.Interact, ignoreIfShown: true, ignoreDelay: false);

            bool piecePickedUp = this.PickUpPiece(piece: closestPiece);
            if (piecePickedUp)
            {
                closestPiece.sprite.rotation = 0f;
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Picked up {closestPiece.readableName}.");
                this.world.hintEngine.CheckForPieceHintToShow(forcedMode: true);
            }
            else
            {
                new TextWindow(text: "My inventory is full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ShowHint, closingTaskHelper: HintEngine.Type.SmallInventory);

                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Inventory full - cannot pick up {closestPiece.readableName}.");
            }
        }

        public bool PickUpPiece(BoardPiece piece)
        {
            bool pieceCollected = false;

            if (piece.GetType() == typeof(Tool)) pieceCollected = this.toolStorage.AddPiece(piece);
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
                    {"toolbarPiece", activeToolbarPiece},
                    {"shootingPower", this.shootingPower},
                    {"offsetX", offsetX},
                    {"offsetY", offsetY},
                    {"buttonHeld", buttonHeld },
                    {"highlightOnly", highlightOnly },
                };

            new Scheduler.Task(menu: null, taskName: activeToolbarPiece.toolbarTask, delay: 0, executeHelper: executeHelper);
            return true;
        }

    }

}
