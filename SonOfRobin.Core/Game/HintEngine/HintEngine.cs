using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class HintEngine
    {
        public enum Type : byte
        {
            Empty = 0,
            Hungry = 1,
            VeryHungry = 2,
            Starving = 3,
            Tired = 4,
            VeryTired = 5,
            SmallInventory = 6,
            MapNegative = 7,
            Lava = 8,
            BreakingItem = 9,
            BrokenItem = 10,
            BurntOutTorch = 11,
            CineIntroduction = 12,
            CineSmallBase = 13,
            CineEndingPart1 = 14,
            CineEndingPart2 = 15,
            CineEndingPart3 = 16,
            AnimalScaredOfFire = 17,
            AnimalCounters = 18,
            ZoomOutLocked = 19,
            Lightning = 20,
            TooDarkToUseTools = 21,
            BadSleep = 22,
        };

        public static readonly Type[] allTypes = (Type[])Enum.GetValues(typeof(Type));

        private const int hintDelay = 1 * 60 * 60; // 1 * 60 * 60
        public const int blockInputDuration = 80;

        private static readonly List<Type> typesThatIgnoreShowHintSetting = new() { Type.CineIntroduction, Type.CineSmallBase, Type.CineEndingPart1, Type.CineEndingPart2, Type.CineEndingPart3, Type.VeryTired, Type.Starving, Type.BrokenItem, Type.BurntOutTorch, Type.Lava, Type.Lightning, Type.TooDarkToUseTools, Type.BadSleep };

        public HashSet<Type> shownGeneralHints = new() { };
        public HashSet<PieceHint.Type> shownPieceHints = new() { };
        public HashSet<Tutorials.Type> shownTutorials = new() { };
        public readonly World world;
        private int waitUntilFrame;

        public HintEngine(World world)
        {
            this.world = world;
            this.waitUntilFrame = 200;
        }

        public bool WaitFrameReached
        { get { return this.world.CurrentUpdate >= this.waitUntilFrame; } }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> hintsData = new Dictionary<string, object>
            {
                {"shownGeneralHints", this.shownGeneralHints },
                {"shownPieceHints", this.shownPieceHints },
                {"shownTutorials", this.shownTutorials },
            };

            return hintsData;
        }

        public void Deserialize(Dictionary<string, Object> hintsData)
        {
            this.shownGeneralHints = (HashSet<Type>)hintsData["shownGeneralHints"];
            this.shownPieceHints = (HashSet<PieceHint.Type>)hintsData["shownPieceHints"];
            this.shownTutorials = (HashSet<Tutorials.Type>)hintsData["shownTutorials"];
            this.waitUntilFrame = this.world.CurrentUpdate + 200;
        }

        public void UpdateWaitFrame()
        { this.waitUntilFrame = this.world.CurrentUpdate + hintDelay; }

        public bool ShowGeneralHint(Type type, bool ignoreDelay = false, string text = "", Texture2D texture = null, BoardPiece piece = null)
        {
            if ((!Preferences.showHints && !typesThatIgnoreShowHintSetting.Contains(type)) || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return false;

            if (!ignoreDelay)
            {
                if (this.world.Player.activeState != BoardPiece.State.PlayerControlledWalking) return false;
                if (!this.WaitFrameReached) return false;
            }

            if (this.shownGeneralHints.Contains(type) || Scheduler.HasTaskChainInQueue || this.world.Player.sleepMode != Player.SleepMode.Awake) return false;
            // only one hint should be shown at once - waitingScenes cause playing next scene after turning off CineMode (playing scene without game being paused)

            this.UpdateWaitFrame();

            switch (type)
            {
                case Type.Empty:
                    {
                        // to allow for empty default action in PieceHint
                        break;
                    }

                case Type.Hungry:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting hungry.", blockInputDefaultDuration: true),
                            new HintMessage(text: "Time to look for | | | something to eat.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "It would be a good idea to | eat now.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "Hmm... | Dinner time?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal) }, blockInputDefaultDuration: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryHungry:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting really | | | hungry.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm really | hungry.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInputDefaultDuration: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Starving:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm starving.\nI need to eat | | | something right now\nor else I'm gonna | die...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Meal), AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm | dying from | hunger.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "| I have to | eat right now!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.BubbleExclamationRed].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInputDefaultDuration: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Tired:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm tired |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm kinda sleepy |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm exhausted |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryTired:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting very sleepy |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm so sleepy... |", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I have to sleep | now...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                            new HintMessage(text: "I'm gonna collapse if I don't go to sleep | now.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.TooDarkToUseTools:
                    {
                        // no Disable(), because this hint should be shown every time

                        MessageLog.Add(text: $"Too dark to use {text}.", bgColor: new Color(105, 3, 18), texture: texture, avoidDuplicates: true);
                        break;
                    }

                case Type.BadSleep:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Sleeping here was really uncomfortable.\nI should really build a | better place to sleep in...", blockInputDefaultDuration: true, imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.TentMedium).texture }),
                        });
                        break;
                    }

                case Type.SmallInventory:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "I cannot carry many items right now.\nWith some | leather I should be able to make a | backpack.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall) }, blockInputDefaultDuration: true) });
                        break;
                    }

                case Type.MapNegative:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"I don't have a map.\nIf I had some | leather and a | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopLeatherBasic).readableName} - I could make one.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopLeatherBasic) }, blockInputDefaultDuration: true) });
                        break;
                    }

                case Type.Lava:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Ouch! This is | lava!", imageList: new List<Texture2D> {AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture}, blockInputDefaultDuration: true) });
                        break;
                    }

                case Type.Lightning:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Aaah! Lightning just hit the water.", blockInputDefaultDuration: true) });
                        break;
                    }

                case Type.BreakingItem:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"My | {text} is getting damaged. It won't last forever.", blockInputDefaultDuration: true, imageList: new List<Texture2D> { texture }),
                            new HintMessage(text: "I should watch my tools durability.", blockInputDefaultDuration: true),
                        });
                        break;
                    }

                case Type.BrokenItem:
                    {
                        // no Disable(), because this hint should be shown every time
                        this.world.Player.pieceInfo.Yield.DropDebris(piece: this.world.Player, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisStone }, particlesToEmit: 7);

                        Sound.QuickPlay(SoundData.Name.DestroyWood);
                        ShowMessageDuringPause(new HintMessage(text: $"My | {text} has fallen apart.", imageList: new List<Texture2D> { texture }, blockInputDefaultDuration: true));

                        break;
                    }

                case Type.BurntOutTorch:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new HintMessage(text: $"My | {text} has burnt out.", imageList: new List<Texture2D> { texture }, blockInputDefaultDuration: true));
                        break;
                    }

                case Type.AnimalScaredOfFire:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: new List<HintMessage> {
                            new HintMessage($"This | {piece.readableName} is scared of | fire!", imageList: new List<Texture2D> { piece.sprite.CroppedAnimFrame.texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture}, blockInputDefaultDuration: true),
                            new HintMessage("I think that I'm safe | here.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall)}, blockInputDefaultDuration: true)
                        });
                        break;
                    }

                case Type.AnimalCounters:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: new List<HintMessage> {
                            new HintMessage($"This | {piece.readableName} had just attacked me!\nIt must be because I have | attacked it first...", imageList: new List<Texture2D> {piece.sprite.CroppedAnimFrame.texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.BloodSplatter1].texture}, blockInputDefaultDuration: true),
                        });
                        break;
                    }

                case Type.ZoomOutLocked:
                    {
                        this.Disable(type: type, delay: 0);

                        Sound.QuickPlay(SoundData.Name.Error);

                        ShowMessageDuringPause(new HintMessage(text: "'Zoom out' is disabled, when world scale is less than 1.", boxType: HintMessage.BoxType.RedBox, blockInputDefaultDuration: true));
                        break;
                    }

                case Type.CineIntroduction:
                    {
                        this.Disable(type: type, delay: 0);

                        Player player = this.world.Player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.cineCurtains.showPercentage = 1f; // no transition here
                        this.world.CineMode = true;

                        // HintMessage.ConvertToTasks() could be used here, but adding one by one makes it easier to add other task types between text.
                        var taskChain = new List<Object>();

                        if (this.world.PlayerName == PieceTemplate.Name.PlayerTestDemoness)
                        {
                            var invertedDialogue = HintMessage.BoxType.InvertedDialogue;

                            this.world.camera.SetZoom(zoom: 1f, setInstantly: true);

                            Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                            { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 2f, zoomSpeedMultiplier: 0.5f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 30, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "   ...    ", boxType: dialogue, delay: 0, blockInputDefaultDuration: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position + new Vector2(0, 10) } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Red }, { "opacity", 0.35f } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate activateLightEngineDlgt = () =>
                            { if (!this.world.HasBeenRemoved) this.world.Player.sprite.lightEngine.Activate(); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: activateLightEngineDlgt, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.FireBurst, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                            { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 3.5f, zoomSpeedMultiplier: 3f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Hello there, human.", boxType: invertedDialogue, delay: 30, blockInputDefaultDuration: false).ConvertToTask());
                            taskChain.Add(new HintMessage(text: "Don't you know how to read?", boxType: invertedDialogue, delay: 30, blockInputDefaultDuration: false).ConvertToTask());
                            taskChain.Add(new HintMessage(text: "It was clearly stated, that i will BREAK your game.", boxType: invertedDialogue, delay: 80, blockInputDefaultDuration: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 300 } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "And I WILL | RUIN your game experience.\nWith pleasure |.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.Heart].texture }, boxType: invertedDialogue, delay: 0).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "My contract says, that I'm here to help test this game, human.", boxType: invertedDialogue, delay: 0).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "Is that clear? Yeah?\nThen let's get started |!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture }, boxType: invertedDialogue, delay: 0).ConvertToTask());
                        }
                        else
                        {
                            new LevelEvent(eventName: LevelEvent.EventName.CheckForPieceHints, level: this.world.ActiveLevel, delay: 60 * 2, boardPiece: null, eventHelper: new Dictionary<string, Object> { { "typesToCheckOnly", new List<PieceHint.Type> { PieceHint.Type.CineCrateStarting } } }); // will be executed after playing this hint

                            this.world.camera.SetZoom(zoom: 3f, setInstantly: true);

                            this.world.solidColorManager.Add(new(color: Color.White, viewOpacity: 1f));

                            Scheduler.ExecutionDelegate setGlobEffectDlgt = () =>
                            {
                                if (!world.HasBeenRemoved) world.globalEffect = new MosaicInstance(textureSize: new Vector2(this.world.FinalRenderTarget.Width, this.world.FinalRenderTarget.Height), blurSize: new Vector2(18, 18), framesLeft: -1);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobEffectDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Where am I?", boxType: dialogue, delay: 80, blockInputDefaultDuration: false).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "    ...    ", boxType: dialogue, delay: 120, blockInputDefaultDuration: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 700 } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate setGlobalTweenerDlgt1 = () =>
                            {
                                if (this.world.HasBeenRemoved) return;
                                this.world.tweenerForGlobalEffect.CancelAndCompleteAll();
                                this.world.globalEffect.intensityForTweener = 1f;
                                this.world.tweenerForGlobalEffect.TweenTo(target: this.world.globalEffect, expression: effect => effect.intensityForTweener, toValue: 0.5f, duration: 5f).Easing(EasingFunctions.SineOut);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobalTweenerDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "The last thing I remember...?", boxType: dialogue, delay: 60, blockInputDefaultDuration: false).ConvertToTask());

                            Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                            {
                                if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.1f);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 30, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Hmm...\n...\n...", boxType: dialogue, delay: 60, blockInputDefaultDuration: false).ConvertToTask());

                            Scheduler.ExecutionDelegate setGlobalTweenerDlgt2 = () =>
                            {
                                if (this.world.HasBeenRemoved) return;
                                this.world.tweenerForGlobalEffect.CancelAndCompleteAll();
                                this.world.tweenerForGlobalEffect.TweenTo(target: this.world.globalEffect, expression: effect => effect.intensityForTweener, toValue: 0f, duration: 5f).Easing(EasingFunctions.SineOut);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobalTweenerDlgt2, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "There was... a terrible storm....", boxType: dialogue, delay: 90, blockInputDefaultDuration: false).ConvertToTask());

                            Vector2 seaOffset = new(SonOfRobinGame.VirtualWidth * 0.65f, SonOfRobinGame.VirtualHeight * 0.65f);

                            int edgeDistLeft = (int)player.sprite.position.X;
                            int edgeDistRight = (int)(this.world.ActiveLevel.width - player.sprite.position.X);
                            int edgeDistUp = (int)player.sprite.position.Y;
                            int edgeDistDown = (int)(this.world.ActiveLevel.height - player.sprite.position.Y);
                            int edgeDistX = edgeDistLeft < edgeDistRight ? -edgeDistLeft : edgeDistRight;
                            int edgeDistY = edgeDistUp < edgeDistDown ? -edgeDistUp : edgeDistDown;
                            if (edgeDistX < 0) seaOffset.X *= -1;
                            if (edgeDistY < 0) seaOffset.Y *= -1;
                            if (Math.Abs(edgeDistX) < Math.Abs(edgeDistY)) seaOffset.Y = 0;
                            else seaOffset.X = 0;
                            Vector2 seaPos = player.sprite.position + seaOffset;

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position + (seaOffset * 0.1f) } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camMoveSpdDlgt1 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(0.15f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 40, executeHelper: camMoveSpdDlgt1, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate trackCoordsDlgt = () => { if (!this.world.HasBeenRemoved) this.world.camera.TrackCoords(position: seaPos, moveInstantly: false); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: trackCoordsDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "What happened to the ship?", boxType: dialogue, delay: 0).ConvertToTask());

                            Scheduler.ExecutionDelegate trackPieceDlgt = () => { if (!world.HasBeenRemoved) world.camera.TrackPiece(player); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60, executeHelper: trackPieceDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "I can't see it anywhere...", boxType: dialogue, delay: 0).ConvertToTask());

                            Scheduler.ExecutionDelegate clearGlobEffectDlgt = () =>
                            {
                                if (!world.HasBeenRemoved) world.globalEffect = null;
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearGlobEffectDlgt, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camMoveSpdDlgt2 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(1f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 40, executeHelper: camMoveSpdDlgt2, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                            {
                                if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.55f, zoomSpeedMultiplier: 3f);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.DunDunDun, storeForLaterUse: true));
                            taskChain.Add(new HintMessage(text: "I guess I'm stranded | here.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.PalmTree].texture }, boxType: dialogue, delay: 0).ConvertToTask());
                        }

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineSmallBase:
                    {
                        this.Disable(type: type, delay: 0);

                        Player player = this.world.Player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;

                        var taskChain = new List<Object>();

                        taskChain.Add(new HintMessage(text: "Well...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true)); // repeated, to make sure it will be executed (in case of waitingScenes being used)

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1.5f, zoomSpeedMultiplier: 0.4f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        Vector2 basePos = player.sprite.position;
                        int distance = 30;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(-distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 60, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance * 1.5f, distance) } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.8f, zoomSpeedMultiplier: 2f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                        if (this.world.PlayerName == PieceTemplate.Name.PlayerTestDemoness)
                        {
                            taskChain.Add(new HintMessage(text: "This camp | | looks like shit.\nDo lowly humans really live in such pitiful conditions?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) }, boxType: dialogue, delay: 30).ConvertToTask());
                        }
                        else
                        {
                            taskChain.Add(new HintMessage(text: "This should be enough for a basic camp | |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) }, boxType: dialogue, delay: 30).ConvertToTask());
                        }

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineEndingPart1:
                    {
                        // no disable code needed

                        BoardPiece boat = piece;
                        Player player = this.world.Player;
                        Random random = player.world.random;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;
                        this.world.cineCurtains.Enabled = true;

                        int fadeToWhiteDuration = 60 * 8;

                        SolidColor colorOverlay = new(color: Color.White, viewOpacity: 0.0f);
                        colorOverlay.transManager.AddTransition(new Transition(transManager: colorOverlay.transManager, outTrans: true, startDelay: 0, duration: fadeToWhiteDuration, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 1f, endRemoveScene: true));
                        this.world.solidColorManager.Add(colorOverlay);

                        var taskChain = new List<Object>();

                        Vector2 walkDistanceUnit = Vector2.Clamp(value1: player.sprite.position - boat.sprite.position, min: new Vector2(-80), max: new Vector2(80));
                        int rndDist = 100;

                        Vector2 walkPos1 = player.sprite.position + (walkDistanceUnit * 2) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos2 = player.sprite.position + (walkDistanceUnit * 3) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos3 = player.sprite.position + (walkDistanceUnit * 4) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos4 = boat.sprite.position + ((walkPos3 - boat.sprite.position) / 3);

                        TimeSpan timeUntilDeparture = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning) + TimeSpan.FromHours(2); // to depart at early morning

                        Scheduler.ExecutionDelegate clockAdvanceDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(timeUntilDeparture), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: fadeToWhiteDuration / 4, executeHelper: clockAdvanceDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clearWeatherDelegate = () =>
                        { if (!this.world.HasBeenRemoved) this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromHours(24)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearWeatherDelegate, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I can't believe it. I've managed to build this boat with my own two hands!", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "The countless hours of work have finally paid off.", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "At last, bidding farewell to this island!", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos1 } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.4f, zoomSpeedMultiplier: 0.1f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Will I miss this place someday?", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "The memories here will always hold a special place in my | heart...", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.BuffHPPlus) }, boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos2 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "It was my home for such a long time...\nSurvival amidst loneliness and relentless challenges.", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos3 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Well... I can't stay here forever,\neven though I might miss this place one day.", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.8f, zoomSpeedMultiplier: 0.15f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos4 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I hope that my | boat can take me back home safely...\nThe waves seem both menacing and inviting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BoatCompleteStanding) }, boxType: dialogue, delay: 0, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, boat.sprite.position } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt3 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.2f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt3, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Alright! It's time to head home!", boxType: dialogue, delay: 30, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        int fadeToWhiteFrames = 60 * 2;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.White }, { "opacity", 1f }, { "fadeInDurationFrames", fadeToWhiteFrames } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: fadeToWhiteFrames + 60, executeHelper: false, storeForLaterUse: true));

                        Level openSeaLevel = new Level(type: Level.LevelType.OpenSea, hasWeather: true, plansWeather: false, hasWater: true, world: this.world, width: 100000, height: 8000, seed: 0);

                        Scheduler.ExecutionDelegate enterLevelDelegate = () => { if (!this.world.HasBeenRemoved) this.world.EnterNewLevel(openSeaLevel); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: enterLevelDelegate, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineEndingPart2:
                    {
                        // no disable code needed

                        Player player = this.world.Player;
                        player.sprite.orientation = Sprite.Orientation.right;
                        player.sprite.CharacterStand();
                        var dialogue = HintMessage.BoxType.Dialogue;

                        BoardPiece boatCruising = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.BoatCompleteCruising, world: this.world);
                        boatCruising.sprite.PlaceOnBoard(randomPlacement: false, position: player.sprite.position, closestFreeSpot: true);
                        boatCruising.AddToStateMachines();
                        new Tracking(level: player.level, targetSprite: boatCruising.sprite, followingSprite: player.sprite, offsetX: 100, offsetY: -20);

                        this.world.CineMode = true;
                        this.world.cineCurtains.showPercentage = 1f; // no transition here
                        this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromDays(100)); // to make sure no weather events are present

                        this.world.solidColorManager.RemoveAll();

                        this.world.solidColorManager.Add(new(color: Color.White, viewOpacity: 1f));

                        var taskChain = new List<Object>();

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 60 * 10 } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clockAdvanceDlgt1 = () =>
                        {
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning) + TimeSpan.FromHours(3)), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clockAdvanceDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate trackCoordsDlgt1 = () => { if (!world.HasBeenRemoved) this.world.camera.TrackCoords(position: player.sprite.position + new Vector2(SonOfRobinGame.VirtualWidth * 6, 0), moveInstantly: true); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 1, executeHelper: trackCoordsDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.15f, setInstantly: true); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camMoveSpdDlgt = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(0.08f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camMoveSpdDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate trackPieceDlgt1 = () => { if (!this.world.HasBeenRemoved) this.world.camera.TrackPiece(player); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 1, executeHelper: trackPieceDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.025f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I've been adrift for weeks now.\nThe days start to blend together...", boxType: dialogue, delay: 60 * 7, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "Since departing from the island, all I see is an endless sea.\nNo sight of land or ships.", boxType: dialogue, delay: 60 * 4, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "This heat is unbearable. I don't have any strength left to row...", boxType: dialogue, delay: 60 * 2, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true)); // weather won't get updated without this

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Clouds, intensity: 1.0f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(60)), storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Fog, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(50)), storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I'm so hungry, but I must endure.\nSoon, I will run out of | | food...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatDried), PieceInfo.GetTexture(PieceTemplate.Name.Apple) }, boxType: dialogue, delay: 60 * 2, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Wind, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(15)), storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Rain, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(30)), storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I'm starting to miss my island already...\nMaybe it wasn't such a good idea to leave?", boxType: dialogue, delay: 60 * 4, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        VisualEffect orbiter = (VisualEffect)PieceTemplate.CreatePiece(world: this.world, templateName: PieceTemplate.Name.Orbiter);
                        Scheduler.ExecutionDelegate addOrbiterDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            orbiter.PlaceOnBoard(randomPlacement: false, position: player.sprite.position);
                            orbiter.universalFloat = 0.4f;
                            orbiter.visualAid = player;
                            this.world.camera.TrackPiece(orbiter);
                            this.world.camera.SetMovementSpeed(0.8f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: addOrbiterDlgt, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Finally, | rain! How refreshing!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.WaterDrop].texture }, boxType: dialogue, delay: 60 * 4, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 60 * 5, executeHelper: new WeatherEvent(type: Weather.WeatherType.Lightning, intensity: 0.35f, startTime: DateTime.MinValue, duration: TimeSpan.FromSeconds(40), transitionLength: TimeSpan.FromSeconds(15)), storeForLaterUse: true));

                        Scheduler.ExecutionDelegate createBadWeatherDelegate = () => { if (!this.world.HasBeenRemoved) this.world.weather.CreateVeryBadWeatherForDuration(TimeSpan.FromHours(24)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: createBadWeatherDelegate, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Oh no, a storm approaches!", boxType: dialogue, delay: 0, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        Sound soundStormLoop = new Sound(name: SoundData.Name.OpenSeaStormLoop, isLooped: true, volumeFadeFrames: 90, ignore3DAlways: true);
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySound, delay: 60 * 5, executeHelper: soundStormLoop, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate updateOrbiterDlgt1 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            orbiter.universalFloat = 2f;
                            this.world.camera.SetMovementSpeed(0.8f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 2, executeHelper: updateOrbiterDlgt1, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I'm heading into the heart of the storm!", boxType: dialogue, delay: 60 * 2, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate updateOrbiterDlgt2 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            VisualEffect orbiter = (VisualEffect)PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: player.sprite.position, templateName: PieceTemplate.Name.Orbiter, closestFreeSpot: true);
                            orbiter.universalFloat = 2f;
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 2, executeHelper: updateOrbiterDlgt2, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hold on tight! The boat's rocking violently!", boxType: dialogue, delay: 60 * 3, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate updateOrbiterDlgt3 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            orbiter.universalFloat = 4f;
                            this.world.camera.SetMovementSpeed(4f);
                            ParticleEngine.TurnOn(sprite: player.sprite, preset: ParticleEngine.Preset.DistortStormCine, duration: 1);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 2, executeHelper: updateOrbiterDlgt3, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.OpenSeaStormCrash, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Aaaaaaah!", boxType: dialogue, delay: 0, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        int fadeInDuration = 60 * 3;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Black }, { "opacity", 1f }, { "fadeInDurationFrames", fadeInDuration } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clockAdvanceDlgt2 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;
                            TimeSpan timeUntilMorning = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning) + TimeSpan.FromHours(2);
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(timeUntilMorning), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: fadeInDuration, executeHelper: clockAdvanceDlgt2, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate stopSoundDlgt = () => { if (!this.world.HasBeenRemoved) soundStormLoop.Stop(); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: stopSoundDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate deleteOrbiterDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            this.world.camera.TrackPiece(player);
                            this.world.camera.ResetMovementSpeed();
                            orbiter.visualAid = null; // has to be cleared, otherwise the player will be destroyed, too
                            orbiter.Destroy();
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: deleteOrbiterDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clearWeatherDelegate = () =>
                        { if (!this.world.HasBeenRemoved) this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromHours(48)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearWeatherDelegate, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 60 * 4, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 60 * 5 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60 * 5, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "I must have passed out...", boxType: dialogue, delay: 60 * 2, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "Somehow, I've survived the storm. My boat seems to be fine, too...", boxType: dialogue, delay: 60 * 3, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate makeRollingTextSceneDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            var textList = Helpers.MakeCreditsTextList();

                            Level islandLevel = this.world.IslandLevel;

                            float fontSizeMultiplier = (float)SonOfRobinGame.VirtualWidth * 0.0008f;

                            SpriteFontBase fontTitle = SonOfRobinGame.FontTommy.GetFont(RollingText.TitleFontSize);
                            SpriteFontBase fontText = SonOfRobinGame.FontTommy.GetFont(RollingText.RegularFontSize);

                            textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<Texture2D>()));

                            textList.Add(new TextWithImages(font: fontTitle, text: "...and now about | your adventure:", imageList: new List<Texture2D> { player.pieceInfo.CroppedFrame.texture }));

                            textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<Texture2D>()));

                            string timePlayedString = string.Format("{0:D2}:{1:D2}", (int)Math.Floor(this.world.TimePlayed.TotalHours), this.world.TimePlayed.Minutes);
                            textList.Add(new TextWithImages(font: fontText, text: $"| time played: {timePlayedString}", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass) }, minMarkerWidthMultiplier: 2f));

                            textList.Add(new TextWithImages(font: fontText, text: $"| distance walked: {player.DistanceWalkedKilometers} km", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BootsAllTerrain) }, minMarkerWidthMultiplier: 2f));

                            textList.Add(new TextWithImages(font: fontText, text: $"| map discovered: {Math.Round(islandLevel.grid.VisitedCellsPercentage * 100, 1)}%", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Map) }, minMarkerWidthMultiplier: 2f));

                            textList.Add(new TextWithImages(font: fontText, text: $"| caves visited: {player.cavesVisited}", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CaveEntranceOutside) }, minMarkerWidthMultiplier: 2f));

                            textList.Add(new TextWithImages(font: fontText, text: $"| locations found: {islandLevel.grid.namedLocations.DiscoveredLocationsCount}/{islandLevel.grid.namedLocations.AllLocationsCount}", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MapMarker) }, minMarkerWidthMultiplier: 2f));

                            textList.Add(new TextWithImages(font: fontText, text: $"days spent on the island: {this.world.islandClock.CurrentDayNo}", imageList: new List<Texture2D> { }, minMarkerWidthMultiplier: 2f));

                            if (this.world.craftStats.CraftedPiecesTotal > 0)
                            {
                                textList.Add(new TextWithImages(font: fontText, text: "| General craft", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.WorkshopAdvanced].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| Items crafted: {this.world.craftStats.CraftedPiecesTotal}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.AxeIron].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| Ingredients used: {this.world.craftStats.UsedIngredientsTotal}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.WoodLogRegular].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| Ingredients saved: {this.world.craftStats.SmartCraftingReducedIngredientCount}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.ChestIron].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: " ", imageList: new List<Texture2D>()));
                            }

                            if (this.world.cookStats.TotalCookCount > 0)
                            {
                                textList.Add(new TextWithImages(font: fontTitle, text: "| Cooking", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.CookingPot].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| meals made: {this.world.cookStats.TotalCookCount}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.MealStandard].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| ingredients used: {this.world.cookStats.AllIngredientsCount}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.MeatRawPrime].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<Texture2D>()));
                            }

                            if (this.world.brewStats.TotalCookCount > 0)
                            {
                                textList.Add(new TextWithImages(font: fontTitle, text: "| Potion brewing", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.AlchemyLabStandard].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| potions made: {this.world.brewStats.TotalCookCount}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.PotionRed].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| ingredients used: {this.world.brewStats.AllIngredientsCount}", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.HerbsCyan].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<Texture2D>()));
                            }

                            if (this.world.meatHarvestStats.TotalHarvestCount > 0)
                            {
                                textList.Add(new TextWithImages(font: fontTitle, text: "| meat harvesting", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.MeatRawPrime].texture }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| animals processed: {this.world.meatHarvestStats.TotalHarvestCount}", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Rabbit) }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontText, text: $"| items obtained: {this.world.meatHarvestStats.ObtainedTotalPieceCount}", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatRawPrime) }, minMarkerWidthMultiplier: 2f));

                                textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<Texture2D>()));
                            }

                            textList.Add(new TextWithImages(font: fontTitle, text: "\n\n ", imageList: new List<Texture2D>()));

                            textList.Add(new TextWithImages(font: fontTitle, text: "|  Thank you for playing!  |", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.BuffHPPlus), TextureBank.GetTexture(TextureBank.TextureName.BuffHPPlus) }));

                            Scheduler.ExecutionDelegate openCineEndingPart3Dlgt = () =>
                            { if (!this.world.HasBeenRemoved) this.world.HintEngine.ShowGeneralHint(type: Type.CineEndingPart3, ignoreDelay: true); };

                            new RollingText(textList: textList, pixelsToScrollEachFrame: 1, offsetPercentX: 0.15f, runAtTheEndDlgt: openCineEndingPart3Dlgt, bgColor: Color.Black * 0.45f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: makeRollingTextSceneDlgt, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineEndingPart3:
                    {
                        // no disable code needed
                        Player player = this.world.Player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;
                        this.world.cineCurtains.showPercentage = 1f; // no transition here

                        var taskChain = new List<Object>();

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 60 * 10 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...\nI'm sure there was something important on that island.\nA secret, waiting to be uncovered...", boxType: dialogue, delay: 60 * 3, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.BoatHorn, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "What???", boxType: dialogue, delay: 30, autoClose: true, blockInputDuration: 60 * 1, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate placeRescueShipDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            VisualEffect rescueShip = (VisualEffect)PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: player.sprite.position + new Vector2(SonOfRobinGame.VirtualWidth * 5, this.world.camera.viewRect.Height * 1.6f), templateName: PieceTemplate.Name.ShipRescue, closestFreeSpot: true);

                            this.world.camera.TrackCoords(new Vector2(rescueShip.sprite.GfxRect.Center.X, rescueShip.sprite.GfxRect.Center.Y));
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: placeRescueShipDlgt, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Ahoy there, on the boat!", boxType: dialogue, delay: 60 * 2, autoClose: true, blockInputDuration: 60 * 3, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "Need a lift to safety and civilization?", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 3, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate trackPieceDlgt2 = () => { if (!this.world.HasBeenRemoved) this.world.camera.TrackPiece(player); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: trackPieceDlgt2, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt3 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 2.5f, zoomSpeedMultiplier: 0.07f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt3, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I don't believe it! A ship!", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "At last, I'm saved!", boxType: dialogue, delay: 60 * 1, autoClose: true, blockInputDuration: 60 * 4, noInput: true).ConvertToTask());

                        int finalFadeInDuration = 60 * 3;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Black }, { "opacity", 1f }, { "fadeInDurationFrames", finalFadeInDuration } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate stopSoundsDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            var nearbyPieces = player.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 10000);
                            var ambientSounds = nearbyPieces.Where(piece => piece.GetType() == typeof(AmbientSound));
                            var seaWaves = nearbyPieces.Where(piece => piece.name == PieceTemplate.Name.SeaWave);
                            var boatsCruising = nearbyPieces.Where(piece => piece.name == PieceTemplate.Name.BoatCompleteCruising);

                            foreach (BoardPiece boat in boatsCruising) boat.activeState = BoardPiece.State.Empty; // stopping boat, to prevent from generating AmbientSound pieces

                            foreach (BoardPiece soundPiece in ambientSounds)
                            {
                                AmbientSound ambientSound = (AmbientSound)soundPiece;
                                ambientSound.activeState = BoardPiece.State.Empty;
                                ambientSound.activeSoundPack.Stop(PieceSoundPackTemplate.Action.Ambient); // will fade out
                            }
                            foreach (BoardPiece wavePiece in seaWaves) wavePiece.Destroy();
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: finalFadeInDuration, executeHelper: stopSoundsDlgt, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 60 * 2, executeHelper: SoundData.Name.MusicBoxEnding, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "  ~ THE END ~  ", boxType: HintMessage.BoxType.GoldBox, delay: 0, autoClose: true, blockInputDuration: 60 * 25, noInput: true).ConvertToTask());

                        Scheduler.ExecutionDelegate returnToMenuDlgt = () => { Scheduler.Task.CloseGame(quitGame: false); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 4, executeHelper: returnToMenuDlgt, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                default:
                    { throw new ArgumentException($"Unsupported hint type - {type}."); }
            }

            return true;
        }

        public void CheckForPieceHintToShow(bool ignorePlayerState = false, bool ignoreInputActive = false, List<PieceHint.Type> typesToCheckOnly = null, PieceTemplate.Name fieldPieceNameToCheck = PieceTemplate.Name.Empty, PieceTemplate.Name newOwnedPieceNameToCheck = PieceTemplate.Name.Empty)
        {
            if ((!ignorePlayerState && this.world.Player.activeState != BoardPiece.State.PlayerControlledWalking) || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return;
            if (!this.WaitFrameReached && typesToCheckOnly == null && fieldPieceNameToCheck == PieceTemplate.Name.Empty && newOwnedPieceNameToCheck == PieceTemplate.Name.Empty) return;

            bool hintShown = PieceHint.CheckForHintToShow(hintEngine: this, player: world.Player, ignoreInputActive: ignoreInputActive, typesToCheckOnly: typesToCheckOnly, fieldPieceNameToCheck: fieldPieceNameToCheck, newOwnedPieceNameToCheck: newOwnedPieceNameToCheck);
            if (hintShown) this.UpdateWaitFrame();
        }

        public static void ShowMessageDuringPause(HintMessage message)
        {
            ShowMessageDuringPause(new List<HintMessage> { message });
        }

        public static void ShowMessageDuringPause(List<HintMessage> messageList)
        {
            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }

        public static void ShowPieceDuringPause(World world, BoardPiece pieceToShow, List<HintMessage> messageList)
        {
            world.camera.TrackPiece(pieceToShow);

            BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(pieceToShow.sprite.GfxRect.Center.X, pieceToShow.sprite.GfxRect.Center.Y), templateName: PieceTemplate.Name.Crosshair);
            new Tracking(level: world.ActiveLevel, targetSprite: pieceToShow.sprite, followingSprite: crossHair.sprite);

            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            // tasks before the messages - inserted at 0, so the last go first

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));

            Scheduler.ExecutionDelegate camZoomDlgt1 = () => { if (!world.HasBeenRemoved) world.camera.SetZoom(zoom: 2f); };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

            Scheduler.ExecutionDelegate addLevelEventDlgt = () =>
            {
                if (world.HasBeenRemoved) return;
                new LevelEvent(eventName: LevelEvent.EventName.Destruction, level: world.ActiveLevel, delay: 60, boardPiece: crossHair);
            };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: addLevelEventDlgt, storeForLaterUse: true));

            // task after the messages - added at the end, ordered normally

            Scheduler.ExecutionDelegate trackPieceDlgt = () => { if (!world.HasBeenRemoved) world.camera.TrackPiece(world.Player); };
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: trackPieceDlgt, storeForLaterUse: true));

            Scheduler.ExecutionDelegate camZoomDlgt2 = () => { if (!world.HasBeenRemoved) world.camera.SetZoom(zoom: 1f); };
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }

        public void RestoreAllHints()
        {
            this.shownPieceHints.Clear();
            this.shownGeneralHints.Clear();
            this.shownTutorials.Clear();
            this.waitUntilFrame = 0;

            MessageLog.Add(debugMessage: true, text: "Hints has been restored.");
        }

        public void Enable(Type type)
        {
            this.shownGeneralHints.Remove(type);
        }

        public void Disable(Type type, int delay = 0)
        {
            if (!this.shownGeneralHints.Contains(type)) this.shownGeneralHints.Add(type);
            if (delay != 0) new LevelEvent(eventName: LevelEvent.EventName.RestoreHint, delay: delay, level: this.world.IslandLevel, boardPiece: null, eventHelper: type);
        }

        public void Disable(PieceHint.Type type)
        { if (!this.shownPieceHints.Contains(type)) this.shownPieceHints.Add(type); }

        public void Disable(Tutorials.Type type)
        { if (!this.shownTutorials.Contains(type)) this.shownTutorials.Add(type); }
    }
}