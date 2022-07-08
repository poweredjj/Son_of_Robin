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

            var debugLines = new List<string>();

            World world = World.GetTopWorld();

            bool worldActive = world != null && !world.worldCreationInProgress;

            if (worldActive)
            {
                if (world.mapMode == World.MapMode.Big) debugLines.Add($"objects {world.PieceCount}");
                else debugLines.Add($"{world.debugText}");
            }

            if (worldActive && world.pieceCountByClass.ContainsKey(typeof(Plant))) debugLines.Add($"plants {world.pieceCountByClass[typeof(Plant)]}");
            if (worldActive && world.pieceCountByClass.ContainsKey(typeof(Animal))) debugLines.Add($", animals {world.pieceCountByClass[typeof(Animal)]}");

            if (worldActive)
            {
                debugLines.Add($"proc. animals: {world.processedNonPlantsCount} plants: {world.processedPlantsCount}");
                debugLines.Add($"loaded textures {world.grid.loadedTexturesCount}");
                debugLines.Add($"tracking count {world.trackingQueue.Count}");
                if (world.trackingQueue.Count > 5000) debugLines.Add("WARNING, CHECK IF CORRECT!");
            }

            debugLines.Add($"snd inst. total: {SoundInstanceManager.CreatedInstancesCount} act: {SoundInstanceManager.ActiveInstancesCount} inact: {SoundInstanceManager.InactiveInstancesCount} inact. names: {SoundInstanceManager.InactiveNamesCount}");

            debugLines.Add($"GC {GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)}");

            //if(SonOfRobinGame.platform == Platform.Desktop) debugText += $"ram free: {SonOfRobinGame.ramCounter.NextValue()}";
            if (worldActive) debugLines.Add($"real time elapsed {world.TimePlayed:hh\\:mm\\:ss}");
            if (worldActive) debugLines.Add($"island time elapsed {world.islandClock.IslandTimeElapsed:hh\\:mm\\:ss} (x{world.updateMultiplier})");
            if (worldActive) debugLines.Add($"island day {world.islandClock.CurrentDayNo} clock {world.islandClock.TimeOfDay:hh\\:mm\\:ss} ({Convert.ToString(world.islandClock.CurrentPartOfDay).ToLower()})");
            debugLines.Add($"{SonOfRobinGame.fps.msg}");

            debugText = String.Join("\n", debugLines);

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

            if (world != null)
            {
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
                    WorldEvent.RemovePieceFromQueue(world: world, pieceToRemove: heart);
                    heart.sprite.opacityFade = null;
                    heart.sprite.opacity = 1;
                    new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: heart.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Right, targetYAlign: YAlign.Top, followingXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, followSlowDown: 5);
                }

                if (Keyboard.HasBeenPressed(Keys.D8))
                {
                    var backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.player.sprite.position, templateName: PieceTemplate.Name.Backlight);
                    new Tracking(world: world, targetSprite: world.player.sprite, followingSprite: backlight.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Center, targetYAlign: YAlign.Bottom);
                }

                if (Keyboard.HasBeenPressed(Keys.D9))
                {
                    if (world == null) return;

                    world.CreateMissingPieces(initialCreation: true, outsideCamera: false, multiplier: 1.0f, clearDoNotCreateList: true);
                }
            }

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
                if (world == null) return;

                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.Kill(); }
            }

            if (Keyboard.HasBeenPressed(Keys.Y))
            {
                if (world == null) return;

                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { sprite.AssignNewName("default"); }
            }

            if (Keyboard.HasBeenPressed(Keys.R))
            {
                if (world == null) return;


                while (true)
                {
                    bool hasBeenMoved = world.player.sprite.SetNewPosition(new Vector2(BoardPiece.Random.Next(0, world.width), BoardPiece.Random.Next(0, world.height)));
                    if (hasBeenMoved) break;
                }
            }

            if (Keyboard.HasBeenPressed(Keys.B))
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
                if (world == null) return;

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

                Microsoft.Xna.Framework.Point point1 = new Microsoft.Xna.Framework.Point((int)player.sprite.position.X, (int)player.sprite.position.Y);
                Microsoft.Xna.Framework.Point point2 = new Microsoft.Xna.Framework.Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y - 100);
                Microsoft.Xna.Framework.Point point3 = new Microsoft.Xna.Framework.Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y + 100);

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
                if (world == null) return;

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
                if (world == null) return;


                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                {
                    if (sprite.boardPiece != world.player) sprite.boardPiece.Destroy();
                }
            }

            if (Keyboard.HasBeenPressed(Keys.W))
            {
                if (world == null) return;


                foreach (var sprite in world.grid.GetAllSprites(Cell.Group.All))
                { if (sprite.boardPiece != world.player) sprite.boardPiece.RemoveFromStateMachines(); }
            }

            //if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "Line 1\nLine 2\nThis is button A | and button B |.\nBelt here >|<\nLast line.", animate: false, useTransition: false, bgColor: Color.DeepSkyBlue, textColor: Color.White, imageList: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, AnimData.framesForPkgs[AnimData.PkgName.BeltMedium].texture });

            if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "If I had more | leather, I could make a | backpack or a | belt.\n>|1|2|3|4|5<", animate: true, useTransition: false, framesPerChar: 1, bgColor: Color.DeepSkyBlue, textColor: Color.White, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium) });


            if (Keyboard.HasBeenPressed(Keys.F2))
            {
                if (world == null) return;

                world.SpectatorMode = !world.SpectatorMode;
            }

            if (Keyboard.HasBeenPressed(Keys.F3) || VirtButton.HasButtonBeenPressed(VButName.DebugClockAdvance))
            {
                if (world == null) return;

                world.islandClock.Advance(amount: 60 * 60 * 2, ignorePause: true);
            }

            if (Keyboard.HasBeenPressed(Keys.F4))
            {
                RemoveAllScenesOfType(typeof(TileMapProcessScene));

                int width = 53; // 53
                int height = 20; // 20
                int seed = -1;

                TileMapProcessScene tileMapProcessScene = new TileMapProcessScene(mapType: TileMap.MapType.DefaultOverworld, width: width, height: height, seed: seed, showVis: true, showProgressBar: false, elementsPerFrame: 50);
            }

            if (Keyboard.HasBeenPressed(Keys.F5))
            {
                RemoveAllScenesOfType(typeof(TileMapProcessScene));

                int width = 350;
                int height = 120;
                int seed = -1;

                TileMapProcessScene tileMapProcessScene = new TileMapProcessScene(mapType: TileMap.MapType.DefaultOverworld, width: width, height: height, seed: seed, showVis: true, showProgressBar: false, elementsPerFrame: 50);
            }

            if (Keyboard.HasBeenPressed(Keys.F6))
            {
                RemoveAllScenesOfType(typeof(CollapseMapProcessScene));

                int width = 400;
                int height = 200;
                int seed = -1;

                CollapseMapProcessScene mapContainer = new CollapseMapProcessScene(mapType: CollapseMapProcessScene.MapType.Test2, width: width, height: height, seed: seed, showVis: true, showProgressBar: false, elementsPerFrame: width * height / 80);
            }

            if (Keyboard.HasBeenPressed(Keys.F7))
            {
                RemoveAllScenesOfType(typeof(CollapseMapProcessScene));

                int width = 220;
                int height = 100;
                int seed = -1;

                CollapseMapProcessScene mapContainer = new CollapseMapProcessScene(mapType: CollapseMapProcessScene.MapType.DefaultOverworld, width: width, height: height, seed: seed, showVis: true, showProgressBar: false, elementsPerFrame: width * height / 80);
            }

            if (Keyboard.HasBeenPressed(Keys.OemPlus))
            {
                if (world == null) return;

                world.hintEngine.shownTutorials = Enum.GetValues(typeof(Tutorials.Type)).Cast<Tutorials.Type>().ToList();
            }

            if (Keyboard.HasBeenPressed(Keys.F8))
            {
                if (world == null) return;

                // int value = world.random.Next(-30, 30);
                //  world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: value, autoRemoveDelay: world.random.Next(100, 500), isPositive: value > 0));

                //    world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.RegenPoison, value: value, autoRemoveDelay: world.random.Next(600, 1200), canKill: true));

                world.player.buffEngine.AddBuff(world: world, buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Haste, value: 2, autoRemoveDelay: 300));

                // world.player.buffEngine.AddBuff(buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Sprint, autoRemoveDelay: 4 * 60, value: world.player.speed), world: world);
            }


            if (Keyboard.HasBeenPressed(Keys.F9))
            {
                SonOfRobinGame.progressBar.TurnOn(newPosX: 0, newPosY: 0, centerHoriz: true, centerVert: true, addTransition: true, entryList: new List<InfoWindow.TextEntry> {
                        new InfoWindow.TextEntry(text: "| First line.", color: Color.White, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Player) }),
                        new InfoWindow.TextEntry(text: "| Second line.", color: Color.Green, scale: 1.5f, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Player) }, justify: InfoWindow.TextEntry.Justify.Center),
                        new InfoWindow.TextEntry(color: Color.White, progressCurrentVal: 2, progressMaxVal: 5),
                        new InfoWindow.TextEntry(text: "| And this is fourth line.", color: Color.LightBlue, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Player) }),
                        new InfoWindow.TextEntry(text: "| This window should be centered.", color: Color.YellowGreen, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Player) }, justify: InfoWindow.TextEntry.Justify.Right),
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
                if (world == null) return;

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
                if (world == null) return;

                SonOfRobinGame.game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                world.camera.fluidMotionDisabled = false;
            }

            if (Keyboard.HasBeenPressed(Keys.LeftShift) || VirtButton.HasButtonBeenPressed(VButName.DebugPause))
            {
                if (world == null) return;


                SonOfRobinGame.game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 3d);
                world.camera.fluidMotionDisabled = false;
            }

            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets))
            {
                if (world == null) return;

                var allSprites = world.grid.GetAllSprites(Cell.Group.ColMovement).Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();

                if (!allSprites.Any()) return;

                var index = BoardPiece.Random.Next(0, allSprites.Count);
                world.camera.TrackPiece(allSprites.ToArray()[index].boardPiece);
            }

            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) world.camera.TrackPiece(world.player);

            if (Keyboard.HasBeenPressed(Keys.X))
            {
                if (world == null) return;


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

            if (Keyboard.HasBeenPressed(Keys.Insert))
            {
                if (world == null) return;

                world.player.sprite.Visible = !world.player.sprite.Visible;
            }
            if (Keyboard.HasBeenPressed(Keys.Home)) Preferences.debugShowRects = !Preferences.debugShowRects;
            if (Keyboard.HasBeenPressed(Keys.PageUp) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftStick)) Preferences.debugShowStates = !Preferences.debugShowStates;
            if (Keyboard.HasBeenPressed(Keys.PageDown) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightStick)) Preferences.debugShowStatBars = !Preferences.debugShowStatBars;
            if (Keyboard.HasBeenPressed(Keys.End)) Preferences.debugShowCellData = !Preferences.debugShowCellData;
            if (Keyboard.HasBeenPressed(Keys.Delete)) Preferences.drawShadows = !Preferences.drawShadows;
        }
    }
}
