using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Sprite
    {
        public enum Orientation : byte
        {
            // must be lowercase, to match animName
            left,

            right,
            up,
            down,
        }

        public enum AdditionalMoveType : byte
        {
            None,
            Minimal,
            Half
        }

        public readonly int id;
        public readonly BoardPiece boardPiece;
        public readonly World world;

        public Vector2 position;
        public Orientation orientation;
        private int lastOrientationChangeFrame; // to avoid flickering when two directions are close during movement
        public float OrientationAngle { get; private set; }
        public float rotation;
        public Vector2 rotationOriginOverride; // used for custom rotation origin, different from the default

        public float opacity;
        public OpacityFade opacityFade;
        public AnimFrame AnimFrame { get; private set; }
        public AnimFrame CroppedAnimFrame { get { return AnimData.GetCroppedFrameForPackage(this.AnimPackage); } }
        public Color color;
        private bool visible;

        public LightEngine lightEngine;
        public ParticleEngine particleEngine;
        public AnimData.PkgName AnimPackage { get; private set; }
        public byte AnimSize { get; private set; }
        public string AnimName { get; private set; }
        private byte currentFrameIndex;
        private short currentFrameTimeLeft; // measured in game frames
        public Rectangle GfxRect { get; private set; }
        public Rectangle ColRect { get; private set; }

        public AllowedTerrain allowedTerrain;

        public bool hasBeenDiscovered;
        public readonly EffectCol effectCol;
        public List<Cell.Group> gridGroups;
        public Cell currentCell; // current cell, that is containing the sprite
        public bool IsOnBoard { get; private set; }

        public Sprite(World world, int id, BoardPiece boardPiece, AnimData.PkgName animPackage, byte animSize, string animName, AllowedTerrain allowedTerrain, bool visible = true, LightEngine lightEngine = null)
        {
            this.id = id; // duplicate from BoardPiece class
            this.boardPiece = boardPiece;
            this.world = world;
            this.rotation = 0f;
            this.rotationOriginOverride = Vector2.Zero;
            this.orientation = Orientation.right;
            this.lastOrientationChangeFrame = 0;
            this.OrientationAngle = 0f;
            this.AnimPackage = animPackage;
            this.AnimSize = animSize;
            this.AnimName = animName;
            this.color = Color.White;
            this.currentFrameIndex = 0;
            this.currentFrameTimeLeft = 0;
            this.GfxRect = Rectangle.Empty;
            this.ColRect = Rectangle.Empty;
            this.allowedTerrain = allowedTerrain;
            this.particleEngine = null;
            this.visible = visible; // initially it is assigned normally
            this.effectCol = new EffectCol(world: world);
            this.hasBeenDiscovered = false;
            this.currentCell = null;
            this.IsOnBoard = false;
            this.opacity = 1f;

            this.AssignFrame(checkForCollision: false);
            this.gridGroups = this.GetGridGroups();

            this.lightEngine = lightEngine;
            if (this.lightEngine != null) this.lightEngine.AssignSprite(this);
        }

        public string CompleteAnimID
        { get { return GetCompleteAnimId(animPackage: this.AnimPackage, animSize: this.AnimSize, animName: this.AnimName); } }

        public bool IsInCameraRect
        { get { return this.world.camera.viewRect.Contains(this.position); } }

        public bool IsInLightSourceRange
        {
            get
            {
                if (this.lightEngine != null && this.lightEngine.IsActive) return true;

                foreach (BoardPiece lightPiece in this.boardPiece.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.LightSource, mainSprite: this, distance: 500))
                {
                    if (lightPiece.sprite.lightEngine != null && lightPiece.sprite.lightEngine.Rect.Contains(this.position)) return true;
                }

                return false;
            }
        }

        public bool BlocksMovement
        { get { return this.boardPiece.pieceInfo != null && this.boardPiece.pieceInfo.blocksMovement; } }

        public bool IgnoresCollisions
        { get { return this.boardPiece.pieceInfo != null && this.boardPiece.pieceInfo.ignoresCollisions; } }

        public bool BlocksPlantGrowth
        { get { return this.boardPiece.pieceInfo != null && this.boardPiece.pieceInfo.blocksPlantGrowth; } }

        public bool IsInWater
        { get { return this.GetFieldValue(Terrain.Name.Height) <= Terrain.waterLevelMax; } }

        public bool CanDrownHere
        { get { return this.GetFieldValue(Terrain.Name.Height) < Terrain.waterLevelMax - 10; } }

        public bool IsOnSand
        { get { return !this.IsInWater && (this.GetFieldValue(Terrain.Name.Humidity) <= 80 || this.GetFieldValue(Terrain.Name.Height) < 105); } }

        public bool IsOnRocks
        { get { return this.GetFieldValue(Terrain.Name.Height) >= Terrain.rocksLevelMin; } }

        public bool IsOnLava
        { get { return this.GetFieldValue(Terrain.Name.Height) >= Terrain.lavaMin; } }

        public bool IsInBiome
        { get { return this.GetFieldValue(Terrain.Name.Biome) > Terrain.biomeMin; } }

        public bool IsDeepInBiome
        { get { return this.GetFieldValue(Terrain.Name.Biome) > Terrain.biomeDeep; } }

        public PieceSoundPackTemplate.Action WalkSoundAction
        {
            get
            {
                if (this.world.ActiveLevel.levelType == Level.LevelType.Cave) return PieceSoundPackTemplate.Action.StepRock;
                if (this.GetExtProperty(name: ExtBoardProps.Name.BiomeSwamp)) return PieceSoundPackTemplate.Action.StepMud;
                if (this.IsOnLava) return PieceSoundPackTemplate.Action.StepLava;
                if (this.IsOnRocks) return PieceSoundPackTemplate.Action.StepRock;
                if (this.IsInWater)
                {
                    if (this.CanDrownHere) return PieceSoundPackTemplate.Action.SwimDeep;
                    else if (this.GetFieldValue(Terrain.Name.Height) < Terrain.waterLevelMax - 5) return PieceSoundPackTemplate.Action.SwimShallow;
                    else return PieceSoundPackTemplate.Action.StepWater;
                }

                if (this.IsOnSand) return PieceSoundPackTemplate.Action.StepSand;

                return PieceSoundPackTemplate.Action.StepGrass;
            }
        }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (this.visible == value) return;

                this.visible = value;
                if (value) Grid.AddToGroup(sprite: this, groupName: Cell.Group.Visible);
                else Grid.RemoveFromGroup(sprite: this, groupName: Cell.Group.Visible);
            }
        }

        public bool ObstructsCameraTarget
        {
            get
            {
                if (!this.IsOnBoard) return false;

                BoardPiece trackedPiece = this.world.camera.TrackedPiece;
                if (trackedPiece == null || !trackedPiece.sprite.visible) return false;

                if (!this.BlocksMovement || this.position.Y < trackedPiece.sprite.position.Y || this.boardPiece.id == trackedPiece.id) return false;
                return this.GfxRect.Contains(trackedPiece.sprite.position);
            }
        }

        public bool PlaceOnBoard(bool randomPlacement, Vector2 position, bool ignoreCollisions = false, bool precisePlacement = false, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false)
        {
            this.position = Vector2.Zero; // needed for placement purposes

            if (randomPlacement && !ignoreCollisions && !AnimData.LoadedPkgs.Contains(this.AnimPackage)) this.LoadPackageAndAssignFrame();

            bool placedCorrectly;

            if (randomPlacement) placedCorrectly = this.FindFreeSpotRandomly(ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity);
            else
            {
                if (closestFreeSpot)
                {
                    placedCorrectly = this.MoveToClosestFreeSpot(startPosition: position, checkIsOnBoard: false, ignoreDensity: ignoreDensity);
                }
                else
                {
                    int minDistance = minDistanceOverride == -1 ? this.boardPiece.pieceInfo.placeMinDistance : minDistanceOverride;
                    int maxDistance = maxDistanceOverride == -1 ? this.boardPiece.pieceInfo.placeMaxDistance : maxDistanceOverride;

                    if (this.boardPiece.IsPlantMadeByPlayer)
                    {
                        minDistance = 0;
                        maxDistance = 60;
                    }

                    placedCorrectly = this.FindFreeSpotNearby(position, minDistance: minDistance, maxDistance: maxDistance, ignoreCollisions: ignoreCollisions, precisePlacement: precisePlacement, ignoreDensity: ignoreDensity);
                }
            }

            if (placedCorrectly) this.IsOnBoard = true;
            else this.RemoveFromBoard();

            return placedCorrectly;
        }

        public void RemoveFromBoard()
        {
            if (this.particleEngine != null && this.particleEngine.HasAnyParticles) this.ReplaceWithParticleEmitter();

            Grid.RemoveSprite(this);
            this.position = new Vector2(-500, -500);  // to fail if trying to use in the future
            this.IsOnBoard = false;
        }

        public void UpdateBoardLocation()
        {
            this.boardPiece.level.grid.UpdateLocation(this);
        }

        public Dictionary<string, object> Serialize()
        {
            Dictionary<string, Object> spriteDataDict = new()
            {
                { "posX", (int)this.position.X },
                { "posY", (int)this.position.Y },
                { "animPackage", this.AnimPackage },
            };

            PieceInfo.Info pieceInfo = this.boardPiece.pieceInfo;

            if (this.allowedTerrain.HasBeenChanged) spriteDataDict["allowedTerrain"] = this.allowedTerrain.Serialize();
            if (this.hasBeenDiscovered) spriteDataDict["hasBeenDiscovered"] = this.hasBeenDiscovered;
            if (this.lightEngine != null && !this.lightEngine.Equals(pieceInfo.lightEngine)) spriteDataDict["lightSource"] = this.lightEngine.Serialize();
            if (this.particleEngine != null)
            {
                var particleData = this.particleEngine.Serialize();
                if (particleData != null) spriteDataDict["particleEngine"] = particleData;
            }
            if (this.color != pieceInfo.color) spriteDataDict["color"] = new byte[] { this.color.R, this.color.G, this.color.B, this.color.A };
            if (this.opacity != pieceInfo.opacity) spriteDataDict["opacity"] = this.opacity;
            if (this.AnimName != pieceInfo.animName) spriteDataDict["animName"] = this.AnimName;
            if (this.AnimSize != pieceInfo.animSize) spriteDataDict["animSize"] = this.AnimSize;

            return spriteDataDict;
        }

        public void Deserialize(object spriteData)
        {
            var spriteDict = (Dictionary<string, Object>)spriteData;

            this.position = new Vector2((int)(Int64)spriteDict["posX"], (int)(Int64)spriteDict["posY"]);

            this.AnimPackage = (AnimData.PkgName)(Int64)spriteDict["animPackage"];
            if (spriteDict.ContainsKey("animName")) this.AnimName = (string)spriteDict["animName"];
            if (spriteDict.ContainsKey("animSize")) this.AnimSize = (byte)(Int64)spriteDict["animSize"];
            this.AssignFrame(checkForCollision: false);

            if (spriteDict.ContainsKey("hasBeenDiscovered")) this.hasBeenDiscovered = (bool)spriteDict["hasBeenDiscovered"];
            if (spriteDict.ContainsKey("allowedTerrain")) this.allowedTerrain = AllowedTerrain.Deserialize(spriteDict["allowedTerrain"]);
            if (spriteDict.ContainsKey("lightSource")) this.lightEngine = LightEngine.Deserialize(lightData: spriteDict["lightSource"], sprite: this);
            if (spriteDict.ContainsKey("particleEngine")) ParticleEngine.Deserialize(particleData: spriteDict["particleEngine"], sprite: this);
            if (spriteDict.ContainsKey("opacity")) this.opacity = (float)(double)spriteDict["opacity"];
            if (spriteDict.ContainsKey("color"))
            {
                var colorArray = (byte[])spriteDict["color"];
                this.color = new Color(r: colorArray[0], g: colorArray[1], b: colorArray[2], alpha: colorArray[3]);
            }
        }

        public byte GetFieldValue(Terrain.Name terrainName)
        {
            if (!this.IsOnBoard) throw new ArgumentException($"Trying to get a field value of '{this.boardPiece.name}' that is not on board.");
            return this.boardPiece.level.grid.terrainByName[terrainName].GetMapData((int)this.position.X, (int)this.position.Y);
        }

        public bool GetExtProperty(ExtBoardProps.Name name)
        {
            if (!this.IsOnBoard) throw new ArgumentException($"Trying to get an ext value of '{this.boardPiece.name}' that is not on board.");
            return this.boardPiece.level.grid.ExtBoardProps.GetValue(name: name, x: (int)position.X, y: (int)position.Y);
        }

        public static string GetCompleteAnimId(AnimData.PkgName animPackage, int animSize, string animName)
        { return $"{animPackage}-{animSize}-{animName}"; }

        public int GetAnimDuration()
        {
            int duration = 0;

            if (AnimData.frameArrayById.ContainsKey(this.CompleteAnimID))
            {
                foreach (var frame in AnimData.frameArrayById[this.CompleteAnimID])
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
                    double angle = this.world.random.NextSingle() * Math.PI * 2;

                    double offsetX = Math.Round(currentDistance * Math.Cos(angle));
                    double offsetY = Math.Round(currentDistance * Math.Sin(angle));
                    Vector2 newPos = startPosition + new Vector2((float)offsetX, (float)offsetY);

                    if (this.SetNewPosition(newPos: newPos, ignoreDensity: ignoreDensity, checkIsOnBoard: checkIsOnBoard)) return true;
                }
            }

            MessageLog.Add(debugMessage: true, text: $"Could not move to closest free spot - {this.boardPiece.name}.", textColor: Color.Violet);

            return false;
        }

        private bool FindFreeSpotRandomly(bool ignoreCollisions = false, bool ignoreDensity = false)
        {
            for (int tryIndex = 0; tryIndex < 4; tryIndex++)
            {
                Cell randomCell = this.boardPiece.level.grid.GetRandomCellForPieceName(pieceName: this.boardPiece.name, returnDummyCellIfInsideCamera: this.world.createMissingPiecesOutsideCamera);

                bool hasBeenMoved = this.SetNewPosition(
                    newPos: new Vector2(this.world.random.Next(randomCell.xMin, randomCell.xMax), this.world.random.Next(randomCell.yMin, randomCell.yMax)),
                    ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                if (hasBeenMoved || ignoreCollisions) return true;
            }

            return false;
        }

        private bool FindFreeSpotNearby(Vector2 startPosition, int minDistance, int maxDistance, bool ignoreCollisions = false, bool precisePlacement = false, bool ignoreDensity = false)
        {
            if (ignoreCollisions)
            {
                this.SetNewPosition(newPos: new Vector2(startPosition.X, startPosition.Y), ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                return true;
            }

            if (precisePlacement) return this.SetNewPosition(newPos: startPosition, ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);

            int numberOfTries = (minDistance == 0 && maxDistance == 0) ? 1 : 4;

            for (int tryIndex = 0; tryIndex < numberOfTries; tryIndex++)
            {
                var offset = new Vector2(this.world.random.Next(minDistance, maxDistance), this.world.random.Next(minDistance, maxDistance));

                if (this.world.random.Next(2) == 1) offset.X *= -1;
                if (this.world.random.Next(2) == 1) offset.Y *= -1;

                Vector2 newPos = startPosition + offset;
                newPos.X = Math.Max(newPos.X, 0);
                newPos.X = Math.Min(newPos.X, this.world.ActiveLevel.width - 1);
                newPos.Y = Math.Max(newPos.Y, 0);
                newPos.Y = Math.Min(newPos.Y, this.world.ActiveLevel.height - 1);

                bool hasBeenMoved = this.SetNewPosition(newPos: newPos, ignoreCollisions: ignoreCollisions, ignoreDensity: ignoreDensity, checkIsOnBoard: false);
                if (hasBeenMoved) return true;
            }

            return false;
        }

        public void SetOrientationByMovement(Vector2 movement, float orientationAngleOverride = -100f)
        {
            if (orientationAngleOverride != -100f) this.OrientationAngle = orientationAngleOverride;
            else if (movement != Vector2.Zero) this.OrientationAngle = Helpers.GetAngleBetweenTwoPoints(start: this.position, end: this.position + movement);

            Orientation newOrientation;

            if (Math.Abs(movement.X) > Math.Abs(movement.Y)) newOrientation = (movement.X < 0) ? Orientation.left : Orientation.right;
            else newOrientation = (movement.Y < 0) ? Orientation.up : Orientation.down;

            if (this.orientation != newOrientation)
            {
                // to avoid flickering
                if (this.world.CurrentUpdate < this.lastOrientationChangeFrame + 12) return;
                this.lastOrientationChangeFrame = this.world.CurrentUpdate;
                this.orientation = newOrientation;
            }

            if (this.AnimName.Contains("walk-")) this.CharacterWalk();
            if (this.AnimName.Contains("stand-")) this.CharacterStand();
        }

        public Sprite FindClosestSprite(List<Sprite> spriteList)
        {
            try
            { return spriteList.OrderBy(s => Vector2.Distance(this.position, s.position)).First(); }
            catch (ArgumentOutOfRangeException)
            { }

            return null;
        }

        public void Kill()
        {
            Type pieceType = this.boardPiece.GetType();

            if (pieceType != typeof(Fruit)) // fruits can't really "die"
            {
                this.CharacterStand(setEvenIfMissing: false);
                if (CheckIfAnimNameExists("dead")) this.AssignNewName("dead");
                else this.color = Color.LightCoral;
            }

            if (pieceType == typeof(Animal)) PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.position, templateName: PieceTemplate.Name.BloodSplatter);
        }

        public void Destroy()
        {
            if (this.particleEngine != null)
            {
                // creating particleEmitter, that will finish particle animation of this destroyed sprite
                if (this.IsOnBoard && this.particleEngine != null && this.particleEngine.HasAnyParticles) this.ReplaceWithParticleEmitter();
                else this.particleEngine?.Dispose();
            }
        }

        private void ReplaceWithParticleEmitter()
        {
            ParticleEngine.TurnOffAll(this); // every "infinite" preset should end
            BoardPiece particleEmitter = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.position, templateName: PieceTemplate.Name.ParticleEmitterEnding, precisePlacement: true);

            AnimData.PkgName newAnimPackage = this.AnimFrame.layer switch
            {
                -1 => AnimData.PkgName.WhiteSpotLayerMinus1,
                0 => AnimData.PkgName.WhiteSpotLayerZero,
                1 => AnimData.PkgName.WhiteSpotLayerOne,
                2 => AnimData.PkgName.WhiteSpotLayerTwo,
                3 => AnimData.PkgName.WhiteSpotLayerThree,
                _ => throw new ArgumentException($"Unsupported layer - {this.AnimFrame.layer}."),
            };

            if (this.particleEngine != null &&
                (this.particleEngine.HasPreset(ParticleEngine.Preset.DustPuff) || this.particleEngine.HasPreset(ParticleEngine.Preset.SmokePuff)))
            {
                // smoke / dust puff should appear above everything else
                newAnimPackage = AnimData.PkgName.WhiteSpotLayerTwo;
            }

            particleEmitter.sprite.AssignNewPackage(newAnimPackage: newAnimPackage, checkForCollision: false);

            this.particleEngine.ReassignSprite(particleEmitter.sprite);
        }

        private List<Cell.Group> GetGridGroups()
        {
            var groupNames = new List<Cell.Group> { Cell.Group.All };

            if (!this.IgnoresCollisions)
            {
                if (this.BlocksPlantGrowth || this.BlocksMovement) groupNames.Add(Cell.Group.ColPlantGrowth); // what blocks movement, should block plant growth too
                if (this.BlocksMovement) groupNames.Add(Cell.Group.ColMovement);
            }
            if (this.visible) groupNames.Add(Cell.Group.Visible);

            return groupNames;
        }

        public bool Move(Vector2 movement, bool splitXY = true)
        {
            List<Vector2> movesToTest = new() { movement };

            float moveDelta = Vector2.Distance(movement, Vector2.Zero);
            movesToTest.Add(new Vector2(movement.X / 2, movement.Y / 2));
            if (moveDelta > 1 && splitXY) movesToTest.Add(new Vector2(Math.Clamp(value: movement.X, min: -1, max: 1), Math.Clamp(value: movement.Y, min: -1, max: 1)));

            foreach (Vector2 testMove in movesToTest)
            {
                if (((int)testMove.X != 0 || (int)testMove.Y != 0) && this.SetNewPosition(this.position + testMove)) return true;
            }
            return false;
        }

        public bool SetNewPosition(Vector2 newPos, bool ignoreCollisions = false, bool updateBoardLocation = true, bool ignoreDensity = false, bool checkIsOnBoard = true)
        {
            if (!this.IsOnBoard && checkIsOnBoard) throw new ArgumentException($"Trying to move '{this.boardPiece.name}' that is not on board.");

            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2(Convert.ToInt32(newPos.X), Convert.ToInt32(newPos.Y)); // to ensure integer values
            this.position = this.world.KeepVector2InWorldBounds(this.position);

            this.UpdateRects();
            if (updateBoardLocation) this.UpdateBoardLocation();
            if (ignoreCollisions) return true;

            bool collisionDetected = this.CheckForCollision(ignoreDensity: ignoreDensity);
            if (collisionDetected)
            {
                this.position = originalPosition;
                this.UpdateRects();
                if (updateBoardLocation) this.UpdateBoardLocation();
            }

            return !collisionDetected;
        }

        public List<Sprite> GetCollidingSpritesAtPosition(Vector2 positionToCheck, List<Cell.Group> cellGroupsToCheck)
        {
            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2(
                x: (int)Math.Clamp(value: positionToCheck.X, min: 0, max: this.world.ActiveLevel.width - 1),
                y: (int)Math.Clamp(value: positionToCheck.Y, min: 0, max: this.world.ActiveLevel.height - 1));
            // to ensure integer values

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
                foreach (Sprite sprite in this.boardPiece.level.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: group))
                {
                    if (this.ColRect.Intersects(sprite.ColRect) && sprite.id != this.id) collidingSprites.Add(sprite);
                }
            }

            return collidingSprites;
        }

        public bool CheckForCollision(bool ignoreDensity = false)
        {
            if (this.world == null) return false;
            if (this.GfxRect.Left <= 0 || this.GfxRect.Right >= this.world.ActiveLevel.width || this.GfxRect.Top <= 0 || this.GfxRect.Bottom >= this.world.ActiveLevel.height) return true;
            if (this.IgnoresCollisions) return false;

            bool plantingMode = this.boardPiece.IsPlantMadeByPlayer;
            if (plantingMode)
            {
                AllowedDensity plantingDensity = new(radius: this.BlocksMovement ? (ushort)50 : (ushort)30, maxNoOfPiecesSameName: 0);

                if (!plantingDensity.CanBePlacedHere(this.boardPiece)) return true;
            }
            else
            {
                if (this.boardPiece.pieceInfo.allowedDensity != null && !ignoreDensity && !this.boardPiece.pieceInfo.allowedDensity.CanBePlacedHere(this.boardPiece)) return true;
            }

            if (!plantingMode && !this.allowedTerrain.CanStandHere(level: this.boardPiece.level, position: this.position)) return true;

            var gridTypeToCheck = this.boardPiece.GetType() == typeof(Plant) ? Cell.Group.ColPlantGrowth : Cell.Group.ColMovement;

            foreach (Sprite sprite in this.boardPiece.level.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: gridTypeToCheck))
            {
                if (this.ColRect.Intersects(sprite.ColRect) && sprite.id != this.id) return true;
            }

            if (plantingMode && Plant.GetFertileGround(this.boardPiece) == null) return true;
            if (this.boardPiece.GetType() == typeof(Plant) && this.IsInCameraRect && !Preferences.debugShowPlantGrowthInCamera && !this.world.BuildMode) return true;

            return false;
        }

        public bool CheckIfOtherSpriteIsWithinRange(Sprite target, int range = 4)
        {
            if (!target.IsOnBoard) return false;

            if (Vector2.Distance(this.position, target.position) <= range * 1.5) return true;

            Rectangle rectangle1 = this.ColRect;
            Rectangle rectangle2 = target.ColRect;

            rectangle1.Inflate(range, range);
            rectangle2.Inflate(range, range);

            return rectangle1.Intersects(rectangle2);
        }

        private void UpdateRects()
        {
            Rectangle gfxRect = new(
                x: (int)(position.X + this.AnimFrame.gfxOffset.X),
                y: (int)(position.Y + this.AnimFrame.gfxOffset.Y),
                width: this.AnimFrame.gfxWidth,
                height: this.AnimFrame.gfxHeight);

            this.GfxRect = gfxRect;

            Rectangle colRect = new(
                x: (int)(position.X + this.AnimFrame.colOffset.X),
                y: (int)(position.Y + this.AnimFrame.colOffset.Y),
                width: this.AnimFrame.colWidth,
                height: this.AnimFrame.colHeight);

            this.ColRect = colRect;
        }

        public void AssignNewPackage(AnimData.PkgName newAnimPackage, bool setEvenIfMissing = true, bool checkForCollision = true)
        {
            if (this.AnimPackage == newAnimPackage) return;

            AnimData.PkgName oldAnimPackage = this.AnimPackage;

            if (setEvenIfMissing || CheckIfAnimPackageExists(newAnimPackage))
            {
                this.AnimPackage = newAnimPackage;
                bool frameAssignedCorrectly = this.AssignFrame(forceRewind: true, checkForCollision: checkForCollision);
                if (!frameAssignedCorrectly) this.AnimPackage = oldAnimPackage;
            }
        }

        public void AssignNewSize(byte newAnimSize, bool setEvenIfMissing = true, bool checkForCollision = true)
        {
            if (this.AnimSize == newAnimSize) return;

            byte oldAnimSize = this.AnimSize;

            if (setEvenIfMissing || CheckIfAnimSizeExists(newAnimSize))
            {
                this.AnimSize = newAnimSize;
                bool frameAssignedCorrectly = this.AssignFrame(forceRewind: true, checkForCollision: checkForCollision);
                if (!frameAssignedCorrectly) this.AnimSize = oldAnimSize;
            }
        }

        public void AssignNewName(string newAnimName, bool setEvenIfMissing = true, bool checkForCollision = true)
        {
            if (this.AnimName == newAnimName) return;

            string oldAnimName = this.AnimName;

            if (setEvenIfMissing || this.CheckIfAnimNameExists(newAnimName))
            {
                this.AnimName = newAnimName;
                bool frameAssignedCorrectly = this.AssignFrame(forceRewind: true, checkForCollision: checkForCollision);
                if (!frameAssignedCorrectly) this.AnimName = oldAnimName;
            }
        }

        public void LoadPackageAndAssignFrame()
        {
            AnimData.LoadPackage(this.AnimPackage);
            this.AssignFrame(checkForCollision: false);
        }

        public void AssignFrameForce(AnimFrame animFrame)
        {
            // does not check collisions, use with caution
            this.AnimFrame = animFrame;
        }

        private bool AssignFrame(bool forceRewind = false, bool checkForCollision = true)
        {
            AnimFrame oldAnimFrame = this.AnimFrame;

            try
            {
                if (forceRewind || this.currentFrameIndex >= AnimData.frameArrayById[this.CompleteAnimID].Length) this.RewindAnim();
                this.AnimFrame = AnimData.frameArrayById[this.CompleteAnimID][this.currentFrameIndex];
            }
            catch (KeyNotFoundException)
            {
                // MessageLog.Add(debugMessage: true, text: $"Anim frame not found {this.CompleteAnimID}.");
                this.AnimFrame = AnimData.GetCroppedFrameForPackage(AnimData.PkgName.Loading);
                this.world?.ActiveLevel.spritesWithAnimPackagesToLoad.Enqueue(this);
            }

            this.currentFrameTimeLeft = this.AnimFrame.duration;

            if (!this.IsOnBoard) return true;

            this.UpdateRects();

            // in case of collision - reverting to a previous, non-colliding animFrame

            bool collisionDetected = checkForCollision && this.CheckForCollision();
            if (collisionDetected)
            {
                this.AnimFrame = oldAnimFrame;
                this.UpdateRects();
            }

            return !collisionDetected;
        }

        public bool CheckIfAnimPackageExists(AnimData.PkgName animPackageToCheck)
        {
            string completeAnimIdToCheck = GetCompleteAnimId(animPackage: animPackageToCheck, animSize: this.AnimSize, animName: this.AnimName);
            return AnimData.frameArrayById.ContainsKey(completeAnimIdToCheck);
        }

        public bool CheckIfAnimSizeExists(byte animSizeToCheck)
        {
            string completeAnimIdToCheck = GetCompleteAnimId(animPackage: this.AnimPackage, animSize: animSizeToCheck, animName: this.AnimName);
            return AnimData.frameArrayById.ContainsKey(completeAnimIdToCheck);
        }

        public bool CheckIfAnimNameExists(string animNameToCheck)
        {
            string completeAnimIdToCheck = GetCompleteAnimId(animPackage: this.AnimPackage, animSize: this.AnimSize, animName: animNameToCheck);
            return AnimData.frameArrayById.ContainsKey(completeAnimIdToCheck);
        }

        public void RewindAnim()
        {
            this.currentFrameIndex = 0;
            this.currentFrameTimeLeft = this.AnimFrame.duration;
        }

        public void UpdateAnimation(bool checkForCollision)
        {
            if (this.AnimFrame.duration == 0) return; // duration == 0 will stop the animation

            this.currentFrameTimeLeft--;
            if (this.currentFrameTimeLeft <= 0)
            {
                this.currentFrameIndex++;
                AssignFrame(checkForCollision: checkForCollision);
            }
        }

        public void CharacterStand(bool setEvenIfMissing = true, bool checkForCollision = true)
        { this.AssignNewName(newAnimName: $"stand-{this.orientation}", setEvenIfMissing: setEvenIfMissing, checkForCollision: checkForCollision); }

        public void CharacterWalk(bool setEvenIfMissing = true)
        { this.AssignNewName(newAnimName: $"walk-{this.orientation}", setEvenIfMissing: setEvenIfMissing); }

        public void Draw(bool calculateSubmerge = true)
        {
            if (!this.boardPiece.exists) return;

            if (!this.hasBeenDiscovered && this.world.MapEnabled && this.world.camera.IsTrackingPlayer && this.world.camera.viewRect.Contains(this.GfxRect)) this.hasBeenDiscovered = true;

            if (this.ObstructsCameraTarget && this.opacityFade == null) new OpacityFade(sprite: this, destOpacity: 0.5f, duration: 10, mode: OpacityFade.Mode.CameraTargetObstruct);

            if (Scene.UpdateStack.Contains(this.world))
            {
                this.opacityFade?.Process();
                if (!this.boardPiece.exists) return; // opacityFade might destroy the piece
            }

            if (Preferences.debugShowRects) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.GfxRect, this.GfxRect, Color.White * 0.35f);
            if (Preferences.debugShowFocusRect && this.boardPiece.GetType() == typeof(Player))
            {
                Rectangle focusRect = this.world.Player.GetFocusRect();
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, focusRect, SonOfRobinGame.WhiteRectangle.Bounds, Color.Yellow * 0.35f);
            }

            bool effectsShouldBeEnabled = this.effectCol.ThereAreEffectsToRender;
            if (!effectsShouldBeEnabled) this.DrawRoutine(calculateSubmerge: calculateSubmerge);
            else
            {
                bool thereWillBeMoreEffects = false;
                while (true)
                {
                    if (effectsShouldBeEnabled) thereWillBeMoreEffects = this.effectCol.TurnOnNextEffect(scene: this.world, currentUpdateToUse: world.CurrentUpdate, drawColor: this.color * this.opacity);
                    this.DrawRoutine(calculateSubmerge: calculateSubmerge);

                    if (!thereWillBeMoreEffects)
                    {
                        SonOfRobinGame.SpriteBatch.End();
                        SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix);
                        break;
                    }
                }
            }

            if (this.particleEngine != null && !Preferences.debugDisableParticles)
            {
                if (Scene.UpdateStack.Contains(this.world)) this.particleEngine.Update();
                if (this.particleEngine.HasAnyParticles)
                {
                    this.particleEngine.Draw();
                    this.boardPiece.level.recentParticlesManager.AddPiece(this.boardPiece);
                }
            }

            if (Preferences.debugShowRects)
            {
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(this.ColRect.X, this.ColRect.Y, this.ColRect.Width, this.ColRect.Height), this.ColRect, Color.Red * 0.55f);

                SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.position.X, (int)this.position.Y, 1, 1), color: Color.Blue, thickness: 2f);
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle((int)this.position.X, (int)this.position.Y, 1, 1), Color.White);
            }

            if (Preferences.debugShowStates && this.boardPiece.GetType() == typeof(Animal) && this.boardPiece.alive) this.DrawState();

            if (Preferences.debugShowStatBars || this.boardPiece.ShowStatBars) this.boardPiece.DrawStatBar();
        }

        public void DrawRoutine(bool calculateSubmerge, int offsetX = 0, int offsetY = 0)
        {
            Rectangle destRect = this.GfxRect;
            if (offsetX != 0 || offsetY != 0)
            {
                destRect.X += offsetX;
                destRect.Y += offsetY;
            }

            if (this.rotation == 0)
            {
                int submergeCorrection = 0;
                if (!this.boardPiece.pieceInfo.floatsOnWater && calculateSubmerge && this.IsInWater)
                {
                    submergeCorrection = (int)Helpers.ConvertRange(oldMin: 0, oldMax: Terrain.waterLevelMax, newMin: Math.Min(4, this.AnimFrame.gfxHeight), newMax: this.AnimFrame.gfxHeight, oldVal: Terrain.waterLevelMax - this.GetFieldValue(Terrain.Name.Height), clampToEdges: true);
                }

                this.AnimFrame.Draw(destRect: destRect, color: this.color, submergeCorrection: submergeCorrection, opacity: this.opacity);
            }
            else
            {
                this.AnimFrame.DrawWithRotation(position: new Vector2(destRect.Center.X, destRect.Center.Y), color: this.color, rotation: this.rotation, rotationOriginOverride: this.rotationOriginOverride, opacity: this.opacity);
            }

            if (this.boardPiece.PieceStorage != null && this.boardPiece.GetType() == typeof(Plant)) this.DrawFruits();
        }

        private void DrawFruits()
        {
            Plant plant = (Plant)this.boardPiece;
            if (Preferences.debugShowFruitRects) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, plant.fruitEngine.FruitAreaRect, Color.Cyan * 0.4f);

            if (plant.PieceStorage.OccupiedSlotsCount == 0) return;
            if (plant.fruitEngine != null && plant.fruitEngine.hiddenFruits) return;

            var fruitList = plant.PieceStorage.GetAllPieces();
            foreach (BoardPiece fruit in fruitList)
            {
                if (this.rotation == 0)
                {
                    // regular drawing
                    fruit.sprite.Draw(calculateSubmerge: false);
                }
                else
                {
                    // drawing with rotation, taking sway into account

                    Sprite fruitSprite = fruit.sprite;

                    Vector2 rotationOriginOverride = new Vector2(this.GfxRect.Left, this.GfxRect.Top) - new Vector2(fruitSprite.GfxRect.Left, fruitSprite.GfxRect.Top);
                    rotationOriginOverride += new Vector2((float)this.AnimFrame.gfxWidth * 0.5f, this.AnimFrame.gfxHeight);
                    rotationOriginOverride /= fruitSprite.AnimFrame.scale; // DrawWithRotation() will multiply rotationOriginOverride by target frame scale

                    float originalFruitRotation = fruitSprite.rotation;
                    fruitSprite.rotation = this.rotation;

                    fruitSprite.AnimFrame.DrawWithRotation(position: new Vector2(fruitSprite.GfxRect.Center.X, fruitSprite.GfxRect.Center.Y), color: fruitSprite.color, rotation: this.rotation, rotationOriginOverride: rotationOriginOverride, opacity: this.opacity);

                    fruitSprite.rotation = originalFruitRotation;
                }
            }
        }

        public void DrawAndKeepInRectBounds(Rectangle destRect, float opacity)
        {
            AnimFrame frameToDraw = this.AnimFrame.cropped ? this.AnimFrame : this.CroppedAnimFrame;
            frameToDraw.DrawAndKeepInRectBounds(destBoundsRect: destRect, color: this.color * opacity);
        }

        private void DrawState()
        {
            string stateTxt = $"{this.boardPiece.activeState}".Replace("Player", "").Replace("Animal", "");
            var stateFont = SonOfRobinGame.FontPressStart2P.GetFont(8);

            Vector2 textSize = Helpers.MeasureStringCorrectly(font: stateFont, stringToMeasure: stateTxt);
            // text position should be integer, otherwise it would get blurry
            Vector2 txtPos = new(
                (int)(this.position.X - (textSize.X / 2)),
            (int)(this.position.Y - (textSize.Y / 2)));

            stateFont.DrawText(batch: SonOfRobinGame.SpriteBatch, text: stateTxt, position: txtPos, color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: 1);
        }

        public static void DrawShadow(Color color, Sprite shadowSprite, Vector2 lightPos, float shadowAngle, int drawOffsetX = 0, int drawOffsetY = 0, float yScaleForce = 0f)
        {
            float distance = Vector2.Distance(lightPos, shadowSprite.position);
            AnimFrame frame = shadowSprite.AnimFrame;

            if (shadowSprite.boardPiece.HasFlatShadow)
            {
                float xDiff = shadowSprite.position.X - lightPos.X;
                float yDiff = shadowSprite.position.Y - lightPos.Y;

                float xLimit = shadowSprite.GfxRect.Width / 8;
                float yLimit = shadowSprite.GfxRect.Height / 8;

                float offsetX = Math.Clamp(value: xDiff / 6f, min: -xLimit, max: xLimit);
                float offsetY = Math.Clamp(value: yDiff / 6f, min: -yLimit, max: yLimit);

                Rectangle simulRect = shadowSprite.GfxRect;
                simulRect.X += (int)offsetX;
                simulRect.Y += (int)offsetY;
                if (!shadowSprite.world.camera.viewRect.Intersects(simulRect)) return;

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

                SonOfRobinGame.SpriteBatch.Draw(
                    frame.texture,
                    position:
                    new Vector2(shadowSprite.position.X + drawOffsetX, shadowSprite.position.Y + drawOffsetY),
                    sourceRectangle: frame.textureRect,
                    color: color * shadowSprite.opacity,
                    rotation: shadowAngle + (float)(Math.PI / 2f),
                    origin: new Vector2(-frame.gfxOffset.X / frame.scale, -(frame.gfxOffset.Y + frame.colOffset.Y) / frame.scale),
                    scale: new Vector2(xScale, yScale),
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }
    }
}