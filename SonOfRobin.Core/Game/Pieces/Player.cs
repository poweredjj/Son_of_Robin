using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Player : BoardPiece
    {
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

        public PieceStorage toolStorage;
        public Player(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte storageWidth = 0, byte storageHeight = 0) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, mass: 50000, maxMassBySize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, canBePickedUp: false)
        {
            this.activeState = State.PlayerControlledWalking;
            this.toolStorage = new PieceStorage(width: 6, height: 1, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Toolbar);

            BoardPiece handTool = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Hand, world: this.world);

            StorageSlot slotToLock = this.toolStorage.FindCorrectSlot(handTool);
            this.toolStorage.AddPiece(handTool);
            slotToLock.locked = true;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["player_toolStorage"] = this.toolStorage.Serialize();

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.toolStorage = PieceStorage.Deserialize(storageData: pieceData["player_toolStorage"], world: this.world, storagePiece: this);
        }

        public override void DrawStatBar()
        {
            new StatBar(width: 80, height: 5, label: "hp", value: (int)this.hitPoints, valueMax: (int)this.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom);
            StatBar.FinishThisBatch();
        }

        public override void SM_PlayerControlledWalking()
        {
            if (this.world.actionKeyList.Count == 0 && this.world.analogMovement.X == 0 && this.world.analogMovement.Y == 0)
            { this.sprite.CharacterStand(); }

            else
            {
                Vector2 movement = new Vector2(0f, 0f);

                if (this.world.analogMovement.X == 0 && this.world.analogMovement.Y == 0)
                {
                    // movement using keyboard and gamepad buttons

                    // X axis movement is bigger than Y, to ensure horizontal orientation priority
                    var movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                {World.ActionKeys.Up, new Vector2(0f, -20f)},
                {World.ActionKeys.Down, new Vector2(0f, 20f)},
                {World.ActionKeys.Left, new Vector2(-40f, 0f)},
                {World.ActionKeys.Right,new Vector2(40f, 0f)},
                  };

                    foreach (var kvp in movementByDirection)
                        if (this.world.actionKeyList.Contains(kvp.Key))
                        { movement += kvp.Value; }
                }
                else
                {
                    // analog movement
                    movement = this.world.analogMovement;
                }

                var currentSpeed = this.speed;
                if (this.world.actionKeyList.Contains(World.ActionKeys.Run))
                {
                    currentSpeed *= 2;
                    movement *= 2;
                }

                Vector2 goalPosition = this.sprite.position + movement;
                this.GoOneStepTowardsGoal(goalPosition, splitXY: true, walkSpeed: currentSpeed);
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

            if (this.world.actionKeyList.Contains(World.ActionKeys.UseToolbarPiece))
            {
                this.UseToolbarPiece();
                return;
            }


        }

        private Vector2 GetCenterOffset()
        {
            int offsetX = 0;
            int offsetY = 0;
            int offset = 15;

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

            if (closestPiece.GetType() == typeof(Container))
            {
                Container container = (Container)closestPiece;
                container.pieceStorage.DropAllPiecesToTheGround(addMovement: true);
            }

            bool pieceCollected = this.pieceStorage.AddPiece(closestPiece);
            if (pieceCollected)
            {
                PlannedEvent.RemovePieceFromQueue(world: this.world, pieceToRemove: closestPiece);

                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Picked up {closestPiece.name}.");
                PieceTemplate.CreateOnBoard(world: this.world, position: closestPiece.sprite.position, templateName: PieceTemplate.Name.Backlight);
            }
            else
            { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Inventory full - cannot pick up {closestPiece.name}."); }

            return;
        }


        private void UseBoardPiece()
        {
            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            var nearbyPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY);

            var interestingPieces = nearbyPieces.Where(piece => piece.boardAction != Scheduler.ActionName.Empty).ToList();
            if (interestingPieces.Count > 0)
            {
                BoardPiece closestPiece = FindClosestPiece(sprite: this.sprite, pieceList: interestingPieces, offsetX: offsetX, offsetY: offsetY);
                new Scheduler.Task(menu: null, actionName: closestPiece.boardAction, delay: 0, executeHelper: closestPiece);
                return;
            }

        }

        private void UseToolbarPiece()
        {
            Vector2 centerOffset = this.GetCenterOffset();
            int offsetX = (int)centerOffset.X;
            int offsetY = (int)centerOffset.Y;

            BoardPiece activeToolbarPiece = this.ActiveToolbarPiece;


            if (activeToolbarPiece != null && activeToolbarPiece.toolbarAction != Scheduler.ActionName.Empty)
            {
                var executeHelper = new Dictionary<string, Object> { };
                executeHelper["player"] = this;
                executeHelper["tool"] = activeToolbarPiece;
                executeHelper["offsetX"] = offsetX;
                executeHelper["offsetY"] = offsetY;

                new Scheduler.Task(menu: null, actionName: activeToolbarPiece.toolbarAction, delay: 0, executeHelper: executeHelper);
                return;
            }




        }


    }

}
