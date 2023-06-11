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

        public RumbleManager()
        {
            this.tweener = new Tweener();
            this.leftMotor = 0f;
            this.rightMotor = 0f;
        }

        public void AddRumble(float value, float durationSeconds, bool bigMotor = false, bool smallMotor = false)
        {
            if (!Preferences.rumbleEnabled || (!bigMotor && !smallMotor)) return;

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
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

        public void Update()
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            Microsoft.Xna.Framework.Input.GamePad.SetVibration(playerIndex: PlayerIndex.One, leftMotor: this.leftMotor, rightMotor: this.rightMotor);
        }
    }
}