using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Cell
    {
        public readonly Grid grid;

        public readonly int cellIndex;
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

        private Cell[] surroundingCells;
        public bool visitedByPlayer;
        public bool temporaryDecorationsCreated;

        public readonly Dictionary<Group, HashSet<Sprite>> spriteGroups;
        public bool HasWater { get; private set; }
        public bool IsAllWater { get; private set; }
        public bool HasLava { get; private set; }

        public readonly List<PieceTemplate.Name> allowedNames; // for initial placing only - because possible piece placement can be changed during its lifecycle

        public static readonly Group[] allGroups = (Group[])Enum.GetValues(typeof(Group));

        public enum Group : byte
        {
            All,
            ColMovement,
            ColPlantGrowth,
            Visible,
            LightSource,
            StateMachinesNonPlants,
            StateMachinesPlants,
        }

        public Cell(Grid grid, int cellIndex, int cellNoX, int cellNoY, int cellWidth, int cellHeight)
        {
            this.grid = grid;

            this.cellIndex = cellIndex;
            this.cellNoX = cellNoX;
            this.cellNoY = cellNoY;

            this.xMin = cellNoX * cellWidth;
            this.yMin = cellNoY * cellHeight;

            this.width = Math.Min(this.xMin + cellWidth, this.grid.width - 1) - this.xMin; // to ensure that it won't go over world bounds
            this.height = Math.Min(this.yMin + cellHeight, this.grid.height - 1) - this.yMin; // to ensure that it won't go over world bounds

            this.xMax = this.xMin + this.width - 1;
            this.yMax = this.yMin + this.height - 1;

            this.rect = new Rectangle(this.xMin, this.yMin, this.width, this.height);

            this.xCenter = this.xMin + (this.width / 2);
            this.yCenter = this.yMin + (this.height / 2);
            this.center = new Vector2(this.xCenter, this.yCenter);

            this.surroundingCells = null; // updated later, after creating whole grid
            this.visitedByPlayer = false;
            this.temporaryDecorationsCreated = false;

            this.spriteGroups = new Dictionary<Group, HashSet<Sprite>> { };
            foreach (Group groupName in allGroups)
            {
                this.spriteGroups[groupName] = new HashSet<Sprite>();
            }

            this.allowedNames = new List<PieceTemplate.Name>();
        }

        public void FillMiscProperties()
        {
            var minValForTerrain = new Dictionary<Terrain.Name, byte>();
            var maxValForTerrain = new Dictionary<Terrain.Name, byte>();

            foreach (Terrain.Name terrainName in Terrain.allTerrains)
            {
                minValForTerrain[terrainName] = this.grid.terrainByName[terrainName].GetMinValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
                maxValForTerrain[terrainName] = this.grid.terrainByName[terrainName].GetMaxValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
            }

            this.HasWater = minValForTerrain[Terrain.Name.Height] <= Terrain.waterLevelMax;
            this.IsAllWater = maxValForTerrain[Terrain.Name.Height] <= Terrain.waterLevelMax;
            this.HasLava = maxValForTerrain[Terrain.Name.Height] >= Terrain.lavaMin;
        }

        public void FillAllowedNames()
        {
            foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
            {
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);
                AllowedTerrain allowedTerrain = pieceInfo.allowedTerrain;
                bool cellCanContainThisPiece = true;

                foreach (Terrain.Name terrainName in Terrain.allTerrains)
                {
                    byte allowedMinVal = allowedTerrain.GetMinValForTerrainName(terrainName);
                    byte allowedMaxVal = allowedTerrain.GetMaxValForTerrainName(terrainName);

                    if (allowedMinVal > 0 || allowedMaxVal < 255)
                    {
                        byte minVal = this.grid.terrainByName[terrainName].GetMinValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
                        byte maxVal = this.grid.terrainByName[terrainName].GetMaxValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);

                        if ((allowedMinVal < minVal && allowedMaxVal < minVal) ||
                            (allowedMinVal > maxVal && allowedMaxVal > maxVal))
                        {
                            cellCanContainThisPiece = false;
                            break;
                        }
                    }
                }

                if (cellCanContainThisPiece)
                {
                    foreach (var kvp in allowedTerrain.GetExtPropertiesDict())
                    {
                        ExtBoardProps.Name name = kvp.Key;
                        bool value = kvp.Value;

                        if (!this.grid.ExtBoardProps.CheckIfContainsPropertyForCell(name: name, value: value, cellNoX: this.cellNoX, cellNoY: this.cellNoY))
                        {
                            cellCanContainThisPiece = false;
                            break;
                        }
                    }
                }

                if (cellCanContainThisPiece) this.allowedNames.Add(pieceName);
            }
        }

        public void SetAsVisited()
        {
            this.visitedByPlayer = true;
        }

        public void CopyFromTemplate(Cell templateCell)
        {
            this.allowedNames.AddRange(templateCell.allowedNames);

            this.HasWater = templateCell.HasWater;
            this.IsAllWater = templateCell.IsAllWater;
            this.HasLava = templateCell.HasLava;
        }

        public void RemoveSprite(Sprite sprite)
        {
            foreach (Group currentGroupName in allGroups)
            { this.spriteGroups[currentGroupName].Remove(sprite); }

            sprite.currentCell = null;
        }

        public void MoveSpriteToOtherCell(Sprite sprite, Cell newCell)
        {
            foreach (Group currentGroupName in allGroups)
            {
                if (this.spriteGroups[currentGroupName].Contains(sprite))
                {
                    this.spriteGroups[currentGroupName].Remove(sprite);
                    newCell.spriteGroups[currentGroupName].Add(sprite);
                }
            }
        }

        public void UpdateGroups(Sprite sprite, List<Group> groupNames)
        {
            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - updating group names.");

            foreach (Group currentGroupName in allGroups)
            {
                if (groupNames.Contains(currentGroupName)) this.spriteGroups[currentGroupName].Add(sprite);
                else this.spriteGroups[currentGroupName].Remove(sprite);
            }
        }

        public void AddToGroup(Sprite sprite, Group groupName)
        {
            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - adding to group {groupName}.");

            this.spriteGroups[groupName].Add(sprite);
        }

        public void RemoveFromGroup(Sprite sprite, Group groupName)
        {
            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - removing from group {groupName}.");

            this.spriteGroups[groupName].Remove(sprite);
        }

        public void UpdateSurroundingCells()
        {
            // should be invoked once, after creating all cells

            int xMinCellNo = Math.Max(this.cellNoX - 1, 0);
            int xMaxCellNo = Math.Min(this.cellNoX + 1, this.grid.noOfCellsX - 1);
            int yMinCellNo = Math.Max(this.cellNoY - 1, 0);
            int yMaxCellNo = Math.Min(this.cellNoY + 1, this.grid.noOfCellsY - 1);

            this.surroundingCells = new Cell[((xMaxCellNo - xMinCellNo + 1) * (yMaxCellNo - yMinCellNo + 1)) - 1];

            int index = 0;
            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                {
                    if (!(x == this.cellNoX && y == this.cellNoY)) this.surroundingCells[index++] = this.grid.cellGrid[x, y];
                }
            }
        }

        public List<Sprite> GetSpritesFromSurroundingCells(Group groupName)
        {
            List<Sprite> surroundingSprites = new();

            foreach (Cell cell in this.surroundingCells)
            {
                surroundingSprites.AddRange(cell.spriteGroups[groupName]);
            }

            return surroundingSprites;
        }

        public float GetDistance(Vector2 target)
        {
            return Vector2.Distance(target, this.center);
        }

        public float GetDistance(Cell cell)
        {
            return Vector2.Distance(cell.center, this.center);
        }

        public void DrawDebugData(Group groupName, bool drawCellData, bool drawPieceData)
        {
            SpriteFontBase font = SonOfRobinGame.FontPressStart2P.GetFont(8);

            // drawing cell ext colors

            if (drawCellData)
            {
                SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.rect, color: Color.White * 0.3f, thickness: 1f);

                var colorForExtPropNames = new Dictionary<ExtBoardProps.Name, Color> {
                { ExtBoardProps.Name.Sea, Color.Cyan },
                { ExtBoardProps.Name.OuterBeach, Color.Red },
                { ExtBoardProps.Name.BiomeSwamp, Color.Green },
                // every name should have its color defined here
            };

                foreach (var kvp in colorForExtPropNames)
                {
                    ExtBoardProps.Name name = kvp.Key;
                    Color color = kvp.Value;

                    if (this.grid.ExtBoardProps.CheckIfContainsPropertyForCell(name: name, value: true, cellNoX: this.cellNoX, cellNoY: this.cellNoY)) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.rect, SonOfRobinGame.WhiteRectangle.Bounds, color * 0.4f);
                }
            }

            // drawing piece data

            if (drawPieceData)
            {
                foreach (Sprite sprite in this.spriteGroups[groupName])
                {
                    string spriteText = $"{sprite.AnimPkg.name}\n{this.cellNoX},{this.cellNoY}\n{sprite.position.X},{sprite.position.Y}";

                    font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: spriteText, position: new Vector2(sprite.ColRect.Left, sprite.ColRect.Bottom), color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: 1);
                }
            }

            // drawing cell info

            if (drawCellData)
            {
                string cellText = $"{this.cellNoX},{this.cellNoY}\n{this.xMin},{this.yMin}";

                font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: cellText, position: new Vector2(this.xMin, this.yMin) + new Vector2(5, 5), color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: 1);
            }
        }
    }
}