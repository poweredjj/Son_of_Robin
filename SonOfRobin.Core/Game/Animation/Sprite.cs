﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Sprite
    {
        public enum Orientation
        {
            // must be lowercase, to match animName
            left,
            right,
            up,
            down
        }

        public enum AdditionalMoveType { None, Minimal, Half }

        public readonly string id;
        public readonly BoardPiece boardPiece;
        public readonly World world;
        public Vector2 position;
        public Orientation orientation;
        public float rotation;
        public float opacity;
        public AnimFrame frame;
        public Color color;
        public LightEngine lightEngine;
        public AnimData.PkgName animPackage;
        public byte animSize;
        public string animName;
        private byte currentFrameIndex;
        private ushort currentFrameTimeLeft; // measured in game frames
        public OpacityFade opacityFade;
        public Rectangle gfxRect;
        public Rectangle colRect;
        public readonly bool blocksMovement;
        public readonly bool blocksPlantGrowth;
        public readonly bool ignoresCollisions;
        public AllowedTerrain allowedTerrain;
        private readonly AllowedDensity allowedDensity;
        private readonly int minDistance;
        private readonly int maxDistance;
        private readonly bool floatsOnWater;
        public bool hasBeenDiscovered;
        public readonly EffectCol effectCol;
        public List<Cell.Group> gridGroups;
        public Cell currentCell; // current cell, that is containing the sprite 
        public bool IsOnBoard { get; private set; }
        public string CompleteAnimID
        { get { return GetCompleteAnimId(animPackage: this.animPackage, animSize: this.animSize, animName: this.animName); } }

        public bool IsInCameraRect { get { return this.world.camera.viewRect.Contains(this.position); } }

        public bool IsInLightSourceRange
        {
            get
            {
                if (this.lightEngine.IsActive) return true;

                foreach (BoardPiece lightPiece in this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.LightSource, mainSprite: this, distance: 500))
                {
                    if (lightPiece.sprite.lightEngine.Rect.Contains(this.position)) return true;
                }

                return false;
            }
        }

        private bool visible;
        public bool IsInWater
        { get { return this.GetFieldValue(TerrainName.Height) < Terrain.waterLevelMax; } }
        public bool CanDrownHere { get { return this.GetFieldValue(TerrainName.Height) < Terrain.waterLevelMax - 10; } }
        public bool IsOnSand
        { get { return !this.IsInWater && (this.GetFieldValue(TerrainName.Humidity) <= 80 || this.GetFieldValue(TerrainName.Height) < 105); } }
        public bool IsOnRock
        { get { return this.GetFieldValue(TerrainName.Height) > 160; } }
        public bool IsOnLava
        { get { return this.GetFieldValue(TerrainName.Height) >= Terrain.lavaMin; } }
        public bool IsInBiome
        { get { return this.GetFieldValue(TerrainName.Biome) > Terrain.biomeMin; } }
        public bool IsDeepInBiome
        { get { return this.GetFieldValue(TerrainName.Biome) > Terrain.biomeDeep; } }

        public PieceSoundPack.Action WalkSoundAction
        {
            get
            {
                if (this.IsOnLava) return PieceSoundPack.Action.StepLava;
                if (this.IsOnRock) return PieceSoundPack.Action.StepRock;
                if (this.IsInWater)
                {
                    if (this.CanDrownHere) return PieceSoundPack.Action.SwimDeep;
                    else if (this.GetFieldValue(TerrainName.Height) < Terrain.waterLevelMax - 5) return PieceSoundPack.Action.SwimShallow;
                    else return PieceSoundPack.Action.StepWater;
                }

                if (this.IsOnSand) return PieceSoundPack.Action.StepSand;

                return PieceSoundPack.Action.StepGrass;
            }
        }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (this.visible == value) return;

                this.visible = value;
                if (value) this.world.grid.AddToGroup(sprite: this, groupName: Cell.Group.Visible);
                else this.world.grid.RemoveFromGroup(sprite: this, groupName: Cell.Group.Visible);
            }
        }

        public bool ObstructsCameraTarget
        {
            get
            {
                if (!this.IsOnBoard) return false;

                BoardPiece trackedPiece = this.world.camera.TrackedPiece;
                if (trackedPiece == null) return false;

                if (!this.blocksMovement || this.position.Y < trackedPiece.sprite.position.Y || this.boardPiece.id == trackedPiece.id) return false;
                return this.gfxRect.Contains(trackedPiece.sprite.position);
            }
        }

        public Sprite(World world, string id, BoardPiece boardPiece, AnimData.PkgName animPackage, byte animSize, string animName, bool ignoresCollisions, AllowedTerrain allowedTerrain, bool blocksMovement = true, bool visible = true, bool floatsOnWater = false, bool fadeInAnim = true, AllowedDensity allowedDensity = null, LightEngine lightEngine = null, int minDistance = 0, int maxDistance = 100, bool blocksPlantGrowth = false)
        {
            this.id = id; // duplicate from BoardPiece class
            this.boardPiece = boardPiece;
            this.world = world;
            this.rotation = 0f;
            this.orientation = Orientation.right;
            this.animPackage = animPackage;
            this.animSize = animSize;
            this.animName = animName;
            this.color = Color.White;
            this.floatsOnWater = floatsOnWater;
            this.currentFrameIndex = 0;
            this.currentFrameTimeLeft = 0;
            this.gfxRect = Rectangle.Empty;
            this.colRect = Rectangle.Empty;
            this.blocksMovement = blocksMovement;
            this.ignoresCollisions = ignoresCollisions;
            this.blocksPlantGrowth = blocksPlantGrowth;
            this.allowedTerrain = allowedTerrain;
            this.allowedDensity = allowedDensity;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
            if (this.allowedDensity != null) this.allowedDensity.FinishCreation(piece: this.boardPiece, sprite: this);
            this.visible = visible; // initially it is assigned normally
            this.effectCol = new EffectCol(world: world);
            this.hasBeenDiscovered = false;
            this.currentCell = null;
            this.IsOnBoard = false;
            if (fadeInAnim)
            {
                this.opacity = 0f;
                this.opacityFade = new OpacityFade(sprite: this, destOpacity: 1f);
            }
            else this.opacity = 1f;

            this.AssignFrame(checkForCollision: false);
            this.gridGroups = this.GetGridGroups();

            this.lightEngine = lightEngine;
            if (this.lightEngine != null) this.lightEngine.AssignSprite(this);
        }

        public bool PlaceOnBoard(Vector2 position, bool ignoreCollisions = false, bool precisePlacement = false, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false)
        {
            this.position = Vector2.Zero; // needed for placement purposes

            int minDistance = minDistanceOverride == -1 ? this.minDistance : minDistanceOverride;
            int maxDistance = maxDistanceOverride == -1 ? this.maxDistance : maxDistanceOverride;

            bool placedCorrectly = closestFreeSpot ?
                this.MoveToClosestFreeSpot(startPosition: position, checkIsOnBoard: false, ignoreDensity: ignoreDensity) :
                this.FindFreeSpot(position, minDistance: minDistance, maxDistance: maxDistance, ignoreCollisions: ignoreCollisions, precisePlacement: precisePlacement, ignoreDensity: ignoreDensity);

            if (placedCorrectly) this.IsOnBoard = true;
            else this.RemoveFromBoard();

            return placedCorrectly;
        }

        public void RemoveFromBoard()
        {
            this.world.grid.RemoveSprite(this);
            this.position = new Vector2(-500, -500);  // to fail if trying to use in the future
            this.IsOnBoard = false;
        }

        public void UpdateBoardLocation()
        {
            this.world.grid.UpdateLocation(this);
        }

        public void Serialize(Dictionary<string, Object> pieceData)
        {
            pieceData["sprite_frame_id"] = this.frame.id;
            pieceData["sprite_positionX"] = this.position.X;
            pieceData["sprite_positionY"] = this.position.Y;
            pieceData["sprite_animPackage"] = this.animPackage;
            pieceData["sprite_animSize"] = this.animSize;
            pieceData["sprite_animName"] = this.animName;
            pieceData["sprite_currentFrameIndex"] = this.currentFrameIndex;
            pieceData["sprite_currentFrameTimeLeft"] = this.currentFrameTimeLeft;
            pieceData["sprite_rotation"] = this.rotation;
            pieceData["sprite_opacity"] = this.opacity;
            pieceData["sprite_opacityFade"] = this.opacityFade;
            pieceData["sprite_orientation"] = this.orientation;
            pieceData["sprite_gridGroups"] = this.gridGroups;
            pieceData["sprite_hasBeenDiscovered"] = this.hasBeenDiscovered;
            pieceData["sprite_allowedTerrain"] = this.allowedTerrain;
            pieceData["sprite_lightSource"] = this.lightEngine == null ? null : this.lightEngine.Serialize();
            pieceData["sprite_color"] = new byte[] { this.color.R, this.color.G, this.color.B, this.color.A };
        }
        public void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.position = new Vector2((float)pieceData["sprite_positionX"], (float)pieceData["sprite_positionY"]);
            this.orientation = (Orientation)pieceData["sprite_orientation"];
            this.rotation = (float)pieceData["sprite_rotation"];
            this.opacity = (float)pieceData["sprite_opacity"];
            this.opacityFade = (OpacityFade)pieceData["sprite_opacityFade"];
            if (this.opacityFade != null) this.opacityFade.sprite = this;
            this.animPackage = (AnimData.PkgName)pieceData["sprite_animPackage"];
            this.animSize = (byte)pieceData["sprite_animSize"];
            this.animName = (string)pieceData["sprite_animName"];
            this.currentFrameIndex = (byte)pieceData["sprite_currentFrameIndex"];
            this.currentFrameTimeLeft = (ushort)pieceData["sprite_currentFrameTimeLeft"];
            this.AssignFrameById((string)pieceData["sprite_frame_id"]);
            this.gridGroups = (List<Cell.Group>)pieceData["sprite_gridGroups"];
            this.hasBeenDiscovered = (bool)pieceData["sprite_hasBeenDiscovered"];
            this.allowedTerrain = (AllowedTerrain)pieceData["sprite_allowedTerrain"];
            this.lightEngine = LightEngine.Deserialize(lightData: pieceData["sprite_lightSource"], sprite: this);
            var colorArray = (byte[])pieceData["sprite_color"];
            this.color = new Color(r: colorArray[0], g: colorArray[1], b: colorArray[2], alpha: colorArray[3]);
        }
        public byte GetFieldValue(TerrainName terrainName)
        {
            if (!this.IsOnBoard) throw new ArgumentException($"Trying to get a field value of '{this.boardPiece.name}' that is not on board.");
            return this.world.grid.GetFieldValue(position: this.position, terrainName: terrainName);
        }

        public bool GetExtProperty(ExtBoardProps.ExtPropName name)
        {
            if (!this.IsOnBoard) throw new ArgumentException($"Trying to get an ext value of '{this.boardPiece.name}' that is not on board.");
            return this.world.grid.GetExtProperty(name: name, position: this.position);
        }

        public static string GetCompleteAnimId(AnimData.PkgName animPackage, byte animSize, string animName)
        { return $"{animPackage}-{animSize}-{animName}"; }

        public int GetAnimDuration()
        {
            int duration = 0;

            if (AnimData.frameListById.ContainsKey(this.CompleteAnimID))
            {
                foreach (var frame in AnimData.frameListById[this.CompleteAnimID])
                { duration += frame.duration; }
            }

            return duration;
        }

        public bool MoveToClosestFreeSpot(Vector2 startPosition, bool checkIsOnBoard = true, bool ignoreDensity = false, int maxDistance = 170)
        {
            if (!this.IsOnBoard && checkIsOnBoard) throw new ArgumentException($"Trying to move '{this.boardPiece.name}' that is not on board.");

            if (this.SetNewPosition(newPos: startPosition, ignoreDensity: ignoreDensity, checkIsOnBoard: checkIsOnBoard)) return true; // checking the center first

            int initialDistance = 4;
            int distanceStep = 6;
            int iterationStepsMultiplier = 3;

            for (int currentDistance = initialDistance; currentDistance < maxDistance; currentDistance += distanceStep)
            {
                for (int tryCounter = 0; tryCounter < iterationStepsMultiplier * currentDistance; tryCounter++)
                {
                    double angle = this.world.random.NextDouble() * Math.PI * 2;

                    double offsetX = Math.Round(currentDistance * Math.Cos(angle));
                    double offsetY = Math.Round(currentDistance * Math.Sin(angle));
                    Vector2 newPos = startPosition + new Vector2((float)offsetX, (float)offsetY);

                    if (this.SetNewPosition(newPos: newPos, ignoreDensity: ignoreDensity, checkIsOnBoard: checkIsOnBoard)) return true;
                }
            }

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Could not move to closest free spot - {this.boardPiece.name}.", color: Color.Violet);

            return false;
        }

        private bool FindFreeSpot(Vector2 startPosition, int minDistance, int maxDistance, bool ignoreCollisions = false, bool precisePlacement = false, bool ignoreDensity = false)
        {
            if (ignoreCollisions)
            {
                this.SetNewPosition(newPos: new Vector2(startPosition.X, startPosition.Y), ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                return true;
            }

            if (precisePlacement) return this.SetNewPosition(newPos: startPosition, ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);

            if (!this.world.CanProcessAnyStateMachineNow) return false;

            int numberOfTries = (minDistance == 0 && maxDistance == 0) ? 1 : 4;

            if (startPosition.X == -100 && startPosition.Y == -100) // -100, -100 will be converted to any position on the map - needed for effective creation of new sprites 
            {
                for (int tryIndex = 0; tryIndex < numberOfTries; tryIndex++)
                {
                    Vector2 newPos = this.GetRandomPosition(outsideCamera: this.world.createMissingPiecesOutsideCamera);

                    bool hasBeenMoved = this.SetNewPosition(newPos: newPos, ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                    if (hasBeenMoved) return true;
                }
            }
            else
            {
                for (int tryIndex = 0; tryIndex < numberOfTries; tryIndex++)
                {
                    var offset = new Vector2(this.world.random.Next(minDistance, maxDistance), this.world.random.Next(minDistance, maxDistance));

                    if (this.world.random.Next(2) == 1) offset.X *= -1;
                    if (this.world.random.Next(2) == 1) offset.Y *= -1;

                    Vector2 newPos = startPosition + offset;
                    newPos.X = Math.Max(newPos.X, 0);
                    newPos.X = Math.Min(newPos.X, this.world.width - 1);
                    newPos.Y = Math.Max(newPos.Y, 0);
                    newPos.Y = Math.Min(newPos.Y, this.world.height - 1);

                    bool hasBeenMoved = this.SetNewPosition(newPos: newPos, ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                    if (hasBeenMoved) return true;
                }
            }

            return false;
        }

        public Vector2 GetRandomPosition(bool outsideCamera)
        {
            Cell cell = this.world.grid.GetRandomCellForPieceName(this.boardPiece.name); // random cell for both cases (fast)

            if (outsideCamera) // needs a cell, that is not intersecting with camera
            {
                Rectangle cameraViewRect = this.world.camera.viewRect;

                if (cameraViewRect.Intersects(cell.rect)) // checking if initial random cell intersects with camera
                {
                    var cellList = this.world.grid.GetCellsForPieceName(this.boardPiece.name); // getting cell list to find non-intersecting cell (slow)

                    while (true) // looking for first non-intersecting cell
                    {
                        if (!cellList.Any())
                        {
                            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.boardPiece.name} - no cells left while searching for a random position outside camera.");
                            return Vector2.One; // a Vector2 needs to be returned, even if this piece cannot be placed there
                        }

                        int cellNo = this.world.random.Next(0, cellList.Count);

                        cell = cellList[cellNo];
                        cellList.RemoveAt(cellNo);
                        if (!cameraViewRect.Intersects(cell.rect)) break;
                    }
                }
            }

            return new Vector2(this.world.random.Next(cell.xMin, cell.xMax), this.world.random.Next(cell.yMin, cell.yMax));
        }

        public void SetOrientationByMovement(Vector2 movement)
        {
            if (Math.Abs(movement.X) > Math.Abs(movement.Y))
            { this.orientation = (movement.X < 0) ? Orientation.left : Orientation.right; }
            else
            { this.orientation = (movement.Y < 0) ? Orientation.up : Orientation.down; }

            if (this.animName.Contains("walk-")) this.CharacterWalk();
            if (this.animName.Contains("stand-")) this.CharacterStand();
        }

        public float GetAngleFromOrientation()
        {
            float degrees;

            switch (this.orientation)
            {
                case Orientation.left:
                    degrees = 180;
                    break;

                case Orientation.right:
                    degrees = 0;
                    break;

                case Orientation.up:
                    degrees = -90;
                    break;

                case Orientation.down:
                    degrees = 90;
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported orientation - {this.orientation}.");
            }

            float radians = (float)(degrees * Helpers.Deg2Rad);
            return radians;
        }

        public Sprite FindClosestSprite(List<Sprite> spriteList)
        {
            try
            {
                spriteList = spriteList.OrderBy(s => Vector2.Distance(this.position, s.position)).ToList();
                return spriteList[0];
            }
            catch (ArgumentOutOfRangeException)
            { }

            return null;
        }

        public void Kill()
        {
            this.CharacterStand(setEvenIfMissing: false);

            if (CheckIfAnimNameExists("dead")) this.AssignNewName("dead");
            else this.color = Color.LightCoral;

            if (this.boardPiece.GetType() == typeof(Animal)) PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.position, templateName: PieceTemplate.Name.BloodSplatter);
        }

        private List<Cell.Group> GetGridGroups()
        {
            var groupNames = new List<Cell.Group> { Cell.Group.All };

            if (!this.ignoresCollisions)
            {
                if (this.blocksPlantGrowth || this.blocksMovement) groupNames.Add(Cell.Group.ColPlantGrowth); // what blocks movement, should block plant growth too
                if (this.blocksMovement) groupNames.Add(Cell.Group.ColMovement);
            }
            if (this.visible) groupNames.Add(Cell.Group.Visible);

            return groupNames;
        }

        public bool Move(Vector2 movement, AdditionalMoveType additionalMoveType = AdditionalMoveType.Minimal)
        {
            List<Vector2> movesToTest = new List<Vector2> { movement };

            switch (additionalMoveType)
            {
                case AdditionalMoveType.Minimal:
                    movesToTest.Add(new Vector2(Math.Max(Math.Min(movement.X, 1), -1), Math.Max(Math.Min(movement.Y, 1), -1)));
                    break;

                case AdditionalMoveType.Half:
                    movesToTest.Add(new Vector2(movement.X / 2, movement.Y / 2));
                    break;

                case AdditionalMoveType.None:
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported additionalMoveType - {additionalMoveType}.");
            }

            foreach (Vector2 testMove in movesToTest)
            {
                if (((int)testMove.X != 0 || (int)testMove.Y != 0) && this.SetNewPosition(this.position + testMove)) return true;
            }
            return false;
        }

        public bool SetNewPosition(Vector2 newPos, bool ignoreCollisions = false, bool updateGridLocation = true, bool ignoreDensity = false, bool checkIsOnBoard = true)
        {
            if (!this.IsOnBoard && checkIsOnBoard) throw new ArgumentException($"Trying to move '{this.boardPiece.name}' that is not on board.");

            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2((int)newPos.X, (int)newPos.Y); // to ensure integer values
            this.position.X = Math.Min(Math.Max(this.position.X, 0), this.world.width - 1);
            this.position.Y = Math.Min(Math.Max(this.position.Y, 0), this.world.height - 1);

            this.UpdateRects();
            if (updateGridLocation) this.UpdateBoardLocation();
            if (ignoreCollisions) return true;

            bool collisionDetected = this.CheckForCollision(ignoreDensity: ignoreDensity);
            if (collisionDetected)
            {
                this.position = originalPosition;
                this.UpdateRects();
                if (updateGridLocation) this.UpdateBoardLocation();
            }

            return !collisionDetected;
        }

        public List<Sprite> GetCollidingSpritesAtPosition(Vector2 positionToCheck, List<Cell.Group> cellGroupsToCheck)
        {
            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2((int)positionToCheck.X, (int)positionToCheck.Y); // to ensure integer values
            this.position.X = Math.Min(Math.Max(this.position.X, 0), this.world.width - 1);
            this.position.Y = Math.Min(Math.Max(this.position.Y, 0), this.world.height - 1);

            this.UpdateRects();
            this.UpdateBoardLocation();

            List<Sprite> collidingSpritesList = this.GetCollidingSprites(cellGroupsToCheck);

            this.position = originalPosition;
            this.UpdateRects();
            this.UpdateBoardLocation();

            return collidingSpritesList;
        }

        public List<Sprite> GetCollidingSprites(List<Cell.Group> cellGroupsToCheck)
        {
            var collidingSprites = new List<Sprite>();

            foreach (Cell.Group group in cellGroupsToCheck)
            {
                foreach (Sprite sprite in this.world.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: group))
                {
                    if (this.colRect.Intersects(sprite.colRect) && sprite.id != this.id) collidingSprites.Add(sprite);
                }
            }

            return collidingSprites;
        }

        public bool CheckForCollision(bool ignoreDensity = false)
        {
            if (this.world == null) return false;
            if (this.gfxRect.Left <= 0 || this.gfxRect.Right >= this.world.width || this.gfxRect.Top <= 0 || this.gfxRect.Bottom >= this.world.height) return true;
            if (this.ignoresCollisions) return false;
            if (this.allowedDensity != null && !ignoreDensity && !this.allowedDensity.CanBePlacedHere()) return true;
            if (!this.allowedTerrain.CanStandHere(world: this.world, position: this.position)) return true;

            var gridTypeToCheck = this.boardPiece.GetType() == typeof(Plant) ? Cell.Group.ColPlantGrowth : Cell.Group.ColMovement;

            foreach (Sprite sprite in this.world.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: gridTypeToCheck))
            {
                if (this.colRect.Intersects(sprite.colRect) && sprite.id != this.id) return true;
            }

            return false;
        }

        public bool CheckIfOtherSpriteIsWithinRange(Sprite target, int range = 4)
        {
            if (!target.IsOnBoard) return false;

            if (Vector2.Distance(this.position, target.position) <= range * 1.5) return true;

            Rectangle rectangle1 = this.colRect;
            Rectangle rectangle2 = target.colRect;

            rectangle1.Inflate(range, range);
            rectangle2.Inflate(range, range);

            return rectangle1.Intersects(rectangle2);
        }

        private void UpdateRects()
        {
            this.gfxRect.X = Convert.ToInt32(position.X + this.frame.gfxOffset.X);
            this.gfxRect.Y = Convert.ToInt32(position.Y + this.frame.gfxOffset.Y);
            this.gfxRect.Width = this.frame.gfxWidth;
            this.gfxRect.Height = this.frame.gfxHeight;

            this.colRect.X = Convert.ToInt32(position.X + this.frame.colOffset.X);
            this.colRect.Y = Convert.ToInt32(position.Y + this.frame.colOffset.Y);
            this.colRect.Width = this.frame.colWidth;
            this.colRect.Height = this.frame.colHeight;
        }

        public void AssignNewPackage(AnimData.PkgName animPackage, bool setEvenIfMissing = true)
        {
            if (this.animPackage != animPackage)
            {
                if (CheckIfAnimPackageExists(animPackage) || setEvenIfMissing)
                {
                    var animPackageCopy = this.animPackage;
                    this.animPackage = animPackage;
                    if (!AssignFrame(forceRewind: true)) this.animPackage = animPackageCopy;
                }
            }
        }
        public void AssignNewSize(byte animSize, bool setEvenIfMissing = true)
        {
            if (this.animSize != animSize)
            {
                if (CheckIFAnimSizeExists(animSize) || setEvenIfMissing)
                {
                    var animSizeCopy = this.animSize;
                    this.animSize = animSize;
                    if (!AssignFrame(forceRewind: true)) this.animSize = animSizeCopy;
                }
            }
        }
        public void AssignNewName(string animName, bool setEvenIfMissing = true)
        {
            if (this.animName != animName)
            {
                if (CheckIfAnimNameExists(animName) || setEvenIfMissing)
                {
                    var animNameCopy = this.animName;

                    this.animName = animName;
                    if (!AssignFrame(forceRewind: true)) this.animName = animNameCopy;
                }
            }
        }
        public bool CheckIfAnimPackageExists(AnimData.PkgName newAnimPackage)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: newAnimPackage, animSize: this.animSize, animName: this.animName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }

        public bool CheckIFAnimSizeExists(byte newAnimSize)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: this.animPackage, animSize: newAnimSize, animName: this.animName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }
        public bool CheckIfAnimNameExists(string newAnimName)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: this.animPackage, animSize: this.animSize, animName: newAnimName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }

        public bool AssignFrame(bool forceRewind = false, bool checkForCollision = true)
        {
            AnimFrame frameCopy = this.frame;

            try
            {
                if (forceRewind || this.currentFrameIndex >= AnimData.frameListById[this.CompleteAnimID].Count) this.RewindAnim();
                this.frame = AnimData.frameListById[this.CompleteAnimID][this.currentFrameIndex];
            }
            catch (KeyNotFoundException) // placeholder frame if the animation was missing
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Anim frame not found {this.CompleteAnimID}.");
                this.frame = AnimData.frameListById["NoAnim-0-default"][0];
            }

            this.currentFrameTimeLeft = this.frame.duration;
            this.UpdateRects();

            if (!blocksMovement) return true;

            // in case of collision - reverting to a previous, non-colliding frame
            if (checkForCollision && this.CheckForCollision() && frameCopy != null)
            {
                this.frame = frameCopy;
                this.currentFrameTimeLeft = this.frame.duration;
                this.UpdateRects();

                return false;
            }

            return true;
        }
        public void AssignFrameById(string frameId)
        // use only when loading game - does not check for collisions
        {
            this.frame = AnimFrame.GetFrameById(frameId);
            this.currentFrameTimeLeft = this.frame.duration;
            this.UpdateRects();
        }

        public void RewindAnim()
        {
            this.currentFrameIndex = 0;
            this.currentFrameTimeLeft = this.frame.duration;
        }

        public void UpdateAnimation()
        {
            if (this.frame.duration == 0) return; // duration == 0 will stop the animation

            this.currentFrameTimeLeft--;
            if (this.currentFrameTimeLeft <= 0)
            {
                this.currentFrameIndex++;
                AssignFrame();
            }
        }

        public void CharacterStand(bool setEvenIfMissing = true)
        { this.AssignNewName(animName: $"stand-{this.orientation}", setEvenIfMissing: setEvenIfMissing); }

        public void CharacterWalk(bool setEvenIfMissing = true)
        { this.AssignNewName(animName: $"walk-{this.orientation}", setEvenIfMissing: setEvenIfMissing); }

        public void Draw(bool calculateSubmerge = true)
        {
            if (!this.hasBeenDiscovered && this.world.MapEnabled && this.world.camera.IsTrackingPlayer && this.world.camera.viewRect.Contains(this.gfxRect)) this.hasBeenDiscovered = true;

            if (this.ObstructsCameraTarget && this.opacityFade == null) this.opacityFade = new OpacityFade(sprite: this, destOpacity: 0.5f, playerObstructMode: true, duration: 10);
            if (Scene.UpdateStack.Contains(this.world)) this.opacityFade?.Process();

            if (Preferences.debugShowRects)
            { SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32(this.gfxRect.X), Convert.ToInt32(this.gfxRect.Y), this.gfxRect.Width, this.gfxRect.Height), this.gfxRect, Color.White * 0.5f); }

            bool effectsShouldBeEnabled = this.effectCol.ThereAreEffectsToRender;
            if (!effectsShouldBeEnabled) this.DrawRoutine(calculateSubmerge);
            else
            {
                bool thereWillBeMoreEffects = false;
                while (true)
                {
                    if (effectsShouldBeEnabled) thereWillBeMoreEffects = this.effectCol.TurnOnNextEffect(scene: this.world, currentUpdateToUse: world.currentUpdate);
                    this.DrawRoutine(calculateSubmerge);

                    if (!thereWillBeMoreEffects)
                    {
                        this.world.StartNewSpriteBatch(enableEffects: false);
                        break;
                    }
                }
            }

            if (Preferences.debugShowRects)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32(this.colRect.X), Convert.ToInt32(this.colRect.Y), this.colRect.Width, this.colRect.Height), this.colRect, Color.Red * 0.7f);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32((this.position.X) - 1), Convert.ToInt32(this.position.Y - 1), 2, 2), Color.White);
            }

            if (Preferences.debugShowStates && this.boardPiece.GetType() == typeof(Animal) && this.boardPiece.alive) this.DrawState();

            if (Preferences.debugShowStatBars || this.boardPiece.ShowStatBars) this.boardPiece.DrawStatBar();
        }

        public void DrawRoutine(bool calculateSubmerge, int offsetX = 0, int offsetY = 0)
        {
            Rectangle destRect = this.gfxRect;
            if (offsetX != 0 || offsetY != 0)
            {
                destRect.X += offsetX;
                destRect.Y += offsetY;
            }

            if (this.rotation == 0)
            {
                int submergeCorrection = !this.floatsOnWater && this.IsInWater && calculateSubmerge ?
                    (Terrain.waterLevelMax - this.GetFieldValue(TerrainName.Height)) / 2 : 0;

                this.frame.Draw(destRect: destRect, color: this.color, submergeCorrection: submergeCorrection, opacity: this.opacity);
            }
            else
            {
                this.frame.DrawWithRotation(position: new Vector2(destRect.Center.X, destRect.Center.Y), color: this.color, rotation: this.rotation, opacity: this.opacity);
            }

            if (this.boardPiece.pieceStorage != null && this.boardPiece.GetType() == typeof(Plant)) this.DrawFruits();
        }

        private void DrawFruits()
        {
            Plant plant = (Plant)this.boardPiece;
            if (Preferences.debugShowFruitRects) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, plant.fruitEngine.FruitAreaRect, Color.Cyan * 0.4f);

            if (plant.pieceStorage.OccupiedSlotsCount == 0) return;
            var fruitList = plant.pieceStorage.GetAllPieces();
            foreach (BoardPiece fruit in fruitList)
            { fruit.sprite.Draw(calculateSubmerge: false); }
        }

        public void DrawAndKeepInRectBounds(Rectangle destRect, float opacity)
        { this.frame.DrawAndKeepInRectBounds(destBoundsRect: destRect, color: this.color * opacity); }


        private void DrawState()
        {
            string stateTxt = $"{this.boardPiece.activeState}".Replace("Player", "").Replace("Animal", "");
            var stateFont = SonOfRobinGame.fontPressStart2P5;

            Vector2 textSize = stateFont.MeasureString(stateTxt);
            // text position should be integer, otherwise it would get blurry
            Vector2 txtPos = new Vector2(
                (int)(this.position.X - (textSize.X / 2)),
                (int)(this.position.Y - (textSize.Y / 2)));

            SonOfRobinGame.spriteBatch.DrawString(stateFont, stateTxt, txtPos + new Vector2(1, 1), Color.Black);
            SonOfRobinGame.spriteBatch.DrawString(stateFont, stateTxt, txtPos, Color.White);
        }

        public static void DrawShadow(Color color, Sprite shadowSprite, Vector2 lightPos, float shadowAngle, int drawOffsetX = 0, int drawOffsetY = 0, float yScaleForce = 0f)
        {
            float distance = Vector2.Distance(lightPos, shadowSprite.position);
            AnimFrame frame = shadowSprite.frame;

            var flatShadowNames = new List<AnimData.PkgName> {
                AnimData.PkgName.WoodLogRegular, AnimData.PkgName.WoodLogHard, AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig4, AnimData.PkgName.MineralsSmall1, AnimData.PkgName.Stone, AnimData.PkgName.WaterRock5, AnimData.PkgName.WoodPlank, AnimData.PkgName.IronBar, AnimData.PkgName.Clay, AnimData.PkgName.Hole, AnimData.PkgName.Granite, AnimData.PkgName.BeltBig, AnimData.PkgName.HumanSkeleton };

            bool flatShadow = flatShadowNames.Contains(shadowSprite.boardPiece.sprite.animPackage) || shadowSprite.rotation != 0;

            if (flatShadow)
            {
                float xDiff = shadowSprite.position.X - lightPos.X;
                float yDiff = shadowSprite.position.Y - lightPos.Y;

                float xLimit = shadowSprite.gfxRect.Width / 8;
                float yLimit = shadowSprite.gfxRect.Height / 8;

                float offsetX = Math.Max(Math.Min(xDiff / 6f, xLimit), -xLimit);
                float offsetY = Math.Max(Math.Min(yDiff / 6f, yLimit), -yLimit);

                Color originalColor = shadowSprite.color;
                shadowSprite.color = color;
                shadowSprite.DrawRoutine(calculateSubmerge: true, offsetX: (int)offsetX + drawOffsetX, offsetY: (int)offsetY + drawOffsetY);
                shadowSprite.color = originalColor;
            }
            else
            {
                float xScale = frame.scale;
                float yScale = Math.Max(frame.scale / distance * 100f, frame.scale * 0.3f);
                yScale = Math.Min(yScale, frame.scale * 3f);
                if (yScaleForce != 0) yScale = frame.scale * yScaleForce;

                SonOfRobinGame.spriteBatch.Draw(
                    frame.texture,
                    position:
                    new Vector2(shadowSprite.position.X + drawOffsetX, shadowSprite.position.Y + drawOffsetY),
                    sourceRectangle: frame.textureRect,
                    color: color * shadowSprite.opacity,
                    rotation: shadowSprite.rotation + shadowAngle + (float)(Math.PI / 2f),
                    origin: new Vector2(-frame.gfxOffset.X / frame.scale, -(frame.gfxOffset.Y + frame.colOffset.Y) / frame.scale),
                    scale: new Vector2(xScale, yScale),
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }

    }
}
