using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using SonOfRobin;
using Studies.Joystick.Abstract;
using System;

namespace Studies.Joystick.Input
{
    public class DualStick
    {
        // How quickly the touch stick follows in FreeFollow mode
        public readonly float aliveZoneFollowSpeed;
        // How far from the alive zone we can get before the touch stick starts to follow in FreeFollow mode
        public readonly float aliveZoneFollowFactor;
        // If we let the touch origin get too close to the screen edge,
        // the direction is less accurate, so push it away from the edge.
        public readonly float edgeSpacing;
        // Where touches register, if they first land beyond this point,
        // the touch wont be registered as occuring inside the stick
        public readonly float aliveZoneSize;
        // Keeps information of last 4 taps
        private readonly TapStart[] tapStarts = new TapStart[4];
        private int tapStartCount = 0;
        // this keeps counting, no ideia why i cant reset it
        private double totalTime;
        public DualStick(float aliveZoneFollowFactor = 1.3f, float aliveZoneFollowSpeed = 0.05f, float edgeSpacing = 25f, float aliveZoneSize = 65f, float deadZoneSize = 5f)
        {
            this.aliveZoneFollowFactor = aliveZoneFollowFactor;
            this.aliveZoneFollowSpeed = aliveZoneFollowSpeed;
            this.edgeSpacing = edgeSpacing;
            this.aliveZoneSize = aliveZoneSize;

            LeftStick = new Stick(deadZoneSize,
                 new Rectangle(0, 100, (int)(TouchPanel.DisplayWidth * 0.3f), TouchPanel.DisplayHeight - 100),
                 aliveZoneSize, aliveZoneFollowFactor, aliveZoneFollowSpeed, edgeSpacing)
            {
                FixedLocation = new Vector2(aliveZoneSize * aliveZoneFollowFactor, TouchPanel.DisplayHeight - aliveZoneSize * aliveZoneFollowFactor)
            };
            RightStick = new Stick(deadZoneSize,
                new Rectangle((int)(TouchPanel.DisplayWidth * 0.5f), 100, (int)(TouchPanel.DisplayWidth * 0.5f), TouchPanel.DisplayHeight - 100),
                aliveZoneSize, aliveZoneFollowFactor, aliveZoneFollowSpeed, edgeSpacing)
            {
                FixedLocation = new Vector2(TouchPanel.DisplayWidth - aliveZoneSize * aliveZoneFollowFactor, TouchPanel.DisplayHeight - aliveZoneSize * aliveZoneFollowFactor)
            };

            //TouchPanel.EnabledGestures = GestureType.None;
            //TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeLeft;
        }
        public Stick RightStick { get; set; }
        public Stick LeftStick { get; set; }
        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            totalTime += dt;

            //var state = TouchPanel.GetState();
            var state = TouchInput.TouchPanelState;
            TouchLocation? leftTouch = null, rightTouch = null;

            if (tapStartCount > state.Count)
                tapStartCount = state.Count;

