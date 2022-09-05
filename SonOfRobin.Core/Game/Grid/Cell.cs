using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Cell
    {
        public bool creationInProgress;
        private int creationStage;
        public readonly Grid grid;
        private readonly World world;
        private readonly Color color;
        public readonly int cellNoX;
        public readonly int cellNoY;
        public readonly int xMin;
        public readonly int xCenter;
        public readonly int xMax;
        public readonly int yMin;
        public readonly int yCenter;
        public readonly int yMax;
        public readonly Rectangle rect;
        public readonly Vector2 center;
        public readonly int width;
        public readonly int height;
        public readonly int dividedWidth;
        public readonly int dividedHeight;

        public List<Cell> surroundingCells;
        public bool VisitedByPlayer { get; private set; }

        public readonly Dictionary<Group, Dictionary<string, Sprite>> spriteGroups;
        public readonly Dictionary<TerrainName, Terrain> terrainByName;
        public BoardGraphics boardGraphics;

        public static readonly Group[] allGroups = (Group[])Enum.GetValues(typeof(Group));
        public static readonly TerrainName[] allTerrains = (TerrainName[])Enum.GetValues(typeof(TerrainName));

        public enum Group
        {
            All,
            ColMovement,
            ColPlantGrowth,
            Visible,
            LightSource,
            StateMachinesNonPlants,
            StateMachinesPlants
        }

        public Cell(Grid grid, World world, int cellNoX, int cellNoY, int xMin, int xMax, int yMin, int yMax, Random random)
        {
            this.creationInProgress = true;
            this.creationStage = 0;

            this.grid = grid;
            this.world = world;
            this.cellNoX = cellNoX;
            this.cellNoY = cellNoY;
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.width = xMax - xMin + 1; // virtual value, simulated for the outside world
            this.height = yMax - yMin + 1; // virtual value, simulated for the outside world
            // Ceiling() to round up in case of odd numbers
            this.dividedWidth = (int)Math.Ceiling((float)width / (float)this.grid.resDivider);  // real storing data capacity
            this.dividedHeight = (int)Math.Ceiling((float)height / (float)this.grid.resDivider); // real storing data capacity
            this.rect = new Rectangle(this.xMin, this.yMin, this.width, this.height);
            this.xCenter = this.xMin + (this.width / 2);
            this.yCenter = this.yMin + (this.height / 2);
            this.center = new Vector2(this.xCenter, this.yCenter);
            this.color = new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
            this.surroundingCells = new List<Cell>();
            this.VisitedByPlayer = false;

            this.spriteGroups = new Dictionary<Group, Dictionary<string, Sprite>> { };
            foreach (Group groupName in allGroups)
            {
                this.spriteGroups[groupName] = new Dictionary<string, Sprite> { };
            }

            this.terrainByName = new Dictionary<TerrainName, Terrain>();

            this.creationStage++;
        }

        public void SetAsVisited()
        {
            this.VisitedByPlayer = true;
        }

        public Object Serialize()
        {
            var cellData = new Dictionary<string, object>
            {
                { "VisitedByPlayer", this.VisitedByPlayer},
            };

            return cellData;
        }

        public void Deserialize(Object cellData)
        {
            var cellDict = (Dictionary<string, object>)cellData;

            this.VisitedByPlayer = (bool)cellDict["VisitedByPlayer"];
        }

        public void RunNextCreationStage()

        {
            switch (this.creationStage)
            {
                case 1:
                    // different noise settings should not be processed together in parallel
                    this.terrainByName[TerrainName.Height] = new Terrain(
                    world: this.world, cell: this, name: TerrainName.Height, frequency: 8f, octaves: 9, persistence: 0.5f, lacunarity: 1.9f, gain: 0.55f, addBorder: true);
                    this.creationStage++;
                    return;

                case 2:
                    // different noise settings should not be processed together in parallel
                    this.terrainByName[TerrainName.Humidity] = new Terrain(
                    world: this.world, cell: this, name: TerrainName.Humidity, frequency: 4.3f, octaves: 9, persistence: 0.6f, lacunarity: 1.7f, gain: 0.6f);

                    this.creationStage++;
                    return;

                case 3:
                    // different noise settings should not be processed together in parallel
                    this.terrainByName[TerrainName.Danger] = new Terrain(
                    world: this.world, cell: this, name: TerrainName.Danger, frequency: 2.9f, octaves: 3, persistence: 0.7f, lacunarity: 1.4f, gain: 0.3f, addBorder: true);

                    this.UpdateBoardGraphics();
                    this.creationStage++;
                    return;

                case 4:
                    // cannot be run in parallel
                    if (Preferences.loadWholeMap) this.boardGraphics.LoadTexture();
                    this.creationInProgress = false;
                    return;

                default:
                    throw new DivideByZeroException($"Unsupported creationStage - {creationStage}.");
            }
        }

        public void CopyFromTemplate(Cell templateCell)
        {
            foreach (TerrainName terainName in allTerrains)
            {
                this.terrainByName[terainName] = templateCell.terrainByName[terainName];
            }

            this.boardGraphics = new BoardGraphics(grid: this.grid, cell: this);
            this.boardGraphics.texture = templateCell.boardGraphics.texture;
        }

        public void UpdateBoardGraphics()
        {
            this.boardGraphics = new BoardGraphics(grid: this.grid, cell: this);
            this.boardGraphics.CreateAndSavePngTemplate();
        }

        public void RemoveSprite(Sprite sprite)
        {
            foreach (Group currentGroupName in allGroups)
            { this.spriteGroups[currentGroupName].Remove(sprite.id); }

            sprite.currentCell = null;
        }

        public void MoveSpriteToOtherCell(Sprite sprite, Cell newCell)
        {
            foreach (Group currentGroupName in allGroups)
            {
                if (this.spriteGroups[currentGroupName].ContainsKey(sprite.id))
                {
                    this.spriteGroups[currentGroupName].Remove(sprite.id);
                    newCell.spriteGroups[currentGroupName][sprite.id] = sprite;
                }
            }
        }

        public void UpdateGroups(Sprite sprite, List<Group> groupNames)
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - updating group names.");

            foreach (Group currentGroupName in allGroups)
            {
                if (groupNames.Contains(currentGroupName))
                {
                    this.spriteGroups[currentGroupName][sprite.id] = sprite;
                }
                else
                {
                    this.spriteGroups[currentGroupName].Remove(sprite.id);
                }
            }
        }

        public void AddToGroup(Sprite sprite, Group groupName)
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - adding to group {groupName}.");

            this.spriteGroups[groupName][sprite.id] = sprite;
        }

        public void RemoveFromGroup(Sprite sprite, Group groupName)
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - removing from group {groupName}.");

            this.spriteGroups[groupName].Remove(sprite.id);
        }

        public List<Sprite> GetSpritesFromSurroundingCells(Group groupName)
        {
            List<Sprite> surroundingSprites = new List<Sprite>();

            foreach (Cell cell in this.surroundingCells)
            {
                surroundingSprites.AddRange(cell.spriteGroups[groupName].Values.ToList());
            }

            return surroundingSprites;
        }

        public float GetDistance(Vector2 target)
        { return Vector2.Distance(target, this.center); }

        public float GetDistance(Cell cell)
        { return Vector2.Distance(cell.center, this.center); }

        public void DrawDebugData(Group groupName)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                this.xMin,
                this.yMin,
                this.width,
                this.height),
                this.color * 0.3f);

            var posFont = SonOfRobinGame.fontPressStart2P5;

            Vector2 txtPos = new Vector2(this.xMin, this.yMin);
            string txtString = $"{this.cellNoX},{this.cellNoY}\n{this.xMin},{this.yMin}";

            SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos + new Vector2(1, 1), Color.Black);
            SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos, Color.White);

            foreach (Sprite sprite in this.spriteGroups[groupName].Values)
            {
                txtPos = sprite.position;
                txtString = $"{sprite.id}\n{sprite.animPackage}\n{this.cellNoX},{this.cellNoY}\n{sprite.position.X},{sprite.position.Y}";

                SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos + new Vector2(1, 1), Color.Black);
                SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos, Color.White);
            }
        }

        public void DrawBackground(float opacity = 1f)
        {
            if (this.boardGraphics.texture == null) return;

            Rectangle destinationRectangle = new Rectangle(this.xMin, this.yMin, this.rect.Width, this.rect.Height);

            SonOfRobinGame.spriteBatch.Draw(this.boardGraphics.texture, destinationRectangle, this.boardGraphics.texture.Bounds, Color.White * opacity);
        }

    }
}
