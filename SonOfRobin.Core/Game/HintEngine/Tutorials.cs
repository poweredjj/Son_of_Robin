using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tutorials
    {
        public enum Type { BreakThing, Equip, BuildWorkshop, GetWood, Mine, Interact, PickUp, Hit, Craft }

        private static readonly HintMessage.Type messageHeaderType = HintMessage.Type.BlueBox;
        private static readonly HintMessage.Type messageTextType = HintMessage.Type.LightBlueBox;
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
                    return messagesToDisplay;
                }
            }

            public Tutorial(Type type, string name, string title, List<HintMessage> messages, bool isShownInTutorialsMenu = true)
            {
                this.type = type;
                this.name = name;
                this.title = new HintMessage(text: title, type: messageHeaderType);
                this.messages = messages;
                this.isShownInTutorialsMenu = isShownInTutorialsMenu;

                tutorials[type] = this;
            }
        }

        public static void ShowTutorial(Type type, bool ignoreIfShown = true, bool checkHintsSettings = true)
        {
            if (checkHintsSettings && !Preferences.showHints) return;

            World world = World.GetTopWorld();

            if (ignoreIfShown)
            {
                if (world != null && world.hintEngine.shownTutorials.Contains(type)) return;
            }

            var messageList = tutorials[type].MessagesToDisplay;
            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);
            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInput: true, executeHelper: taskChain);

            if (world != null) world.hintEngine.shownTutorials.Add(type);
        }


        public static List<Tutorial> TutorialsInMenu
        { get { return tutorials.Values.Where(tutorial => tutorial.isShownInTutorialsMenu).ToList(); } }


        public static void RefreshData()
        {
            tutorials.Clear();

            new Tutorial(type: Type.BreakThing, name: "destroying items without tools", title: "How to destroy field item without any tools.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Make sure that the hand tool is selected on toolbar.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"2. Walk next to the item and press {ButtonScheme.buttonNameRT} button." :
                    "2. Walk next to the item and press 'USE ITEM' button.", type: messageTextType) });

            new Tutorial(type: Type.GetWood, name: "acquiring wood", title: "How to acquire wood.",
              messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the axe on toolbar.", type: messageTextType),
                new HintMessage(text: "2. Exit inventory.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"3. Select the axe using '{ButtonScheme.buttonNameLB}' and '{ButtonScheme.buttonNameRB}' buttons." :"3. Touch the axe on toolbar to select it.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"4. Walk next to the tree and press '{ButtonScheme.buttonNameRT}' button." :
                    "4. Walk next to the tree and press 'USE ITEM' button.", type: messageTextType) });

            new Tutorial(type: Type.Mine, name: "mining", title: "How to mine.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the pickaxe on toolbar.", type: messageTextType),
                new HintMessage(text: "2. Exit inventory.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"3. Select the pickaxe using '{ButtonScheme.buttonNameLB}' and '{ButtonScheme.buttonNameRB}' buttons." :
                    "3. Touch the pickaxe on toolbar to select it.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"4. Walk next to mineral deposit and press '{ButtonScheme.buttonNameRT}' button." :
                    "4. Walk next to mineral deposit and press 'USE ITEM' button.", type: messageTextType) });

            new Tutorial(type: Type.Equip, name: "using equipment", title: "Using equipment.",
              messages: new List<HintMessage> {
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    "1. Press left dpad button to enter 'equip' menu." :
                    "1. Press 'equip' button to enter 'equip' menu.", type: messageTextType),
                new HintMessage(text: "2. Place the item in its slot.", type: messageTextType) });

            new Tutorial(type: Type.Interact, name: "interacting", title: "Interacting with field objects.",
                messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in green can be activated.", type: messageTextType),
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"To activate highlighted object, press '{ButtonScheme.buttonNameA}' button." :
                    "To activate highlighted object, press 'INTERACT' button.", type: messageTextType)});

            new Tutorial(type: Type.PickUp, name: "pick up", title: "Picking up objects.",
              messages: new List<HintMessage> {
                    new HintMessage(text: "Objects highlighted in blue can be picked up.", type: messageTextType),
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"To pick up highlighted object, press '{ButtonScheme.buttonNameX}' button." :
                    "To activate highlighted object, press 'PICK UP' button.", type: messageTextType)  });

            new Tutorial(type: Type.Hit, name: "hit", title: "Hitting objects.",
             messages: new List<HintMessage> {
                    new HintMessage(text: "Objects with red outline can be hit.", type: messageTextType),
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"To hit highlighted object, press '{ButtonScheme.buttonNameRT}' button." :
                    "To hit highlighted object, press 'USE TOOL' button.", type: messageTextType),
                    new HintMessage(text: "You will hit the object using active tool from the toolbar.", type: messageTextType)});

            new Tutorial(type: Type.Craft, name: "craft", title: "Crafting new items.",
            messages: new List<HintMessage> {
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    "1. Select the item you want to craft, using up and down buttons." :
                    "1. Select the item you want to craft by touching it once.\nYou can use the scroll bar on the right to see more items.", type: messageTextType),
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"2. Activate craft by pressing '{ButtonScheme.buttonNameA}' button." :
                    "2. Activate craft by pressing it a second time.", type: messageTextType),
                    new HintMessage(text: "3. If you have needed ingredients, the item will be crafted.", type: messageTextType)});

            new Tutorial(type: Type.BuildWorkshop, name: "building a workshop", title: "Building a workshop",
             messages: new List<HintMessage>  {
                new HintMessage(text: "To build a workshop, enter craft menu and select 'crafting workshop'.\nTo make it, you will need some wood.", type: messageTextType)
            });

        }

    }
}
