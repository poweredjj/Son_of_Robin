using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum TaskName { Empty, CreateNewWorld, CreateNewWorldNow, QuitGame, OpenMenuTemplate, OpenMainMenu, OpenConfirmationMenu, SaveGame, LoadGame, LoadGameNow, ReturnToMainMenu, SavePrefs, ProcessConfirmation, OpenCraftMenu, CraftOnPosition, Craft, Hit, CreateNewPiece, CreateDebugPieces, OpenContainer, DeleteObsoleteSaves, DropFruit, GetEaten, GetDrinked, ExecuteTaskWithDelay, AddWorldEvent, ShowTextWindow, OpenShelterMenu, SleepInsideShelter, SleepOutside, ForceWakeUp, TempoFastForward, TempoStop, TempoPlay, CameraTrackPiece, CameraTrackCoords, CameraSetZoom, ShowCookingProgress, RestoreHints, OpenMainMenuIfSpecialKeysArePressed, CheckForPieceHints, ShowHint, ExecuteTaskList, ExecuteTaskChain, ShowTutorialInMenu, ShowTutorialOnTheField, RemoveScene, ChangeSceneInputType, SetCineMode, AddTransition, SolidColorRemoveAll, SkipCinematics, DeleteTemplates, SetSpectatorMode, SwitchLightSource, ResetControls, SaveControls, CheckForNonSavedControls, RebuildMenu, RebuildAllMenus, CheckForIncorrectPieces, RestartWorld, ResetNewWorldSettings, PlaySound, PlaySoundByName, AllowPieceToBeHit, SetPlayerPointWalkTarget, ShowCraftStats }

        private readonly static Dictionary<int, List<Task>> queue = new Dictionary<int, List<Task>>();
        private static int inputTurnedOffUntilFrame = 0;

        public static void ProcessQueue()
        {
            if (SonOfRobinGame.currentUpdate >= inputTurnedOffUntilFrame) Input.GlobalInputActive = true;

            var framesToProcess = queue.Keys.Where(frameNo => SonOfRobinGame.currentUpdate >= frameNo).ToList();
            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                foreach (Task task in queue[frameNo]) // if System.InvalidOperationException occurs, most likely some task has been added to current frame queue
                { task.Execute(); }

                queue.Remove(frameNo);
            }
        }

        public static void RemoveAllTasksOfName(TaskName taskName)
        {
            foreach (int frameNo in queue.Keys.ToList())
            {
                queue[frameNo] = queue[frameNo].Where(task => task.taskName != taskName).ToList();
            }
        }

        public static void ClearQueue()
        {
            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Clearing Scheduler queue.");
            queue.Clear();
        }

        public struct Task
        {
            public readonly TaskName taskName;
            private Object executeHelper;
            private readonly int delay;
            private int frame;
            private readonly bool turnOffInputUntilExecution;

            public string TaskText
            {
                get
                {
                    string turnOffInputText = this.turnOffInputUntilExecution ? "(input off)" : "";
                    return $"{this.taskName} {turnOffInputText}";
                }
            }

            public Task(TaskName taskName, Object executeHelper = null, bool turnOffInputUntilExecution = false, int delay = 0, bool storeForLaterUse = false)
            {
                this.taskName = taskName;
                this.executeHelper = executeHelper;
                this.turnOffInputUntilExecution = turnOffInputUntilExecution;
                this.delay = delay;

                if (!storeForLaterUse)
                {
                    this.frame = SonOfRobinGame.currentUpdate + this.delay;

                    if (this.turnOffInputUntilExecution) this.TurnOffInput();

                    this.Process();
                }
                else this.frame = -1;
            }

            private void TurnOffInput()
            {
                Input.GlobalInputActive = false;
                inputTurnedOffUntilFrame = Math.Max(this.frame, inputTurnedOffUntilFrame);
            }

            public void Process()
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} processing task {this.TaskText}", color: Color.LightYellow);

                if (this.delay <= 0) this.Execute();
                else this.AddToQueue();
            }

            private void AddToQueue()
            {
                if (this.delay == 0) throw new ArgumentException($"Tried to add task with delay {this.delay} to queue.");

                //  MessageLog.AddMessage(msgType: MsgType.User, message: $"Adding to queue '{this.taskName}' - delay {this.delay}.", color: Color.White); // for testing

                if (this.turnOffInputUntilExecution) this.TurnOffInput();
                if (this.frame == -1) this.frame = SonOfRobinGame.currentUpdate + this.delay;
                if (!queue.ContainsKey(this.frame)) queue[this.frame] = new List<Task>();
                queue[this.frame].Add(this);
            }

            public void Execute()
            {
                World world;

                switch (taskName)
                {
                    case TaskName.Empty:
                        return;

                    case TaskName.RebuildMenu:
                        {
                            Menu menu = (Menu)executeHelper;
                            menu.Rebuild(instantScroll: false);
                            return;
                        }

                    case TaskName.RebuildAllMenus:
                        {
                            Menu.RebuildAllMenus();
                            return;
                        }

                    case TaskName.OpenMenuTemplate:
                        {
                            // example executeHelper for this task
                            // var menuTemplateData = new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.Main }, { "scenesToLink", scenesToLink } };

                            var menuTemplateData = (Dictionary<string, Object>)executeHelper;
                            MenuTemplate.Name templateName = (MenuTemplate.Name)menuTemplateData["templateName"];

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: templateName);

                            if (menuTemplateData.ContainsKey("scenesToLink"))
                            {
                                List<Scene> scenesToLink = (List<Scene>)menuTemplateData["scenesToLink"];
                                menu.AddLinkedScenes(scenesToLink);
                            }

                            if (templateName == MenuTemplate.Name.GameOver) Scene.RemoveAllScenesOfType(typeof(TextWindow));

                            return;
                        }

                    case TaskName.OpenMainMenu:
                        {
                            new Task(taskName: TaskName.OpenMenuTemplate, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.Main } });

                            return;
                        }

                    case TaskName.OpenCraftMenu:
                        {
                            Workshop workshop = (Workshop)executeHelper;

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);
                            if (menu == null) return; // if crafting was impossible at the moment

                            workshop.TurnOn();

                            var worldEventData = new Dictionary<string, object> {
                                {"boardPiece", workshop },
                                {"delay", 300 },
                                {"eventName", WorldEvent.EventName.TurnOffWorkshop },
                            };

                            menu.AddClosingTask(closingTask: TaskName.AddWorldEvent, closingTaskHelper: worldEventData);
                            return;
                        }

                    case TaskName.OpenConfirmationMenu:
                        {
                            MenuTemplate.CreateConfirmationMenu(confirmationData: executeHelper);
                            return;
                        }

                    case TaskName.ProcessConfirmation:
                        {
                            var confirmationData = (Dictionary<string, Object>)executeHelper;
                            new Task(taskName: (TaskName)confirmationData["taskName"], executeHelper: confirmationData["executeHelper"]);

                            return;
                        }

                    case TaskName.CreateNewWorld:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            new Task(taskName: TaskName.CreateNewWorldNow, turnOffInputUntilExecution: true, delay: 13, executeHelper: executeHelper);

                            return;
                        }

                    case TaskName.CreateNewWorldNow:
                        {
                            // example executeHelper for this task
                            // var createData = new Dictionary<string, Object> {{ "width", width }, { "height", height }, { "seed", seed }, {"resDivider", resDivider }, {"initialMaxAnimalsMultiplier", initialMaxAnimalsMultiplier}, {"addAgressiveAnimals", true }};                

                            int width, height, seed, resDivider, initialMaxAnimalsMultiplier;
                            bool addAgressiveAnimals, playerFemale;

                            if (executeHelper == null)
                            {
                                width = Preferences.newWorldWidth;
                                height = Preferences.newWorldHeight;
                                seed = Preferences.NewWorldSeed;
                                resDivider = Preferences.newWorldResDivider;
                                initialMaxAnimalsMultiplier = Preferences.newWorldMaxAnimalsMultiplier;
                                addAgressiveAnimals = Preferences.newWorldAgressiveAnimals;
                                playerFemale = Preferences.newWorldPlayerFemale;
                            }
                            else
                            {
                                var createData = (Dictionary<string, Object>)executeHelper;
                                width = (int)createData["width"];
                                height = (int)createData["height"];
                                seed = (int)createData["seed"];
                                resDivider = (int)createData["resDivider"];
                                initialMaxAnimalsMultiplier = (int)createData["initialMaxAnimalsMultiplier"];
                                addAgressiveAnimals = (bool)createData["addAgressiveAnimals"];
                                playerFemale = (bool)createData["playerFemale"];
                            }

                            new World(width: width, height: height, seed: seed, resDivider: resDivider, playerFemale: playerFemale, initialMaxAnimalsMultiplier: initialMaxAnimalsMultiplier, addAgressiveAnimals: addAgressiveAnimals);

                            return;
                        }

                    case TaskName.RestartWorld:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            World oldWorld = (World)executeHelper;
                            bool playerFemale = oldWorld.playerFemale;

                            new World(width: oldWorld.width, height: oldWorld.height, seed: oldWorld.seed, playerFemale: playerFemale, resDivider: oldWorld.resDivider, initialMaxAnimalsMultiplier: oldWorld.initialMaxAnimalsMultiplier, addAgressiveAnimals: oldWorld.addAgressiveAnimals);
                            oldWorld.Remove();

                            return;
                        }

                    case TaskName.ResetNewWorldSettings:
                        {
                            Preferences.ResetNewWorldSettings();
                            return;
                        }

                    case TaskName.CreateNewPiece:
                        {
                            world = World.GetTopWorld();
                            if (world != null)
                            {
                                var executeData = (Dictionary<string, Object>)executeHelper;
                                Vector2 position = (Vector2)executeData["position"];
                                PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];
                                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: position, templateName: templateName);
                                if (piece.sprite.IsOnBoard) piece.sprite.MoveToClosestFreeSpot(position);
                            }
                            return;
                        }

                    case TaskName.CreateDebugPieces:
                        {
                            world = World.GetTopWorld();
                            if (world == null)
                            {
                                MessageLog.AddMessage(msgType: MsgType.User, message: "Could not create selected item, because no world was found.");
                                return;
                            }

                            Player player = world.player;

                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Vector2 position = (Vector2)executeData["position"];
                            PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];

                            int piecesCreated = 0;
                            int attemptNo = 0;

                            while (true)
                            {
                                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: position, templateName: templateName, closestFreeSpot: true);
                                int amountToCreate = piece.stackSize;

                                if (piece.sprite.IsOnBoard)
                                {
                                    player.PickUpPiece(piece);
                                    piecesCreated++;
                                }

                                if (piecesCreated >= amountToCreate) break;

                                attemptNo++;
                                if (attemptNo == 1000)
                                {
                                    MessageLog.AddMessage(msgType: MsgType.User, message: $"Max number of attempts exceeded while trying to create '{templateName}'.");
                                    break;
                                }
                            }

                            MessageLog.AddMessage(msgType: MsgType.User, message: $"{piecesCreated} '{templateName}' pieces created.");

                            return;
                        }

                    case TaskName.SaveGame:
                        {
                            // example executeHelper for this task
                            // var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "1" }, { "showMessage", false }, {"quitGameAfterSaving", false} };

                            var saveParams = (Dictionary<string, Object>)executeHelper;
                            world = (World)saveParams["world"];
                            string saveSlotName = (string)saveParams["saveSlotName"];
                            bool showMessage = false;
                            if (saveParams.ContainsKey("showMessage")) showMessage = (bool)saveParams["showMessage"];
                            bool quitGameAfterSaving = false;
                            if (saveParams.ContainsKey("quitGameAfterSaving")) quitGameAfterSaving = (bool)saveParams["quitGameAfterSaving"];

                            new LoaderSaver(saveMode: true, saveSlotName: saveSlotName, world: world, showSavedMessage: showMessage, quitGameAfterSaving: quitGameAfterSaving);

                            return;
                        }

                    case TaskName.LoadGame:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            Scene.RemoveAllScenesOfType(typeof(TextWindow));

                            new Task(taskName: TaskName.LoadGameNow, turnOffInputUntilExecution: true, delay: 17, executeHelper: this.executeHelper);

                            return;
                        }

                    case TaskName.LoadGameNow:
                        {
                            new LoaderSaver(saveMode: false, saveSlotName: (string)executeHelper);

                            return;
                        }

                    case TaskName.SavePrefs:
                        {
                            Preferences.Save();
                            return;
                        }

                    case TaskName.OpenContainer:
                        {
                            BoardPiece container = (BoardPiece)executeHelper;

                            world = World.GetTopWorld();
                            if (world == null) return;

                            if (container.GetType() == typeof(Cooker))
                            {
                                if (world.player.AreEnemiesNearby && !world.player.IsActiveFireplaceNearby)
                                {
                                    new TextWindow(text: "I can't cook with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: TaskName.ShowTutorialOnTheField, closingTaskHelper: new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreDelay", true } });
                                    return;
                                }

                                if (world.player.IsVeryTired)
                                {
                                    new TextWindow(text: "I'm too tired to cook...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
                                    return;
                                }
                            }

                            Inventory.SetLayout(newLayout: Inventory.Layout.InventoryAndChest, player: world.player, chest: container);
                        }

                        return;

                    case TaskName.Craft:
                        {
                            var recipe = (Craft.Recipe)executeHelper;
                            recipe.TryToProducePieces(player: World.GetTopWorld().player, showMessages: true);
                        }
                        return;


                    case TaskName.Hit:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            Tool activeTool = (Tool)executeData["toolbarPiece"];
                            int shootingPower = (int)executeData["shootingPower"];
                            int offsetX = (int)executeData["offsetX"];
                            int offsetY = (int)executeData["offsetY"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];
                            world = player.world;

                            if (activeTool.shootsProjectile)
                            {
                                activeTool.Use(shootingPower: shootingPower, targets: null);
                                return;
                            }

                            var targets = new List<BoardPiece> { };

                            var nearbyPieces = world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: activeTool.range == 0 ? (ushort)60 : (ushort)activeTool.range, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);

                            nearbyPieces = nearbyPieces.Where(piece => piece.yield != null && piece.exists).ToList();

                            if (activeTool.range == 0)
                            {
                                var animals = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal)).ToList();
                                if (animals.Count > 0) nearbyPieces = animals;

                                BoardPiece targetPiece = BoardPiece.FindClosestPiece(sprite: player.sprite, pieceList: nearbyPieces, offsetX: offsetX, offsetY: offsetY);
                                if (targetPiece == null) return;
                                targets.Add(targetPiece);
                            }
                            else
                            {
                                Rectangle areaRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);

                                switch (player.sprite.orientation)
                                {
                                    case Sprite.Orientation.left:
                                        areaRect.Width = player.sprite.gfxRect.Left;
                                        break;

                                    case Sprite.Orientation.right:
                                        areaRect.X = player.sprite.gfxRect.Center.X;
                                        areaRect.Width = world.width - player.sprite.gfxRect.Center.X;
                                        break;

                                    case Sprite.Orientation.up:
                                        areaRect.Height = player.sprite.gfxRect.Center.Y;
                                        break;

                                    case Sprite.Orientation.down:
                                        areaRect.Y = player.sprite.gfxRect.Center.Y;
                                        areaRect.Height = world.height - player.sprite.gfxRect.Center.Y;
                                        break;

                                    default:
                                        throw new ArgumentException($"Unsupported orientation - {player.sprite.orientation}.");
                                }

                                targets = nearbyPieces.Where(piece => areaRect.Contains(piece.sprite.position)).ToList();
                            }

                            activeTool.Use(shootingPower: shootingPower, targets: targets, highlightOnly: highlightOnly);

                            return;
                        }

                    case TaskName.GetEaten:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            BoardPiece food = (BoardPiece)executeData["toolbarPiece"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (player.fedLevel >= player.maxFedLevel * 0.95f)
                            {
                                if (!highlightOnly) new TextWindow(text: "I am full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

                                return;
                            }

                            if (highlightOnly)
                            {
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            world = World.GetTopWorld();

                            player.soundPack.Play(action: PieceSoundPack.Action.Eat, ignore3D: true);

                            // adding "generic" regen based on mass (if meal does not contain poison or regen)
                            if (!BuffEngine.BuffListContainsPoisonOrRegen(food.buffList)) food.buffList.Add(new BuffEngine.Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)(food.Mass / 3), autoRemoveDelay: 60 * 60));
                            player.AcquireEnergy(food.Mass * 40f);

                            foreach (BuffEngine.Buff buff in food.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: world); }

                            food.hitPoints = 0;
                        }
                        return;

                    case TaskName.GetDrinked:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            Potion potion = (Potion)executeData["toolbarPiece"];
                            StorageSlot slot = (StorageSlot)executeData["slot"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (highlightOnly)
                            {
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            world = World.GetTopWorld();

                            Sound.QuickPlay(SoundData.Name.Drink);

                            // all parameters should be increased using buffs

                            foreach (BuffEngine.Buff buff in potion.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: world); }

                            BoardPiece emptyContainter = PieceTemplate.Create(templateName: potion.convertsToWhenUsed, world: world);
                            slot.DestroyPieceAndReplaceWithAnother(emptyContainter);

                            player.CheckLowHP();

                            return;
                        }

                    case TaskName.SwitchLightSource:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            PortableLight portableLight = (PortableLight)executeData["toolbarPiece"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];

                            if (highlightOnly)
                            {
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            bool buttonHeld = (bool)executeData["buttonHeld"];
                            if (buttonHeld) return;

                            portableLight.IsOn = !portableLight.IsOn;

                            return;
                        }

                    case TaskName.ResetControls:
                        {
                            // example executeHelper for this task
                            // var resetData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "useDefault", true } };

                            var resetData = (Dictionary<string, Object>)executeHelper;
                            bool gamepad = (bool)resetData["gamepad"];
                            bool useDefault = (bool)resetData["useDefault"];
                            bool showMessage = resetData.ContainsKey("showMessage") ? (bool)resetData["showMessage"] : true;

                            if (gamepad)
                            {
                                InputPackage sourcePackage = useDefault ? InputMapper.defaultMappingGamepad : InputMapper.currentMappingGamepad;

                                InputMapper.currentMappingGamepad = sourcePackage.MakeCopy();
                                InputMapper.newMappingGamepad = sourcePackage.MakeCopy();
                            }
                            else
                            {
                                InputPackage sourcePackage = useDefault ? InputMapper.defaultMappingKeyboard : InputMapper.currentMappingKeyboard;

                                InputMapper.currentMappingKeyboard = sourcePackage.MakeCopy();
                                InputMapper.newMappingKeyboard = sourcePackage.MakeCopy();
                            }

                            InputMapper.RebuildMappings();
                            Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh tutorials, etc.
                            new Task(taskName: TaskName.SavePrefs);

                            if (showMessage)
                            {
                                string type = gamepad ? "Gamepad" : "Keyboard";
                                new TextWindow(text: $"{type} controls has been reset.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false);
                            }

                            return;
                        }

                    case TaskName.SaveControls:
                        {
                            // example executeHelper for this task
                            // var saveData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "openMenuIfNotValid", true } };

                            var saveData = (Dictionary<string, Object>)executeHelper;
                            bool gamepad = (bool)saveData["gamepad"];
                            bool openMenuIfNotValid = (bool)saveData["openMenuIfNotValid"];

                            InputPackage newMapping = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;

                            bool isValid = newMapping.Validate(gamepad: gamepad);
                            if (!isValid)
                            {
                                if (openMenuIfNotValid) MenuTemplate.CreateMenuFromTemplate(templateName: gamepad ? MenuTemplate.Name.Gamepad : MenuTemplate.Name.Keyboard);
                                return;
                            }

                            if (gamepad) InputMapper.currentMappingGamepad = InputMapper.newMappingGamepad.MakeCopy();
                            else InputMapper.currentMappingKeyboard = InputMapper.newMappingKeyboard.MakeCopy();

                            InputMapper.RebuildMappings();
                            Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh tutorials, etc.
                            new Task(taskName: TaskName.SavePrefs);

                            string type = gamepad ? "Gamepad" : "Keyboard";
                            new TextWindow(text: $"{type} controls has been saved.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false);
                            Sound.QuickPlay(name: SoundData.Name.Ding2, volume: 1f);

                            return;
                        }

                    case TaskName.CheckForNonSavedControls:
                        {
                            var gamepad = (bool)executeHelper;

                            InputPackage newControls = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;
                            InputPackage currentControls = gamepad ? InputMapper.currentMappingGamepad : InputMapper.currentMappingKeyboard;

                            if (!currentControls.Equals(newControls))
                            {
                                var optionList = new List<object>();

                                var saveData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "openMenuIfNotValid", true } };
                                optionList.Add(new Dictionary<string, object> { { "label", "save controls" }, { "taskName", TaskName.SaveControls }, { "executeHelper", saveData } });
                                var resetData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "useDefault", false }, { "showMessage", false } };
                                optionList.Add(new Dictionary<string, object> { { "label", "undo changes" }, { "taskName", TaskName.ResetControls }, { "executeHelper", resetData } });

                                var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "There are unsaved changes." }, { "customOptionList", optionList } };

                                new Task(taskName: TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                            }

                            return;
                        }

                    case TaskName.DropFruit:
                        {
                            Plant fruitPlant = (Plant)executeHelper;
                            fruitPlant.DropFruit();

                            return;
                        }

                    case TaskName.DeleteObsoleteSaves:
                        {
                            SaveHeaderManager.DeleteObsoleteSaves();
                            return;
                        }

                    case TaskName.ExecuteTaskWithDelay:
                        {
                            // example executeHelper for this task
                            // var delayData = new Dictionary<string, Object> { { "taskName", TaskName.TurnOffWorkshop }, { "executeHelper", workshop }, { "delay", 300 } };

                            var delayData = (Dictionary<string, Object>)executeHelper;
                            new Task(taskName: (TaskName)delayData["taskName"], executeHelper: delayData["executeHelper"], delay: (int)delayData["delay"]);

                            return;
                        }

                    case TaskName.AddWorldEvent:
                        {
                            world = (World)Scene.GetTopSceneOfType(typeof(World));
                            if (world == null) return;

                            var worldEventData = (Dictionary<string, Object>)executeHelper;

                            WorldEvent.EventName eventName = (WorldEvent.EventName)worldEventData["eventName"];
                            BoardPiece boardPiece = (BoardPiece)worldEventData["boardPiece"];
                            int delay = (int)worldEventData["delay"];

                            new WorldEvent(eventName: eventName, world: world, delay: delay, boardPiece: boardPiece);

                            return;
                        }

                    case TaskName.ShowTextWindow:
                        {
                            var textWindowData = (Dictionary<string, Object>)executeHelper;

                            string text = (string)textWindowData["text"];

                            List<Texture2D> imageList;
                            if (textWindowData.ContainsKey("imageList")) imageList = (List<Texture2D>)textWindowData["imageList"];
                            else imageList = new List<Texture2D>();

                            bool checkForDuplicate = false;
                            if (textWindowData.ContainsKey("checkForDuplicate")) checkForDuplicate = (bool)textWindowData["checkForDuplicate"];

                            bool useTransition = true;
                            if (textWindowData.ContainsKey("useTransition")) useTransition = (bool)textWindowData["useTransition"];

                            bool useTransitionOpen = false;
                            if (textWindowData.ContainsKey("useTransitionOpen")) useTransitionOpen = (bool)textWindowData["useTransitionOpen"];

                            bool useTransitionClose = false;
                            if (textWindowData.ContainsKey("useTransitionClose")) useTransitionClose = (bool)textWindowData["useTransitionClose"];

                            bool animate = true;
                            if (textWindowData.ContainsKey("animate")) animate = (bool)textWindowData["animate"];

                            TaskName closingTask = TaskName.Empty;
                            if (textWindowData.ContainsKey("closingTask")) closingTask = (TaskName)textWindowData["closingTask"];

                            Object closingTaskHelper = null;
                            if (textWindowData.ContainsKey("closingTaskHelper")) closingTaskHelper = textWindowData["closingTaskHelper"];

                            bool blocksUpdatesBelow = false;
                            if (textWindowData.ContainsKey("blocksUpdatesBelow")) blocksUpdatesBelow = (bool)textWindowData["blocksUpdatesBelow"];

                            int blockInputDuration = 0;
                            if (textWindowData.ContainsKey("blockInputDuration")) blockInputDuration = (int)textWindowData["blockInputDuration"];

                            SoundData.Name sound = SoundData.Name.Empty;
                            if (textWindowData.ContainsKey("sound")) sound = (SoundData.Name)textWindowData["sound"];

                            Color bgColor = Color.DarkBlue;
                            Color textColor = Color.White;

                            if (textWindowData.ContainsKey("bgColor"))
                            {
                                var bgColorValues = (List<byte>)textWindowData["bgColor"];
                                bgColor = new Color(bgColorValues[0], bgColorValues[1], bgColorValues[2]);
                            }

                            if (textWindowData.ContainsKey("textColor"))
                            {
                                var textColorValues = (List<byte>)textWindowData["textColor"];
                                textColor = new Color(textColorValues[0], textColorValues[1], textColorValues[2]);
                            }

                            int framesPerChar = 0;
                            if (textWindowData.ContainsKey("framesPerChar")) framesPerChar = (int)textWindowData["framesPerChar"];

                            new TextWindow(text: text, imageList: imageList, useTransition: useTransition, useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate, closingTask: closingTask, closingTaskHelper: closingTaskHelper, blockInputDuration: blockInputDuration, blocksUpdatesBelow: blocksUpdatesBelow, startingSound: sound);
                            return;
                        }

                    case TaskName.SleepOutside:
                        {
                            Player player = World.GetTopWorld()?.player;
                            if (player == null) return;

                            SleepEngine sleepEngine;

                            if (player.sprite.IsInWater)
                            {
                                if (player.sprite.CanDrownHere)
                                {
                                    player.hitPoints = 0;
                                    player.sprite.Visible = false;
                                    player.Kill();

                                    new TextWindow(text: "You have drowned.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 220);

                                    return;
                                }

                                sleepEngine = SleepEngine.OutdoorSleepWet;
                            }
                            else sleepEngine = SleepEngine.OutdoorSleepDry;

                            player.GoToSleep(sleepEngine: sleepEngine, zzzPos: player.sprite.position, wakeUpBuffs: new List<BuffEngine.Buff> { });
                            return;
                        }


                    case TaskName.OpenShelterMenu:
                        {
                            // executeHelper will go through menu - to "SleepInsideShelter" case
                            MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Shelter, executeHelper: this.executeHelper);
                            return;
                        }

                    case TaskName.SleepInsideShelter:
                        {
                            Shelter shelterPiece = (Shelter)executeHelper;
                            SleepEngine sleepEngine = shelterPiece.sleepEngine;
                            World.GetTopWorld()?.player.GoToSleep(sleepEngine: sleepEngine, zzzPos: new Vector2(shelterPiece.sprite.gfxRect.Center.X, shelterPiece.sprite.gfxRect.Center.Y), wakeUpBuffs: shelterPiece.buffList);

                            return;
                        }

                    case TaskName.ForceWakeUp:
                        {
                            Player player = (Player)executeHelper;
                            player.WakeUp(force: true);
                            return;
                        }

                    case TaskName.TempoStop:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;

                            // world.updateMultiplier = 0 is not used here, to allow for playing ambient sounds

                            world.stateMachineTypesManager.DisableMultiplier();
                            world.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(AmbientSound) }, everyFrame: true, nthFrame: true);

                            world.islandClock.Pause();

                            return;
                        }

                    case TaskName.TempoPlay:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;

                            world.updateMultiplier = 1;
                            world.stateMachineTypesManager.DisableMultiplier();
                            world.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
                            world.islandClock.Resume();

                            return;
                        }

                    case TaskName.TempoFastForward:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            SonOfRobinGame.game.IsFixedTimeStep = false;

                            world.updateMultiplier = (int)executeHelper;
                            world.stateMachineTypesManager.DisableMultiplier();
                            world.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
                            world.islandClock.Resume();

                            return;
                        }

                    case TaskName.CameraTrackPiece:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            BoardPiece piece = (BoardPiece)executeHelper;
                            world.camera.TrackPiece(piece);

                            return;
                        }

                    case TaskName.CameraTrackCoords:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.TrackCoords(position: (Vector2)executeHelper);

                            return;
                        }

                    case TaskName.CameraSetZoom:
                        {
                            // example executeHelper for this task
                            // var saveParams = new Dictionary<string, Object> { { "zoom", 2f }, { "zoomSpeedMultiplier", 2f }};

                            world = World.GetTopWorld();
                            if (world == null) return;

                            var zoomData = (Dictionary<string, Object>)executeHelper;
                            float zoom = (float)zoomData["zoom"];
                            float zoomSpeedMultiplier = 1f;
                            if (zoomData.ContainsKey("zoomSpeedMultiplier")) zoomSpeedMultiplier = (float)zoomData["zoomSpeedMultiplier"];

                            world.camera.SetZoom(zoom: zoom, zoomSpeedMultiplier: zoomSpeedMultiplier);

                            return;
                        }

                    case TaskName.ShowCookingProgress:
                        {
                            Cooker cooker = (Cooker)executeHelper;
                            cooker.ShowCookingProgress();

                            return;
                        }

                    case TaskName.RestoreHints:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;
                            world.hintEngine.RestoreAllHints();

                            return;
                        }

                    case TaskName.ExecuteTaskList:
                        {
                            List<Object> taskList = (List<Object>)executeHelper;
                            foreach (Object taskObject in taskList)
                            {
                                Task task = (Task)taskObject;
                                task.Process();
                            }

                            return;
                        }

                    case TaskName.ExecuteTaskChain:
                        {
                            List<Object> taskChain = (List<Object>)executeHelper;

                            Task currentTask = (Task)taskChain[0];
                            taskChain.RemoveAt(0);

                            if (currentTask.taskName == TaskName.ShowTextWindow)
                            {
                                // If text window will be opened, the delay will depend on the player, so it is unknown.
                                // So, the next task should be run after closing this text window.

                                if (taskChain.Count > 0)
                                {
                                    var executeHelper = currentTask.executeHelper;
                                    var textWindowData = (Dictionary<string, Object>)executeHelper;
                                    textWindowData["closingTask"] = TaskName.ExecuteTaskChain;
                                    textWindowData["closingTaskHelper"] = taskChain;
                                    currentTask.executeHelper = textWindowData;
                                }
                                currentTask.Process();
                            }
                            else
                            {
                                // in other cases, the delay should be known

                                currentTask.Process(); // must go before new Task(), to maintain correct execute order

                                if (taskChain.Count > 0) new Task(taskName: TaskName.ExecuteTaskChain, executeHelper: taskChain, delay: currentTask.delay, turnOffInputUntilExecution: true);
                            }

                            if (taskChain.Count == 0) Input.GlobalInputActive = true; // to ensure that input will be active at the end


                            return;
                        }

                    case TaskName.OpenMainMenuIfSpecialKeysArePressed:
                        {
                            Input.GlobalInputActive = true;
                            if (InputMapper.IsPressed(InputMapper.Action.SecretLicenceBypass)) new Task(taskName: TaskName.OpenMainMenu);
                            else new Task(taskName: TaskName.QuitGame);

                            return;
                        }

                    case TaskName.CheckForPieceHints:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            List<PieceHint.Type> typesToCheckOnly = null;
                            if (executeHelper != null) typesToCheckOnly = (List<PieceHint.Type>)executeHelper;

                            world.hintEngine.CheckForPieceHintToShow(forcedMode: true, ignoreInputActive: true, typesToCheckOnly: typesToCheckOnly);

                            return;
                        }

                    case TaskName.ShowHint:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            var hintType = (HintEngine.Type)executeHelper;
                            world.hintEngine.ShowGeneralHint(type: hintType, ignoreDelay: true);

                            return;
                        }

                    case TaskName.ShowTutorialInMenu:
                        {
                            Tutorials.ShowTutorialInMenu(type: (Tutorials.Type)executeHelper);

                            return;
                        }

                    case TaskName.ShowTutorialOnTheField:
                        {
                            var tutorialData = (Dictionary<string, Object>)executeHelper;

                            // example executeHelper for this task
                            // var tutorialData = new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreHintsSetting", false }, { "ignoreDelay", false }, { "ignoreIfShown", false } };

                            Tutorials.Type type = (Tutorials.Type)tutorialData["tutorial"];
                            World worldToUse = (World)tutorialData["world"];

                            bool ignoreHintsSetting = tutorialData.ContainsKey("ignoreHintsSetting") ? (bool)tutorialData["ignoreHintsSetting"] : false;
                            bool ignoreDelay = tutorialData.ContainsKey("ignoreDelay") ? (bool)tutorialData["ignoreDelay"] : false;
                            bool ignoreIfShown = tutorialData.ContainsKey("ignoreIfShown") ? (bool)tutorialData["ignoreIfShown"] : true;

                            Tutorials.ShowTutorialOnTheField(type: type, world: worldToUse, ignoreHintsSetting: ignoreHintsSetting, ignoreDelay: ignoreDelay, ignoreIfShown: ignoreIfShown);

                            return;
                        }

                    case TaskName.ReturnToMainMenu:
                        CloseGame(quitGame: false);
                        return;

                    case TaskName.QuitGame:
                        CloseGame(quitGame: true);
                        return;

                    case TaskName.RemoveScene:
                        {
                            var removeData = (Dictionary<string, Object>)executeHelper;
                            Scene scene = (Scene)removeData["scene"];
                            bool fadeOut = (bool)removeData["fadeOut"];
                            int fadeOutDuration = (int)removeData["fadeOutDuration"];

                            if (fadeOut) scene.transManager.AddTransition(new Transition(transManager: scene.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: fadeOutDuration, endRemoveScene: true, startSwapParams: true)); // TODO verify if this works correctly
                            else scene.Remove();

                            return;
                        }

                    case TaskName.ChangeSceneInputType:
                        {
                            // example executeHelper for this task
                            // var inputData = new Dictionary<string, Object> { { "scene", world }, { "inputType",  Scene.InputTypes.Normal }};

                            var inputData = (Dictionary<string, Object>)executeHelper;

                            Scene scene = (Scene)inputData["scene"];
                            Scene.InputTypes inputType = (Scene.InputTypes)inputData["inputType"];

                            scene.InputType = inputType;
                            return;
                        }

                    case TaskName.SetCineMode:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            world.CineMode = (bool)executeHelper;
                            return;
                        }

                    case TaskName.AddTransition:
                        {
                            // example executeHelper for this task
                            // var transData = new Dictionary<string, Object> { { "scene", scene }, { "transition",  transition }};

                            var transData = (Dictionary<string, Object>)executeHelper;

                            Scene scene = (Scene)transData["scene"];
                            Transition transition = (Transition)transData["transition"];

                            scene.transManager.AddTransition(transition);
                            return;
                        }

                    case TaskName.SolidColorRemoveAll:
                        {
                            // example executeHelper for this task
                            // var removeData = new Dictionary<string, Object> { { "manager", this.solidColorManager }, { "delay", 10 } }

                            var removeData = (Dictionary<string, Object>)executeHelper;
                            SolidColorManager solidColorManager = (SolidColorManager)removeData["manager"];
                            int delay = removeData.ContainsKey("delay") ? (int)removeData["delay"] : 0;

                            solidColorManager.RemoveAll(delay);

                            return;
                        }

                    case TaskName.SkipCinematics:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Skipping cinematics", color: Color.White);

                            var textWindows = Scene.GetAllScenesOfType(typeof(TextWindow));
                            foreach (var scene in textWindows)
                            {
                                TextWindow textWindow = (TextWindow)scene;
                                textWindow.RemoveWithoutExecutingTask(); // every cine task will be tied to text window
                            }
                            ClearQueue(); // to be sure, that no task will be executed
                            world.CineMode = false;

                            return;
                        }

                    case TaskName.DeleteTemplates:
                        {
                            Dictionary<string, Object> deleteData;
                            List<string> pathsToDelete;
                            int pathCount;

                            bool firstRun = executeHelper == null;

                            if (firstRun)
                            {
                                var templatePaths = Directory.GetDirectories(SonOfRobinGame.worldTemplatesPath);
                                var correctSaves = SaveHeaderManager.CorrectSaves;
                                var pathsToKeep = new List<string>();

                                foreach (string templatePath in templatePaths)
                                {
                                    GridTemplate gridTemplate = GridTemplate.LoadHeader(templatePath);
                                    if (gridTemplate == null || gridTemplate.IsObsolete) continue;

                                    if (correctSaves.Where(saveHeader =>
                                    saveHeader.seed == gridTemplate.seed &&
                                    saveHeader.width == gridTemplate.width &&
                                    saveHeader.height == gridTemplate.height
                                    ).ToList().Count > 0) pathsToKeep.Add(templatePath);
                                }

                                pathsToDelete = templatePaths.Where(path => !pathsToKeep.Contains(path)).ToList();

                                if (pathsToDelete.Count == 0)
                                {
                                    new TextWindow(text: "No obsolete templates were found.", textColor: Color.White, bgColor: Color.Blue, useTransition: true, animate: true);
                                    return;
                                }

                                executeHelper = new Dictionary<string, Object> { { "pathsToDelete", pathsToDelete }, { "pathCount", pathsToDelete.Count } };
                            }

                            SonOfRobinGame.game.IsFixedTimeStep = false;

                            deleteData = (Dictionary<string, Object>)executeHelper;
                            pathsToDelete = (List<string>)deleteData["pathsToDelete"];
                            pathCount = (int)deleteData["pathCount"];

                            string currentPath = pathsToDelete[0];

                            SonOfRobinGame.progressBar.TurnOn(curVal: pathCount - pathsToDelete.Count + 1, maxVal: pathCount, text: $"Deleting templates...\n{Path.GetFileName(currentPath)}", addTransition: false, turnOffInput: true);

                            if (!firstRun)
                            {
                                // first run should only turn on progress bar                              
                                pathsToDelete.RemoveAt(0);
                                Directory.Delete(path: currentPath, recursive: true);
                            }

                            if (pathsToDelete.Count == 0)
                            {
                                if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
                                SonOfRobinGame.progressBar.TurnOff();
                                return;
                            }

                            var newDeleteData = new Dictionary<string, Object> { { "pathsToDelete", pathsToDelete }, { "pathCount", pathCount } };
                            new Task(taskName: TaskName.DeleteTemplates, turnOffInputUntilExecution: true, delay: 1, executeHelper: newDeleteData);

                            return;
                        }

                    case TaskName.SetSpectatorMode:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            world.SpectatorMode = (bool)executeHelper;

                            return;
                        }

                    case TaskName.CheckForIncorrectPieces:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            // counting incorrect pieces

                            var incorrectBoardPieces = new List<BoardPiece>();
                            var incorrectStoragePieces = new List<BoardPiece>();
                            var piecesByID = new Dictionary<string, List<BoardPiece>>();
                            var duplicatedPiecesByID = new Dictionary<string, List<BoardPiece>>();

                            foreach (Sprite sprite in world.grid.GetAllSprites(Cell.Group.All))
                            {
                                if (sprite.boardPiece.exists && !sprite.IsOnBoard) incorrectBoardPieces.Add(sprite.boardPiece);
                                if (piecesByID.ContainsKey(sprite.id)) piecesByID[sprite.id].Add(sprite.boardPiece);
                                else piecesByID[sprite.id] = new List<BoardPiece> { sprite.boardPiece };

                                var storageList = new List<PieceStorage> { sprite.boardPiece.pieceStorage };

                                if (sprite.boardPiece.GetType() == typeof(Player)) // need to be updated with new classes using storages other than "this.pieceStorage"
                                {
                                    Player player = (Player)sprite.boardPiece;
                                    storageList.Add(player.toolStorage);
                                    storageList.Add(player.equipStorage);
                                }

                                foreach (PieceStorage currentStorage in storageList)
                                {
                                    if (currentStorage == null) continue;
                                    foreach (BoardPiece storedPiece in currentStorage.GetAllPieces())
                                    {
                                        if (piecesByID.ContainsKey(storedPiece.id)) piecesByID[storedPiece.id].Add(storedPiece);
                                        else piecesByID[storedPiece.id] = new List<BoardPiece> { storedPiece };

                                        // fruits are in storage, but on board at the same time (but not on the grid) - to display correctly
                                        if (storedPiece.sprite.IsOnBoard && storedPiece.GetType() != typeof(Fruit)) incorrectStoragePieces.Add(storedPiece);
                                    }
                                }
                            }

                            foreach (List<BoardPiece> pieceList in piecesByID.Values)
                            {
                                if (pieceList.Count > 1)
                                {
                                    duplicatedPiecesByID[pieceList[0].id] = pieceList;
                                }
                            }

                            // building error list

                            var errorsFound = new List<string>();
                            if (incorrectBoardPieces.Any()) errorsFound.Add($"Pieces placed incorrectly on the field: {incorrectBoardPieces.Count}.");
                            if (incorrectStoragePieces.Any()) errorsFound.Add($"Pieces placed incorrectly in the storage: {incorrectStoragePieces.Count}.");
                            if (duplicatedPiecesByID.Any())
                            {
                                errorsFound.Add($"Duplicated pieces ({duplicatedPiecesByID.Count}):");

                                foreach (List<BoardPiece> duplicatedPieceList in duplicatedPiecesByID.Values)
                                {
                                    errorsFound.Add($"- {duplicatedPieceList[0].readableName} ID {duplicatedPieceList[0].id} x{duplicatedPieceList.Count}");
                                }
                            }

                            bool isCorrect = !errorsFound.Any();
                            string message = errorsFound.Any() ? String.Join("\n", errorsFound) : "No incorrect pieces found.";

                            // showing message

                            new TextWindow(text: message, animate: true, useTransition: true, bgColor: isCorrect ? Color.Green : Color.DarkRed, textColor: Color.White);

                            return;
                        }

                    case TaskName.PlaySound:
                        {
                            Sound sound = (Sound)executeHelper;
                            sound.Play();

                            return;
                        }

                    case TaskName.PlaySoundByName:
                        {
                            SoundData.Name soundName = (SoundData.Name)executeHelper;
                            Sound.QuickPlay(soundName);

                            return;
                        }

                    case TaskName.AllowPieceToBeHit:
                        {
                            BoardPiece piece = (BoardPiece)executeHelper;
                            piece.canBeHit = true;

                            return;
                        }

                    case TaskName.SetPlayerPointWalkTarget:
                        {
                            var setData = (Dictionary<Player, Vector2>)executeHelper;

                            foreach (var kvp in setData)
                            {
                                kvp.Key.pointWalkTarget = kvp.Value;
                            }

                            return;
                        }

                    case TaskName.ShowCraftStats:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            bool showIngredients = (bool)executeHelper;

                            if (showIngredients) world.craftStats.DisplayUsedIngredientsSummary();
                            else world.craftStats.DisplayCraftedPiecesSummary();

                            return;
                        }

                    default:
                        throw new DivideByZeroException($"Unsupported taskName - {taskName}.");
                }
            }

            private static void CloseGame(bool quitGame)
            {
                World world = World.GetTopWorld();

                var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                foreach (World currWorld in worldScenes)
                {
                    if (!currWorld.demoMode) world.Remove();
                }

                Scene.RemoveAllScenesOfType(typeof(TextWindow));
                Scene.RemoveAllScenesOfType(typeof(Menu));
                Scene.RemoveAllScenesOfType(typeof(Inventory));
                Scene.RemoveAllScenesOfType(typeof(PieceContextMenu));

                if (quitGame) SonOfRobinGame.quitGame = true;
                else new Task(taskName: TaskName.OpenMainMenu, turnOffInputUntilExecution: true, delay: 0);

                GC.Collect();
            }
        }

    }
}
