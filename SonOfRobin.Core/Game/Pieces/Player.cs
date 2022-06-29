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
        private float stamina;
        private float fatigue;
        public float maxFatigue;
        public float shootingAngle;
        private int shootingPower;
        public SleepEngine sleepEngine;
        public BoardPiece visualAid;

        public BoardPiece ActiveToolbarPiece
        {
            get
            {
                var invScenes = Scene.GetAllScenesOfType(typeof(Inventory));
                foreach (Scene scene in invScenes)
                {
                    Inventory invScene = (Inventory)scene;
                    if (invScene.layout == Inventory.Layout.SingleBottom || invScene.layout == Inventory.Layout.DualBottom)
                    {
                        StorageSlot activeSlot = invScene.ActiveSlot;
                        if (activeSlot == null) return null;
                        return activeSlot.GetTopPiece();
                    }
                }
                return null;
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

        public float Fatigue
        {
            get { return this.fatigue; }
            set
            {
                if (Preferences.debugGodMode) return;

                this.fatigue = Math.Min(Math.Max(value, 0), this.maxFatigue);

                if (this.FatiguePercent > 0.5f) this.world.hintEngine.Show(HintEngine.Type.Tired);
                if (this.FatiguePercent < 0.2f) this.world.hintEngine.EnableType(HintEngine.Type.Tired);

                if (this.fatigue == this.maxFatigue) new Scheduler.Task(menu: null, actionName: Scheduler.ActionName.SleepOutside, delay: 0, executeHelper: null);
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
        public Player(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte storageWidth = 0, byte storageHeight = 0) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, mass: 50000, maxMassBySize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, canBePickedUp: false, maxHitPoints: 200, fadeInAnim: false)
        {
            this.maxFedLevel = 40000;
            this.fedLevel = maxFedLevel;
            this.maxStamina = 300;
            this.stamina = maxStamina;
            this.maxFatigue = 2000;
            this.fatigue = 0;
            this.sleepEngine = SleepEngine.OutdoorSleepDry; // to be changed later
            this.activeState = State.PlayerControlledWalking;
            this.toolStorage = new PieceStorage(width: 6, height: 1, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Toolbar);
            this.shootingAngle = -100; // -100 == no real value
            this.shootingPower = 0;

            BoardPiece handTool = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Hand, world: this.world);

            StorageSlot slotToLock = this.toolStorage.FindCorrectSlot(handTool);
            this.toolStorage.AddPiece(handTool);
            slotToLock.locked = true;
        }

        public override void Kill()
        {
            base.Kill();
            MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.GameOver);
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["player_toolStorage"] = this.toolStorage.Serialize();
            pieceData["player_fedLevel"] = this.fedLevel;
            pieceData["player_maxFedLevel"] = this.maxFedLevel;
            pieceData["player_stamina"] = this.stamina;
            pieceData["player_maxStamina"] = this.maxStamina;
            pieceData["player_fatigue"] = this.fatigue;
            pieceData["player_maxFatigue"] = this.maxFatigue;
            pieceData["player_sleepEngine"] = this.sleepEngine;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.toolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], world: this.world, storagePiece: this);
            this.fedLevel = (int)pieceData["player_fedLevel"];
            this.maxFedLevel = (int)pieceData["player_maxFedLevel"];
            this.stamina = (float)pieceData["player_stamina"];
            this.maxStamina = (float)pieceData["player_maxStamina"];
            this.fatigue = (float)pieceData["player_fatigue"];
            this.maxFatigue = (float)pieceData["player_maxFatigue"];
            this.sleepEngine = (SleepEngine)pieceData["player_sleepEngine"];
        }

        public override void DrawStatBar()
        {
            if (this.activeState == State.PlayerControlledShooting) // small bar below the player
            {
                new StatBar(width: 80, height: 5, label: "power", value: (int)this.shootingPower, valueMax: (int)maxShootingPower, colorMin: new Color(220, 255, 0), colorMax: new Color(255, 0, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom);
                StatBar.FinishThisBatch();
            }

            // big bars (top center)

            Rectangle cameraRect = this.world.camera.viewRect;

            float scaleX = this.world.viewParams.scaleX;
            float scaleY = this.world.viewParams.scaleY;

            int width = (int)(SonOfRobinGame.VirtualWidth * scaleX * 0.25f);
            int height = (int)(SonOfRobinGame.VirtualHeight * scaleY * 0.015f);

            int posX = (int)(float)cameraRect.Center.X;
            int posY = (int)(cameraRect.Top + (SonOfRobinGame.VirtualHeight * 0.01f * scaleY));

            new StatBar(width: width, height: height, label: "food", value: (int)this.fedLevel, valueMax: (int)this.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: true, drawFromTop: true, labelAtLeft: false);
            new StatBar(width: width, height: height, label: "fatigue", value: (int)this.Fatigue, valueMax: (int)this.maxFatigue, colorMin: new Color(255, 255, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: true, drawFromTop: true, labelAtLeft: false);
            new StatBar(width: width, height: height, label: "stamina", value: (int)this.stamina, valueMax: (int)this.maxStamina, colorMin: new Color(100, 100, 100), colorMax: new Color(255, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: true, centerX: true, drawFromTop: true, labelAtLeft: false);
            new StatBar(width: width, height: height, label: "health", value: (int)this.hitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, ignoreIfAtMax: true, centerX: true, drawFromTop: true, labelAtLeft: false);

            StatBar.FinishThisBatch();
        }

        public void ExpendEnergy(float energyAmount, bool addFatigue = true)
        {
            if (Preferences.debugGodMode) return;

            if (this.fedLevel > 0) { this.fedLevel = Convert.ToInt32(Math.Max(this.fedLevel - Math.Max(energyAmount / 2, 1), 0)); }
            else { this.hitPoints = Math.Max(this.hitPoints - 0.05f, 0); }

            if (addFatigue) this.Fatigue += 0.01f;
        }

        public void AcquireEnergy(float energyAmount)
        {
            energyAmount *= this.efficiency;

            this.hitPoints = Math.Min(this.hitPoints + (energyAmount * 2), this.maxHitPoints);
            this.fedLevel = Math.Min(this.fedLevel + Convert.ToInt32(energyAmount * 2), this.maxFedLevel);
            this.Stamina = this.maxStamina;
        }

        public override void SM_PlayerControlledWalking()
        {
            this.ExpendEnergy(0.1f);

            if (!this.Walk()) this.Stamina = Math.Min(this.Stamina + 1, this.maxStamina);

            if (this.world.actionKeyList.Contains(World.ActionKeys.ShootingMode))
            {
                if (this.TryToEnterShootingMode()) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPiecePress))
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: false)) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPieceHold))
            {
                if (this.UseToolbarPiece(isInShootingMode: false, buttonHeld: true)) return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.PickUp))
            {
                this.PickUpNearestPiece();
                return;
            }

            if (this.world.actionKeyList.Contains(World.ActionKeys.Interact))
            {
                this.UseBoardPiece();
                return;
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
                    movement *= 2;
                }
            }

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

            if (cancelPressed || !this.ActiveToolbarWeaponHasAmmo)
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
            var sleepInterruptActions = new List<World.ActionKeys> { World.ActionKeys.UseToolbarPiecePress, World.ActionKeys.Inventory, World.ActionKeys.MapToggle, World.ActionKeys.PickUp, World.ActionKeys.FieldCraft, World.ActionKeys.Run };

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
            if (this.world.currentUpdate % 10 == 0) ProgressBar.ChangeValues(curVal: (int)(this.maxFatigue - this.Fatigue), maxVal: (int)this.maxFatigue, text: "Sleeping...");
        }

        public void GoToSleep(SleepEngine sleepEngine, Vector2 zzzPos)
        {
            if (!this.CanSleepNow)
            {
                new TextWindow(text: "You cannot sleep right now.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: true, checkForDuplicate: true);
                return;
            }

            this.sleepEngine = sleepEngine;

            if (this.visualAid != null)
            {
                this?.visualAid.Destroy();
                this.visualAid = null;
            }

            this.visualAid = PieceTemplate.CreateOnBoard(world: world, position: zzzPos, templateName: PieceTemplate.Name.Zzz);
            WorldEvent.RemovePieceFromQueue(pieceToRemove: this.visualAid, world: this.world);

            if (!this.sleepEngine.canBeAttacked) this.sprite.Visible = false;
            this.world.touchLayout = TouchLayout.WorldSleep;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldSleep;
            this.activeState = State.PlayerControlledSleep;

            SolidColor solidColor = new SolidColor(color: Color.Black, viewOpacity: 0.75f, clearScreen: false);
            solidColor.AddTransition(new Transition(type: Transition.TransType.In, duration: 20, scene: solidColor, blockInput: false, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }));

            int fastForwardSpeed = 20;
            new Scheduler.Task(menu: null, actionName: Scheduler.ActionName.EnableFastForward, delay: 20, executeHelper: fastForwardSpeed);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: "Going to sleep.");
        }

        public void WakeUp(bool force = false)
        {
            if (!this.CanWakeNow && !force)
            {
                new TextWindow(text: "You are too tired to wake up now.", textColor: Color.White, bgColor: Color.Red, useTransition: false, animate: false, checkForDuplicate: true);
                return;
            }

            ProgressBar.Hide();

            this?.visualAid.Destroy();
            this.visualAid = null;

            this.activeState = State.PlayerControlledWalking;
            this.world.touchLayout = TouchLayout.WorldMain;
            this.world.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.sprite.Visible = true;

            world.updateMultiplier = 1;
            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

            Scene existingSolidColor = Scene.GetTopSceneOfType(typeof(SolidColor));
            if (existingSolidColor != null)
            {
                existingSolidColor.AddTransition(new Transition(type: Transition.TransType.Out, duration: 20, scene: existingSolidColor, blockInput: false, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }, removeScene: true));
            }

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
                        {World.ActionKeys.Up, new Vector2(0f, -20f)},
                        {World.ActionKeys.Down, new Vector2(0f, 20f)},
                        {World.ActionKeys.Left, new Vector2(-40f, 0f)},
                        {World.ActionKeys.Right, new Vector2(40f, 0f)},
                        };
                }
                else
                {
                    movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                        {World.ActionKeys.Up, new Vector2(0f, -20f)},
                        {World.ActionKeys.Down, new Vector2(0f, 20f)},
                        {World.ActionKeys.Left, new Vector2(-20f, 0f)},
                        {World.ActionKeys.Right, new Vector2(20f, 0f)},
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



        private void PickUpNearestPiece()
        {
            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            var interestingPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY);
            interestingPieces = interestingPieces.Where(piece => piece.canBePickedUp).ToList();
            if (interestingPieces.Count == 0) return;

            BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: interestingPieces, offsetX: offsetX, offsetY: offsetY);
            closestPiece.sprite.rotation = 0f;

            if (closestPiece.GetType() == typeof(Container))
            {
                Container container = (Container)closestPiece;
                container.pieceStorage.DropAllPiecesToTheGround(addMovement: true);
            }

            bool piecePickedUp = this.PickUpPiece(piece: closestPiece);
            if (piecePickedUp) { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Picked up {closestPiece.name}."); }
            else
            {
                new TextWindow(text: "Inventory full.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Inventory full - cannot pick up {closestPiece.name}.");
            }
        }

        public bool PickUpPiece(BoardPiece piece)
        {
            bool pieceCollected = this.pieceStorage.AddPiece(piece);
            if (pieceCollected)
            {
                WorldEvent.RemovePieceFromQueue(world: this.world, pieceToRemove: piece);
                Tracking.RemoveFromTrackingQueue(world: this.world, pieceToRemove: piece);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UseBoardPiece()
        {
            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            var nearbyPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 47, offsetX: offsetX, offsetY: offsetY);

            var interestingPieces = nearbyPieces.Where(piece => piece.boardAction != Scheduler.ActionName.Empty).ToList();
            if (interestingPieces.Count > 0)
            {
                BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: interestingPieces, offsetX: offsetX, offsetY: offsetY);
                new Scheduler.Task(menu: null, actionName: closestPiece.boardAction, delay: 0, executeHelper: closestPiece);
            }
        }

        private bool TryToEnterShootingMode()
        {
            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;

            if (activeToolbarPiece?.GetType() != typeof(Tool)) return false;

            Tool activeTool = (Tool)activeToolbarPiece;
            if (activeTool.shootsProjectile)
            {
                if (activeTool.CheckForAmmo(removePiece: false) == null) return false;

                this.world.touchLayout = TouchLayout.WorldShoot;
                this.world.tipsLayout = ControlTips.TipsLayout.WorldShoot;
                this.activeState = State.PlayerControlledShooting;
                return true;
            }

            return false;
        }

        private bool UseToolbarPiece(bool isInShootingMode, bool buttonHeld = false)
        {
            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;

            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            if (activeToolbarPiece != null && activeToolbarPiece.toolbarAction != Scheduler.ActionName.Empty)
            {
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
                };

                new Scheduler.Task(menu: null, actionName: activeToolbarPiece.toolbarAction, delay: 0, executeHelper: executeHelper);
                return true;
            }

            return false;
        }

    }

}
