using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class LoadingTips
    {
        private static readonly TimeSpan singleTipMaxDuration = TimeSpan.FromSeconds(30);
        private static string currentTip;
        private static DateTime lastTipGenerated = new DateTime(1900, 1, 1);

        private static readonly List<string> allTips = new List<string>
        {
            "Opened chests are always empty.",
            "Some food must be cooked before eating.",
            "Crates washed ashore contain valuable items.",
            "Herbs can be used to make potions and during cooking.",
            "Poisoned meal can be used to poison an animal.",
            "At night, you need a light source (like a torch) to do most things.",
            "A map makes navigating the island much easier.",
            "You can make a bigger backpack to carry more items.",
            "You can make a bigger belt to carry more tools.",
            "Dig sites contain different items, depending on the area.",
            "Tools will eventually break down from use.",

            // TODO add more tips
        };

        private static List<string> tipsToDisplay = new List<string>();

        public static string GetTip()
        {
            if (DateTime.Now - lastTipGenerated < singleTipMaxDuration) return currentTip;

            if (tipsToDisplay.Count == 0) tipsToDisplay = allTips.ToList();

            int randomTipNo = SonOfRobinGame.random.Next(0, tipsToDisplay.Count);

            currentTip = tipsToDisplay[randomTipNo];
            tipsToDisplay.RemoveAt(randomTipNo);

            lastTipGenerated = DateTime.Now;

            return currentTip;
        }
    }
}