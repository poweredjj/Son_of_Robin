using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

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
        public readonly Vector2 center;
        public readonly int width;
        public readonly int height;
        public List<Cell> surroundingCells;

        public readonly Dictionary<Group, Dictionary<string, Sprite>> spriteGroups;
        public readonly Dictionary<TerrainName, Terrain> terrainByName;
        public BoardGraphics boardGraphics;

        public enum Group
        {
            All,
            ColBlocking,
            ColAll,
            Visible,
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
            this.width = xMax - xMin + 1;
            this.height = yMax - yMin + 1;
            this.xCenter = this.xMin + (this.width / 2);
            this.yCenter = this.yMin + (this.height / 2);
            this.center = new Vector2(this.xCenter, this.yCenter);
            this.color = new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
            this.surroundingCells = new List<Cell>();

            this.spriteGroups = new Dictionary<Group, Dictionary<string, Sprite>> { };
            foreach (Group groupName in (Group[])Enum.GetValues(typeof(Group)))
            { this.spriteGroups[groupName] = new Dictionary<string, Sprite> { }; }

            this.terrainByName = new Dictionary<TerrainName, Terrain>();

            this.creationStage++;
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

                    this.UpdateBoardGraphics();
                    this.creationStage++;
                    return;

                case 3:
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
            foreach (TerrainName terainName in (Group[])Enum.GetValues(typeof(TerrainName)))
            { this.terrainByName[terainName] = templateCell.terrainByName[terainName]; }

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
            foreach (Group currentGroupName in (Group[])Enum.GetValues(typeof(Group)))
            { this.spriteGroups[currentGroupName].Remove(sprite.id); }

            sprite.currentCell = null;
        }

        public void MoveSpriteToOtherCell(Sprite sprite, Cell newCell)
        {
            foreach (Group currentGroupName in (Group[])Enum.GetValues(typeof(Group)))
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
            foreach (Group currentGroupName in (Group[])Enum.GetValues(typeof(Group)))
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
        { this.spriteGroups[groupName][sprite.id] = sprite; }

        public void RemoveFromGroup(Sprite sprite, Group groupName)
        { this.spriteGroups[groupName].Remove(sprite.id); }

        public List<Sprite> GetSpritesFromSurroundingCells(Group groupName)
        {
            List<Sprite> surroundingSprites = new List<Sprite>();

            foreach (Cell cell in this.surroundingCells)
            { surroundingSprites.AddRange(cell.spriteGroups[groupName].Values.ToList()); }

            return surroundingSprites;
        }

        public float GetDistance(Vector2 target)
        { return Vector2.Distance(target, this.center); }

        public float GetDistance(Cell cell)
        { return Vector2.Distance(cell.center, this.center); }

        public void DrawDebugData(Group groupName)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                Convert.ToInt32(this.xMin),
                Convert.ToInt32(this.yMin),
                this.width,
                this.height),
                this.color * 0.3f);

            var posFont = SonOfRobinGame.fontSmall;

            Vector2 txtPos = new Vector2(this.xMin, this.yMin);
            string txtString = $"{this.cellNoX},{this.cellNoY}\n{this.xMin},{this.yMin}";

            SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos + new Vector2(1, 1), Color.Black);
            SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos, Color.White);

            foreach (Sprite sprite in this.spriteGroups[groupName].Values)
            {
                txtPos = sprite.position;
                txtString = $"{sprite.id}\n{this.cellNoX},{this.cellNoY}\n{sprite.position.X},{sprite.position.Y}";

                SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos + new Vector2(1, 1), Color.Black);
                SonOfRobinGame.spriteBatch.DrawString(posFont, txtString, txtPos, Color.White);
            }
        }

        public void DrawBackground()
        {
            try
            {
                Rectangle sourceRectangle = new Rectangle(0, 0, this.boardGraphics.texture.Width, this.boardGraphics.texture.Height);
                Rectangle destinationRectangle = new Rectangle(this.xMin, this.yMin, this.boardGraphics.texture.Width, this.boardGraphics.texture.Height);
                SonOfRobinGame.spriteBatch.Draw(this.boardGraphics.texture, destinationRectangle, sourceRectangle, Color.White);
            }
            catch (NullReferenceException)
            { }
        }

    }
}
