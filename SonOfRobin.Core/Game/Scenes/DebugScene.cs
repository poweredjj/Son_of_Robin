﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class DebugScene : Scene
    {
        public static readonly SpriteFont font = SonOfRobinGame.fontMedium;
        public static string debugText = "";

        public DebugScene() : base(inputType: InputTypes.Always, tipsLayout: ControlTips.TipsLayout.Empty, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        {
            this.viewParams.posX = 5;
            this.viewParams.posY = 5;
            this.AddTransition(this.GetTransition(inTrans: true));
        }

        public override void Remove()
        {
            if (this.transition == null)
            {
                this.AddTransition(this.GetTransition(inTrans: false));
                return;
            }

            base.Remove();
        }

        public override void Update(GameTime gameTime)
        {
            ProcessDebugInput();

            debugText = "";

            World world = World.GetTopWorld();
            if (world != null && !world.creationInProgress)
            {
                if (world.mapMode == World.MapMode.Big)
                { debugText += $"objects {world.PieceCount}"; }
                else
                { debugText += $"{world.debugText}"; }

                debugText += "\n";

                if (world.pieceCountByClass.ContainsKey(typeof(Plant))) debugText += $"plants {world.pieceCountByClass[typeof(Plant)]}";
                if (world.pieceCountByClass.ContainsKey(typeof(Animal))) debugText += $", animals {world.pieceCountByClass[typeof(Animal)]}";
                debugText += $"\nproc. plants {world.processedPlantsCount}";
                debugText += $"\nloaded textures {world.grid.loadedTexturesCount}";

                TimeSpan elapsedTime = TimeSpan.FromMilliseconds(world.currentUpdate * 16.67);
                debugText += $"\ntime {elapsedTime:hh\\:mm\\:ss} (x{world.updateMultiplier})";
                debugText += $"\n{SonOfRobinGame.fps.msg}";
            }

            Vector2 txtSize = font.MeasureString(debugText);
            this.viewParams.width = (int)txtSize.X;
            this.viewParams.height = (int)txtSize.Y;
        }

        public override void Draw()
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                { SonOfRobinGame.spriteBatch.DrawString(font, debugText, new Vector2(x, y), Color.Black); }
            }

            SonOfRobinGame.spriteBatch.DrawString(font, debugText, Vector2.Zero, Color.White);
        }

        public Transition GetTransition(bool inTrans)
        {
            Transition.TransType transType = inTrans ? Transition.TransType.In : Transition.TransType.Out;
            bool removeScene = !inTrans;

            int width = this.viewParams.width == SonOfRobinGame.VirtualWidth ? 300 : this.viewParams.width;

            return new Transition(type: transType, duration: 12, scene: this, removeScene: removeScene, paramsToChange: new Dictionary<string, float> { { "posX", this.viewParams.posX - width } });
        }

        public void ProcessDebugInput()
        {
            World world = World.GetTopWorld();
            //if (Keyboard.HasBeenPressed(Keys.Escape)) SonOfRobinGame.quitGame = true;


            if (Keyboard.HasBeenPressed(Keys.D1))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.TentSmall);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D2))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.TentMedium);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D3))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.TentBig);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D4))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.AxeIron);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D5))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.PickaxeIron);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D6))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.ArrowWood);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D7))
            {
                var heart = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Heart);
                new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: heart.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Right, targetYAlign: YAlign.Top,
                    followingXAlign: XAlign.Left, followingYAlign: YAlign.Bottom);
            }

            if (Keyboard.HasBeenPressed(Keys.D8))
            {
                var backlight = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Backlight);
                new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: backlight.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Center, targetYAlign: YAlign.Bottom);
            }

            if (Keyboard.IsPressed(Keys.D9)) world.CreateMissingPieces(outsideCamera: false, multiplier: 1.0f, clearDoNotCreateList: true);

            if (Keyboard.HasBeenPressed(Keys.G))
            {
                var menuScene = GetBottomSceneOfType(typeof(Menu));
                if (menuScene != null) menuScene.MoveToTop();
            }

            if (Keyboard.HasBeenPressed(Keys.T))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.ColAll))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.Kill(); }
            }

            if (Keyboard.HasBeenPressed(Keys.Y))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.ColAll))
                { sprite.AssignNewName("default"); }
            }

            if (Keyboard.HasBeenPressed(Keys.R))
            {
                while (true)
                {
                    bool hasBeenMoved = world.player.sprite.SetNewPosition(new Vector2(SonOfRobinGame.random.Next(0, world.width), SonOfRobinGame.random.Next(0, world.height)));
                    if (hasBeenMoved) break;
                }
            }

            if (Keyboard.HasBeenPressed(Keys.X)) world.grid.UnloadTexturesIfMemoryLow();

            if (Keyboard.HasBeenPressed(Keys.U))
            {
                if (world == null) return;

                Player player = world.player;
                player.Fatigue = player.maxFatigue;
            }

            if (Keyboard.HasBeenPressed(Keys.I))
            {
                if (world == null) return;

                Player player = world.player;
                player.Fatigue = player.maxFatigue * 0.8f;
            }

            if (Keyboard.HasBeenPressed(Keys.O))
            {
                if (world == null) return;

                world.AutoSave(force: true);
            }


            if (Keyboard.HasBeenPressed(Keys.K))
            {
                var piecesWithinDistance = world.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColAll, mainSprite: world.player.sprite, distance: 150);
                foreach (BoardPiece piece in piecesWithinDistance)
                {
                    if (world.player.sprite.CheckIfOtherSpriteIsWithinRange(target: piece.sprite))
                    { PieceTemplate.CreateOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Heart); }
                    else
                    { PieceTemplate.CreateOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight); }
                }
                BoardPiece closestPiece = BoardPiece.FindClosestPiece(sprite: world.player.sprite, pieceList: piecesWithinDistance);

                if (closestPiece != null)
                {
                    BoardPiece backlight = PieceTemplate.CreateOnBoard(world: world, position: closestPiece.sprite.position, templateName: PieceTemplate.Name.Backlight);
                    backlight.sprite.color = new Color(0, 128, 128);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Q) || VirtButton.HasButtonBeenPressed(VButName.DebugClear))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.Destroy(); }
            }

            if (Keyboard.HasBeenPressed(Keys.W))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.RemoveFromStateMachines(); }
            }

            if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", useTransition: true, bgColor: Color.DeepSkyBlue, textColor: Color.White);
            if (Keyboard.HasBeenPressed(Keys.F2)) ProgressBar.Hide();
            if (Keyboard.HasBeenPressed(Keys.F3)) world.player.pieceStorage.AddPiece(piece: PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Fox, world: world), dropIfDoesNotFit: true);

            if (Keyboard.HasBeenPressed(Keys.F4))
            {
                Craft.PopulateAllCategories();
                Craft.Recipe recipe = new Craft.Recipe(pieceToCreate: PieceTemplate.Name.MineralsSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Shell, 4 } });
                recipe.TryToProducePieces(storage: world.player.pieceStorage);
            }

            if (Keyboard.HasBeenPressed(Keys.F5)) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Test message.");
            if (Keyboard.HasBeenPressed(Keys.F6)) ProgressBar.ChangeValues(curVal: 1, maxVal: 5, text: "progressbar test\nsecond line\nand third line");
            if (Keyboard.HasBeenPressed(Keys.F12) || VirtButton.HasButtonBeenPressed(VButName.DebugRemoveTopScene)) RemoveTopScene();


            if (Keyboard.HasBeenPressed(Keys.LeftAlt) || VirtButton.HasButtonBeenPressed(VButName.DebugFastForward))
            {
                if (SonOfRobinGame.game.IsFixedTimeStep)
                {  // at first, only IsFixedTimeStep should be changed
                    SonOfRobinGame.game.IsFixedTimeStep = false;
                }
                else
                { world.updateMultiplier *= 2; }

                SonOfRobinGame.game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                world.camera.fluidMotionDisabled = true;
            }

            if (Keyboard.HasBeenPressed(Keys.LeftControl) || VirtButton.HasButtonBeenPressed(VButName.DebugPlay))
            {
                SonOfRobinGame.game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                world.camera.fluidMotionDisabled = false;
            }

            if (Keyboard.HasBeenPressed(Keys.LeftShift) || VirtButton.HasButtonBeenPressed(VButName.DebugPause))
            {
                SonOfRobinGame.game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 3d);
                world.camera.fluidMotionDisabled = false;
            }

            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets))
            {
                var allSprites = world.grid.GetAllSprites(Cell.Group.ColBlocking).Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();

                var index = SonOfRobinGame.random.Next(0, allSprites.Count);
                world.camera.TrackPiece(allSprites.ToArray()[index].boardPiece);
            }

            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) world.camera.TrackPiece(world.player);

            if (Keyboard.HasBeenPressed(Keys.Z))
            {
                AnimPkg currentPackageName = world.player.sprite.animPackage;

                while (true)
                {
                    var packageNames = new List<AnimPkg> { AnimPkg.DemonMaidPink, AnimPkg.DemonMaidYellow, AnimPkg.Blonde, AnimPkg.Sailor, AnimPkg.FoxGinger, AnimPkg.Frog1, AnimPkg.CrabGreen };
                    var packageName = packageNames[SonOfRobinGame.random.Next(0, packageNames.Count)];
                    if (packageName != currentPackageName)
                    {
                        world.player.sprite.AssignNewPackage(animPackage: packageName, setEvenIfMissing: false);
                        world.player.sprite.AssignNewSize(1);
                        break;
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Insert)) world.player.sprite.Visible = !world.player.sprite.Visible;
            if (Keyboard.HasBeenPressed(Keys.Home)) Preferences.debugShowRects = !Preferences.debugShowRects;
            if (Keyboard.HasBeenPressed(Keys.PageUp) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftStick)) Preferences.debugShowStates = !Preferences.debugShowStates;
            if (Keyboard.HasBeenPressed(Keys.PageDown) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightStick)) Preferences.debugShowStatBars = !Preferences.debugShowStatBars;
            if (Keyboard.HasBeenPressed(Keys.End)) Preferences.debugShowCellData = !Preferences.debugShowCellData;
            if (Keyboard.HasBeenPressed(Keys.Delete)) Preferences.debugUseMultipleThreads = !Preferences.debugUseMultipleThreads;
        }
    }
}
