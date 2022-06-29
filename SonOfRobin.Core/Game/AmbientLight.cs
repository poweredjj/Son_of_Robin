using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AmbientLight
    {
        public struct SunLightData
        {
            private static readonly SunLightData noSun = new SunLightData(timeOfDay: TimeSpan.FromHours(0), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsColor: Color.Transparent);

            private static readonly List<SunLightData> sunDataList = new List<SunLightData>
            {
                new SunLightData(timeOfDay: TimeSpan.FromHours(0), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsColor: Color.Transparent),
                new SunLightData(timeOfDay: TimeSpan.FromHours(4), sunPos: new Vector2(400, -400), sunShadowsLength: 3f, sunShadowsColor: Color.Transparent),
                new SunLightData(timeOfDay: TimeSpan.FromHours(5), sunPos: new Vector2(350, -225), sunShadowsLength: 2.65f, sunShadowsColor: Color.Black * 0.4f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(6), sunPos: new Vector2(300, -50), sunShadowsLength: 2.3f, sunShadowsColor: Color.Black * 0.4f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(12), sunPos: new Vector2(0, 1000), sunShadowsLength: 0.2f, sunShadowsColor: Color.Black * 0.6f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(18), sunPos: new Vector2(-300, -50), sunShadowsLength: 2.3f, sunShadowsColor: Color.Black * 0.4f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(20), sunPos: new Vector2(-400, -400), sunShadowsLength: 3f, sunShadowsColor: Color.Transparent),
                new SunLightData(timeOfDay: TimeSpan.FromHours(24), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsColor: Color.Transparent),
            };

            public readonly TimeSpan timeOfDay;
            public readonly Vector2 sunPos;
            public readonly float sunShadowsLength;
            public readonly Color sunShadowsColor;

            public SunLightData(TimeSpan timeOfDay, Vector2 sunPos, float sunShadowsLength, Color sunShadowsColor)
            {
                this.timeOfDay = timeOfDay;
                this.sunPos = sunPos;
                this.sunShadowsLength = sunShadowsLength;
                this.sunShadowsColor = sunShadowsColor;
            }

            public static SunLightData CalculateSunLight(DateTime currentDateTime)
            {
                TimeSpan currentTimeOfDay = currentDateTime.TimeOfDay;

                if (currentTimeOfDay < TimeSpan.FromHours(4) || currentTimeOfDay > TimeSpan.FromHours(20)) return noSun;  // to avoid checking these ranges

                SunLightData prevLightData, nextLightData;

                for (int i = 1; i <= sunDataList.Count; i++) // from 1 to count of elements, to properly check the whole range
                {
                    // setting next and previous light values

                    if (i < lightDataList.Count) nextLightData = sunDataList[i];
                    else nextLightData = sunDataList[0];

                    prevLightData = sunDataList[i - 1];

                    // checking for time match

                    if (prevLightData.timeOfDay == currentTimeOfDay) return prevLightData;
                    else if (nextLightData.timeOfDay == currentTimeOfDay) return nextLightData;
                    else if (prevLightData.timeOfDay < currentTimeOfDay && currentTimeOfDay < nextLightData.timeOfDay)
                    {
                        // calculating values

                        float minutesBetween = (float)(nextLightData.timeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes);
                        float prevColorOpacity = 1f - (float)((currentTimeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes) / minutesBetween);
                        float nextColorOpacity = 1f - (float)((nextLightData.timeOfDay.TotalMinutes - currentTimeOfDay.TotalMinutes) / minutesBetween);

                        Color newSunShadowsColor = new Color(
                            (byte)((prevLightData.sunShadowsColor.R * prevColorOpacity) + (nextLightData.sunShadowsColor.R * nextColorOpacity)),
                            (byte)((prevLightData.sunShadowsColor.G * prevColorOpacity) + (nextLightData.sunShadowsColor.G * nextColorOpacity)),
                            (byte)((prevLightData.sunShadowsColor.B * prevColorOpacity) + (nextLightData.sunShadowsColor.B * nextColorOpacity)),
                            (byte)((prevLightData.sunShadowsColor.A * prevColorOpacity) + (nextLightData.sunShadowsColor.A * nextColorOpacity))
                            );

                        float newSunShadowsLength = (prevLightData.sunShadowsLength * prevColorOpacity) + (nextLightData.sunShadowsLength * nextColorOpacity);
                        Vector2 newSunPos = (prevLightData.sunPos * prevColorOpacity) + (nextLightData.sunPos * nextColorOpacity);

                        return new SunLightData(timeOfDay: currentTimeOfDay, sunPos: newSunPos, sunShadowsLength: newSunShadowsLength, sunShadowsColor: newSunShadowsColor);
                    }
                }

                return noSun;
            }
        }

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
                return new AmbientLightData(
                    timeOfDay: ambientLightData.timeOfDay + TimeSpan.FromDays(1),
                    darknessColor: ambientLightData.darknessColor,
                    lightColor: ambientLightData.lightColor);
            }
        }

        private static readonly List<AmbientLightData> lightDataList = new List<AmbientLightData>
        {
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(0), darknessColor: Color.Black * 0.9f, lightColor: Color.Transparent),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(3), darknessColor: Color.Black * 0.8f, lightColor: Color.Transparent),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(4), darknessColor: Color.DarkBlue * 0.75f, lightColor: Color.Transparent),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(5), darknessColor: Color.Brown * 0.6f, lightColor: Color.Orange * 0.4f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(6), darknessColor: Color.Transparent, lightColor: Color.Gold * 0.25f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(6.5), darknessColor: Color.Transparent, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(12), darknessColor: Color.Transparent, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(15), darknessColor: Color.Transparent, lightColor: Color.White * 0.1f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(16.5), darknessColor: Color.Transparent, lightColor: Color.Gold * 0.25f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(18), darknessColor:Color.Brown * 0.6f, lightColor: Color.Orange * 0.4f),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(19), darknessColor: Color.DarkBlue * 0.75f, lightColor: Color.Transparent),
            new AmbientLightData(timeOfDay: TimeSpan.FromHours(20), darknessColor: Color.Black * 0.8f, lightColor: Color.Transparent),
        };

        public static AmbientLightData CalculateLightAndDarknessColors(DateTime currentDateTime)
        {
            TimeSpan currentTimeOfDay = currentDateTime.TimeOfDay;

            //   return new AmbientLightData(timeOfDay: currentTimeOfDay, darknessColor: Color.Black, lightColor: Color.Transparent); // for testing

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
