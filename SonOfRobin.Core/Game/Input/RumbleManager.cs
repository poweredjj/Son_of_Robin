using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using Xamarin.Essentials;

namespace SonOfRobin
{
    public class RumbleManager
    {
        private Tweener tweener;
        public float leftMotor;
        public float rightMotor;
        private DateTime lastRumbleAdded;

        private TimeSpan TimeSinceLastRumble { get { return DateTime.Now - lastRumbleAdded; } }

        public RumbleManager()
        {
            this.tweener = new Tweener();
            this.leftMotor = 0f;
            this.rightMotor = 0f;
            this.lastRumbleAdded = DateTime.MinValue;
        }

        public void AddRumble(float value, float durationSeconds, bool bigMotor = false, bool smallMotor = false, float minSecondsSinceLastRumble = 0)
        {
            if (!Preferences.rumbleEnabled ||
                (!bigMotor && !smallMotor) ||
                (SonOfRobinGame.platform != Platform.Mobile && Input.currentControlType != Input.ControlType.Gamepad)) return;

            if (minSecondsSinceLastRumble > 0 && this.TimeSinceLastRumble < TimeSpan.FromSeconds(minSecondsSinceLastRumble)) return;

            this.lastRumbleAdded = DateTime.Now;

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (value < 0.65f) return; // to avoid making strong vibration for small values (mobile has no value setting)
                Vibration.Vibrate(TimeSpan.FromSeconds(durationSeconds));
                return;
            }

            if (bigMotor)
            {
                Tween existingTween = this.tweener.FindTween(target: this, memberName: "leftMotor");

                if (existingTween == null || !existingTween.IsAlive)
                {
                    this.tweener.TweenTo(target: this, expression: rumbleMan => rumbleMan.leftMotor, toValue: value, duration: durationSeconds / 2, delay: 0)
                        .AutoReverse().Easing(EasingFunctions.SineInOut);
                }
            }
            if (smallMotor)
            {
                Tween existingTween = this.tweener.FindTween(target: this, memberName: "rightMotor");

                if (existingTween == null || !existingTween.IsAlive)
                {
                    this.tweener.TweenTo(target: this, expression: rumbleMan => rumbleMan.rightMotor, toValue: value, duration: durationSeconds / 2, delay: 0)
                        .AutoReverse().Easing(EasingFunctions.SineInOut);
                }
            }
        }

        public void StopAll()
        {
            this.tweener.CancelAndCompleteAll();
            this.leftMotor = 0f;
            this.rightMotor = 0f;
        }

        public void Update()
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            Microsoft.Xna.Framework.Input.GamePad.SetVibration(playerIndex: PlayerIndex.One, leftMotor: this.leftMotor, rightMotor: this.rightMotor);
        }
    }
}