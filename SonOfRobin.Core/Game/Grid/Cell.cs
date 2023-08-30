using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Cell
    {
        public readonly Grid grid;

        public readonly int cellNoX;
        public readonly int cellNoY;

        public readonly int xMin;
        public readonly int xCenter;
        public readonly int xMax;
        public readonly int yMin;
        public readonly int yCenter;
        public readonly int yMax;

        public readonly Rectangle rect;
        private readonly Rectangle previewRect; // must match grid.WholeIslandPreviewScale
        public readonly Vector2 center;

        public readonly int width;
        public readonly int height;

        public List<Cell> surroundingCells;
        public bool VisitedByPlayer { get; private set; }
        public bool temporaryDecorationsCreated;

        public readonly Dictionary<Group, HashSet<Sprite>> spriteGroups;
        public BoardGraphics boardGraphics;
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

        public Cell(Grid grid, int cellNoX, int cellNoY, int cellWidth, int cellHeight)
        {
            this.grid = grid;

            this.cellNoX = cellNoX;
            this.cellNoY = cellNoY;

            this.xMin = cellNoX * cellWidth;
            this.yMin = cellNoY * cellHeight;

            this.width = Math.Min(this.xMin + cellWidth, this.grid.width - 1) - this.xMin; // to ensure that it won't go over world bounds
            this.height = Math.Min(this.yMin + cellHeight, this.grid.height - 1) - this.yMin; // to ensure that it won't go over world bounds

            this.xMax = this.xMin + this.width - 1;
            this.yMax = this.yMin + this.height - 1;

            this.rect = new Rectangle(this.xMin, this.yMin, this.width, this.height);
            float scale = this.grid.wholeIslandPreviewScale;
            this.previewRect = new Rectangle((int)(this.rect.X * scale), (int)(this.rect.Y * scale), width: (int)(this.width * scale), height: (int)(this.height * scale));
            this.boardGraphics = new BoardGraphics(grid: this.grid, cell: this);

            this.xCenter = this.xMin + (this.width / 2);
            this.yCenter = this.yMin + (this.height / 2);
            this.center = new Vector2(this.xCenter, this.yCenter);

            this.surroundingCells = new List<Cell>();
            this.VisitedByPlayer = false;
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
            this.boardGraphics.ReplaceTexture(texture: templateCell.boardGraphics.Texture);
            templateCell.boardGraphics.hasBeenCopiedElsewhere = true;
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
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - updating group names.");

            foreach (Group currentGroupName in allGroups)
            {
                if (groupNames.Contains(currentGroupName)) this.spriteGroups[currentGroupName].Add(sprite);
                else this.spriteGroups[currentGroupName].Remove(sprite);
            }
        }

        public void AddToGroup(Sprite sprite, Group groupName)
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - adding to group {groupName}.");

            this.spriteGroups[groupName].Add(sprite);
        }

        public void RemoveFromGroup(Sprite sprite, Group groupName)
        {
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{this.world.currentUpdate} '{sprite.boardPiece.readableName}' - removing from group {groupName}.");

            this.spriteGroups[groupName].Remove(sprite);
        }

        public IEnumerable<Sprite> GetSpritesFromSurroundingCells(Group groupName)
        {
            List<Sprite> surroundingSprites = new List<Sprite>();

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
            SpriteFont font = SonOfRobinGame.FontPressStart2P5;

            // drawing cell ext colors

            if (drawCellData)
            {
                Helpers.DrawRectangleOutline(rect: this.rect, color: Color.White * 0.3f, borderWidth: 1);

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
                    string spriteText = $"{sprite.AnimPackage}\n{this.cellNoX},{this.cellNoY}\n{sprite.position.X},{sprite.position.Y}";
                    Helpers.DrawTextWithOutline(font: font, text: spriteText, pos: new Vector2(sprite.ColRect.Left, sprite.ColRect.Bottom), color: Color.White, outlineColor: Color.Black, outlineSize: 1);
                }
            }

            // drawing cell info

            if (drawCellData)
            {
                string cellText = $"{this.cellNoX},{this.cellNoY}\n{this.xMin},{this.yMin}";

                Helpers.DrawTextWithOutline(font: font, text: cellText, pos: new Vector2(this.xMin, this.yMin) + new Vector2(5, 5), color: Color.White, outlineColor: Color.Black, outlineSize: 1);
            }
        }

        public void DrawBackgroundWaterSimulation(Color waterColor)
        {
            if (this.boardGraphics.Texture != null) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.rect, waterColor);
        }

        public void DrawBackground(float opacity = 1f)
        {
            if (this.boardGraphics.Texture != null) SonOfRobinGame.SpriteBatch.Draw(this.boardGraphics.Texture, this.rect, this.boardGraphics.Texture.Bounds, Color.White * opacity);
            else SonOfRobinGame.SpriteBatch.Draw(this.grid.WholeIslandPreviewTexture, this.rect, this.previewRect, Color.White * opacity);
        }
    }
}