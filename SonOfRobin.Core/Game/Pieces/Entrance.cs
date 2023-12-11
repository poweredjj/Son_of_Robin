using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Entrance : BoardPiece
    {
        private Level targetLevel;
        private bool isBlocked;
        private readonly bool hasWater;
        private readonly bool goesDown;
        private readonly int maxDepth;
        private readonly Level.LevelType levelType;

        public Entrance(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool goesDown, bool hasWater, Level.LevelType levelType, int maxDepth,
             byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false, State activeState = State.Empty) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: activeState, rotatesWhenDropped: rotatesWhenDropped)
        {
            this.levelType = levelType;
            this.goesDown = goesDown;
            this.hasWater = hasWater;
            this.isBlocked = false;
            this.maxDepth = maxDepth;

            if (!this.goesDown && this.level != null) this.targetLevel = this.level.parentLevel;
        }

        public void Enter()
        {
            if (Preferences.showHints && !this.world.HintEngine.shownTutorials.Contains(Tutorials.Type.Caves))
            {
                Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Caves, world: this.world, ignoreDelay: true);
                return;
            }

            if (this.isBlocked)
            {
                new TextWindow(text: "I can't enter this | cave, because the entrance has crumbled...", imageList: new List<Texture2D> { this.sprite.CroppedAnimFrame.Texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                return;
            }

            if (this.targetLevel == null)
            {
                if (this.level.levelType == Level.LevelType.Island)
                {
                    this.sprite.AssignNewName(newAnimName: "blocked", checkForCollision: false);
                    this.isBlocked = true;
                    this.world.Player.cavesVisited++;
                }

                int levelSize = 7000 + (1200 * (this.level.depth - 1));
                this.targetLevel = new Level(type: this.levelType, world: this.world, seed: this.world.random.Next(1, 9999), width: levelSize, height: levelSize, hasWater: this.hasWater);
            }

            if (this.targetLevel.depth == 0 && Scene.GetTopSceneOfType(type: typeof(Menu), includeEndingScenes: true) == null)
            {
                var confirmationData = new Dictionary<string, Object> { { "taskName", Scheduler.TaskName.UseEntrance }, { "executeHelper", this } };
                MenuTemplate.CreateConfirmationMenu(question: "Want to exit? You won't be able to come back.", confirmationData: confirmationData, blocksUpdatesBelow: true);
                return;
            }

            string levelText = this.targetLevel.depth == 0 ? "" : $" level {this.targetLevel.depth}";
            string fullText = $"Entering {this.targetLevel.levelType.ToString().ToLower()}{levelText}.";
            if (this.targetLevel.depth == 0) fullText = "Going out.";

            MessageLog.Add(text: fullText, texture: PieceInfo.GetTexture(this.name), avoidDuplicates: true);

            this.world.EnterNewLevel(this.targetLevel);
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["entrance_isBlocked"] = this.isBlocked;
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.isBlocked = (bool)pieceData["entrance_isBlocked"];
        }

        public override void SM_CaveEntranceDisappear()
        {
            if (this.level.depth >= this.maxDepth)
            {
                this.Destroy(); // removing itself, if would lead too deep
                return;
            }

            if (this.isBlocked)
            {
                Sound.QuickPlay(SoundData.Name.StonesFalling);
                new RumbleEvent(force: 0.15f, bigMotor: true, smallMotor: false, fadeInSeconds: 0.8f, durationSeconds: 0f, fadeOutSeconds: 0.8f);
                new RumbleEvent(force: 0.10f, bigMotor: false, smallMotor: true, fadeInSeconds: 1.2f, durationSeconds: 0f, fadeOutSeconds: 1.3f);

                Scheduler.ExecutionDelegate addRumbleDlgt1 = () =>
                { new RumbleEvent(force: 0.15f, smallMotor: true, bigMotor: true, fadeInSeconds: 0.0f, durationSeconds: 0.0f, fadeOutSeconds: 0.4f); };
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 120, executeHelper: addRumbleDlgt1);

                Scheduler.ExecutionDelegate addRumbleDlgt2 = () =>
                { new RumbleEvent(force: 0.12f, smallMotor: true, bigMotor: true, fadeInSeconds: 0.0f, durationSeconds: 0.0f, fadeOutSeconds: 0.25f); };
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 150, executeHelper: addRumbleDlgt2);

                new Yield().DropDebris(piece: this, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DustPuff }, particlesToEmit: 120);
                new Yield().DropDebris(piece: this, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.SmokePuff }, particlesToEmit: 40);

                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 60 * 8, destroyPiece: true);
                this.activeState = State.Empty;
            }
        }
    }
}