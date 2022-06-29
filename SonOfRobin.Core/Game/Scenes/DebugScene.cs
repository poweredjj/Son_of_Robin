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
        public static readonly SpriteFont font = SonOfRobinGame.fontFreeSansBold12;
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

            bool worldActive = world != null && !world.worldCreationInProgress;

            if (worldActive)
            {
                if (world.mapMode == World.MapMode.Big) debugText += $"\nobjects {world.PieceCount}";
                else debugText += $"{world.debugText}";
            }

            debugText += "\n";

            if (worldActive && world.pieceCountByClass.ContainsKey(typeof(Plant))) debugText += $"plants {world.pieceCountByClass[typeof(Plant)]}";
            if (worldActive && world.pieceCountByClass.ContainsKey(typeof(Animal))) debugText += $", animals {world.pieceCountByClass[typeof(Animal)]}";
            if (worldActive) debugText += $"\nproc. animals: {world.processedNonPlantsCount} plants: {world.processedPlantsCount}";
            if (worldActive) debugText += $"\nloaded textures {world.grid.loadedTexturesCount}";
            if (worldActive) debugText += $"\ntracking count {world.trackingQueue.Count}";
            if (worldActive && world.trackingQueue.Count > 5000) debugText += " <--- WARNING, CHECK IF CORRECT!";
            debugText += $"\nsnd inst. total: {SoundInstanceManager.CreatedInstancesCount} act: {SoundInstanceManager.ActiveInstancesCount} inact: {SoundInstanceManager.InactiveInstancesCount} inact. names: {SoundInstanceManager.InactiveNamesCount}";

            debugText += $"\nGC {GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)}";

            //if(SonOfRobinGame.platform == Platform.Desktop) debugText += $"\nram free: {SonOfRobinGame.ramCounter.NextValue()}";
            if (worldActive) debugText += $"\nreal time elapsed {world.TimePlayed:hh\\:mm\\:ss}";
            if (worldActive) debugText += $"\nisland time elapsed {world.islandClock.IslandTimeElapsed:hh\\:mm\\:ss} (x{world.updateMultiplier})";
            if (worldActive) debugText += $"\nisland day {world.islandClock.CurrentDayNo} clock {world.islandClock.TimeOfDay:hh\\:mm\\:ss} ({Convert.ToString(world.islandClock.CurrentPartOfDay).ToLower()})";
            debugText += $"\n{SonOfRobinGame.fps.msg}";

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
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Campfire, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D2))
            {
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.ChestTreasureNormal, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D3))
            {
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.ChestTreasureBig, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D4))
            {
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Coal, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D5))
            {
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.BeltMedium, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D6))
            {
                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.ArrowWood, closestFreeSpot: true);
            }

            if (Keyboard.HasBeenPressed(Keys.D7))
            {
                var heart = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Heart);
                new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: heart.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Right, targetYAlign: YAlign.Top,
                    followingXAlign: XAlign.Left, followingYAlign: YAlign.Bottom);
            }

            if (Keyboard.HasBeenPressed(Keys.D8))
            {
                var backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Backlight);
                new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: backlight.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Center, targetYAlign: YAlign.Bottom);
            }

            if (Keyboard.HasBeenPressed(Keys.D9)) world.CreateMissingPieces(initialCreation: true, outsideCamera: false, multiplier: 1.0f, clearDoNotCreateList: true);

            if (Keyboard.HasBeenPressed(Keys.D0))
            {
                if (world == null) return;

                Vector2 motion = new Vector2(world.random.Next(-20, 20), world.random.Next(-20, 20));

                world.transManager.AddMultipleTransitions(outTrans: true, duration: world.random.Next(4, 10), playCount: -1, replaceBaseValue: false, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.02f, paramsToChange: new Dictionary<string, float> { { "PosX", motion.X }, { "PosY", motion.Y } });

                SolidColor redOverlay = new SolidColor(color: Color.DarkRed, viewOpacity: 0.0f);
                redOverlay.transManager.AddTransition(new Transition(transManager: redOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f, endRemoveScene: true));
                world.solidColorManager.Add(redOverlay);
            }

            if (Keyboard.HasBeenPressed(Keys.OemMinus))
            {
                if (world == null) return;

                foreach (var sprite in world.camera.GetVisibleSprites(groupName: Cell.Group.All))
                {
                    if (sprite.boardPiece != world.player) Tool.HitTarget(attacker: world.player, target: sprite.boardPiece, hitPower: 99999, targetPushMultiplier: 1f);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.G))
            {
                var menuScene = GetBottomSceneOfType(typeof(Menu));
                if (menuScene != null) menuScene.MoveToTop();
            }

            if (Keyboard.HasBeenPressed(Keys.T))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.Kill(); }
            }

            if (Keyboard.HasBeenPressed(Keys.Y))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { sprite.AssignNewName("default"); }
            }

            if (Keyboard.HasBeenPressed(Keys.R))
            {
                while (true)
                {
                    bool hasBeenMoved = world.player.sprite.SetNewPosition(new Vector2(BoardPiece.Random.Next(0, world.width), BoardPiece.Random.Next(0, world.height)));
                    if (hasBeenMoved) break;
                }
            }

            if (Keyboard.HasBeenPressed(Keys.B) || VirtButton.HasButtonBeenPressed(VButName.DebugPlayManySounds))
            {
                for (int i = 0; i < 256; i++)
                {
                    Sound.QuickPlay(SoundData.Name.ArrowFly);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.U))
            {
                if (world == null) return;

                Player player = world.player;
                player.Fatigue = player.maxFatigue;
            }

            if (Keyboard.HasBeenPressed(Keys.J))
            {
                if (world == null) return;

                Player player = world.player;
                player.Fatigue = player.maxFatigue * 0.8f;
            }

            if (Keyboard.HasBeenPressed(Keys.O))
            {
                if (world == null) return;
                Player player = world.player;
                player.sprite.RemoveFromBoard();
            }

            if (Keyboard.HasBeenPressed(Keys.K) || Keyboard.HasBeenPressed(Keys.L))
            {
                bool compareWithBottom = Keyboard.HasBeenPressed(Keys.K);

                var piecesWithinDistance = world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: world.player.sprite, distance: 100, compareWithBottom: compareWithBottom);
                foreach (BoardPiece piece in piecesWithinDistance)
                {
                    if (world.player.sprite.CheckIfOtherSpriteIsWithinRange(target: piece.sprite))
                    { PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Heart); }
                    else
                    { PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight); }
                }
                BoardPiece closestPiece = BoardPiece.FindClosestPiece(sprite: world.player.sprite, pieceList: piecesWithinDistance);

                if (closestPiece != null)
                {
                    BoardPiece backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: closestPiece.sprite.position, templateName: PieceTemplate.Name.Backlight);
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

                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point1.X, point1.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point2.X, point2.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point3.X, point3.Y), templateName: PieceTemplate.Name.Heart);

                var piecesInsideTriangle = world.grid.GetPiecesInsideTriangle(groupName: Cell.Group.All, point1: point1, point2: point2, point3: point3);
                foreach (BoardPiece piece in piecesInsideTriangle)
                {
                    piece.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: piece.sprite.frame.textureSize, priority: 0, framesLeft: 60));
                    PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.A) || VirtButton.HasButtonBeenPressed(VButName.DebugBreakAll))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                {
                    if (sprite.boardPiece != world.player)
                    {
                        BoardPiece boardPiece = sprite.boardPiece;

                        if (boardPiece != null && boardPiece.exists && boardPiece.yield != null) boardPiece.yield.DropFinalPieces();
                        boardPiece.Destroy();
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Z) || VirtButton.HasButtonBeenPressed(VButName.DebugBreakVisible))
            {
                if (world == null) return;

                foreach (var sprite in world.camera.GetVisibleSprites(groupName: Cell.Group.All))
                {
                    if (sprite.boardPiece != world.player)
                    {
                        BoardPiece boardPiece = sprite.boardPiece;

                        if (boardPiece != null && boardPiece.exists && boardPiece.yield != null) boardPiece.yield.DropFinalPieces();
                        boardPiece.Destroy();
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Q) || VirtButton.HasButtonBeenPressed(VButName.DebugClear))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                {
                    if (sprite.boardPiece != world.player) sprite.boardPiece.Destroy();
                }
            }

            if (Keyboard.HasBeenPressed(Keys.W))
            {
                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.RemoveFromStateMachines(); }
            }

            if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "Line 1\nLine 2\nThis is button A | and button B |.\nBelt here >|<\nLast line.", animate: false, useTransition: false, bgColor: Color.DeepSkyBlue, textColor: Color.White, imageList: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, AnimData.framesForPkgs[AnimData.PkgName.SkullAndBones].texture });

            if (Keyboard.HasBeenPressed(Keys.F2))
            {
                var taskChain = new List<Object> { };

                taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));

                var bgColor1 = new List<byte> { Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B };
                var textWindowData1 = new Dictionary<string, Object> { { "text", "Message 1" }, { "bgColor", bgColor1 } };
                taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ShowTextWindow, turnOffInputUntilExecution: true, delay: 1, executeHelper: textWindowData1, storeForLaterUse: true));

                var bgColor2 = new List<byte> { Color.DarkCyan.R, Color.DarkCyan.G, Color.DarkCyan.B };
                var textWindowData2 = new Dictionary<string, Object> { { "text", "Message 2" }, { "bgColor", bgColor2 } };
                taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ShowTextWindow, turnOffInputUntilExecution: true, delay: 60, executeHelper: textWindowData2, storeForLaterUse: true));

                taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));

                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
            }

            if (Keyboard.HasBeenPressed(Keys.F3))
            {
                if (world == null) return;

                world.SpectatorMode = !world.SpectatorMode;
            }

            if (Keyboard.HasBeenPressed(Keys.F4))
            {
                Sound sound = new Sound(name: SoundData.Name.Navigation);
                sound.Play();
            }

            if (Keyboard.HasBeenPressed(Keys.F5)) SonOfRobinGame.progressBar.TurnOn(curVal: 2, maxVal: 5, text: $"Loading game - replacing save slot data...              ...");
            if (Keyboard.HasBeenPressed(Keys.F6)) SonOfRobinGame.progressBar.TurnOn(curVal: 6, maxVal: 6, text: $"Loading game - events...");

            if (Keyboard.HasBeenPressed(Keys.OemPlus))
            {
                if (world == null) return;
                world.hintEngine.shownTutorials = Enum.GetValues(typeof(Tutorials.Type)).Cast<Tutorials.Type>().ToList();
            }

            if (Keyboard.HasBeenPressed(Keys.F7))
            {
                int value = world.random.Next(-30, 30);
                //  world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: value, autoRemoveDelay: world.random.Next(100, 500), isPositive: value > 0));

                //    world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.RegenPoison, value: value, autoRemoveDelay: world.random.Next(600, 1200), canKill: true));

                world.player.buffEngine.AddBuff(buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Sprint, autoRemoveDelay: 4 * 60, value: world.player.speed), world: world);
            }

            if (Keyboard.HasBeenPressed(Keys.F8))
            { world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Haste, value: 2, autoRemoveDelay: 300)); }

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

            if (Keyboard.HasBeenPressed(Keys.F12)) RemoveTopScene();

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
                var allSprites = world.grid.GetAllSprites(Cell.Group.ColMovement).Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();

                if (!allSprites.Any()) return;

                var index = BoardPiece.Random.Next(0, allSprites.Count);
                world.camera.TrackPiece(allSprites.ToArray()[index].boardPiece);
            }

            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) world.camera.TrackPiece(world.player);

            if (Keyboard.HasBeenPressed(Keys.X))
            {
                AnimData.PkgName currentPackageName = world.player.sprite.animPackage;

                while (true)
                {
                    var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.PlayerMale, AnimData.PkgName.PlayerFemale, AnimData.PkgName.FoxGinger, AnimData.PkgName.Frog1, AnimData.PkgName.CrabGreen, AnimData.PkgName.TigerWhite };
                    var packageName = packageNames[BoardPiece.Random.Next(0, packageNames.Count)];
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
            if (Keyboard.HasBeenPressed(Keys.Delete)) Preferences.drawShadows = !Preferences.drawShadows;
        }
    }
}
