using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class LightEngine
    {
        public bool IsActive { get; private set; }
        private Sprite sprite;
        private int width;
        private int height;
        private readonly float opacity;
        private readonly Color color;
        private readonly bool colorActive;
        private readonly float addedGfxRectMultiplier;
        private readonly bool glowOnlyAtNight;
        public readonly bool castShadows;
        public int tempShadowMaskIndex;

        public LightEngine(float opacity, Color color, bool colorActive, bool castShadows, float addedGfxRectMultiplier = 0f, bool isActive = true, Sprite sprite = null, int width = 0, int height = 0, int size = 0, bool glowOnlyAtNight = false)
        {
            this.IsActive = isActive;
            this.sprite = sprite;
            this.width = size == 0 ? width : size;
            this.height = size == 0 ? height : size;
            this.opacity = opacity;
            this.color = color;
            this.colorActive = colorActive;
            this.addedGfxRectMultiplier = addedGfxRectMultiplier;
            this.glowOnlyAtNight = glowOnlyAtNight;
            this.castShadows = castShadows;
        }

        public bool Equals(LightEngine lightEngine)
        {
            return
                this.IsActive == lightEngine.IsActive &&
                this.width == lightEngine.width &&
                this.height == lightEngine.height &&
                this.opacity == lightEngine.opacity &&
                this.color == lightEngine.color &&
                this.colorActive == lightEngine.colorActive &&
                this.addedGfxRectMultiplier == lightEngine.addedGfxRectMultiplier &&
                this.glowOnlyAtNight == lightEngine.glowOnlyAtNight &&
                this.castShadows == lightEngine.castShadows;
        }

        public int Width
        { get { return (int)(this.sprite.gfxRect.Width * this.addedGfxRectMultiplier) + width; } set { this.width = value; } }

        public int Height
        { get { return (int)(this.sprite.gfxRect.Height * this.addedGfxRectMultiplier) + height; } set { this.height = value; } }

        public int Size
        {
            get { return Math.Max(this.Width, this.Height); }
            set
            {
                this.width = value;
                this.height = value;
            }
        }

        public float Opacity
        {
            get
            {
                if (!this.IsActive || (this.glowOnlyAtNight && this.sprite.world.islandClock.CurrentPartOfDay != IslandClock.PartOfDay.Night)) return 0f;

                if (this.sprite.opacityFade != null &&
                    this.sprite.opacityFade.mode != OpacityFade.Mode.CameraTargetObstruct &&
                    this.sprite.opacityFade.mode != OpacityFade.Mode.CameraTargetObstructRevert) return this.opacity * this.sprite.opacity;
                return this.opacity;
            }
        }

        public Color Color
        { get { return this.IsActive ? this.color * this.Opacity : Color.Transparent; } }

        public bool ColorActive
        {
            get
            {
                return this.colorActive && this.glowOnlyAtNight && this.sprite.world.islandClock.CurrentPartOfDay != IslandClock.PartOfDay.Night ? false : this.colorActive;
            }
        }

        public Rectangle Rect
        {
            get
            {
                Point centerPos = this.sprite.gfxRect.Center;
                return new Rectangle(x: centerPos.X - (this.Width / 2), y: centerPos.Y - (this.Height / 2), width: this.Width, height: this.Height);
            }
        }

        public void AssignSprite(Sprite newSprite)
        {
            this.sprite = newSprite;
            if (this.IsActive) this.Activate();
        }

        public void Activate()
        {
            this.IsActive = true;

            if (!this.sprite.gridGroups.Contains(Cell.Group.LightSource)) this.sprite.gridGroups.Add(Cell.Group.LightSource);
            if (this.sprite.IsOnBoard) this.sprite.currentCell.UpdateGroups(sprite: this.sprite, groupNames: this.sprite.gridGroups);
        }

        public void Deactivate()
        {
            this.IsActive = false;

            if (this.sprite.gridGroups.Contains(Cell.Group.LightSource)) this.sprite.gridGroups.Remove(Cell.Group.LightSource);
            if (this.sprite.IsOnBoard) this.sprite.currentCell.UpdateGroups(sprite: this.sprite, groupNames: this.sprite.gridGroups);
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> lightData = new Dictionary<string, object>
            {
                { "width", this.width },
                { "height", this.height },
                { "opacity", this.opacity },
                { "color", new List<Byte>{color.R, color.G, color.B, color.A} },
                { "colorActive", this.colorActive },
                { "addedGfxRectMultiplier", this.addedGfxRectMultiplier },
                { "isActive", this.IsActive },
                { "glowOnlyAtNight", this.glowOnlyAtNight },
                { "castShadows", this.castShadows },
            };

            return lightData;
        }

        public static LightEngine Deserialize(Object lightData, Sprite sprite)
        {
            var lightDict = (Dictionary<string, Object>)lightData;

            int width = (int)(Int64)lightDict["width"];
            int height = (int)(Int64)lightDict["height"];
            float opacity = (float)(double)lightDict["opacity"];
            var colorData = (List<Byte>)lightDict["color"];
            Color color = new Color(colorData[0], colorData[1], colorData[2], colorData[3]);
            bool colorActive = (bool)lightDict["colorActive"];
            float addedGfxRectMultiplier = (float)(double)lightDict["addedGfxRectMultiplier"];
            bool isActive = (bool)lightDict["isActive"];
            bool glowOnlyAtNight = (bool)lightDict["glowOnlyAtNight"];
            bool castShadows = (bool)lightDict["castShadows"];

            LightEngine lightEngine = new LightEngine(sprite: sprite, width: width, height: height, opacity: opacity, color: color, colorActive: colorActive, addedGfxRectMultiplier: addedGfxRectMultiplier, isActive: isActive, glowOnlyAtNight: glowOnlyAtNight, castShadows: castShadows);

            if (lightEngine.IsActive) lightEngine.Activate();

            return lightEngine;
        }
    }
}