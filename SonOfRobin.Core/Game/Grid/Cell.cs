﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Cell
    {
        public readonly Grid grid;
        public readonly World world;
        public readonly Color color;

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
        public BoardGraphics boardGraphics;

        public readonly List<PieceTemplate.Name> allowedNames; // for initialRangesByTerrainName only - because currentRangesByTerrainName can be changed anytime

        public static readonly Group[] allGroups = (Group[])Enum.GetValues(typeof(Group));

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

        public Cell(Grid grid, World world, int cellNoX, int cellNoY, int cellWidth, int cellHeight, Random random)
        {
            this.grid = grid;
            this.world = world;

            this.cellNoX = cellNoX;
            this.cellNoY = cellNoY;

            this.xMin = cellNoX * cellWidth;
            this.xMax = this.xMin + cellWidth - 1;
            this.xMax = Math.Min(this.xMax, this.world.width - 1);

            this.yMin = cellNoY * cellHeight;
            this.yMax = this.yMin + cellHeight - 1;
            this.yMax = Math.Min(this.yMax, this.world.height - 1);

            this.width = this.xMax - this.xMin; // virtual value, simulated for the outside world
            this.height = this.yMax - this.yMin; // virtual value, simulated for the outside world

            this.dividedWidth = this.width / this.grid.resDivider; // real storing data capacity
            this.dividedHeight = this.height / this.grid.resDivider; // real storing data capacity

            this.rect = new Rectangle(this.xMin, this.yMin, this.width + 1, this.height + 1);
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

            this.allowedNames = new List<PieceTemplate.Name>();
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
                    AllowedRange allowedRange = allowedTerrain.GetInitialRangeForTerrainName(terrainName);
                    if (allowedRange != null)
                    {
                        byte minVal = this.grid.GetMinValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY);
                        byte maxVal = this.grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY);

                        if ((allowedRange.Min < minVal && allowedRange.Max < minVal) ||
                            (allowedRange.Min > maxVal && allowedRange.Max > maxVal))
                        {
                            cellCanContainThisPiece = false;
                            break;
                        }
                    }
                }

                if (cellCanContainThisPiece)
                {
                    foreach (var kvp in allowedTerrain.GetInitialExtPropertiesDict())
                    {
                        ExtBoardProps.ExtPropName name = kvp.Key;
                        bool value = kvp.Value;

                        if (!this.grid.ExtBoardProps.CheckIfContainsProperty(name: name, value: value, cellNoX: this.cellNoX, cellNoY: this.cellNoY))
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
            this.VisitedByPlayer = true;
        }

        public Object Serialize()
        {
            var cellData = new Dictionary<string, object>
            {
                { "VisitedByPlayer", this.VisitedByPlayer },
            };

            return cellData;
        }

        public void Deserialize(Object cellData)
        {
            var cellDict = (Dictionary<string, object>)cellData;

            this.VisitedByPlayer = (bool)cellDict["VisitedByPlayer"];
        }

        public void CopyFromTemplate(Cell templateCell)
        {
            this.boardGraphics = new BoardGraphics(grid: this.grid, cell: this);
            this.boardGraphics.texture = templateCell.boardGraphics.texture;
            this.allowedNames.AddRange(templateCell.allowedNames);
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
                if (groupNames.Contains(currentGroupName)) this.spriteGroups[currentGroupName][sprite.id] = sprite;
                else this.spriteGroups[currentGroupName].Remove(sprite.id);
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
        {
            return Vector2.Distance(target, this.center);
        }

        public float GetDistance(Cell cell)
        {
            return Vector2.Distance(cell.center, this.center);
        }

        public void DrawDebugData(Group groupName)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.rect, this.color * 0.3f);

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

            SonOfRobinGame.spriteBatch.Draw(this.boardGraphics.texture, this.rect, this.boardGraphics.texture.Bounds, Color.White * opacity);
        }
    }
}