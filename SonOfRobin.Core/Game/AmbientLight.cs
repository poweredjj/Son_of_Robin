using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AmbientLight
    {
        public struct AmbientLightData
        {
            public readonly TimeSpan timeOfDay;
            public readonly Color darknessColor;
            public readonly Color lightColor;
            public AmbientLightData(TimeSpan timeOfDay, Color darknessColor, Color lightColor)
            {
                this.timeOfDay = timeOfDay;
                this.darknessColor = darknessColor;
                this.lightColor = lightColor;
            }

            public static AmbientLightData MakeCopyPlus24H(AmbientLightData ambientLightData)
            {
                return new AmbientLightData(timeOfDay: ambientLightData.timeOfDay + TimeSpan.FromDays(1), darknessColor: ambientLightData.darknessColor, lightColor: ambientLightData.lightColor);
            }
        }

        private static readonly List<AmbientLightData> lightDataList = new List<AmbientLightData>
        {
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(0), darknessColor: Color.Black * 0.8f, lightColor: Color.White * 0f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(3), darknessColor: Color.Black * 0.75f, lightColor: Color.White * 0f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(4), darknessColor: Color.DarkBlue * 0.4f, lightColor: Color.White * 0f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(5), darknessColor: Color.White * 0f, lightColor: Color.DarkOrange * 0.3f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(6), darknessColor: Color.Gold * 0.2f, lightColor: Color.Gold * 0.3f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(6.5), darknessColor: Color.White * 0f, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(12), darknessColor: Color.White * 0f, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(15), darknessColor: Color.White * 0f, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(16.5), darknessColor: Color.Gold * 0.2f, lightColor: Color.Gold * 0.3f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(18), darknessColor: Color.White * 0f, lightColor: Color.DarkOrange * 0.3f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(19), darknessColor: Color.DarkBlue * 0.4f, lightColor: Color.White * 0f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(20), darknessColor: Color.Black * 0.75f, lightColor: Color.White * 0f),
        };

        public static AmbientLightData CalculateLightAndDarknessColors(DateTime currentDateTime)
        {
            TimeSpan currentTimeOfDay = currentDateTime.TimeOfDay;

            AmbientLightData prevLightData, nextLightData;
            float minutesBetween, prevColorOpacity, nextColorOpacity;

            for (int i = 1; i <= lightDataList.Count; i++) // from 1 to count of elements, to properly check the whole range
            {
                // setting next and previous ambient light values

                if (i < lightDataList.Count) nextLightData = lightDataList[i];
                else nextLightData = AmbientLightData.MakeCopyPlus24H(lightDataList[0]); // after the last element, the first one should be used (with added 24h)

                prevLightData = lightDataList[i - 1];

                // checking for time match

                if (prevLightData.timeOfDay == currentTimeOfDay) return prevLightData;
                else if (nextLightData.timeOfDay == currentTimeOfDay) return nextLightData;
                else if (prevLightData.timeOfDay < currentTimeOfDay && currentTimeOfDay < nextLightData.timeOfDay)
                {
                    if (nextLightData.lightColor == prevLightData.lightColor &&
                        nextLightData.darknessColor == prevLightData.darknessColor) return nextLightData;

                    // calculating final color values

                    minutesBetween = (float)(nextLightData.timeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes);
                    prevColorOpacity = 1f - (float)((currentTimeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes) / minutesBetween);
                    nextColorOpacity = 1f - (float)((nextLightData.timeOfDay.TotalMinutes - currentTimeOfDay.TotalMinutes) / minutesBetween);

                    Color newLightColor = new Color(
                        (byte)((prevLightData.lightColor.R * prevColorOpacity) + (nextLightData.lightColor.R * nextColorOpacity)),
                        (byte)((prevLightData.lightColor.G * prevColorOpacity) + (nextLightData.lightColor.G * nextColorOpacity)),
                        (byte)((prevLightData.lightColor.B * prevColorOpacity) + (nextLightData.lightColor.B * nextColorOpacity)),
                        (byte)((prevLightData.lightColor.A * prevColorOpacity) + (nextLightData.lightColor.A * nextColorOpacity))
                        );

                    Color newDarkColor = new Color(
                        (byte)((prevLightData.darknessColor.R * prevColorOpacity) + (nextLightData.darknessColor.R * nextColorOpacity)),
                        (byte)((prevLightData.darknessColor.G * prevColorOpacity) + (nextLightData.darknessColor.G * nextColorOpacity)),
                        (byte)((prevLightData.darknessColor.B * prevColorOpacity) + (nextLightData.darknessColor.B * nextColorOpacity)),
                        (byte)((prevLightData.darknessColor.A * prevColorOpacity) + (nextLightData.darknessColor.A * nextColorOpacity))
                        );

                    return new AmbientLightData(timeOfDay: currentTimeOfDay, darknessColor: newDarkColor, lightColor: newLightColor);
                }
            }

            throw new ArgumentException($"Cannot calculate ambient color for time {currentTimeOfDay}.");
        }

    }
}
