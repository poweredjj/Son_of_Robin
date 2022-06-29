using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tutorials
    {
        public enum Type { BreakThing, Equip, BuildWorkshop, GetWood, Mine }

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
                    "3. Select the axe using L1 and R1 buttons." :"3. Touch the axe on toolbar to select it.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"4. Walk next to the tree and press {ButtonScheme.buttonNameRT} button." :
                    "4. Walk next to the tree and press 'USE ITEM' button.", type: messageTextType) });

            new Tutorial(type: Type.Mine, name: "mining", title: "How to mine.",
                messages: new List<HintMessage> {
                new HintMessage(text: "1. Enter inventory and place the pickaxe on toolbar.", type: messageTextType),
                new HintMessage(text: "2. Exit inventory.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    "3. Select the pickaxe using L1 and R1 buttons." :
                    "3. Touch the pickaxe on toolbar to select it.", type: messageTextType),
                new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    $"4. Walk next to mineral deposit and press {ButtonScheme.buttonNameRT} button." :
                    "4. Walk next to mineral deposit and press 'USE ITEM' button.", type: messageTextType) });

            new Tutorial(type: Type.Equip, name: "using equipment", title: "Using equipment.",
              messages: new List<HintMessage> {
                    new HintMessage(text: SonOfRobinGame.platform == Platform.Desktop ?
                    "1. Press left dpad button to enter 'equip' menu." :
                    "1. Press 'equip' button to enter 'equip' menu.", type: messageTextType),
                new HintMessage(text: "2. Place the item in its slot.", type: messageTextType) });

            new Tutorial(type: Type.BuildWorkshop, name: "building a workshop", title: "Building a workshop",
             messages: new List<HintMessage>  {
                new HintMessage(text: "To build a workshop, enter craft menu and select 'crafting workshop'.\nTo make it, you will need some wood.", type: messageTextType)
            });

        }

    }
}
