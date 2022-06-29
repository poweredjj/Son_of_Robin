using Microsoft.Xna.Framework;
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

        public DebugScene() : base(inputType: InputTypes.Always, tipsLayout: ControlTips.TipsLayout.Empty, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        {
            this.transManager.AddTransition(this.GetTransition(inTrans: true));
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding) this.transManager.AddTransition(this.GetTransition(inTrans: false));
            else base.Remove();
        }

        public Transition GetTransition(bool inTrans)
        {
            return new Transition(transManager: this.transManager, outTrans: !inTrans, baseParamName: "PosX", targetVal: this.viewParams.PosX - 700, duration: 12, endRemoveScene: !inTrans);
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
                //if(SonOfRobinGame.platform == Platform.Desktop) debugText += $"\nram free: {SonOfRobinGame.ramCounter.NextValue()}";

                TimeSpan elapsedTime = TimeSpan.FromMilliseconds(world.currentUpdate * 16.67);
                debugText += $"\ntime {elapsedTime:hh\\:mm\\:ss} (x{world.updateMultiplier})";
                debugText += $"\n{SonOfRobinGame.fps.msg}";
            }

            Vector2 txtSize = font.MeasureString(debugText);
            this.viewParams.Width = (int)txtSize.X;
            this.viewParams.Height = (int)txtSize.Y;
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

        public void ProcessDebugInput()
        {
            World world = World.GetTopWorld();

            if (Keyboard.HasBeenPressed(Keys.D1))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Scythe);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D2))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Rabbit);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D3))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.WoodLog);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D4))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Coal);
                if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(world.player.sprite.position);
            }

            if (Keyboard.HasBeenPressed(Keys.D5))
            {
                BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.BeltMedium);
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

            if (Keyboard.HasBeenPressed(Keys.D9)) world.CreateMissingPieces(outsideCamera: false, multiplier: 1.0f, clearDoNotCreateList: true);

            if (Keyboard.HasBeenPressed(Keys.D0))
            {
                if (world == null) return;

                Vector2 motion = new Vector2(world.random.Next(-20, 20), world.random.Next(-20, 20));

                world.transManager.AddMultipleTransitions(outTrans: true, duration: world.random.Next(4, 10), playCount: -1, replaceBaseValue: false, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.02f, paramsToChange: new Dictionary<string, float> { { "PosX", motion.X }, { "PosY", motion.Y } });
            }

            if (Keyboard.HasBeenPressed(Keys.OemMinus))
            {
                if (world == null) return;

                world.hintEngine.ShowGeneralHint(type: HintEngine.Type.CineIntroduction, ignoreDelay: true);

                var taskChain = new List<Object>();
            }


            if (Keyboard.HasBeenPressed(Keys.OemPlus))
            {
                if (world == null) return;

                Player player = world.player;

                player.sprite.SetOrientationByMovement(new Vector2(0, -5));
            }


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
                Player player = world.player;
                player.sprite.RemoveFromGrid();
            }

            if (Keyboard.HasBeenPressed(Keys.K) || Keyboard.HasBeenPressed(Keys.L))
            {
                bool compareWithBottom = Keyboard.HasBeenPressed(Keys.K);

                var piecesWithinDistance = world.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColAll, mainSprite: world.player.sprite, distance: 150, compareWithBottom: compareWithBottom);
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

            if (Keyboard.HasBeenPressed(Keys.OemComma))
            {
                if (world == null) return;
                Player player = world.player;

                Point point1 = new Point((int)player.sprite.position.X, (int)player.sprite.position.Y);
                Point point2 = new Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y - 100);
                Point point3 = new Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y + 100);

                PieceTemplate.CreateOnBoard(world: world, position: new Vector2(point1.X, point1.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateOnBoard(world: world, position: new Vector2(point2.X, point2.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateOnBoard(world: world, position: new Vector2(point3.X, point3.Y), templateName: PieceTemplate.Name.Heart);

                var piecesInsideTriangle = world.grid.GetPiecesInsideTriangle(groupName: Cell.Group.All, point1: point1, point2: point2, point3: point3);
                foreach (BoardPiece piece in piecesInsideTriangle)
                {
                    piece.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: piece.sprite.frame.originalTextureSize, priority: 0, framesLeft: 60));
                    PieceTemplate.CreateOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight);
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

            if (Keyboard.HasBeenPressed(Keys.F2))
            {
                var taskChain = new List<Object> { };

                taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));

                var bgColor1 = new List<byte> { Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B };
                var textWindowData1 = new Dictionary<string, Object> { { "text", "Message 1" }, { "bgColor", bgColor1 } };
                taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.OpenTextWindow, turnOffInput: true, delay: 1, executeHelper: textWindowData1, storeForLaterUse: true));

                var bgColor2 = new List<byte> { Color.DarkCyan.R, Color.DarkCyan.G, Color.DarkCyan.B };
                var textWindowData2 = new Dictionary<string, Object> { { "text", "Message 2" }, { "bgColor", bgColor2 } };
                taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.OpenTextWindow, turnOffInput: true, delay: 60, executeHelper: textWindowData2, storeForLaterUse: true));

                taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));

                new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInput: true, executeHelper: taskChain);
            }

            if (Keyboard.HasBeenPressed(Keys.F3)) world.player.pieceStorage.AddPiece(piece: PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Fox, world: world), dropIfDoesNotFit: true);

            if (Keyboard.HasBeenPressed(Keys.F4))
            {
                if (world == null) return;

                Craft.PopulateAllCategories();
                Craft.Recipe recipe = new Craft.Recipe(pieceToCreate: PieceTemplate.Name.MineralsSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Shell, 4 } });
                recipe.TryToProducePieces(player: world.player);
            }

            if (Keyboard.HasBeenPressed(Keys.F5)) SonOfRobinGame.progressBar.TurnOn(curVal: 2, maxVal: 5, text: $"Loading game - replacing save slot data...              ...");
            if (Keyboard.HasBeenPressed(Keys.F6)) SonOfRobinGame.progressBar.TurnOn(curVal: 6, maxVal: 6, text: $"Loading game - events...");

            if (Keyboard.HasBeenPressed(Keys.F7))
            {
                int value = world.random.Next(1, 5);
                world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Strength, value: value, autoRemoveDelay: world.random.Next(100, 500), isPositive: value > 0));
            }

            if (Keyboard.HasBeenPressed(Keys.F8))
            { world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.EnableMap, value: null, autoRemoveDelay: 600, isPositive: true)); }

            if (Keyboard.HasBeenPressed(Keys.F9))
            {
                SonOfRobinGame.progressBar.TurnOn(newPosX: 0, newPosY: 0, centerHoriz: true, centerVert: true, entryList: new List<InfoWindow.TextEntry> {
                        new InfoWindow.TextEntry(text: "First line.", color: Color.White, frame: AnimData.frameListById["Blonde-0-default"][0]),
                        new InfoWindow.TextEntry(text: "Second line.", color: Color.Green, scale: 1.5f, frame: AnimData.frameListById["Blonde-0-default"][0]),
                        new InfoWindow.TextEntry(color: Color.White, progressCurrentVal: 2, progressMaxVal: 5),
                        new InfoWindow.TextEntry(text: "And this is fourth line.", color: Color.LightBlue, frame: AnimData.frameListById["Blonde-0-default"][0]),
                        new InfoWindow.TextEntry(text: "This window should be centered.", color: Color.YellowGreen, frame: AnimData.frameListById["Blonde-0-default"][0]),
                    });
            }

            if (Keyboard.HasBeenPressed(Keys.F10))
            {
                SonOfRobinGame.progressBar.TurnOn(newPosX: 0, newPosY: 0,
                    entryList: new List<InfoWindow.TextEntry> {
                        new InfoWindow.TextEntry(text: "Second line.", color: Color.Green),
                        new InfoWindow.TextEntry(text: "And this is third line.", color: Color.LightBlue),
                        new InfoWindow.TextEntry(text: "This window should be at 0,0.", color: Color.Yellow),
                });
            }

            if (Keyboard.HasBeenPressed(Keys.F11)) SonOfRobinGame.progressBar.TurnOff();

            if (Keyboard.HasBeenPressed(Keys.F12) || VirtButton.HasButtonBeenPressed(VButName.DebugRemoveTopScene)) RemoveTopScene();

            if (Keyboard.HasBeenPressed(Keys.LeftAlt) || VirtButton.HasButtonBeenPressed(VButName.DebugFastForward))
            {
                if (SonOfRobinGame.game.IsFixedTimeStep)
                {  // at first, only IsFixedTimeStep should be changed
                    SonOfRobinGame.game.IsFixedTimeStep = false;
                }
                else world.updateMultiplier *= 2;

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
                    var packageNames = new List<AnimPkg> { AnimPkg.Blonde, AnimPkg.Sailor, AnimPkg.FoxGinger, AnimPkg.Frog1, AnimPkg.CrabGreen, AnimPkg.TigerWhite };
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
