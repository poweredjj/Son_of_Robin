using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AmbientLight
    {
        public readonly struct SunLightData
        {
            private static readonly SunLightData noSun = new(timeOfDay: TimeSpan.FromHours(0), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsOpacity: 0f, shadowBlurSize: 0f);

            private static readonly SunLightData[] sunDataArray = new SunLightData[]
            {
                new SunLightData(timeOfDay: TimeSpan.FromHours(0), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsOpacity: 0f, shadowBlurSize: 0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(4), sunPos: new Vector2(400, -400), sunShadowsLength: 3f, sunShadowsOpacity: 0f, shadowBlurSize: 6.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(5), sunPos: new Vector2(350, -225), sunShadowsLength: 2.65f, sunShadowsOpacity:  0.4f, shadowBlurSize: 4.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(6), sunPos: new Vector2(300, -50), sunShadowsLength: 2.3f, sunShadowsOpacity:  0.4f, shadowBlurSize: 1.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(12), sunPos: new Vector2(0, 1000), sunShadowsLength: 0.2f, sunShadowsOpacity:  0.6f, shadowBlurSize: 0.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(18), sunPos: new Vector2(-300, -50), sunShadowsLength: 2.3f, sunShadowsOpacity:  0.4f, shadowBlurSize: 4.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(20), sunPos: new Vector2(-400, -400), sunShadowsLength: 3f, sunShadowsOpacity: 0f, shadowBlurSize: 6.0f),
                new SunLightData(timeOfDay: TimeSpan.FromHours(24), sunPos: Vector2.Zero, sunShadowsLength: 0f, sunShadowsOpacity: 0f, shadowBlurSize: 0f),
            };

            public readonly TimeSpan timeOfDay;
            public readonly Vector2 sunPos;
            public readonly float sunShadowsLength;
            public readonly float sunShadowsOpacity;
            public readonly float shadowBlurSize;

            public SunLightData(TimeSpan timeOfDay, Vector2 sunPos, float sunShadowsLength, float sunShadowsOpacity, float shadowBlurSize)
            {
                this.timeOfDay = timeOfDay;
                this.sunPos = sunPos;
                this.sunShadowsLength = sunShadowsLength;
                this.sunShadowsOpacity = sunShadowsOpacity;
                this.shadowBlurSize = shadowBlurSize;
            }

            public static SunLightData MergeTwo(TimeSpan timeOfDay, SunLightData sunLightData1, SunLightData sunLightData2, float firstOpacity, float secondOpacity)
            {
                return new SunLightData(
                    timeOfDay: timeOfDay,
                    sunPos: (sunLightData1.sunPos * firstOpacity) + (sunLightData2.sunPos * secondOpacity),
                    sunShadowsLength: (sunLightData1.sunShadowsLength * firstOpacity) + (sunLightData2.sunShadowsLength * secondOpacity),
                    sunShadowsOpacity: (sunLightData1.sunShadowsOpacity * firstOpacity) + (sunLightData2.sunShadowsOpacity * secondOpacity),
                    shadowBlurSize: (sunLightData1.shadowBlurSize * firstOpacity) + (sunLightData2.shadowBlurSize * secondOpacity));
            }

            public static SunLightData CalculateSunLight(DateTime currentDateTime, Weather weather)
            {
                TimeSpan currentTimeOfDay = currentDateTime.TimeOfDay;
                if ((currentTimeOfDay < TimeSpan.FromHours(4) || currentTimeOfDay > TimeSpan.FromHours(20)) && weather.LightningPercentage == 0) return noSun;  // to avoid checking these ranges

                if (weather.LightningPercentage > 0)
                {
                    int sunPosX = (int)(((weather.LightningPosMultiplier.X * 2) - 1f) * 400);
                    int sunPosY = (int)(((weather.LightningPosMultiplier.Y * 2) - 1f) * 400);

                    return new SunLightData(timeOfDay: currentTimeOfDay, sunPos: new Vector2(sunPosX, sunPosY), sunShadowsLength: 2.5f, sunShadowsOpacity: 0.75f, shadowBlurSize: 0f);
                }

                SunLightData prevLightData, nextLightData;

                for (int i = 1; i <= sunDataArray.Length; i++) // from 1 to count of elements, to properly check the whole range
                {
                    // setting next and previous light values

                    if (i < lightDataArray.Length) nextLightData = sunDataArray[i];
                    else nextLightData = sunDataArray[0];

                    prevLightData = sunDataArray[i - 1];

                    // checking for time match

                    if (prevLightData.timeOfDay == currentTimeOfDay) return prevLightData;
                    else if (nextLightData.timeOfDay == currentTimeOfDay) return nextLightData;
                    else if (prevLightData.timeOfDay < currentTimeOfDay && currentTimeOfDay < nextLightData.timeOfDay)
                    {
                        // calculating values

                        float minutesBetween = (float)(nextLightData.timeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes);
                        float prevColorOpacity = 1f - (float)((currentTimeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes) / minutesBetween);
                        float nextColorOpacity = 1f - (float)((nextLightData.timeOfDay.TotalMinutes - currentTimeOfDay.TotalMinutes) / minutesBetween);

                        return MergeTwo(timeOfDay: currentTimeOfDay, sunLightData1: prevLightData, sunLightData2: nextLightData, firstOpacity: prevColorOpacity, secondOpacity: nextColorOpacity);
                    }
                }

                return noSun;
            }
        }

        public readonly struct AmbientLightData
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

        private static readonly Dictionary<Level.LevelType, AmbientLightData> staticAmbientLightForLevelType = new Dictionary<Level.LevelType, AmbientLightData>
        {
            { Level.LevelType.Cave, new AmbientLightData(timeOfDay: TimeSpan.FromHours(0), darknessColor: Color.Black * 0.85f, lightColor: Color.Transparent) },
        };

        private static readonly AmbientLightData[] lightDataArray = new AmbientLightData[]
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

        public static AmbientLightData CalculateLightAndDarknessColors(DateTime currentDateTime, Weather weather, Level level)
        {
            if (staticAmbientLightForLevelType.ContainsKey(level.levelType)) return staticAmbientLightForLevelType[level.levelType];

            AmbientLightData rawAmbientLightData = CalculateRawLightAndDarknessColors(currentDateTime: currentDateTime);

            if (weather.CloudsPercentage == 0 && weather.LightningPercentage == 0) return rawAmbientLightData;

            float cloudsPercentage = weather.CloudsPercentage;
            float lightningPercentage = weather.LightningPercentage;

            Color lightColor = rawAmbientLightData.lightColor;
            Color darknessColor = rawAmbientLightData.darknessColor;

            if (cloudsPercentage > 0)
            {
                lightColor = Helpers.Blend2Colors(firstColor: lightColor, secondColor: Color.Transparent, firstColorOpacity: 1 - cloudsPercentage, secondColorOpacity: cloudsPercentage);
                darknessColor = Helpers.DarkenFirstColorWithSecond(firstColor: darknessColor, secondColor: Color.Black * 0.4f, firstColorOpacity: 1 - cloudsPercentage, secondColorOpacity: cloudsPercentage);
            }

            if (lightningPercentage > 0)
            {
                lightColor = Helpers.Blend2Colors(firstColor: lightColor, secondColor: new Color(181, 237, 255) * 2f, firstColorOpacity: 1 - lightningPercentage, secondColorOpacity: lightningPercentage);
                darknessColor = Helpers.Blend2Colors(firstColor: darknessColor, secondColor: Color.Transparent, firstColorOpacity: 1 - lightningPercentage, secondColorOpacity: lightningPercentage);
            }

            return new AmbientLightData(timeOfDay: currentDateTime.TimeOfDay, darknessColor: darknessColor, lightColor: lightColor);
        }

        private static AmbientLightData CalculateRawLightAndDarknessColors(DateTime currentDateTime)
        {
            // raw colors - without taking weather effects into account

            TimeSpan currentTimeOfDay = currentDateTime.TimeOfDay;

            // return new AmbientLightData(timeOfDay: currentTimeOfDay, darknessColor: Color.Black * 0.7f, lightColor: Color.Transparent); // for testing

            for (int i = 1; i <= lightDataArray.Length; i++) // from 1 to count of elements, to properly check the whole range
            {
                // setting next and previous ambient light values

                // after the last element, the first one should be used (with added 24h)
                AmbientLightData nextLightData = i < lightDataArray.Length ? lightDataArray[i] : AmbientLightData.MakeCopyPlus24H(lightDataArray[0]);
                AmbientLightData prevLightData = lightDataArray[i - 1];

                // checking for time match

                if (prevLightData.timeOfDay == currentTimeOfDay) return prevLightData;
                else if (nextLightData.timeOfDay == currentTimeOfDay) return nextLightData;
                else if (prevLightData.timeOfDay < currentTimeOfDay && currentTimeOfDay < nextLightData.timeOfDay)
                {
                    if (nextLightData.lightColor == prevLightData.lightColor &&
                        nextLightData.darknessColor == prevLightData.darknessColor) return nextLightData;

                    // calculating final color values

                    float minutesBetween = (float)(nextLightData.timeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes);
                    float prevColorOpacity = 1f - (float)((currentTimeOfDay.TotalMinutes - prevLightData.timeOfDay.TotalMinutes) / minutesBetween);
                    float nextColorOpacity = 1f - (float)((nextLightData.timeOfDay.TotalMinutes - currentTimeOfDay.TotalMinutes) / minutesBetween);

                    Color newLightColor = Helpers.Blend2Colors(firstColor: prevLightData.lightColor, secondColor: nextLightData.lightColor, firstColorOpacity: prevColorOpacity, secondColorOpacity: nextColorOpacity);

                    Color newDarkColor = Helpers.Blend2Colors(firstColor: prevLightData.darknessColor, secondColor: nextLightData.darknessColor, firstColorOpacity: prevColorOpacity, secondColorOpacity: nextColorOpacity);

                    return new AmbientLightData(timeOfDay: currentTimeOfDay, darknessColor: newDarkColor, lightColor: newLightColor);
                }
            }

            throw new ArgumentException($"Cannot calculate ambient color for time {currentTimeOfDay}.");
        }
    }
}