using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tutorials
    {
        public enum Type : byte
        {
            BreakThing = 0,
            Equip = 1,
            BuildWorkshop = 2,
            GetWood = 3,
            Mine = 4,
            Interact = 5,
            PickUp = 6,
            Hit = 7,
            Craft = 8,
            KeepingAnimalsAway = 9,
            ShootProjectile = 10,
            Cook = 11,
            ShakeFruit = 12,
            AnimalAttacking = 13,
            Torch = 14,
            Fireplace = 15,
            TooDarkToReadMap = 16,
            TooDarkToSeeAnything = 17,
            Heat = 18,
            CraftRecipeLevels = 19,
            SwampPoison = 20,
            SmartCrafting = 21,
            HowToSave = 22,
            CombineItems = 23,
            ResourcefulCrafting = 24,
            PotionBrew = 25,
            CookLevels = 26,
            BrewLevels = 27,
            GeneralCraftLevels = 28,
            Plant = 29,
            HarvestMeat = 30,
            MeatHarvestLevels = 31,
        }

        private static readonly HintMessage.BoxType messageHeaderType = HintMessage.BoxType.BlueBox;
        private static readonly HintMessage.BoxType messageTextType = HintMessage.BoxType.LightBlueBox;
        public static readonly Dictionary<Type, Tutorial> tutorials = new Dictionary<Type, Tutorial> { };

        public readonly struct Tutorial
        {
            public readonly Type type;
            public readonly string name;
            private readonly HintMessage title;
            private readonly List<HintMessage> messages;
            public readonly bool isShownInTutorialsMenu;

            public List<HintMessage> MessagesToDisplay
            {
                get
                {
                    var messagesToDisplay = messages.ToList();
                    messagesToDisplay.Insert(0, this.title);

                    messagesToDisplay = messagesToDisplay.OrderBy(message => message.boxType).ToList();

                    var dialogue = messagesToDisplay.Where(message => message.boxType == HintMessage.BoxType.Dialogue).ToList();
                    var others = messagesToDisplay.Where(message => message.boxType != HintMessage.BoxType.Dialogue).ToList();

                    messagesToDisplay.Clear();
                    messagesToDisplay.AddRange(dialogue);
                    messagesToDisplay.AddRange(others);

                    return messagesToDisplay;
                }
            }

            public List<HintMessage> MessagesToDisplayInMenu
            { get { return this.MessagesToDisplay.Where(message => !message.fieldOnly).ToList(); } }

            public Tutorial(Type type, string name, string title, List<HintMessage> messages, bool isShownInTutorialsMenu = true)
            {
                this.type = type;
                this.name = name;
                this.title = new HintMessage(text: title, boxType: messageHeaderType, blockInput: false, startingSound: SoundData.Name.Notification2);
                this.messages = messages;
                this.isShownInTutorialsMenu = isShownInTutorialsMenu;

                tutorials[type] = this;
            }
        }

        public static void ShowTutorialInMenu(Type type)
        {
            var taskChain = HintMessage.ConvertToTasks(messageList: tutorials[type].MessagesToDisplayInMenu, playSounds: false);
            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }

        public static void ShowTutorialOnTheField(Type type, World world, bool ignoreHintsSetting = false, bool ignoreDelay = false, bool ignoreIfShown = true)
        {
            if (!ignoreHintsSetting && !Preferences.showHints) return;

            HintEngine hintEngine = world.HintEngine;

            if (ignoreIfShown && hintEngine.shownTutorials.Contains(type)) return;

            if (Scheduler.HasTaskChainInQueue || world.Player.sleepMode != Player.SleepMode.Awake || world.CineMode || world.BuildMode) return;
            // only one tutorial / hint should be shown at once - waitingScenes cause playing next scene after turning off CineMode (playing scene without game being paused)

            if (!ignoreDelay)
            {
                if (!hintEngine.WaitFrameReached) return;
                hintEngine.UpdateWaitFrame();
            }

            var messageList = tutorials[type].MessagesToDisplay;
            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true)); // 1 frame delay is needed to allow time for some messages (tired, too dark)
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

            hintEngine.shownTutorials.Add(type);
        }

        public static List<Tutorial> TutorialsInMenu
        { get { return tutorials.Values.Where(tutorial => tutorial.isShownInTutorialsMenu).ToList(); } }

        public static void RefreshData()
        {
            tutorials.Clear();

            new Tutorial(type: Type.BreakThing, name: "destroying items", title: "How to destroy an item on the field.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Make sure that a | tool is selected on toolbar.",
                imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.KnifeSimple) }, boxType: messageTextType),
                new HintMessage(text:"2. Walk next to the item you want to destroy and press |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType)});

            new Tutorial(type: Type.GetWood, name: "acquiring wood", title: "How to acquire wood.",
              messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | axe on toolbar.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone)}, boxType: messageTextType),
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.GlobalCancelReturnSkip)}, boxType: messageTextType),
                Preferences.ShowTouchTips ?
                 new HintMessage(text:"3. Touch the | axe on toolbar to select it.", imageList: new List<Texture2D> {PieceInfo.GetTexture(PieceTemplate.Name.AxeStone)}, boxType: messageTextType):
                new HintMessage(text:"3. Select the | axe using | and |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.AxeStone), InputMapper.GetTexture(InputMapper.Action.ToolbarPrev), InputMapper.GetTexture(InputMapper.Action.ToolbarNext)}, boxType: messageTextType),
                new HintMessage(text:"4. Walk next to the | tree and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TreeBig), InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType)});

            new Tutorial(type: Type.Mine, name: "mining", title: "How to mine.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | pickaxe on toolbar.", imageList: new List<Texture2D>{PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone)}, boxType: messageTextType),
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.GlobalCancelReturnSkip)}, boxType: messageTextType),
                Preferences.ShowTouchTips ?
                new HintMessage(text:"3. Touch the | pickaxe on toolbar to select it.", imageList: new List<Texture2D> {PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone)}, boxType: messageTextType):
                new HintMessage(text:"3. Select the | pickaxe using | and |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), InputMapper.GetTexture(InputMapper.Action.ToolbarPrev), InputMapper.GetTexture(InputMapper.Action.ToolbarNext)}, boxType: messageTextType),
                new HintMessage(text:"4. Walk next to the | mineral deposit and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit), InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType)});

            new Tutorial(type: Type.Torch, name: "using torch", title: "Using torch.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | torch on toolbar.", imageList: new List<Texture2D>{PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall)}, boxType: messageTextType),
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.GlobalCancelReturnSkip)}, boxType: messageTextType),
                Preferences.ShowTouchTips ?
                new HintMessage(text:"3. Touch the | torch on toolbar to select it.", imageList: new List<Texture2D> {PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall)}, boxType: messageTextType):
                new HintMessage(text:"3. Select the | torch using | and |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), InputMapper.GetTexture(InputMapper.Action.ToolbarPrev), InputMapper.GetTexture(InputMapper.Action.ToolbarNext)}, boxType: messageTextType),
                new HintMessage(text:"4. Set the | torch on | fire by pressing |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType),
                new HintMessage(text:"5. To extinguish the | fire,\npress | again (with | torch selected).", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece), PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall)}, boxType: messageTextType),
                new HintMessage(text: "Keep in mind, that the | torch\nwill burn out after some time.", imageList: new List<Texture2D> {PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall)}, boxType: messageTextType)});

            new Tutorial(type: Type.Fireplace, name: "using fireplace", title: "Using fireplace.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Walk next to the | fireplace and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall), InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                new HintMessage(text: "2. Put some | | | fuel inside.\nThe more fuel is inside, the longer it will | burn.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.Coal), AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, boxType: messageTextType),
                new HintMessage(text: "3. Use | to start the fire.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, boxType: messageTextType),
                new HintMessage(text: "You can add or remove | | | fuel at any time.",imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, boxType: messageTextType),
                new HintMessage(text: "Use | to put out the flame instantly.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.WaterDrop].texture}, boxType: messageTextType)});

            new Tutorial(type: Type.TooDarkToReadMap, name: "reading map at night", title: "Reading map at night.",
                messages: new List<HintMessage> {
                new HintMessage(text: "The | map can't be read at night.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Map) }, boxType: messageTextType),
                new HintMessage(text: "You must use a | torch\nor be near to light source |\nto read map at night.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchBig), AnimData.framesForPkgs[AnimData.PkgName.Flame].texture }, boxType: messageTextType)
                });

            new Tutorial(type: Type.TooDarkToSeeAnything, name: "when it is dark", title: "When it is dark.",
                messages: new List<HintMessage> {
                new HintMessage(text: "At night, you cannot:\n-pick up any items ||\n-use most tools ||\n-craft |\n-use other stuff |||", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.Stone), PieceInfo.GetTexture(PieceTemplate.Name.AxeStone), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic), PieceInfo.GetTexture(PieceTemplate.Name.Anvil), PieceInfo.GetTexture(PieceTemplate.Name.ChestIron), PieceInfo.GetTexture(PieceTemplate.Name.CookingPot) }, boxType: messageTextType),
                new HintMessage(text: "However, you can still do these things\nif you're near to a light source | or have a torch |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, PieceInfo.GetTexture(PieceTemplate.Name.TorchBig) }, boxType: messageTextType)
                });

            new Tutorial(type: Type.Heat, name: "heat", title: "Heat.",
                messages: new List<HintMessage> {
                new HintMessage(text: "It is getting really hot.\nI feel dizzy...", boxType: HintMessage.BoxType.Dialogue, fieldOnly: true),
                new HintMessage(text: "At noon, if no clouds are present,\nhigh temperature makes you get tired much faster.", boxType: messageTextType),
                new HintMessage(text: "You can cool yourself for a while by entering water |.",imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.WaterDrop].texture }, boxType: messageTextType),
                new HintMessage(text: "The best solution is to wear equipment, that protects from heat |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HatSimple) }, boxType: messageTextType)
                });

            new Tutorial(type: Type.Equip, name: "using equipment", title: "Using equipment.",
                 messages: new List<HintMessage> {
                     new HintMessage(text: "1. Press | to enter 'inventory' menu.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldInventory)}, boxType: messageTextType),
                     new HintMessage(text: "2. Place the item in its slot (chest, legs, etc).", boxType: messageTextType),
                     new HintMessage(text: "You can also use the 'equip' contextual option for faster equipping.", boxType: messageTextType)
                 });

            new Tutorial(type: Type.Interact, name: "interacting", title: "Interacting with field objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in green can be activated.", boxType: messageTextType),
                    new HintMessage(text: "To activate highlighted object, press |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                    });

            new Tutorial(type: Type.PickUp, name: "picking up items", title: "Picking up objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in blue can be picked up.", boxType: messageTextType),
                    new HintMessage(text: "To pick up highlighted object, press |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldPickUp)}, boxType: messageTextType)});

            new Tutorial(type: Type.Hit, name: "hitting", title: "Hitting objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects with red outline can be hit.", boxType: messageTextType),
                    new HintMessage(text: "To hit highlighted object, press |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType),
                    new HintMessage(text: "You will hit the object using active tool from the toolbar.", boxType: messageTextType)});

            new Tutorial(type: Type.Craft, name: "crafting", title: "Crafting new items.",
                messages: new List<HintMessage> {
                    Preferences.ShowTouchTips ?
                    new HintMessage(text: "1. Select the item you want to craft by pressing it once.", boxType: messageTextType):
                    new HintMessage(text: "1. Select the item you want to craft, using | and |.", imageList: new List<Texture2D> { InputMapper.GetTexture(InputMapper.Action.GlobalUp), InputMapper.GetTexture(InputMapper.Action.GlobalDown)}, boxType: messageTextType),
                    Preferences.ShowTouchTips ?
                    new HintMessage(text: "2. Activate craft by pressing it a second time.", boxType: messageTextType):
                    new HintMessage(text: "2. Activate craft by pressing |.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.GlobalConfirm)}, boxType: messageTextType),
                    new HintMessage(text: "3. If you have all necessary ingredients and some free space\nthe item will be crafted.", boxType: messageTextType)});

            new Tutorial(type: Type.GeneralCraftLevels, name: "general crafting levels", title: "General crafting skill levels.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Your | crafting skills will slowly get better.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMaster) }, boxType: messageTextType),
                    new HintMessage(text: "Increasing your general crafting level will make crafting:\n- | faster\n- | less tiresome\n - | make stronger items sometimes", imageList: new List<Texture2D>{ TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass), TextureBank.GetTexture(TextureBank.TextureName.SimpleSleep), TextureBank.GetTexture(TextureBank.TextureName.SimpleArrowUp) }, boxType: messageTextType),
                 });

            new Tutorial(type: Type.CraftRecipeLevels, name: "craft recipe levels", title: "Craft recipe skill levels.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Every | craft recipe has a skill level.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMaster) }, boxType: messageTextType),
                    new HintMessage(text: "After being crafted a certain number of times,\nthe recipe will level up.", boxType: messageTextType),
                    new HintMessage(text: "With every recipe level, crafting it will require less time and effort.", boxType: messageTextType),
                });

            new Tutorial(type: Type.CookLevels, name: "cooking levels", title: "Cooking skill levels.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Your | cooking skills get better over time.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CookingPot) }, boxType: messageTextType),
                    new HintMessage(text: "Increasing your cooking level will make your | meals:\n- | prepared faster\n- | have more nutritious value\n- | have beneficial effects more often", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Meal), TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass), TextureBank.GetTexture(TextureBank.TextureName.SimpleBurger), TextureBank.GetTexture(TextureBank.TextureName.SimpleArrowUp) }, boxType: messageTextType),
                    new HintMessage(text: "Try using | | | unique ingredients, to level up faster.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.MeatRawPrime), PieceInfo.GetTexture(PieceTemplate.Name.Carrot), PieceInfo.GetTexture(PieceTemplate.Name.Acorn) }, boxType: messageTextType),
                });

            new Tutorial(type: Type.BrewLevels, name: "brewing levels", title: "Brewing skill levels.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Your | brewing skills get better over time.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture }, boxType: messageTextType),
                    new HintMessage(text: "Increasing your brewing level will make your | potions:\n- | prepared faster\n- | have stronger effects\n- | work longer", imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture, TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass), TextureBank.GetTexture(TextureBank.TextureName.SimpleArrowUp), TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass) }, boxType: messageTextType),
                    new HintMessage(text: "Try using unique | | ingredients\nand | | boosters, to level up faster.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRoasted), PieceInfo.GetTexture(PieceTemplate.Name.HerbsCyan), PieceInfo.GetTexture(PieceTemplate.Name.HerbsGreen) }, boxType: messageTextType),
                });

            new Tutorial(type: Type.MeatHarvestLevels, name: "meat harvesting levels", title: "Meat harvesting levels.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Your | meat harvesting skills get better over time.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.MeatRawPrime].texture }, boxType: messageTextType),
                    new HintMessage(text: "With each level, you get a better chance to get | | | bonus items.", imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.MeatRawPrime].texture, AnimData.framesForPkgs[AnimData.PkgName.Fat].texture, AnimData.framesForPkgs[AnimData.PkgName.Leather].texture }, boxType: messageTextType),
                });

            new Tutorial(type: Type.KeepingAnimalsAway, name: "keeping animals away", title: "Keeping animals away.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "When | enemies are nearby,\nyou cannot | craft, | cook or do some other things.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Tiger), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopAdvanced), PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)}, boxType: messageTextType),

                    new HintMessage(text: "To scare off enemies, it is best to build a | campfire\nand | make sure the | fire is burning.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, boxType: messageTextType),
                });

            new Tutorial(type: Type.BuildWorkshop, name: "building a workshop", title: "Building a workshop.",
             messages: new List<HintMessage>  {
                new HintMessage(text: "To build a | workshop,\nenter craft menu and select | 'essential workshop'.\nTo make it, you will need some | wood.",  imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular)}, boxType: messageTextType)
                });

            HintMessage shootingMessage;
            if (Preferences.ShowTouchTips) shootingMessage = new HintMessage(text: "3. Tilt right analog stick in desired direction.", boxType: messageTextType);
            else
            {
                if (Input.currentControlType == Input.ControlType.Gamepad) shootingMessage = new HintMessage(text: "3. Tilt the | in desired direction.", imageList: new List<Texture2D> { InputMapper.GetTexture(InputMapper.Action.WorldCameraMove) }, boxType: messageTextType);
                else
                {
                    var imageList = InputMapper.GetTextures(InputMapper.Action.WorldWalk);
                    string markers = "";
                    for (int i = 0; i < imageList.Count; i++) markers += "|";

                    shootingMessage = new HintMessage(text: $"3. Use {markers} to select direction.", imageList: imageList, boxType: messageTextType);
                }
            }

            new Tutorial(type: Type.ShootProjectile, name: "using projectile weapon", title: "Using a projectile weapon.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "1. Enter inventory and place the | projectile weapon on toolbar.",imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BowBasic)}, boxType: messageTextType),
                    Preferences.ShowTouchTips ?
                    new HintMessage(text:"2. Touch the | projectile weapon on toolbar to select it.", imageList: new List<Texture2D> {PieceInfo.GetTexture(PieceTemplate.Name.BowBasic)}, boxType: messageTextType):
                    new HintMessage(text:"3. Select the | projectile weapon using | and |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BowBasic), InputMapper.GetTexture(InputMapper.Action.ToolbarPrev), InputMapper.GetTexture(InputMapper.Action.ToolbarNext)}, boxType: messageTextType),
                    shootingMessage,
                    Preferences.ShowTouchTips ?
                    new HintMessage(text:"4. Press | to shoot.", imageList: new List<Texture2D> { VirtButton.GetLabelTexture(VButName.Shoot) }, boxType: messageTextType):
                    new HintMessage(text:"4. Press | to start shooting.\nRelease | to shoot.", imageList: new List<Texture2D> {InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece), InputMapper.GetTexture(InputMapper.Action.WorldUseToolbarPiece)}, boxType: messageTextType),
                    });

            new Tutorial(type: Type.Cook, name: "cooking", title: "How to cook.",
                messages: new List<HintMessage>  {
                    new HintMessage(text:"1. Stand next to the | cooking site and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CookingPot), InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                    new HintMessage(text:"2. Place some | | | ingredients into | the cooking site.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatRawPrime), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), PieceInfo.GetTexture(PieceTemplate.Name.Clam), PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)}, boxType: messageTextType),
                    new HintMessage(text: "3. You will also need to place some | | fuel\ninto | the cooking site.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)}, boxType: messageTextType),
                    new HintMessage(text: "4. Use the | flame to start cooking |.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, boxType: messageTextType),
                    });

            new Tutorial(type: Type.HarvestMeat, name: "harvesting meat", title: "Harvesting meat from animals.",
                messages: new List<HintMessage>  {
                    new HintMessage(text:"1. Stand next to the | meat harvesting workshop and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting), InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                    new HintMessage(text:"2. Place an animal | on the table.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Rabbit)}, boxType: messageTextType),
                    new HintMessage(text: "3. Use the | knife to harvest meat.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatHarvestTrigger)}, boxType: messageTextType),
                    });

            new Tutorial(type: Type.PotionBrew, name: "brewing potions", title: "How to make potions.",
                messages: new List<HintMessage>  {
                    new HintMessage(text:$"1. Stand next to the | { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName } and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.AlchemyLabStandard), InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                    new HintMessage(text:$"2. Place ingredients | | | into | the { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName }.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRoasted), PieceInfo.GetTexture(PieceTemplate.Name.AlchemyLabStandard) }, boxType: messageTextType),
                    new HintMessage(text: "3. You will also need to insert some | | fuel.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank) }, boxType: messageTextType),
                    new HintMessage(text:"4. Inserting boosters | | |\nis optional, but beneficial.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsRed), PieceInfo.GetTexture(PieceTemplate.Name.HerbsYellow), PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlue) }, boxType: messageTextType),
                    new HintMessage(text: "5. Use the | flame to start brewing |.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture}, boxType: messageTextType),
                });

            new Tutorial(type: Type.ShakeFruit, name: "getting fruits and vegetables", title: "How to get fruits or vegetables.",
                messages: new List<HintMessage>  {
                    new HintMessage(text:"1. Stand next to the | | | plant,\nthat | | fruits (or | vegetables) grow on and press |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TreeBig), PieceInfo.GetTexture(PieceTemplate.Name.BananaTree), PieceInfo.GetTexture(PieceTemplate.Name.TomatoPlant), PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), InputMapper.GetTexture(InputMapper.Action.WorldInteract)}, boxType: messageTextType),
                    new HintMessage(text: "2. It will fall nearby.", boxType: messageTextType),
             });

            new Tutorial(type: Type.AnimalAttacking, name: "red exclamation mark", title: "Being attacked.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "A | mark means than an animal is attacking you.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BubbleExclamationRed) }, boxType: messageTextType),
                    new HintMessage(text: "You should run away from it, or try to fight it.", boxType: messageTextType)});

            new Tutorial(type: Type.SwampPoison, name: "poisonous swamp", title: "Poisonous swamp.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "Ouch!.\nMy feet burn!", boxType: HintMessage.BoxType.Dialogue, fieldOnly: true),
                    new HintMessage(text: "Swamp areas are poisonous.", boxType: messageTextType),
                    new HintMessage(text: "You need | protective boots, to walk there safely.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BootsProtective)}, boxType: messageTextType),
                    new HintMessage(text: "To get rid of the poison, you need to wait or drink an | antidote potion.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.PotionBlue].texture }, boxType: messageTextType),
                    });

            new Tutorial(type: Type.SmartCrafting, name: "smart crafting", title: "Smart crafting.",
               messages: new List<HintMessage>  {
                    new HintMessage(text: "Sometimes you will use less | | | materials when crafting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.IronNail), PieceInfo.GetTexture(PieceTemplate.Name.Granite)}, boxType: messageTextType),
                    new HintMessage(text: "This is called 'smart crafting'.", boxType: messageTextType),
                    new HintMessage(text: "It depends on each recipe craft level |\nand on your base craft skill.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Star].texture }, boxType: messageTextType),
                   });

            new Tutorial(type: Type.ResourcefulCrafting, name: "resourceful crafting", title: "Resourceful crafting.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "Hmm... I know how to improve my | crafting process.", imageList: new List<Texture2D> { TextureBank.GetTexture("input/VirtButton/craft") }, boxType: HintMessage.BoxType.Dialogue, fieldOnly: true),
                    new HintMessage(text: "After spending some time | crafting,\nyou will unlock resourceful crafting.",  imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopMaster).texture }, boxType: messageTextType),
                    new HintMessage(text: "Then you can use ||| ingredients\nstored inside nearby | chests.", imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.WoodLogRegular).texture, PieceInfo.GetInfo(PieceTemplate.Name.Granite).texture, PieceInfo.GetInfo(PieceTemplate.Name.Clay).texture, PieceInfo.GetInfo(PieceTemplate.Name.ChestWooden).texture }, boxType: messageTextType),
                    new HintMessage(text: "You will see a | marker\nabove every | chest used.", imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.BubbleCraftGreen).texture, PieceInfo.GetInfo(PieceTemplate.Name.ChestWooden).texture }, boxType: messageTextType),
                    });

            new Tutorial(type: Type.HowToSave, name: "saving", title: "Saving the game.",
                messages: new List<HintMessage> {
                new HintMessage(text: "I'm starting to get tired...", boxType: HintMessage.BoxType.Dialogue, fieldOnly: true),
                new HintMessage(text: "You can rest and save your game inside shelter |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentSmall) }, boxType: messageTextType),
                new HintMessage(text: "You can make one using crafting menu.", boxType: messageTextType)
                });

            new Tutorial(type: Type.CombineItems, name: "combining items", title: "Combining items.",
                messages: new List<HintMessage> {
                new HintMessage(text: "You can combine | | two items into | one.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.LanternEmpty), PieceInfo.GetTexture(PieceTemplate.Name.Candle), PieceInfo.GetTexture(PieceTemplate.Name.LanternFull) }, boxType: messageTextType),
                new HintMessage(text: "1. Open inventory.", boxType: messageTextType),
                new HintMessage(text: "2. Pick | one item.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Candle) }, boxType: messageTextType),
                new HintMessage(text: "3. Release it over | the other item.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.LanternEmpty) }, boxType: messageTextType),
                new HintMessage(text: "After confirmation, a | new item will be created.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.LanternFull) }, boxType: messageTextType)});

            new Tutorial(type: Type.Plant, name: "planting", title: "Planting seeds.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1.Craft a | fertile ground.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FertileGroundMedium) }, boxType: messageTextType),
                new HintMessage(text: "2. Stand close to the | fertile ground.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FertileGroundMedium) }, boxType: messageTextType),
                new HintMessage(text: "3. Select the seed | you want to plant\nfrom inventory and use 'plant' option.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.SeedsGeneric) }, boxType: messageTextType),
                new HintMessage(text: "4. Pick a spot, where the | plant is colored green.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoffeeShrub) }, boxType: messageTextType)});

            CheckData();
        }

        private static void CheckData()
        {
            foreach (Type type in (Type[])Enum.GetValues(typeof(Type)))
            {
                if (!tutorials.ContainsKey(type)) throw new ArgumentException($"No tutorial for type {type}.");
            }
        }
    }
}