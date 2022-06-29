using Microsoft.Xna.Framework;
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
        public static int maxDistanceOverride = -1; // -1 will not affect sprite creation; higher values will override one sprite creation
        public static bool ignoreDensityOverride = false; // will ignore density for one sprite creation

        public readonly string id;
        public readonly BoardPiece boardPiece;
        public readonly World world;
        public Vector2 position;
        public bool placedCorrectly;
        public Orientation orientation;
        public float rotation;
        public float opacity;
        public AnimFrame frame;
        public Color color;
        public AnimPkg animPackage;
        public byte animSize;
        public string animName;
        private byte currentFrameIndex;
        private ushort currentFrameTimeLeft; // measured in game frames
        public OpacityFade opacityFade;
        public Rectangle gfxRect;
        public Rectangle colRect;
        public readonly bool blocksMovement;
        public readonly bool checksFullCollisions;
        public readonly bool ignoresCollisions;
        public AllowedFields allowedFields;
        private readonly AllowedDensity allowedDensity;
        private readonly bool floatsOnWater;
        public readonly bool isShownOnMiniMap;
        public bool hasBeenDiscovered;
        public readonly EffectCol effectCol;
        public Cell currentCell; // current cell, that is containing the sprite 
        public List<Cell.Group> gridGroups;

        public string CompleteAnimID
        { get { return GetCompleteAnimId(animPackage: this.animPackage, animSize: this.animSize, animName: this.animName); } }

        private bool visible;

        public bool IsInWater
        { get { return this.GetFieldValue(TerrainName.Height) < Terrain.waterLevelMax; } }
        public bool IsInLava
        { get { return this.GetFieldValue(TerrainName.Height) >= Terrain.lavaMin; } }
        public bool IsInDangerZone
        { get { return this.GetFieldValue(TerrainName.Danger) > Terrain.saveZoneMax; } }
        public bool IsDeepInDangerZone
        { get { return this.GetFieldValue(TerrainName.Danger) > Terrain.saveZoneMax * 1.3; } }

        public bool CanDrownHere { get { return this.GetFieldValue(TerrainName.Height) < (Terrain.waterLevelMax - 10); } }
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (this.visible == value) return;

                this.visible = value;
                if (value)
                { this.world.grid.AddToGroup(sprite: this, groupName: Cell.Group.Visible); }
                else
                { this.world.grid.RemoveFromGroup(sprite: this, groupName: Cell.Group.Visible); }
            }
        }

        public bool ObstructsPlayer
        {
            get
            {
                try
                {
                    if (!this.blocksMovement || this.position.Y < this.boardPiece.world.player.sprite.position.Y || this.boardPiece.name == PieceTemplate.Name.Player) return false;
                    return this.gfxRect.Contains(this.boardPiece.world.player.sprite.position);
                }
                catch (NullReferenceException)
                { return false; }
            }
        }

        public Sprite(World world, string id, BoardPiece boardPiece, Vector2 position, AnimPkg animPackage, byte animSize, string animName, bool ignoresCollisions, AllowedFields allowedFields, bool blocksMovement = true, bool visible = true, bool checksFullCollisions = false, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, bool fadeInAnim = true, bool placeAtBeachEdge = false, bool isShownOnMiniMap = false, AllowedDensity allowedDensity = null)
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
            this.checksFullCollisions = checksFullCollisions;
            this.ignoresCollisions = ignoresCollisions;
            this.allowedFields = allowedFields;
            this.allowedDensity = allowedDensity;
            if (ignoreDensityOverride)
            {
                this.allowedDensity = null;
                ignoreDensityOverride = false;
            }
            if (this.allowedDensity != null) this.allowedDensity.FinishCreation(piece: this.boardPiece, sprite: this);
            this.visible = visible; // initially it is assigned normally
            this.isShownOnMiniMap = isShownOnMiniMap;
            this.effectCol = new EffectCol(sprite: this);
            this.hasBeenDiscovered = false;
            this.currentCell = null;
            if (fadeInAnim)
            {
                this.opacity = 0f;
                this.opacityFade = new OpacityFade(sprite: this, destOpacity: 1f);
            }
            else
            { this.opacity = 1f; }

            AssignFrame(); // frame needs to be set before checking for free spot

            this.gridGroups = this.GetGridGroups();

            this.placedCorrectly = this.FindFreeSpot(position, minDistance: minDistance, maxDistance: maxDistance, findAtBeachEdge: placeAtBeachEdge); // this.position is set here
            if (!this.placedCorrectly)
            {
                this.RemoveFromGrid();
                return;
            }
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
            pieceData["sprite_allowedFields"] = this.allowedFields;
        }
        public void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.position = new Vector2((float)pieceData["sprite_positionX"], (float)pieceData["sprite_positionY"]);
            this.orientation = (Orientation)pieceData["sprite_orientation"];
            this.rotation = (float)pieceData["sprite_rotation"];
            this.opacity = (float)pieceData["sprite_opacity"];
            this.opacityFade = (OpacityFade)pieceData["sprite_opacityFade"];
            if (this.opacityFade != null) this.opacityFade.sprite = this;
            this.animPackage = (AnimPkg)pieceData["sprite_animPackage"];
            this.animSize = (byte)pieceData["sprite_animSize"];
            this.animName = (string)pieceData["sprite_animName"];
            this.currentFrameIndex = (byte)pieceData["sprite_currentFrameIndex"];
            this.currentFrameTimeLeft = (ushort)pieceData["sprite_currentFrameTimeLeft"];
            this.AssignFrameById((string)pieceData["sprite_frame_id"]);
            this.gridGroups = (List<Cell.Group>)pieceData["sprite_gridGroups"];
            this.hasBeenDiscovered = (bool)pieceData["sprite_hasBeenDiscovered"];
            this.allowedFields = (AllowedFields)pieceData["sprite_allowedFields"];
        }
        public byte GetFieldValue(TerrainName terrainName)
        {
            return this.world.grid.GetFieldValue(position: this.position, terrainName: terrainName);
        }

        public static string GetCompleteAnimId(AnimPkg animPackage, byte animSize, string animName)
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
        public bool MoveToClosestFreeSpot(Vector2 startPosition, bool extensiveSearch = false)
        {
            if (!this.placedCorrectly) throw new DivideByZeroException($"Trying to move '{this.boardPiece.name}' that was not placed correctly.");

            int threshold = extensiveSearch ? 10000 : 450;
            ushort maxDistance = 0;

            for (int i = 0; i < threshold; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Vector2 movement = startPosition + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));

                    if (this.SetNewPosition(newPos: movement)) return true;
                }

                maxDistance += 2;
            }

            if (this.FindFreeSpot(startPosition: startPosition, minDistance: 0, maxDistance: 65535)) return true;

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Could not move to closest free spot - {this.boardPiece.name}.", color: Color.Violet);

            return false;
        }

        private bool FindFreeSpot(Vector2 startPosition, ushort minDistance, ushort maxDistance, bool findAtBeachEdge = false)
        {
            if (this.world.freePiecesPlacingMode)
            {
                this.SetNewPosition(newPos: new Vector2(startPosition.X, startPosition.Y));
                return true;
            }
            if (maxDistanceOverride != -1)
            {
                minDistance = 0;
                maxDistance = (ushort)maxDistanceOverride;
                maxDistanceOverride = -1;
            }

            if (findAtBeachEdge)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.FindFreeSpotAtBeachEdge()) return true;
                }

                return false; // if no free spot at the edge was found
            }

            int numberOfTries = (minDistance == 0 && maxDistance == 0) ? 1 : 4;
            Vector2 newPosition;

            if (startPosition.X == -100 && startPosition.Y == -100) // -100, -100 will be converted to any position on the map - needed for effective creation of new sprites 
            {
                for (int tryIndex = 0; tryIndex < numberOfTries; tryIndex++)
                {
                    if (this.world.createMissinPiecesOutsideCamera)
                    { newPosition = this.world.camera.GetRandomPositionOutsideCameraView(); }
                    else
                    { newPosition = new Vector2(this.world.random.Next(0, this.world.width - 1), this.world.random.Next(0, this.world.height - 1)); }

                    bool hasBeenMoved = this.SetNewPosition(newPos: newPosition);
                    if (hasBeenMoved) return true;
                }
            }

            else
            {
                for (int tryIndex = 0; tryIndex < numberOfTries; tryIndex++)
                {
                    var offset = new Vector2(this.world.random.Next(minDistance, maxDistance), this.world.random.Next(minDistance, maxDistance));

                    if (this.world.random.Next(2) == 1) { offset.X *= -1; }
                    if (this.world.random.Next(2) == 1) { offset.Y *= -1; }

                    newPosition = startPosition + offset;
                    newPosition.X = Math.Max(newPosition.X, 0);
                    newPosition.X = Math.Min(newPosition.X, this.world.width - 1);
                    newPosition.Y = Math.Max(newPosition.Y, 0);
                    newPosition.Y = Math.Min(newPosition.Y, this.world.height - 1);

                    bool hasBeenMoved = this.SetNewPosition(newPos: newPosition);
                    if (hasBeenMoved) return true;
                }
            }

            return false;
        }

        private bool FindFreeSpotAtBeachEdge()
        {
            bool useWidth = this.world.random.Next(0, 2) == 0;
            bool minMax = this.world.random.Next(0, 2) == 0;
            Vector2 startPos;

            if (useWidth)
            { startPos = new Vector2(this.world.random.Next((int)(this.world.width * 0.05f), (int)(this.world.width * 0.95f)), minMax ? this.world.height : 0); }
            else
            { startPos = new Vector2(minMax ? this.world.width : 0, this.world.random.Next((int)(this.world.height * 0.05f), (int)(this.world.height * 0.95f))); }

            int stepWidth = 3;
            if (minMax) stepWidth *= -1;
            Vector2 step = useWidth ? new Vector2(0, stepWidth) : new Vector2(stepWidth, 0);

            Rectangle worldRect = new Rectangle(0, 0, this.world.width, this.world.height);
            Vector2 currentPos = startPos;
            int height;

            while (true)
            {
                currentPos += step;
                if (!worldRect.Contains(currentPos)) return false;

                height = this.world.grid.GetFieldValue(terrainName: TerrainName.Height, position: currentPos);
                if (height > Terrain.waterLevelMax + 10) return false;

                if (height >= Terrain.waterLevelMax + 1 && this.SetNewPosition(newPos: currentPos)) return true;
            }
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

            if (CheckIFAnimNameExists("dead"))
            { this.AssignNewName("dead"); }
            else
            { this.color = Color.LightCoral; }

            if (this.boardPiece.GetType() == typeof(Animal)) PieceTemplate.CreateOnBoard(world: this.world, position: this.position, templateName: PieceTemplate.Name.BloodSplatter);
        }

        public void Destroy()
        { this.RemoveFromGrid(); }


        private List<Cell.Group> GetGridGroups()
        {
            var groupNames = new List<Cell.Group> { Cell.Group.All };

            if (!this.ignoresCollisions)
            {
                groupNames.Add(Cell.Group.ColAll);
                if (this.blocksMovement) groupNames.Add(Cell.Group.ColBlocking);
            }
            if (this.visible) groupNames.Add(Cell.Group.Visible);
            if (this.isShownOnMiniMap) groupNames.Add(Cell.Group.MiniMap);

            return groupNames;
        }

        public void RemoveFromGrid()
        { this.world.grid.RemoveSprite(this); }

        public void UpdateGridLocation()
        { this.world.grid.UpdateLocation(this); }

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

        public bool SetNewPosition(Vector2 newPos, bool ignoreCollisions = false, bool updateGridLocation = true)
        {
            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2((int)newPos.X, (int)newPos.Y); // to ensure integer values
            this.position.X = Math.Min(Math.Max(this.position.X, 0), this.world.width - 1);
            this.position.Y = Math.Min(Math.Max(this.position.Y, 0), this.world.height - 1);

            this.UpdateRects();
            if (updateGridLocation) this.UpdateGridLocation();
            if (ignoreCollisions) return true;

            bool collisionDetected = this.CheckForCollision();
            if (collisionDetected)
            {
                this.position = originalPosition;
                this.UpdateRects();
                if (updateGridLocation) this.UpdateGridLocation();
            }

            return !collisionDetected;
        }

        public List<Sprite> GetCollidingSpritesAtPosition(Vector2 positionToCheck)
        {
            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2((int)positionToCheck.X, (int)positionToCheck.Y); // to ensure integer values
            this.position.X = Math.Min(Math.Max(this.position.X, 0), this.world.width - 1);
            this.position.Y = Math.Min(Math.Max(this.position.Y, 0), this.world.height - 1);

            this.UpdateRects();
            this.UpdateGridLocation();

            List<Sprite> collidingSpritesList = this.GetCollidingSprites();

            this.position = originalPosition;
            this.UpdateRects();
            this.UpdateGridLocation();

            return collidingSpritesList;
        }

        public bool CheckForCollision()
        {
            if (this.world.freePiecesPlacingMode) return false;

            if (this.gfxRect.Left <= 0 || this.gfxRect.Right >= this.world.width || this.gfxRect.Top <= 0 || this.gfxRect.Bottom >= this.world.height) return true;
            if (this.ignoresCollisions) return false;
            if (this.allowedDensity != null && !this.allowedDensity.CanBePlacedHere()) return true;
            if (!this.allowedFields.CanStandHere(world: this.world, position: this.position)) return true;

            var gridTypeToCheck = this.checksFullCollisions ? Cell.Group.ColAll : Cell.Group.ColBlocking;

            foreach (Sprite sprite in this.world.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: gridTypeToCheck))
            {
                if (this.colRect.Intersects(sprite.colRect) && sprite.id != this.id) return true;
            }

            return false;
        }

        public bool CheckIfOtherSpriteIsWithinRange(Sprite target, int range = 4)
        {
            //if (!target.Visible) return false;

            if (Vector2.Distance(this.position, target.position) <= range * 1.5) return true;

            Rectangle rectangle1 = this.colRect;
            Rectangle rectangle2 = target.colRect;

            rectangle1.Inflate(range, range);
            rectangle2.Inflate(range, range);

            return rectangle1.Intersects(rectangle2);
        }

        public List<Sprite> GetCollidingSprites()
        {
            var collidingSprites = new List<Sprite>();

            foreach (Sprite sprite in this.world.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: Cell.Group.ColAll))
            { if (this.colRect.Intersects(sprite.colRect) && sprite.id != this.id) collidingSprites.Add(sprite); }

            return collidingSprites;
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

        public void AssignNewPackage(AnimPkg animPackage, bool setEvenIfMissing = true)
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
                if (CheckIFAnimNameExists(animName) || setEvenIfMissing)
                {
                    var animNameCopy = this.animName;

                    this.animName = animName;
                    if (!AssignFrame(forceRewind: true)) this.animName = animNameCopy;
                }
            }
        }
        public bool CheckIfAnimPackageExists(AnimPkg newAnimPackage)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: newAnimPackage, animSize: this.animSize, animName: this.animName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }

        public bool CheckIFAnimSizeExists(byte newAnimSize)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: this.animPackage, animSize: newAnimSize, animName: this.animName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }
        public bool CheckIFAnimNameExists(string newAnimName)
        {
            string newCompleteAnimID = GetCompleteAnimId(animPackage: this.animPackage, animSize: this.animSize, animName: newAnimName);
            return AnimData.frameListById.ContainsKey(newCompleteAnimID);
        }

        public bool AssignFrame(bool forceRewind = false)
        {
            AnimFrame frameCopy = this.frame;

            try
            {
                if (forceRewind || this.currentFrameIndex >= AnimData.frameListById[this.CompleteAnimID].Count) this.RewindAnim();
                this.frame = AnimData.frameListById[this.CompleteAnimID][this.currentFrameIndex];
            }
            catch (KeyNotFoundException) // placeholder frame if the animation was missing
            { this.frame = AnimData.frameListById["NoAnim-0-default"][0]; }

            this.currentFrameTimeLeft = this.frame.duration;
            this.UpdateRects();

            if (!blocksMovement) return true;

            // in case of collision - reverting to a previous, non-colliding frame
            if (this.CheckForCollision() && frameCopy != null)
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
            if (this.world.mapEnabled && !this.hasBeenDiscovered && this.world.camera.viewRect.Contains(this.gfxRect)) this.hasBeenDiscovered = true;

            if (this.ObstructsPlayer && this.opacityFade == null) this.opacityFade = new OpacityFade(sprite: this, destOpacity: 0.5f, playerObstructMode: true, duration: 10);
            this.opacityFade?.Process();

            if (Preferences.debugShowRects)
            { SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32(this.gfxRect.X), Convert.ToInt32(this.gfxRect.Y), this.gfxRect.Width, this.gfxRect.Height), this.gfxRect, Color.White * 0.5f); }

            bool effectsShouldBeEnabled = this.effectCol.ThereAreEffectsToRender;
            if (!effectsShouldBeEnabled) this.DrawRoutine(calculateSubmerge);
            else
            {
                bool thereWillBeMoreEffects = false;
                while (true)
                {
                    if (effectsShouldBeEnabled) thereWillBeMoreEffects = this.effectCol.TurnOnNextEffect(world: this.world);
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
            if (Preferences.debugShowStatBars ||
                this.world.currentUpdate < this.boardPiece.showStatBarsTillFrame ||
                this.boardPiece.GetType() == typeof(Player))
            { this.boardPiece.DrawStatBar(); }
        }

        private void DrawRoutine(bool calculateSubmerge)
        {
            if (this.rotation == 0)
            {
                int submergeCorrection = !this.floatsOnWater && this.IsInWater && calculateSubmerge ?
                    (Terrain.waterLevelMax - this.GetFieldValue(TerrainName.Height)) / 2 : 0;

                this.frame.Draw(destRect: this.gfxRect, color: this.color, submergeCorrection: submergeCorrection, opacity: this.opacity);
            }
            else
            {
                this.frame.DrawWithRotation(position: new Vector2(this.gfxRect.Center.X, this.gfxRect.Center.Y), color: this.color, rotation: this.rotation, opacity: this.opacity);
            }

            if (this.boardPiece.pieceStorage != null && this.boardPiece.GetType() == typeof(Plant)) this.DrawFruits();

        }

        private void DrawFruits()
        {
            Plant plant = (Plant)this.boardPiece;
            if (Preferences.debugShowFruitRects) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, plant.fruitEngine.FruitAreaRect, Color.Cyan * 0.4f);

            if (plant.pieceStorage.NotEmptySlotsCount == 0) return;
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
                Convert.ToInt32(this.position.X - (textSize.X / 2)),
                Convert.ToInt32(this.position.Y - (textSize.Y / 2)));

            SonOfRobinGame.spriteBatch.DrawString(stateFont, stateTxt, txtPos + new Vector2(1, 1), Color.Black);
            SonOfRobinGame.spriteBatch.DrawString(stateFont, stateTxt, txtPos, Color.White);
        }

    }
}
