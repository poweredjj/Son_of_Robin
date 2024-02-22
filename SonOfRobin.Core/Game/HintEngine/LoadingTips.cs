using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class LoadingTips
    {
        private static readonly TimeSpan singleTipMaxDuration = TimeSpan.FromSeconds(30);
        private static string currentTip;
        private static DateTime lastTipGenerated = DateTime.MinValue;

        private static readonly List<string> allTips = new()
        {
            "Some food types must be cooked before eating.",
            "Crates washed ashore contain valuable items.",
            "Herbs can be used to make potions.",
            "At night, you need a light source (like a torch) to do most things.",
            "A map makes navigating the island much easier.",
            "You can make a bigger backpack to carry more items.",
            "You can make a bigger belt to carry more tools.",
            "Dig sites contain different items, depending on the area.",
            "You need a shovel to dig up treasure.",
            "Tools will eventually break down from use.",
            "You can use a regular axe to mine stones, but it will be less effective than a pickaxe.",
            "Wood and stone pickaxes are too weak to mine crystals.",
            "To get herbs, cut down plants with a scythe.",
            "You can find mineral deposits in the mountains.",
            "You need protective boots, to walk safely on swamp areas.",
            "You will walk slowly in the mountains, unless you have proper boots.",
            "A campfire scares animals away.",
            "Workshops are needed to make most things.",
            "Dried meat can be eaten safely and you can carry a lot of it.",
            "Potions can be made in alchemy lab.",
            "Equipment can be made using leather workshop.",
            "Sometimes you will use less ingredients when crafting - it is called 'smart crafting'.",
            "If you run out of tools, you can acquire wood by using your basic knife.",
            "At noon, you will get tired much faster.",
            "You can get rid of poison by drinking antidote.",
            "You are immune to heat, while being wet.",
            "You have to use meat workshop to get meat from an animal.",
            "Old equipment can be ripped apart to recover materials.",
            "You cannot craft when inside cave.",
            "Cave entrance will crumble after going out.",
            "Running low on ram? Try disabling demo world.",
        };

        private static List<string> tipsToDisplay = new();

        public static void ChangeTip()
        {
            lastTipGenerated = new DateTime(1900, 1, 1); // this will force getting next tip at GetTip()
        }

        public static string GetTip()
        {
            if (DateTime.Now - lastTipGenerated < singleTipMaxDuration) return currentTip;

            if (tipsToDisplay.Count == 0) tipsToDisplay = allTips.ToList();

            int randomTipNo = SonOfRobinGame.random.Next(tipsToDisplay.Count);

            currentTip = tipsToDisplay[randomTipNo];
            tipsToDisplay.RemoveAt(randomTipNo);

            lastTipGenerated = DateTime.Now;

            return currentTip;
        }
    }
}