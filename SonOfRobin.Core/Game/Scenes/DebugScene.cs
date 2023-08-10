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
        public static readonly SpriteFont font = SonOfRobinGame.FontFreeSansBold10;
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

        public override void Update()
        {
            ProcessDebugInput();

            var debugLines = new List<string>();

            World world = World.GetTopWorld();

            bool worldActive = world != null && !world.WorldCreationInProgress;

            if (worldActive)
            {
                if (world.map.FullScreen) debugLines.Add($"objects {world.PieceCount}");
                else debugLines.Add($"{world.debugText}");
            }

            int plantCount = worldActive && world.pieceCountByClass.ContainsKey(typeof(Plant)) ? world.pieceCountByClass[typeof(Plant)] : 0;
            int animalCount = worldActive && world.pieceCountByClass.ContainsKey(typeof(Animal)) ? world.pieceCountByClass[typeof(Animal)] : 0;

            if (plantCount > 0 && animalCount > 0) debugLines.Add($"plants {plantCount}, animals {animalCount}");

            if (worldActive)
            {
                debugLines.Add($"proc. non-plants: {world.ProcessedNonPlantsCount} plants: {world.ProcessedPlantsCount}");
                debugLines.Add($"loaded textures {world.Grid.loadedTexturesCount}");
                debugLines.Add($"tracking count {world.trackingManager.TrackingCount} swayCount {world.swayManager.SwayEventsCount}");
                if (world.trackingManager.TrackingCount > 5000) debugLines.Add("WARNING, CHECK IF CORRECT!");
            }

            debugLines.Add($"snd inst. total: {ManagedSoundInstance.CreatedInstancesCount} act: {ManagedSoundInstance.ActiveInstancesCount} inact: {ManagedSoundInstance.InactiveInstancesCount}");

            debugLines.Add($"GC {GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)} worlds left {World.DestroyedNotReleasedWorldCount} last draw {LastDrawDuration.Milliseconds} last update {LastUpdateDuration.Milliseconds}");

            if (SonOfRobinGame.FreeRamMegabytesLeft >= 0) debugLines.Add($"ram free: {SonOfRobinGame.FreeRamMegabytesLeft}");

            if (worldActive)
            {
                string weatherText = "";
                foreach (var kvp in world.weather.GetIntensityForAllWeatherTypes())
                {
                    if (kvp.Value > 0) weatherText += $"{kvp.Key}: {Math.Round(kvp.Value, 2)} ";
                }
                if (weatherText.Length > 0) weatherText = $"weather {weatherText}";

                debugLines.Add($"heat {world.HeatQueueSize} {weatherText}");
                debugLines.Add($"real time elapsed {world.TimePlayed:hh\\:mm\\:ss}");
                debugLines.Add($"island time elapsed {world.islandClock.IslandTimeElapsed:hh\\:mm\\:ss} (x{world.updateMultiplier})");
                debugLines.Add($"island day {world.islandClock.CurrentDayNo} clock {world.islandClock.TimeOfDay:hh\\:mm\\:ss} ({Convert.ToString(world.islandClock.CurrentPartOfDay).ToLower()})");
                // debugLines.Add($"time until morning: {world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning).TotalMinutes}");
            }

            debugLines.Add($"rumble (events {RumbleManager.EventsCount}): BigMotor {Math.Round(RumbleManager.BigMotorCurrentForce, 1)} SmallMotor {Math.Round(RumbleManager.SmallMotorCurrentForce, 1)}");

            SimpleFps fps = SonOfRobinGame.fps;
            debugLines.Add($"FPS: {fps.FPS} updates: {fps.Updates} frames: {fps.Frames}");

            debugText = String.Join("\n", debugLines);

            Vector2 txtSize = font.MeasureString(debugText);
            this.viewParams.Width = (int)txtSize.X;
            this.viewParams.Height = (int)txtSize.Y;
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Helpers.DrawTextWithOutline(font: font, text: debugText, pos: Vector2.Zero, color: Color.White * this.viewParams.drawOpacity, outlineColor: Color.Black * this.viewParams.drawOpacity, outlineSize: 1);

            SonOfRobinGame.SpriteBatch.End();
        }

        public void ProcessDebugInput()
        {
            World world = World.GetTopWorld();

            // if (Mouse.LeftIsDown) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} left mouse button");
            // if (Mouse.MiddleIsDown) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} middle mouse button");
            // if (Mouse.RightIsDown) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} right mouse button");

            if (world != null)
            {
                if (Keyboard.HasBeenPressed(Keys.D1))
                {
                    var visiblePieces = world.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColMovement, compareWithCameraRect: true);

                    foreach (BoardPiece piece in visiblePieces)
                    {
                        ParticleEngine.TurnOn(sprite: piece.sprite, preset: ParticleEngine.Preset.DebrisSoot, duration: 1, update: true);
                    }
                }

                if (Keyboard.HasBeenPressed(Keys.D2))
                {
                    var visiblePieces = world.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColMovement, compareWithCameraRect: true);

                    foreach (BoardPiece piece in visiblePieces)
                    {
                        piece.pieceInfo.Yield?.DropDebris(piece: piece, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisSoot });
                    }
                }

                if (Keyboard.HasBeenPressed(Keys.D3))
                {
                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.Player.sprite.position, templateName: PieceTemplate.Name.ChestTreasureBig, closestFreeSpot: true);
                }

                if (Keyboard.HasBeenPressed(Keys.D4))
                {
                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.Player.sprite.position, templateName: PieceTemplate.Name.Coal, closestFreeSpot: true);
                }

                if (Keyboard.HasBeenPressed(Keys.D5))
                {
                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.Player.sprite.position, templateName: PieceTemplate.Name.BeltBig, closestFreeSpot: true);
                }

                if (Keyboard.HasBeenPressed(Keys.D6))
                {
                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.Player.sprite.position, templateName: PieceTemplate.Name.ArrowWood, closestFreeSpot: true);
                }

                if (Keyboard.HasBeenPressed(Keys.D7))
                {
                    var heart = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: world.Player.sprite.position, templateName: PieceTemplate.Name.Heart);
                    world.worldEventManager.RemovePieceFromQueue(heart);
                    heart.sprite.opacityFade = null;
                    heart.sprite.opacity = 1;
                    new Tracking(world: world, targetSprite: world.Player.sprite, followingSprite: heart.sprite, offsetX: 0, offsetY: 0, targetXAlign: XAlign.Right, targetYAlign: YAlign.Top, followingXAlign: XAlign.Left, followingYAlign: YAlign.Bottom, followSlowDown: 5);
                }

                if (Keyboard.HasBeenPressed(Keys.D9))
                {
                    world.CreateMissingPieces(initialCreation: false, maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f, clearDoNotCreateList: true);
                }

                if (Keyboard.HasBeenPressed(Keys.D0))
                {
                    world.CreateMissingPieces(initialCreation: true, outsideCamera: false, multiplier: 1.0f, clearDoNotCreateList: true);
                }

                if (Keyboard.HasBeenPressed(Keys.OemMinus))
                {
                    foreach (BoardPiece piece in world.Grid.GetPiecesInCameraView(groupName: Cell.Group.All))
                    {
                        if (piece != world.Player) Tool.HitTarget(attacker: world.Player, target: piece, hitPower: 99999, targetPushMultiplier: 1f);
                    }
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

                foreach (var sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                { if (sprite.boardPiece != world.Player) sprite.boardPiece.Kill(); }
            }

            if (Keyboard.HasBeenPressed(Keys.Y))
            {
                if (world == null) return;

                foreach (var sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                { sprite.AssignNewName("default"); }
            }

            if (Keyboard.HasBeenPressed(Keys.R))
            {
                if (world == null) return;

                while (true)
                {
                    bool hasBeenMoved = world.Player.sprite.SetNewPosition(new Vector2(BoardPiece.Random.Next(0, world.width), BoardPiece.Random.Next(0, world.height)));
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

                Player player = world.Player;
                player.Fatigue = player.maxFatigue;
            }

            if (Keyboard.HasBeenPressed(Keys.J))
            {
                if (world == null) return;

                Player player = world.Player;
                player.Fatigue = player.maxFatigue * 0.8f;
            }

            if (Keyboard.HasBeenPressed(Keys.O))
            {
                if (world == null) return;

                Player player = world.Player;
                player.sprite.RemoveFromBoard();
            }

            if (Keyboard.HasBeenPressed(Keys.K) || Keyboard.HasBeenPressed(Keys.L))
            {
                if (world == null) return;

                bool compareWithBottom = Keyboard.HasBeenPressed(Keys.K);

                var piecesWithinDistance = world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: world.Player.sprite, distance: 100, compareWithBottom: compareWithBottom);
                foreach (BoardPiece piece in piecesWithinDistance)
                {
                    if (world.Player.sprite.CheckIfOtherSpriteIsWithinRange(target: piece.sprite))
                    { PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Heart); }
                    else
                    { PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight); }
                }
                BoardPiece closestPiece = BoardPiece.FindClosestPiece(sprite: world.Player.sprite, pieceList: piecesWithinDistance);

                if (closestPiece != null)
                {
                    BoardPiece backlight = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: closestPiece.sprite.position, templateName: PieceTemplate.Name.Backlight);
                    backlight.sprite.color = new Color(0, 128, 128);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.OemComma))
            {
                if (world == null) return;
                Player player = world.Player;

                Point point1 = new Point((int)player.sprite.position.X, (int)player.sprite.position.Y);
                Point point2 = new Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y - 100);
                Point point3 = new Point((int)player.sprite.position.X + 200, (int)player.sprite.position.Y + 100);

                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point1.X, point1.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point2.X, point2.Y), templateName: PieceTemplate.Name.Heart);
                PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(point3.X, point3.Y), templateName: PieceTemplate.Name.Heart);

                var piecesInsideTriangle = world.Grid.GetPiecesInsideTriangle(groupName: Cell.Group.All, point1: point1, point2: point2, point3: point3);
                foreach (BoardPiece piece in piecesInsideTriangle)
                {
                    piece.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: piece.sprite.AnimFrame.textureSize, priority: 0, framesLeft: 60));
                    PieceTemplate.CreateAndPlaceOnBoard(world: world, position: piece.sprite.position, templateName: PieceTemplate.Name.Backlight);
                }
            }

            if (Keyboard.HasBeenPressed(Keys.A) || VirtButton.HasButtonBeenPressed(VButName.DebugBreakAll))
            {
                if (world == null) return;

                foreach (var sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                {
                    if (sprite.boardPiece != world.Player)
                    {
                        BoardPiece boardPiece = sprite.boardPiece;

                        if (boardPiece != null && boardPiece.exists && boardPiece.pieceInfo.Yield != null) boardPiece.pieceInfo.Yield.DropFinalPieces(piece: boardPiece);
                        boardPiece.Destroy();
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Z) || VirtButton.HasButtonBeenPressed(VButName.DebugBreakVisible))
            {
                if (world == null) return;

                foreach (BoardPiece piece in world.Grid.GetPiecesInCameraView(groupName: Cell.Group.All))
                {
                    if (piece != world.Player)
                    {
                        if (piece != null && piece.exists && piece.pieceInfo.Yield != null) piece.pieceInfo.Yield.DropFinalPieces(piece: piece);
                        piece.Destroy();
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Q) || VirtButton.HasButtonBeenPressed(VButName.DebugClear))
            {
                if (world == null) return;

                foreach (var sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                {
                    if (sprite.boardPiece != world.Player) sprite.boardPiece.Destroy();
                }
            }

            if (Keyboard.HasBeenPressed(Keys.W))
            {
                if (world == null) return;

                foreach (var sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                { if (sprite.boardPiece != world.Player) sprite.boardPiece.RemoveFromStateMachines(); }
            }

            //if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "Line 1\nLine 2\nThis is button A | and button B |.\nBelt here >|<\nLast line.", animate: false, useTransition: false, bgColor: Color.DeepSkyBlue, textColor: Color.White, imageList: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, AnimData.framesForPkgs[AnimData.PkgName.BeltMedium].texture });

            //if (Keyboard.HasBeenPressed(Keys.F1)) new TextWindow(text: "If I had more | leather, I could make a | backpack or a | belt.\n>|1|2|3|4|5<", animate: true, useTransition: false, framesPerChar: 1, bgColor: Color.DeepSkyBlue, textColor: Color.White, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig), PieceInfo.GetTexture(PieceTemplate.Name.BeltBig) });

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    if (world == null) return;

            //    world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CineSmallBase, ignoreDelay: true);
            //    world.HintEngine.ShowGeneralHint(type: HintEngine.Type.Lava, ignoreDelay: true);
            //}

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    if (world == null) return;

            //    world.camera.AddRandomShake();
            //}

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    new RumbleEvent(force: 0.35f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.6f);
            //}

            if (Keyboard.HasBeenPressed(Keys.F1))
            {
                if (world == null) return;

                ParticleEngine.TurnOn(sprite: world.Player.sprite, preset: ParticleEngine.Preset.BloodDripping, duration: 1);
            }

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    if (world == null) return;
            //    world.SpectatorMode = !world.SpectatorMode;
            //}

            //if (Keyboard.HasBeenPressed(Keys.F2))
            //{
            //    if (world == null) return;

            //    new TextWindow(text: "This is a test message.\nLong test message.\nEven longer test message.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, priority: 0, animSound: world.DialogueSound);
            //}

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    float percentage = Math.Min(SonOfRobinGame.FullScreenProgressBar.ProgressBarPercentage + 0.05f, 1f);
            //    if (percentage == 1) percentage = 0.005f;
            //    SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: LoadingTips.GetTip());
            //}

            //if (Keyboard.HasBeenPressed(Keys.F1))
            //{
            //    if (world == null) return;

            //    world.weather.AddEvent(new WeatherEvent(type: Weather.WeatherType.Lightning, intensity: 1f, startTime: world.islandClock.IslandDateTime, duration: TimeSpan.FromSeconds(50), transitionLength: TimeSpan.FromSeconds(22)));

            //    world.weather.AddEvent(new WeatherEvent(type: Weather.WeatherType.Clouds, intensity: 1f, startTime: world.islandClock.IslandDateTime, duration: TimeSpan.FromSeconds(100), transitionLength: TimeSpan.FromSeconds(5)));
            //}

            //if (Keyboard.HasBeenPressed(Keys.F2))
            //{
            //    if (world == null) return;

            //    world.weather.AddEvent(new WeatherEvent(type: Weather.WeatherType.Rain, intensity: 0.05f, startTime: world.islandClock.IslandDateTime, duration: TimeSpan.FromMinutes(30), transitionLength: TimeSpan.FromMinutes(3)));
            //}

            if (Keyboard.HasBeenPressed(Keys.F2))
            {
                if (world == null) return;

                world.weather.AddEvent(new WeatherEvent(type: Weather.WeatherType.Lightning, intensity: 1.0f, startTime: world.islandClock.IslandDateTime, duration: TimeSpan.FromMinutes(2), transitionLength: TimeSpan.FromMinutes(0.3f)));
            }

            //if (Keyboard.HasBeenPressed(Keys.F3))
            //{
            //    if (world == null) return;

            //    world.weather.AddEvent(new WeatherEvent(type: Weather.WeatherType.Lightning, intensity: 1f, startTime: world.islandClock.IslandDateTime, duration: TimeSpan.FromMinutes(4), transitionLength: TimeSpan.FromMinutes(1f)));
            //} 
            
            
            if (Keyboard.HasBeenPressed(Keys.F3))
            {
                if (world == null) return;

                new WorldEvent(eventName: WorldEvent.EventName.RestorePieceCreation, delay: 5 * 60, world: world, boardPiece: null, eventHelper: PieceTemplate.Name.CoalDeposit);
            }

            //if (Keyboard.HasBeenPressed(Keys.F2))
            //{
            //    // a shockwave

            //    if (world == null) return;

            //    Vector2 windOriginLocation = world.Player.sprite.position;

            //    var plantSpriteList = new List<Sprite>();
            //    world.Grid.GetSpritesInCameraViewAndPutIntoList(camera: world.camera, groupName: Cell.Group.ColPlantGrowth, spriteListToFill: plantSpriteList);

            //    foreach (Sprite sprite in plantSpriteList)
            //    {
            //        if (!sprite.boardPiece.canBePickedUp)
            //        {
            //            float distance = Vector2.Distance(windOriginLocation, sprite.position);

            //            world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: (sprite.position - windOriginLocation).X > 0 ? 0.25f : -0.25f, playSound: false, delayFrames: (int)distance / 20);
            //        }
            //    }
            //}

            //if (Keyboard.HasBeenPressed(Keys.F2))
            //{
            //    float percentage = Math.Min(SonOfRobinGame.FullScreenProgressBar.ProgressBarPercentage + 0.05f, 1f);
            //    SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: LoadingTips.GetTip(), optionalText: "loading game...");
            //}

            //if (Keyboard.HasBeenPressed(Keys.F2)) SonOfRobinGame.FullScreenProgressBar.TurnOff();

            if (Keyboard.HasBeenPressed(Keys.F4) || VirtButton.HasButtonBeenPressed(VButName.DebugClockAdvance))
            {
                if (world == null) return;

                world.islandClock.Advance(amount: 30 * 60 * 1, ignorePause: true);
            }

            //if (Keyboard.HasBeenPressed(Keys.F4))
            //{
            //    string sourcePngName = "upscale_test_small.png";
            //    string targetPngName = "upscale_test_big.png";

            //    Texture2D textureToUpscale = GfxConverter.LoadTextureFromPNG(Path.Combine(SonOfRobinGame.gameDataPath, sourcePngName));

            //    if (textureToUpscale == null)
            //    {
            //        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Source filename {sourcePngName} not found.");
            //        return;
            //    }

            //    Texture2D upscaledTexture = BoardTextureUpscaler3x.UpscaleTexture(textureToUpscale);

            //    GfxConverter.SaveTextureAsPNG(filename: Path.Combine(SonOfRobinGame.gameDataPath, targetPngName), upscaledTexture);

            //    new TextWindow(text: "Original vs upscaled: | |", imageList: new List<Texture2D> { textureToUpscale, upscaledTexture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false);
            //}

            if (Keyboard.HasBeenPressed(Keys.F5) || VirtButton.HasButtonBeenPressed(VButName.DebugClockAdvance))
            {
                if (world == null) return;

                for (int i = 0; i < 1000; i++)
                {
                    world.islandClock.Advance(amount: 60 * 60 * 5, ignorePause: true);
                    world.weather.Update();
                    if (world.weather.RainPercentage > 0.7f) return;
                }

                MessageLog.AddMessage(msgType: MsgType.Debug, message: "Weather checking timed out.");
            }

            //if (Keyboard.HasBeenPressed(Keys.F5))
            //{
            //new TextWindow(text: "Test 1", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1);

            //new TextWindow(text: "Test 2.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1);

            //new TextWindow(text: "Text window 1", animate: false, useTransition: true, bgColor: Color.DeepSkyBlue, textColor: Color.White, startingSound: SoundData.Name.Tick);
            //new TextWindow(text: "Text window 2", animate: false, useTransition: true, bgColor: Color.DeepSkyBlue, textColor: Color.White, startingSound: SoundData.Name.ChestOpen);
            //new TextWindow(text: "Text window 3", animate: false, useTransition: true, bgColor: Color.DeepSkyBlue, textColor: Color.White, startingSound: SoundData.Name.ChestClose);
            //}

            if (Keyboard.HasBeenPressed(Keys.F6))
            {
                ManagedSoundInstance.PauseAll();
            }

            if (Keyboard.HasBeenPressed(Keys.F7))
            {
                ManagedSoundInstance.ResumeAll();
            }

            if (Keyboard.HasBeenPressed(Keys.OemPlus))
            {
                if (world == null) return;

                world.HintEngine.shownTutorials = Enum.GetValues(typeof(Tutorials.Type)).Cast<Tutorials.Type>().ToList();
            }

            if (Keyboard.HasBeenPressed(Keys.F8))
            {
                if (world == null) return;

                // int value = world.random.Next(-30, 30);
                // world.player.buffEngine.AddBuff(world: world, buff: new Buff(type: BuffEngine.BuffType.Strength, value: value, autoRemoveDelay: world.random.Next(100, 500), isPositive: value > 0));

                // world.player.buffEngine.AddBuff(world: world, buff: new Buff(type: BuffEngine.BuffType.RegenPoison, value: value, autoRemoveDelay: world.random.Next(600, 1200), canKill: true));

                //world.Player.buffEngine.AddBuff(world: world, buff: new Buff(type: BuffEngine.BuffType.Haste, value: 2, autoRemoveDelay: 300));

                world.Player.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.MaxFatigue, autoRemoveDelay: 4 * 60, value: 600f), world: world);
            }

            if (Keyboard.HasBeenPressed(Keys.F9))
            {
                SonOfRobinGame.SmallProgressBar.TurnOn(newPosX: 0, newPosY: 0, centerHoriz: true, centerVert: true, addTransition: true, entryList: new List<InfoWindow.TextEntry> {
                        new InfoWindow.TextEntry(text: "| First line.", color: Color.White, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit) }),
                        new InfoWindow.TextEntry(text: "| Second line.", color: Color.Green, scale: 1.5f, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit) }, justify: InfoWindow.TextEntry.Justify.Center),
                        new InfoWindow.TextEntry(color: Color.White, progressCurrentVal: 2, progressMaxVal: 5),
                        new InfoWindow.TextEntry(text: "| And this is fourth line.", color: Color.LightBlue, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit) }),
                        new InfoWindow.TextEntry(text: "| This window should be centered.", color: Color.YellowGreen, imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit) }, justify: InfoWindow.TextEntry.Justify.Right),
                    });
            }

            if (Keyboard.HasBeenPressed(Keys.F10))
            {
                SonOfRobinGame.SmallProgressBar.TurnOn(newPosX: 0, newPosY: 0,
                    entryList: new List<InfoWindow.TextEntry> {
                        new InfoWindow.TextEntry(text: "Second line.", color: Color.Green),
                        new InfoWindow.TextEntry(text: "And this is third line.", color: Color.LightBlue),
                        new InfoWindow.TextEntry(text: "This window should be at 0,0.", color: Color.Yellow),
                });
            }

            if (Keyboard.HasBeenPressed(Keys.F11)) SonOfRobinGame.SmallProgressBar.TurnOff();

            if (Keyboard.HasBeenPressed(Keys.F12)) RemoveTopScene();

            if (Keyboard.HasBeenPressed(Keys.LeftAlt) || VirtButton.HasButtonBeenPressed(VButName.DebugFastForward))
            {
                if (world == null) return;

                if (SonOfRobinGame.Game.IsFixedTimeStep)
                {  // at first, only IsFixedTimeStep should be changed
                    SonOfRobinGame.Game.IsFixedTimeStep = false;
                }
                else world.updateMultiplier *= 2;

                SonOfRobinGame.Game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            }

            if (Keyboard.HasBeenPressed(Keys.LeftControl) || VirtButton.HasButtonBeenPressed(VButName.DebugPlay))
            {
                if (world == null) return;

                SonOfRobinGame.Game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.Game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            }

            if (Keyboard.HasBeenPressed(Keys.LeftShift) || VirtButton.HasButtonBeenPressed(VButName.DebugPause))
            {
                if (world == null) return;

                SonOfRobinGame.Game.IsFixedTimeStep = true;
                world.updateMultiplier = 1;
                SonOfRobinGame.Game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 3d);
            }

            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets))
            {
                if (world == null) return;

                var allSprites = world.Grid.GetSpritesFromAllCells(Cell.Group.ColMovement).Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();

                if (!allSprites.Any()) return;

                var index = BoardPiece.Random.Next(0, allSprites.Count);
                world.camera.TrackPiece(trackedPiece: allSprites.ToArray()[index].boardPiece, moveInstantly: true);
            }

            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) world.camera.TrackPiece(trackedPiece: world.Player, moveInstantly: true);

            if (Keyboard.HasBeenPressed(Keys.X))
            {
                if (world == null) return;

                AnimData.PkgName currentPackageName = world.Player.sprite.AnimPackage;

                while (true)
                {
                    var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.PlayerBoy, AnimData.PkgName.PlayerGirl, AnimData.PkgName.FoxGinger, AnimData.PkgName.Frog1, AnimData.PkgName.CrabGreen, AnimData.PkgName.TigerWhite };
                    var packageName = packageNames[BoardPiece.Random.Next(0, packageNames.Count)];
                    if (packageName != currentPackageName)
                    {
                        world.Player.sprite.AssignNewPackage(newAnimPackage: packageName, setEvenIfMissing: false);
                        world.Player.sprite.AssignNewSize(1);
                        break;
                    }
                }
            }

            if (Keyboard.HasBeenPressed(Keys.Insert))
            {
                if (world == null) return;

                world.Player.sprite.Visible = !world.Player.sprite.Visible;
            }
            if (Keyboard.HasBeenPressed(Keys.Home)) Preferences.debugShowRects = !Preferences.debugShowRects;
            if (Keyboard.HasBeenPressed(Keys.PageUp) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftStick)) Preferences.debugShowStates = !Preferences.debugShowStates;
            if (Keyboard.HasBeenPressed(Keys.PageDown) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightStick)) Preferences.debugShowStatBars = !Preferences.debugShowStatBars;
            if (Keyboard.HasBeenPressed(Keys.End)) Preferences.debugShowCellData = !Preferences.debugShowCellData;
            if (Keyboard.HasBeenPressed(Keys.Delete)) Preferences.drawShadows = !Preferences.drawShadows;
        }
    }
}