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
        public static int maxDistanceOverride = -1; // -1 will not affect sprite creation; higher values will override one sprite creation

        public readonly string id;
        public readonly BoardPiece boardPiece;
        private readonly World world;
        public Vector2 position;
        public bool placedCorrectly;
        public Orientation orientation;
        public AnimFrame frame;
        public Color color;
        public AnimPkg animPackage;
        public byte animSize;
        public string animName;
        byte currentFrameIndex;
        ushort currentFrameTimeLeft; // measured in game frames
        public Rectangle gfxRect;
        public Rectangle colRect;
        public readonly bool blocksMovement;
        public readonly bool checksFullCollisions;
        public readonly bool ignoresCollisions;
        public AllowedFields allowedFields;
        private readonly bool floatsOnWater;
        public Cell currentCell; // coordinates of current cell, that is containing the sprite 
        public List<Cell.Group> gridGroups;

        public string CompleteAnimID
        { get { return GetCompleteAnimId(animPackage: this.animPackage, animSize: this.animSize, animName: this.animName); } }

        private bool visible;

        public bool IsInWater
        {
            get
            { return this.GetFieldValue(TerrainName.Height) < Terrain.waterLevelMax; }
        }
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                this.visible = value;
                if (value)
                { this.world.grid.AddToGroup(sprite: this, groupName: Cell.Group.Visible); }
                else
                { this.world.grid.RemoveFromGroup(sprite: this, groupName: Cell.Group.Visible); }
            }
        }

        public Sprite(World world, string id, BoardPiece boardPiece, Vector2 position, AnimPkg animPackage, byte animSize, string animName, bool ignoresCollisions, AllowedFields allowedFields, bool blocksMovement = true, bool visible = true, bool checksFullCollisions = false, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false)
        {
            this.id = id; // duplicate from BoardPiece class
            this.boardPiece = boardPiece;
            this.world = world;
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
            this.visible = visible; // initially it is assigned normally
            this.currentCell = null;

            AssignFrame(); // frame needs to be set before checking for free spot

            this.gridGroups = this.GetGridGroups();

            this.placedCorrectly = this.FindFreeSpot(position, minDistance: minDistance, maxDistance: maxDistance); // this.position is set here
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
            pieceData["sprite_orientation"] = this.orientation;
            pieceData["sprite_gridGroups"] = this.gridGroups;
        }

        public void Deserialize(Dictionary<string, Object> spriteData)
        {
            this.position = new Vector2((float)spriteData["sprite_positionX"], (float)spriteData["sprite_positionY"]);
            this.orientation = (Orientation)spriteData["sprite_orientation"];
            this.animPackage = (AnimPkg)spriteData["sprite_animPackage"];
            this.animSize = (byte)spriteData["sprite_animSize"];
            this.animName = (string)spriteData["sprite_animName"];
            this.currentFrameIndex = (byte)spriteData["sprite_currentFrameIndex"];
            this.currentFrameTimeLeft = (ushort)spriteData["sprite_currentFrameTimeLeft"];
            this.AssignFrameById((string)spriteData["sprite_frame_id"]);
            this.gridGroups = (List<Cell.Group>)spriteData["sprite_gridGroups"];
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

        public bool MoveToClosestFreeSpot(Vector2 startPosition)
        {
            if (!this.placedCorrectly) throw new DivideByZeroException($"Trying to move '{this.boardPiece.name}' that was not placed correctly.");

            ushort maxDistance = 0;

            for (int i = 0; i < 300; i++)
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

        private bool FindFreeSpot(Vector2 startPosition, ushort minDistance, ushort maxDistance)
        {
            if (this.world.piecesLoadingMode)
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
                    if (hasBeenMoved) { return true; }
                }
            }

            return false;
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

            return groupNames;
        }

        public void RemoveFromGrid()
        { this.world.grid.RemoveSprite(this); }

        public void UpdateGridLocation()
        { this.world.grid.UpdateLocation(this); }


        public bool Move(Vector2 movement)
        {
            var hasBeenMoved = false;

            // normal and minimal movement (in case of collision)
            Vector2[] movesToTest = { movement, new Vector2(Math.Max(Math.Min(movement.X, 1), -1), Math.Max(Math.Min(movement.Y, 1), -1)) };

            foreach (Vector2 testMove in movesToTest)
            {
                hasBeenMoved = this.SetNewPosition(this.position + testMove);
                if (hasBeenMoved) break;
            }

            return hasBeenMoved;
        }

        public bool SetNewPosition(Vector2 newPos, bool ignoreCollisions = false, bool updateGridLocation = true)
        {
            var originalPosition = new Vector2(this.position.X, this.position.Y);

            this.position = new Vector2((int)newPos.X, (int)newPos.Y); // to ensure integer values
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

            return !collisionDetected; // negated "collisionDetected" == "hasBeenMoved"
        }

        public bool CheckForCollision()
        {
            if (this.world.piecesLoadingMode) return false;

            // checking world boundaries
            if (this.gfxRect.Left <= 0 || this.gfxRect.Right >= this.world.width || this.gfxRect.Top <= 0 || this.gfxRect.Bottom >= this.world.height)
            { return true; }

            if (this.ignoresCollisions) return false;

            if (!this.allowedFields.CanStandHere(position: this.position)) return true;

            var gridTypeToCheck = (this.checksFullCollisions) ? Cell.Group.ColAll : Cell.Group.ColBlocking;

            foreach (Sprite sprite in this.world.grid.GetSpritesFromSurroundingCells(sprite: this, groupName: gridTypeToCheck))
            {
                if (this.colRect.Intersects(sprite.colRect) && sprite.id != this.id)
                    return true;
            }
            return false;
        }

        public bool CheckIfOtherSpriteIsWithinRange(Sprite target, int range = 4)
        {
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
                    this.animPackage = animPackage;
                    AssignFrame(forceRewind: true);
                }
            }
        }
        public void AssignNewSize(byte animSize, bool setEvenIfMissing = true)
        {
            if (this.animSize != animSize)
            {
                if (CheckIFAnimSizeExists(animSize) || setEvenIfMissing)
                {
                    this.animSize = animSize;
                    AssignFrame(forceRewind: true);
                }
            }
        }
        public void AssignNewName(string animName, bool setEvenIfMissing = true)
        {
            if (this.animName != animName)
            {
                if (CheckIFAnimNameExists(animName) || setEvenIfMissing)
                {
                    this.animName = animName;
                    AssignFrame(forceRewind: true);
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

        public void AssignFrame(bool forceRewind = false)
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

            if (!blocksMovement) return;

            // in case of collision - reverting to a previous, non-colliding frame
            if (this.CheckForCollision() && frameCopy != null)
            {
                this.frame = frameCopy;
                this.currentFrameTimeLeft = this.frame.duration;
                this.UpdateRects();
            }
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
            if (Preferences.debugShowRects)
            { SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32(this.gfxRect.X), Convert.ToInt32(this.gfxRect.Y), this.gfxRect.Width, this.gfxRect.Height), this.gfxRect, Color.White * 0.5f); }

            Rectangle correctedGfxRect;

            if (!this.floatsOnWater && this.IsInWater && calculateSubmerge)
            {
                correctedGfxRect = this.gfxRect;
                int submergeCorrection = Convert.ToInt32((Terrain.waterLevelMax - this.GetFieldValue(TerrainName.Height)) / 2);
                correctedGfxRect.Height = Math.Max(correctedGfxRect.Height / 2, correctedGfxRect.Height - submergeCorrection);
            }
            else
            { correctedGfxRect = this.gfxRect; }

            this.frame.Draw(gfxRect: correctedGfxRect, color: this.color);

            if (this.boardPiece.pieceStorage != null && this.boardPiece.GetType() == typeof(Plant)) this.DrawFruits();

            if (Preferences.debugShowRects)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32(this.colRect.X), Convert.ToInt32(this.colRect.Y), this.colRect.Width, this.colRect.Height), this.colRect, Color.Red * 0.7f);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(Convert.ToInt32((this.position.X) - 1), Convert.ToInt32((this.position.Y) - 1), 2, 2), Color.White);
            }

            if (Preferences.debugShowStates && this.boardPiece.GetType() == typeof(Animal)) this.DrawState();
            if (Preferences.debugShowStatBars || this.world.currentUpdate < this.boardPiece.showHitPointsTillFrame) this.boardPiece.DrawStatBar();
        }

        private void DrawFruits()
        {
            Plant plant = (Plant)this.boardPiece;

            if (plant.pieceStorage.NotEmptySlotsCount == 0) return;

            var fruitList = plant.pieceStorage.GetAllPieces();
            foreach (BoardPiece fruit in fruitList)
            { fruit.sprite.Draw(calculateSubmerge: false); }
        }

        public void Draw(Rectangle destRect, float opacity)
        { this.frame.DrawAndKeepInRectBounds(destBoundsRect: destRect, color: this.color * opacity); }

        private void DrawState()
        {
            string stateTxt = $"{this.boardPiece.activeState}".Replace("Player", "").Replace("Animal", "");
            var stateFont = SonOfRobinGame.fontSmall;

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
