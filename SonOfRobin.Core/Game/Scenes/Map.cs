using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Map : Scene
    {
        private readonly World world;
        public readonly bool fullScreen;
        private float multiplier;
        private bool dirtyBackground;
        private RenderTarget2D terrainGfx;
        private bool initialized;

        // max screen percentage, that minimap may occupy
        private static readonly float minimapMaxPercentWidth = 0.35f;
        private static readonly float minimapMaxPercentHeight = 0.5f;

        private void UpdateViewPos()
        {
            if (this.fullScreen)
            { this.viewParams.CenterView(); }
            else
            {
                var margin = Convert.ToInt32(Math.Ceiling(Math.Min(SonOfRobinGame.VirtualWidth / 30f, SonOfRobinGame.VirtualHeight / 30f)));

                this.viewParams.posX = SonOfRobinGame.VirtualWidth - this.viewParams.width - margin;
                this.viewParams.posY = SonOfRobinGame.VirtualHeight - this.viewParams.height - margin;
            }
        }

        private Transition InTransition
        { get { return this.GetTransition(inTrans: true); } }

        private Transition OutTransition
        { get { return this.GetTransition(inTrans: false); } }

        public Map(World world, bool fullScreen, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.drawActive = false;
            this.updateActive = false;
            this.world = world;
            this.fullScreen = fullScreen;
            this.initialized = false;
        }

        public void TurnOn(bool addTransition = true)
        {
            if (this.fullScreen) this.InputType = InputTypes.Normal;

            this.updateActive = true;
            this.drawActive = true;
            this.blocksDrawsBelow = this.fullScreen;

            float maxPercentWidth = (this.fullScreen) ? 1 : minimapMaxPercentWidth;
            float maxPercentHeight = (this.fullScreen) ? 1 : minimapMaxPercentHeight;

            float multiplierX = ((float)SonOfRobinGame.VirtualWidth / (float)world.width) * maxPercentWidth;
            float multiplierY = ((float)SonOfRobinGame.VirtualHeight / (float)world.height) * maxPercentHeight;
            this.multiplier = Math.Min(multiplierX, multiplierY);

            this.viewParams.width = Convert.ToInt32(world.width * this.multiplier);
            this.viewParams.height = Convert.ToInt32(world.height * this.multiplier);

            this.UpdateViewPos();
            if (addTransition) this.AddTransition(this.InTransition);

            if (this.terrainGfx == null || this.terrainGfx.Width != this.viewParams.width || this.terrainGfx.Height != this.viewParams.height)
            {
                this.terrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.width, this.viewParams.height, false, SurfaceFormat.Color, DepthFormat.None);
                this.dirtyBackground = true;
            }
        }

        public void TurnOff(bool addTransition = true)
        {
            this.InputType = InputTypes.None;
            this.blocksDrawsBelow = false;
            if (addTransition) this.AddTransition(this.OutTransition);
        }

        public Transition GetTransition(bool inTrans)
        {
            Transition.TransType transType = inTrans ? Transition.TransType.In : Transition.TransType.Out;
            bool turnOffDraw = !inTrans;
            bool turnOffUpdate = !inTrans;

            this.UpdateViewPos();

            if (this.fullScreen)
            {
                Rectangle viewRect = this.world.camera.viewRect;
                float transScaleX = SonOfRobinGame.VirtualWidth / (float)this.world.width * this.world.viewParams.scaleX;
                float transScaleY = SonOfRobinGame.VirtualHeight / (float)this.world.height * this.world.viewParams.scaleY;
                float transScale = Math.Min(transScaleX, transScaleY);

                float transPosX = 1f / (float)this.world.width * SonOfRobinGame.VirtualWidth;
                float transPosY = 1f / (float)this.world.height * SonOfRobinGame.VirtualHeight;
                float transPos = Math.Min(transPosX, transPosY);

                return new Transition(type: transType, duration: 15, scene: this, blockInput: false, paramsToChange: new Dictionary<string, float> { { "posX", -viewRect.Left * transPos }, { "posY", -viewRect.Top * transPos }, { "scaleX", transScale }, { "scaleY", transScale } }, turnOffDraw: turnOffDraw, turnOffUpdate: turnOffUpdate);
            }
            else
            {
                return new Transition(type: transType, duration: 8, scene: this, blockInput: false,
                    paramsToChange: new Dictionary<string, float> { { "posY", this.viewParams.posY + this.viewParams.height } }, turnOffDraw: turnOffDraw, turnOffUpdate: turnOffUpdate);
            }
        }

        public void UpdateResolution()
        { if (this.updateActive) this.TurnOn(addTransition: false); }

        public void UpdateBackground()
        {
            if (!this.dirtyBackground) return;

            this.StartRenderingToTarget(this.terrainGfx);

            int width = (int)(this.world.width * this.multiplier);
            int height = (int)(this.world.height * this.multiplier);

            Texture2D mapTexture = BoardGraphics.CreateEntireMapTexture(width: width, height: height, grid: this.world.grid, multiplier: this.multiplier);
            Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
            SonOfRobinGame.spriteBatch.Draw(mapTexture, sourceRectangle, sourceRectangle, Color.White);

            this.EndRenderingToTarget();

            this.dirtyBackground = false;
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Map background updated ({this.viewParams.width}x{this.viewParams.height}).", color: Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            if (!this.initialized && !this.world.creationInProgress)
            {
                // to avoid lag when opening the map for the first time
                this.TurnOn(addTransition: false);
                this.TurnOff(addTransition: false);
            }

            this.UpdateBackground(); // it's best to update background graphics in Update() (SetRenderTarget in Draw() must go first)
            this.ProcessInput();
        }

        private void ProcessInput()
        {
            if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadRight) ||
                Keyboard.HasBeenPressed(Keys.Escape) ||
                Keyboard.HasBeenPressed(Keys.M) ||
                VirtButton.HasButtonBeenPressed(VButName.Return)) this.world.ToggleMapMode();
        }

        public override void Draw()
        {
            // filling screen with water color
            if (this.fullScreen) SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // drawing background (terrain)
            SonOfRobinGame.spriteBatch.Draw(this.terrainGfx, new Rectangle(0, 0, this.viewParams.width, this.viewParams.height), Color.White * this.viewParams.drawOpacity);

            // drawing pieces
            if (this.fullScreen)
            {
                var groupName = (this.fullScreen) ? Cell.Group.Visible : Cell.Group.ColBlocking;

                foreach (Sprite sprite in world.grid.GetAllSprites(groupName)) // regular "foreach", because spriteBatch is not thread-safe
                {
                    byte size;
                    Color color;

                    if (sprite.boardPiece.GetType() == typeof(Player))
                    {
                        size = 3;
                        color = Color.Black;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Plant))
                    {
                        size = 2;
                        color = sprite.boardPiece.alive ? Color.Green : Color.DarkGreen;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Decoration))
                    {
                        size = 2;
                        color = Color.Gray;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Animal))
                    {
                        if (sprite.boardPiece.name == PieceTemplate.Name.Fox)
                        { color = Color.Red; }
                        else
                        { color = Color.Yellow; }

                        size = 3;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Collectible))
                    {
                        size = 2;
                        color = Color.Blue;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Workshop))
                    {
                        size = 2;
                        color = Color.Brown;
                    }

                    else if (sprite.boardPiece.GetType() == typeof(Container))
                    {
                        size = 2;
                        color = Color.Orange;
                    }

                    else
                    { continue; }

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                       Convert.ToInt32((sprite.position.X * this.multiplier) - size / 2),
                       Convert.ToInt32((sprite.position.Y * this.multiplier) - size / 2),
                       size, size), color * this.viewParams.drawOpacity);
                }

            }
            else // this.fullScreen == false
            {
                Sprite sprite = this.world.player.sprite;
                int size = 3;
                Color color = Color.Black;

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                      Convert.ToInt32((sprite.position.X * this.multiplier) - size / 2),
                      Convert.ToInt32((sprite.position.Y * this.multiplier) - size / 2),
                      size, size), color * this.viewParams.drawOpacity);
            }

            // drawing camera FOV
            var viewRect = this.world.camera.viewRect;

            int fovX = Convert.ToInt32(viewRect.Left * this.multiplier);
            int fovY = Convert.ToInt32(viewRect.Top * this.multiplier);
            int fovWidth = Convert.ToInt32(viewRect.Width * this.multiplier);
            int fovHeight = Convert.ToInt32(viewRect.Height * this.multiplier);
            int borderWidth = (this.fullScreen) ? 2 : 1;

            Rectangle fovRect = new Rectangle(fovX, fovY, fovWidth, fovHeight);
            Helpers.DrawRectangleOutline(rect: fovRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: borderWidth);
        }

        public void StartRenderingToTarget(RenderTarget2D newRenderTarget)
        {
            SonOfRobinGame.spriteBatch.Begin();
            SonOfRobinGame.graphicsDevice.SetRenderTarget(newRenderTarget);
        }

        public void EndRenderingToTarget()
        { SonOfRobinGame.spriteBatch.End(); }

    }
}
