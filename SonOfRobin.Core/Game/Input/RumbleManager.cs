using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace SonOfRobin
{
    public class RumbleEvent
    {
        private readonly float targetForce;

        public readonly bool bigMotor;
        public readonly bool smallMotor;

        private int durationFramesLeft;
        private int fadeInFramesLeft;
        private int fadeOutFramesLeft;

        private readonly float fadeInValChangePerFrame;
        private readonly float fadeOutValChangePerFrame;
        public float CurrentForce { get; private set; }
        public bool HasEnded { get; private set; }

        public RumbleEvent(float force, float durationSeconds, bool bigMotor = false, bool smallMotor = false, float fadeInSeconds = 0, float fadeOutSeconds = 0, float minSecondsSinceLastRumble = 0f)
        {
            if (!RumbleManager.RumbleIsActive || (!bigMotor && !smallMotor)) return;

            this.targetForce = Math.Max(Math.Min(force, 1f), 0);
            if (this.targetForce == 0) return;

            if (minSecondsSinceLastRumble > 0 && RumbleManager.TimeSinceLastRumbleEvent < TimeSpan.FromSeconds(minSecondsSinceLastRumble)) return;

            durationSeconds = Math.Max(Math.Min(durationSeconds, 10), 0);
            fadeInSeconds = Math.Max(Math.Min(fadeInSeconds, 5), 0);
            fadeOutSeconds = Math.Max(Math.Min(fadeOutSeconds, 5), 0);

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (this.targetForce < 0.65f) return; // to avoid making strong vibration for small values (mobile has no value setting)
                Vibration.Vibrate(TimeSpan.FromSeconds(durationSeconds + fadeInSeconds + fadeOutSeconds)); // vibration on mobile is generally very crude
                return;
            }

            this.bigMotor = bigMotor;
            this.smallMotor = smallMotor;

            int durationFrames = (int)(durationSeconds * 60);
            int fadeInFrames = (int)(fadeInSeconds * 60);
            int fadeOutFrames = (int)(fadeOutSeconds * 60);

            this.durationFramesLeft = durationFrames;
            this.fadeInFramesLeft = fadeInFrames;
            this.fadeOutFramesLeft = fadeOutFrames;

            if (fadeInFrames > 0) this.fadeInValChangePerFrame = force / fadeInFrames;
            if (fadeOutFrames > 0) this.fadeOutValChangePerFrame = force / fadeOutFrames;

            this.CurrentForce = this.fadeInFramesLeft == 0 ? this.targetForce : 0;
            this.HasEnded = false;

            RumbleManager.AddEvent(this);
        }

        public void Update()
        {
            if (this.fadeInFramesLeft > 0)
            {
                this.CurrentForce += this.fadeInValChangePerFrame;
                this.fadeInFramesLeft--;
                return;
            }

            if (this.durationFramesLeft > 0)
            {
                this.CurrentForce = this.targetForce;
                this.durationFramesLeft--;
                return;
            }

            if (this.fadeOutFramesLeft > 0)
            {
                this.CurrentForce -= this.fadeOutValChangePerFrame;
                this.fadeOutFramesLeft--;
                if (this.fadeOutFramesLeft == 0) this.CurrentForce = 0;
            }

            if (this.fadeInFramesLeft <= 0 && this.fadeOutFramesLeft <= 0 && this.durationFramesLeft <= 0) this.HasEnded = true;
        }
    }

    public class RumbleManager
    {
        private static List<RumbleEvent> rumbleEventsList = new List<RumbleEvent>();

        public static float SmallMotorCurrentForce { get; private set; }
        public static float BigMotorCurrentForce { get; private set; }

        public static int EventsCount { get { return rumbleEventsList.Count; } }

        private static DateTime lastEventTime = DateTime.MinValue;

        public static TimeSpan TimeSinceLastRumbleEvent { get { return DateTime.Now - lastEventTime; } }

        public static bool RumbleIsActive
        {
            get
            {
                return Preferences.DebugMode ||
                       (Preferences.rumbleEnabled && (Input.currentControlType == Input.ControlType.Gamepad || SonOfRobinGame.platform == Platform.Mobile));
            }
        }

        public static void AddSimpleRumble(float force, float durationSeconds, bool bigMotor = false, bool smallMotor = false)
        {
            if (!bigMotor && !smallMotor) return;

            new RumbleEvent(force: force, durationSeconds: 0, bigMotor: bigMotor, smallMotor: smallMotor, fadeInSeconds: durationSeconds / 2, fadeOutSeconds: durationSeconds / 2);
        }

        public static void AddEvent(RumbleEvent rumbleEvent)
        {
            rumbleEventsList.Add(rumbleEvent);
            lastEventTime = DateTime.Now;
        }

        public static void StopAll()
        {
            rumbleEventsList.Clear();
        }

        public static void Update()
        {
            var newRumbleEventsList = new List<RumbleEvent>();

            SmallMotorCurrentForce = 0;
            BigMotorCurrentForce = 0;

            foreach (RumbleEvent rumbleEvent in rumbleEventsList)
            {
                rumbleEvent.Update();
                if (!rumbleEvent.HasEnded)
                {
                    if (rumbleEvent.smallMotor) SmallMotorCurrentForce = Math.Max(rumbleEvent.CurrentForce, SmallMotorCurrentForce);
                    if (rumbleEvent.bigMotor) BigMotorCurrentForce = Math.Max(rumbleEvent.CurrentForce, BigMotorCurrentForce);

                    newRumbleEventsList.Add(rumbleEvent);
                }
            }

            Microsoft.Xna.Framework.Input.GamePad.SetVibration(playerIndex: PlayerIndex.One, leftMotor: BigMotorCurrentForce, rightMotor: SmallMotorCurrentForce);

            rumbleEventsList = newRumbleEventsList;
        }
    }
}