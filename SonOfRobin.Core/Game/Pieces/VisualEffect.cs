using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VisualEffect : BoardPiece
    {
        private Tweener tweener;
        public float universalFloat; // universal usage within state machines

        public VisualEffect(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState,
            byte animSize = 0, string animName = "default", bool visible = true, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, visible: visible, activeState: activeState, lightEngine: lightEngine)
        {
            this.universalFloat = 1f;
        }

        public override void SM_ScarePredatorsAway()
        {
            if (this.world.CurrentUpdate % 60 != 0) return;

            var nearbyPieces = this.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 700, compareWithBottom: true);
            var predatorPieces = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal) && ((Animal)piece).Eats.Contains(this.world.Player.name));

            foreach (BoardPiece piece in predatorPieces)
            {
                MessageLog.Add(debugMessage: true, text: $"{Helpers.FirstCharToUpperCase(this.readableName)} - scaring off {piece.readableName}.");

                Animal animal = (Animal)piece;
                animal.target = this;
                animal.aiData.Reset();
                animal.activeState = State.AnimalFlee;
            }
        }

        public override void SM_MapMarkerShowAndCheck()
        {
            if (this.world.CurrentUpdate % 10 != 0) return;

            this.sprite.opacity = 1;

            if (!this.world.Player.buffEngine.HasBuff(BuffEngine.BuffType.EnableMap) ||
                !this.world.map.CheckIfPlayerCanReadTheMap(showMessage: false))
            {
                this.sprite.opacity = 0;
                return;
            }

            if (Preferences.destroyMapMarkerWhenReached && Vector2.Distance(this.sprite.position, this.world.Player.sprite.position) < 50)
            {
                this.Destroy(); // will be destroyed right away if map was enabled by using god mode
                this.world.map.soundMarkerRemove.Play();
                MessageLog.Add(text: "Map marker has been reached.", texture: PieceInfo.GetTexture(PieceTemplate.Name.MapMarker));
            }
        }

        public override void SM_SeaWaveMove()
        {
            Rectangle cameraIntersectRect = this.sprite.GfxRect;
            cameraIntersectRect.Inflate(cameraIntersectRect.Width, 0);
            if (!this.world.camera.viewRect.Intersects(cameraIntersectRect)) return; // sprite.IsInCameraRect would leave some "stuck" waves at camera edges

            if (this.tweener == null) this.tweener = new Tweener();

            Tween tweenPos = this.tweener.FindTween(target: this.sprite, memberName: "position");
            if (tweenPos == null)
            {
                Vector2 waveTarget = new Vector2(this.world.ActiveLevel.width / 2, this.world.ActiveLevel.height / 2);
                if (this.world.weather.WindPercentage > 0)
                {
                    Vector2 windTarget = new Vector2(this.world.ActiveLevel.width * this.world.weather.WindOriginX, this.world.ActiveLevel.height * this.world.weather.WindOriginY);
                    waveTarget = Vector2.Lerp(waveTarget, windTarget, this.world.weather.WindPercentage);
                }

                float angle = Helpers.GetAngleBetweenTwoPoints(start: this.sprite.position, end: waveTarget);

                this.sprite.opacity = 0f;
                this.sprite.rotation = angle;
                Vector2 beachPos = this.sprite.position;

                int oneStepDistance = 50;
                int maxDistance = 600;
                Vector2 oneStepOffset = new Vector2((int)Math.Round(oneStepDistance * Math.Cos(angle)), (int)Math.Round(oneStepDistance * Math.Sin(angle)));

                for (int distance = 0; distance < maxDistance; distance += oneStepDistance)
                {
                    beachPos += oneStepOffset;
                    if (this.level.grid.terrainByName[Terrain.Name.Height].GetMapData((int)beachPos.X, (int)beachPos.Y) > Terrain.waterLevelMax) break;
                }

                float delay = this.world.random.Next(15);
                float duration = this.world.random.Next(4, 8);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: beachPos, duration: duration, delay: delay)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticOut);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.opacity, toValue: 0.7f, duration: duration / 5, delay: delay)
                    .Easing(EasingFunctions.QuadraticIn);

                this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Ambient);
            }
            else
            {
                if (tweenPos.Completion >= 0.7f || !this.sprite.IsInWater)
                {
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Ambient);

                    Tween tweenOpacity = this.tweener.FindTween(target: this.sprite, memberName: "opacity");
                    if (tweenOpacity == null || tweenOpacity.IsComplete)
                    {
                        this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.opacity, toValue: 0f, duration: tweenPos.Duration / 3, delay: 0)
                            .Easing(EasingFunctions.QuadraticOut);
                    }
                }
                else
                {
                    if (this.sprite.opacity > 0)
                    {
                        ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterWaveDraw, particlesToEmit: 1, duration: (int)(this.sprite.opacity * 5));
                        if (this.sprite.opacity > 0.3f) ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterWaveDistort, particlesToEmit: 1, duration: (int)(this.sprite.opacity * 3));
                    }
                }
            }

            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly

            // pushing player / animals

            if (this.level.levelType == Level.LevelType.Island && this.sprite.opacity > 0.5f && this.world.CurrentUpdate % 15 == 0 && this.world.random.Next(2) == 0)
            {
                List<Sprite> collidingSpritesList = this.sprite.GetCollidingSpritesAtPosition(positionToCheck: this.sprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.Visible });

                foreach (Sprite collidingSprite in collidingSpritesList)
                {
                    BoardPiece collidingPiece = collidingSprite.boardPiece;

                    if (collidingPiece.pieceInfo.getsPushedByWaves && !collidingPiece.HasPassiveMovement && Vector2.Distance(this.sprite.position, collidingSprite.position) <= 50)
                    {
                        collidingPiece.activeSoundPack.Play(PieceSoundPackTemplate.Action.SwimShallow);

                        float angle = Helpers.GetAngleBetweenTwoPoints(start: collidingSprite.position, end: new Vector2(this.world.ActiveLevel.width / 2, this.world.ActiveLevel.height / 2));
                        int pushDistance = this.world.random.Next(400, 800);

                        Vector2 pushMovement = new Vector2((int)Math.Round(pushDistance * Math.Cos(angle)), (int)Math.Round(pushDistance * Math.Sin(angle)));

                        collidingPiece.AddPassiveMovement(movement: Helpers.VectorKeepBelowSetValue(vector: pushMovement, maxVal: 400f));

                        ParticleEngine.TurnOn(sprite: collidingSprite, preset: ParticleEngine.Preset.WaterWalk, particlesToEmit: 15, duration: 7);
                        ParticleEngine.TurnOn(sprite: collidingSprite, preset: ParticleEngine.Preset.WaterDistortWalk, particlesToEmit: 23, duration: 7);

                        // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Wave - adding movement {Math.Round(pushMovement.X, 1)},{Math.Round(pushMovement.Y, 1)} {collidingPiece.name}");
                    }
                }
            }
        }

        public override void SM_FogMoveRandomly()
        {
            // Suitable only for passive temporary decorations, that will never be moved "manually".
            // It should only be used for temporary decorations or pieces that will not get saved.

            if (this.pieceInfo.visFogExplodesWhenBurns && this.IsBurning)
            {
                this.world.HintEngine.Disable(PieceHint.Type.ExplosiveGas);

                BoardPiece explosion = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.sprite.position, templateName: PieceTemplate.Name.Explosion, closestFreeSpot: true);
                explosion.sprite.AssignNewSize(3);
                new RumbleEvent(force: 1f, bigMotor: true, fadeInSeconds: 0.25f, durationSeconds: 0, fadeOutSeconds: 0.25f);

                var piecesWithinRange = this.level.grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: this.sprite, distance: 230, compareWithBottom: true);
                foreach (BoardPiece piece in piecesWithinRange)
                {
                    if (piece.pieceInfo.fireAffinity > 0) piece.HeatLevel += 2;
                }

                this.HeatLevel = 0;
                this.sprite.color = Color.Yellow;
                this.sprite.opacity = 1f;

                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 60, destroyPiece: true);
                this.RemoveFromStateMachines();

                return;
            }

            if (!this.sprite.IsInCameraRect) return;

            if (this.tweener == null && this.sprite.opacityFade == null) // tween should not start before opacityFade finishes
            {
                this.tweener = new Tweener();

                int maxDistance = 180;

                Vector2 newPos = this.sprite.position + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));
                newPos = this.sprite.world.KeepVector2InWorldBounds(newPos);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: newPos,
                    duration: this.world.random.Next(6, 15), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.rotation, toValue: (float)(Random.NextSingle() * 2) - 1,
                    duration: this.world.random.Next(6, 15), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.opacity, toValue: (float)(Random.NextSingle() * 0.3) + 0.2f,
                    duration: this.world.random.Next(4, 20), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            if (this.tweener != null)
            {
                this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
                this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly
            }
        }

        public override void SM_WeatherFogMoveRandomly()
        {
            // Suitable only for passive temporary decorations, that will never be moved "manually".
            // It should only be used for temporary decorations or pieces that will not get saved.

            if (!this.sprite.IsInCameraRect) return;

            if (this.tweener == null)
            {
                this.tweener = new Tweener();
                int maxDistance = 300;

                Vector2 newPos = this.sprite.position + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));
                newPos = this.sprite.world.KeepVector2InWorldBounds(newPos);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: newPos,
                    duration: this.world.random.Next(30, 60), delay: 0)
                    .RepeatForever(repeatDelay: 0.0f)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly
        }

        public override void SM_EmitParticles()
        {
            if (!this.sprite.particleEngine.HasAnyParticles) this.Destroy();
        }

        public override void SM_HasteCloneFollowPlayer()
        {
            if (!this.world.Player.buffEngine.HasBuff(BuffEngine.BuffType.Haste) && this.sprite.opacityFade == null)
            {
                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 60, destroyPiece: true);
            }

            this.sprite.AssignFrameForce(this.world.Player.sprite.AnimFrame);
        }

        public override void SM_EndingBoatCruise()
        {
            this.sprite.Move(new Vector2(1, 0));
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterCruiseCine, particlesToEmit: 6, duration: 15);
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.DistortCruiseCine, particlesToEmit: 6, duration: 15);

            if (this.world.weather.RainPercentage > 0.7f && this.world.weather.WindPercentage > 0.7f) ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.WaterSplashCine, particlesToEmit: 20, duration: 1);
        }

        public override void SM_OscillateAroundTarget()
        {
            if (this.visualAid == null || !this.visualAid.exists || !this.visualAid.sprite.IsOnBoard)
            {
                this.Destroy();
                return;
            }

            if (this.tweener == null)
            {
                this.sprite.SetNewPosition(this.visualAid.sprite.position);
                this.tweener = new Tweener();
            }

            Tween tweenPos = this.tweener.FindTween(target: this.sprite, memberName: "position");
            if (tweenPos == null)
            {
                Vector2 targetPosDiff = this.visualAid.sprite.position - this.sprite.position;

                int baseOffsetDist = (int)(30 * this.universalFloat);

                int xDist = (int)Math.Abs(this.sprite.position.X - this.visualAid.sprite.position.X);
                int yDist = (int)Math.Abs(this.sprite.position.Y - this.visualAid.sprite.position.Y);

                int xOffset = this.world.random.Next(baseOffsetDist / 3, (int)(baseOffsetDist * 1.4f * this.universalFloat));
                int yOffset = this.world.random.Next(baseOffsetDist / 3, (int)(baseOffsetDist * 1.4f * this.universalFloat));

                if (this.sprite.position.X > this.visualAid.sprite.position.X) xOffset *= -1; // placing on opposite side of target
                if (this.sprite.position.Y > this.visualAid.sprite.position.Y) yOffset *= -1; // placing on opposite side of target

                Vector2 newPos = this.visualAid.sprite.position + new Vector2(xOffset, yOffset);

                this.tweener.TweenTo(target: this.sprite, expression: sprite => sprite.position, toValue: newPos,
                    duration: (this.universalFloat * 0.2f) + (this.world.random.NextSingle() * 0.3f), delay: 0)
                    .Easing(EasingFunctions.QuadraticInOut);
            }

            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.sprite.SetNewPosition(this.sprite.position); // to update grid, because tweener will change the position directly
        }
    }
}