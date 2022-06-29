﻿using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tutorials
    {
        public enum Type { BreakThing, Equip, BuildWorkshop, GetWood, Mine, Interact, PickUp, Hit, Craft, ShootProjectile, Cook, ShakeFruit, AnimalAttacking, DangerZone, Torch, Fireplace }

        private static readonly HintMessage.BoxType messageHeaderType = HintMessage.BoxType.BlueBox;
        private static readonly HintMessage.BoxType messageTextType = HintMessage.BoxType.LightBlueBox;
        public static readonly Dictionary<Type, Tutorial> tutorials = new Dictionary<Type, Tutorial> { };

        public struct Tutorial
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
                this.title = new HintMessage(text: title, boxType: messageHeaderType);
                this.messages = messages;
                this.isShownInTutorialsMenu = isShownInTutorialsMenu;

                tutorials[type] = this;
            }
        }

        public static void ShowTutorial(Type type, bool ignoreIfShown = true, bool checkHintsSettings = true, bool ignoreDelay = false, bool menuMode = false)
        {
            if (checkHintsSettings && !Preferences.showHints) return;

            World world = World.GetTopWorld();
            HintEngine hintEngine = null;
            if (world != null) hintEngine = world.hintEngine;

            if (ignoreIfShown)
            {
                if (hintEngine != null && hintEngine.shownTutorials.Contains(type)) return;
            }

            if (!ignoreDelay && hintEngine != null)
            {
                if (!hintEngine.WaitFrameReached) return;
                hintEngine.UpdateWaitFrame();
            }

            var messageList = menuMode ? tutorials[type].MessagesToDisplayInMenu : tutorials[type].MessagesToDisplay;
            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            taskChain.Insert(0, new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));
            taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

            if (hintEngine != null) hintEngine.shownTutorials.Add(type);
        }


        public static List<Tutorial> TutorialsInMenu
        { get { return tutorials.Values.Where(tutorial => tutorial.isShownInTutorialsMenu).ToList(); } }


        public static void RefreshData()
        {
            tutorials.Clear();

            new Tutorial(type: Type.BreakThing, name: "destroying items without tools", title: "How to destroy field item without any tools.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Make sure that the | hand tool is selected on toolbar.",
                imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.Hand].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"2. Walk next to the item and press |.", imageList: new List<Texture2D> {ButtonScheme.buttonRT}, boxType: messageTextType) :
                new HintMessage(text:"2. Walk next to the item and press 'USE ITEM' button.", boxType: messageTextType)});

            new Tutorial(type: Type.GetWood, name: "acquiring wood", title: "How to acquire wood.",
              messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | axe on toolbar.", imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.AxeStone].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {ButtonScheme.buttonB}, boxType: messageTextType):
                new HintMessage(text: "2. Exit inventory.", boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"3. Select the | axe using | and |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.AxeStone].texture, ButtonScheme.buttonLB, ButtonScheme.buttonRB}, boxType: messageTextType) :
                new HintMessage(text:"3. Touch the | axe on toolbar to select it.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.AxeStone].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"4. Walk next to the | tree and press |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.TreeBig].texture, ButtonScheme.buttonRT}, boxType: messageTextType) :
                new HintMessage(text:"4. Walk next to the | tree and press 'USE ITEM' button.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.TreeBig].texture}, boxType: messageTextType)});

            new Tutorial(type: Type.Mine, name: "mining", title: "How to mine.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | pickaxe on toolbar.", imageList: new List<Texture2D>{AnimData.framesForPkgs[AnimData.PkgName.PickaxeStone].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {ButtonScheme.buttonB}, boxType: messageTextType):
                new HintMessage(text: "2. Exit inventory.", boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"3. Select the | pickaxe using | and |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.PickaxeStone].texture, ButtonScheme.buttonLB, ButtonScheme.buttonRB}, boxType: messageTextType) :
                new HintMessage(text:"3. Touch the | pickaxe on toolbar to select it.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.PickaxeStone].texture}, boxType: messageTextType),
                 SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"4. Walk next to the | mineral deposit and press |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.IronDeposit].texture, ButtonScheme.buttonRT}, boxType: messageTextType) :
                new HintMessage(text:"4. Walk next to | mineral deposit and press 'USE ITEM' button.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.IronDeposit].texture}, boxType: messageTextType) });

            new Tutorial(type: Type.Torch, name: "using torch", title: "Using torch.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the | torch on toolbar.", imageList: new List<Texture2D>{AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text: "2. Exit inventory by pressing |.", imageList: new List<Texture2D> {ButtonScheme.buttonB}, boxType: messageTextType):
                new HintMessage(text: "2. Exit inventory.", boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"3. Select the | torch using | and |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Torch].texture, ButtonScheme.buttonLB, ButtonScheme.buttonRB}, boxType: messageTextType) :
                new HintMessage(text:"3. Touch the | torch on toolbar to select it.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"4. Set the | torch on | fire by pressing |.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Torch].texture, AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, ButtonScheme.buttonRT}, boxType: messageTextType) :
                new HintMessage(text:"4. Set the | torch on | fire by pressing 'USE ITEM' button.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType),
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text:"5. To extinguish the | fire, press | again (with | torch selected).", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, ButtonScheme.buttonRT, AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType) :
                new HintMessage(text:"5. To extinguish the | fire, press 'USE ITEM' button again (with | torch selected).", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType),
                new HintMessage(text: "Keep in mind, that the | torch will burn out after some time.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Torch].texture}, boxType: messageTextType)});

            new Tutorial(type: Type.Fireplace, name: "using fireplace", title: "Using fireplace.",
                messages: new List<HintMessage> {
                SonOfRobinGame.platform == Platform.Desktop ?
                new HintMessage(text: "1. Walk next to the | fireplace and press |.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Campfire].texture, ButtonScheme.buttonA}, boxType: messageTextType):
                new HintMessage(text: "1. Walk next to the | fireplace and activate it.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Campfire].texture}, boxType: messageTextType),
                new HintMessage(text: "2. Put some | | | fuel inside.\nThe more fuel is inside, the longer it will | burn.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.WoodLog].texture, AnimData.framesForPkgs[AnimData.PkgName.WoodPlank].texture, AnimData.framesForPkgs[AnimData.PkgName.Coal].texture, AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, boxType: messageTextType),
                new HintMessage(text: "3. Use | to start the fire.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, boxType: messageTextType),
                new HintMessage(text: "You can add or remove | | | fuel at any time.",imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.WoodLog].texture, AnimData.framesForPkgs[AnimData.PkgName.WoodPlank].texture, AnimData.framesForPkgs[AnimData.PkgName.Coal].texture }, boxType: messageTextType),
                new HintMessage(text: "Use | to put out the flame instantly.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.WaterDrop].texture}, boxType: messageTextType)});

            new Tutorial(type: Type.Equip, name: "using equipment", title: "Using equipment.",
                 messages: new List<HintMessage> {
                     SonOfRobinGame.platform == Platform.Desktop ?
                     new HintMessage(text: "1. Press | to enter 'equip' menu.", imageList: new List<Texture2D> {ButtonScheme.dpadLeft}, boxType: messageTextType):
                     new HintMessage(text: "1. Press 'equip' button to enter 'equip' menu.", boxType: messageTextType),
                     new HintMessage(text: "2. Place the item in its slot.", boxType: messageTextType) });

            new Tutorial(type: Type.Interact, name: "interacting", title: "Interacting with field objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in green can be activated.", boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text: "To activate highlighted object, press |.", imageList: new List<Texture2D> {ButtonScheme.buttonA}, boxType: messageTextType):
                    new HintMessage(text: "To activate highlighted object, press 'INTERACT' button.", boxType: messageTextType)});

            new Tutorial(type: Type.PickUp, name: "picking up items", title: "Picking up objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in blue can be picked up.", boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text: "To pick up highlighted object, press |.", imageList: new List<Texture2D> {ButtonScheme.buttonX}, boxType: messageTextType):
                    new HintMessage(text: "To pick up highlighted object, press 'PICK UP' button.", boxType: messageTextType)});

            new Tutorial(type: Type.Hit, name: "hitting", title: "Hitting objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects with red outline can be hit.", boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text: "To hit highlighted object, press |.", imageList: new List<Texture2D> {ButtonScheme.buttonRT}, boxType: messageTextType):
                    new HintMessage(text: "To hit highlighted object, press 'USE TOOL' button.", boxType: messageTextType),
                    new HintMessage(text: "You will hit the object using active tool from the toolbar.", boxType: messageTextType)});

            new Tutorial(type: Type.Craft, name: "crafting", title: "Crafting new items.",
                messages: new List<HintMessage> {
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text: "1. Select the item you want to craft, using | and |.", imageList: new List<Texture2D> {ButtonScheme.dpadUp, ButtonScheme.dpadDown}, boxType: messageTextType):
                    new HintMessage(text: "1. Select the item you want to craft by pressing it once.\nYou can use the scroll bar on the right to see more items.", boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text: "2. Activate craft by pressing |.", imageList: new List<Texture2D> {ButtonScheme.buttonA}, boxType: messageTextType):
                    new HintMessage(text: "2. Activate craft by pressing it a second time.", boxType: messageTextType),
                    new HintMessage(text: "3. If you have ingredients needed, the item will be crafted.", boxType: messageTextType)});

            new Tutorial(type: Type.BuildWorkshop, name: "building a workshop", title: "Building a workshop.",
             messages: new List<HintMessage>  {
                new HintMessage(text: "To build a workshop, enter craft menu and select 'crafting workshop'.\nTo make it, you will need some wood.", boxType: messageTextType)
            });

            new Tutorial(type: Type.ShootProjectile, name: "using projectile weapon", title: "Using a projectile weapon.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "1. Enter inventory and place the | | projectile weapon on toolbar.",imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Sling].texture, AnimData.framesForPkgs[AnimData.PkgName.BowWood].texture}, boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text:"3. Select the | | projectile weapon using | and |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Sling].texture, AnimData.framesForPkgs[AnimData.PkgName.BowWood].texture, ButtonScheme.buttonLB, ButtonScheme.buttonRB}, boxType: messageTextType) :
                    new HintMessage(text:"2. Touch the | | projectile weapon on toolbar to select it.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Sling].texture, AnimData.framesForPkgs[AnimData.PkgName.BowWood].texture}, boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text:"3. Hold | pressed and tilt the right | in desired direction.", imageList: new List<Texture2D> {ButtonScheme.buttonLT,  ButtonScheme.rightStick}, boxType: messageTextType) :
                    new HintMessage(text:"3. Tilt right analog stick in desired direction.", boxType: messageTextType),
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text:"4. Press | to shoot.", imageList: new List<Texture2D> {ButtonScheme.buttonRT}, boxType: messageTextType) :
                    new HintMessage(text:"4. Press 'SHOOT' button to shoot.", boxType: messageTextType)});

            new Tutorial(type: Type.Cook, name: "cooking", title: "How to cook.",
                messages: new List<HintMessage>  {
                    SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text:"1. Stand next to the | cooking site and press |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.CookingPot].texture, ButtonScheme.buttonA}, boxType: messageTextType) :
                    new HintMessage(text:"1. Stand next to the | cooking site and press 'INTERACT' button.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.CookingPot].texture}, boxType: messageTextType),
                    new HintMessage(text:"2. Place some | | | ingredients in | storage.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.RawMeat].texture, AnimData.framesForPkgs[AnimData.PkgName.Tomato].texture, AnimData.framesForPkgs[AnimData.PkgName.Herbs].texture, AnimData.framesForPkgs[AnimData.PkgName.CookingPot].texture}, boxType: messageTextType),
                    new HintMessage(text: "3. You will also need to place some | | | fuel\ninto | the cooking site.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.WoodLog].texture, AnimData.framesForPkgs[AnimData.PkgName.WoodPlank].texture, AnimData.framesForPkgs[AnimData.PkgName.Coal].texture, AnimData.framesForPkgs[AnimData.PkgName.CookingPot].texture}, boxType: messageTextType),
                    new HintMessage(text: "4. Use the | flame to start cooking |.", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, AnimData.framesForPkgs[AnimData.PkgName.MealStandard].texture}, boxType: messageTextType),
              });

            new Tutorial(type: Type.ShakeFruit, name: "getting fruits and vegetables", title: "How to get fruits or vegetables.",
                messages: new List<HintMessage>  {
                     SonOfRobinGame.platform == Platform.Desktop ?
                    new HintMessage(text:"1. Stand next to the | | | plant,\nthat | | fruits (or | vegetables) grow on and press |.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.TreeBig].texture, AnimData.framesForPkgs[AnimData.PkgName.PalmTree].texture, AnimData.framesForPkgs[AnimData.PkgName.TomatoPlant].texture, AnimData.framesForPkgs[AnimData.PkgName.Apple].texture, AnimData.framesForPkgs[AnimData.PkgName.Banana].texture, AnimData.framesForPkgs[AnimData.PkgName.Tomato].texture, ButtonScheme.buttonA}, boxType: messageTextType) :
                    new HintMessage(text:"1. Stand next to the | | | plant,\nthat | | fruits (or | vegetables) grow on and press 'INTERACT' button.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.TreeBig].texture, AnimData.framesForPkgs[AnimData.PkgName.PalmTree].texture, AnimData.framesForPkgs[AnimData.PkgName.TomatoPlant].texture, AnimData.framesForPkgs[AnimData.PkgName.Apple].texture, AnimData.framesForPkgs[AnimData.PkgName.Banana].texture, AnimData.framesForPkgs[AnimData.PkgName.Tomato].texture}, boxType: messageTextType),
                    new HintMessage(text: "2. It will fall nearby.", boxType: messageTextType),
             });

            new Tutorial(type: Type.AnimalAttacking, name: "red exclamation mark", title: "Being attacked.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "A | mark means than an animal is attacking | you.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Exclamation].texture, AnimData.framesForPkgs[AnimData.PkgName.Blonde].texture}, boxType: messageTextType),
                    new HintMessage(text: "You should run away from it, or try to fight it.", boxType: messageTextType)});

            new Tutorial(type: Type.DangerZone, name: "danger zones", title: "Danger zones.",
                messages: new List<HintMessage>  {
                    new HintMessage(text: "This dark area is... strange.\nI have a feeling that it is | not safe there.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.BloodSplatter1].texture}, boxType: HintMessage.BoxType.Dialogue, fieldOnly: true),
                    new HintMessage(text: "Darker terrain indicate danger.", boxType: messageTextType),
                    new HintMessage(text: "Some animals will not go outside of these zones.", boxType: messageTextType),
                    new HintMessage(text: "There are some items and plants, that can only be found here.", boxType: messageTextType)});
        }

    }
}