            foreach (TouchLocation loc in state)
            {
                if (loc.State == TouchLocationState.Released)
                {
                    int tapStartId = -1;
                    for (int i = 0; i < tapStartCount; ++i)
                    {
                        if (tapStarts[i].Id == loc.Id)
                        {
                            tapStartId = i;
                            break;
                        }
                    }
                    if (tapStartId >= 0)
                    {
                        for (int i = tapStartId; i < tapStartCount - 1; ++i)
                            tapStarts[i] = tapStarts[i + 1];
                        tapStartCount--;
                    }
                    continue;
                }
                else if (loc.State == TouchLocationState.Pressed && tapStartCount < tapStarts.Length)
                {
                    tapStarts[tapStartCount] = new TapStart(loc.Id, totalTime, loc.Position);
                    tapStartCount++;
                }

                if (LeftStick.touchLocation.HasValue && loc.Id == LeftStick.touchLocation.Value.Id)
                {
                    leftTouch = loc;
                    continue;
                }
                if (RightStick.touchLocation.HasValue && loc.Id == RightStick.touchLocation.Value.Id)
                {
                    rightTouch = loc;
                    continue;
                }

                if (!loc.TryGetPreviousLocation(out TouchLocation locPrev))
                    locPrev = loc;

                if (!LeftStick.touchLocation.HasValue)
                {
                    if (LeftStick.StartRegion.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                    {
                        if (LeftStick.Style == TouchStickStyle.Fixed)
                        {
                            if (Vector2.Distance(locPrev.Position, LeftStick.StartLocation) < aliveZoneSize)
                            {
                                leftTouch = locPrev;
                            }
                        }
                        else
                        {
                            leftTouch = locPrev;
                            LeftStick.StartLocation = leftTouch.Value.Position;
                            if (LeftStick.StartLocation.X < LeftStick.StartRegion.Left + edgeSpacing)
                                LeftStick.StartLocation.X = LeftStick.StartRegion.Left + edgeSpacing;
                            if (LeftStick.StartLocation.Y > LeftStick.StartRegion.Bottom - edgeSpacing)
                                LeftStick.StartLocation.Y = LeftStick.StartRegion.Bottom - edgeSpacing;
                        }
                        continue;
                    }
                }

                if (!RightStick.touchLocation.HasValue && locPrev.Id != RightStick.lastExcludedRightTouchId)
                {
                    if (RightStick.StartRegion.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                    {
                        bool excluded = false;
                        foreach (Rectangle r in RightStick.startExcludeRegions)
                        {
                            if (r.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                            {
                                excluded = true;
                                RightStick.lastExcludedRightTouchId = locPrev.Id;
                                continue;
                            }
                        }
                        if (excluded)
                            continue;
                        RightStick.lastExcludedRightTouchId = -1;
                        if (RightStick.Style == TouchStickStyle.Fixed)
                        {
                            if (Vector2.Distance(locPrev.Position, RightStick.StartLocation) < aliveZoneSize)
                            {
                                rightTouch = locPrev;
                            }
                        }
                        else
                        {
                            rightTouch = locPrev;
                            RightStick.StartLocation = rightTouch.Value.Position;
                            if (RightStick.StartLocation.X > RightStick.StartRegion.Right - edgeSpacing)
                                RightStick.StartLocation.X = RightStick.StartRegion.Right - edgeSpacing;
                            if (RightStick.StartLocation.Y > RightStick.StartRegion.Bottom - edgeSpacing)
                                RightStick.StartLocation.Y = RightStick.StartRegion.Bottom - edgeSpacing;
                        }
                        continue;
                    }
                }
            }

            LeftStick.Update(state, leftTouch, dt);
            RightStick.Update(state, rightTouch, dt);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D backgroundTx = SonOfRobinGame.textureByName["virtual_joypad_background"];
            Texture2D stickTx = SonOfRobinGame.textureByName["virtual_joypad_stick"];

            float scale = Preferences.globalScale;

            float backgroundSize = aliveZoneSize / scale * 2;
            float stickSize = aliveZoneSize * 0.6f / scale;


            DrawGraphicsCentered(texture: backgroundTx, position: LeftStick.StartLocation / scale, spriteBatch: spriteBatch, width: backgroundSize, height: backgroundSize);
            DrawGraphicsCentered(texture: backgroundTx, position: RightStick.StartLocation / scale, spriteBatch: spriteBatch, width: backgroundSize, height: backgroundSize);

            DrawGraphicsCentered(texture: stickTx, position: LeftStick.GetPositionVector(aliveZoneSize) / scale, spriteBatch: spriteBatch, width: stickSize, height: stickSize);
            DrawGraphicsCentered(texture: stickTx, position: RightStick.GetPositionVector(aliveZoneSize) / scale, spriteBatch: spriteBatch, width: stickSize, height: stickSize);
        }

        private void DrawGraphicsCentered(Texture2D texture, Vector2 position, SpriteBatch spriteBatch, float width, float height)
        {
            if (position.X == 0 && position.Y == 0) return;

            Vector2 posCentered = position - new Vector2(width / 2, height / 2);

            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle destinationRectangle = new Rectangle(
                Convert.ToInt32(posCentered.X),
                Convert.ToInt32(posCentered.Y),
                Convert.ToInt32((float)width),
                Convert.ToInt32((float)height));

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

    }
}