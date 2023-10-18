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
        private readonly bool goesDown;
        private readonly Level.LevelType levelType;

        public Entrance(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool goesDown, Level.LevelType levelType,
             byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false, State activeState = State.Empty) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: activeState, rotatesWhenDropped: rotatesWhenDropped)
        {
            this.levelType = levelType;
            this.goesDown = goesDown;
            this.isBlocked = false;

            if (!this.goesDown && this.level != null) this.targetLevel = this.level.parentLevel;
        }

        public void Enter()
        {
            if (this.isBlocked)
            {
                new TextWindow(text: $"I can't enter this | cave, because the entrance has crumbled...", imageList: new List<Texture2D> { this.sprite.CroppedAnimFrame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                return;
            }

            if (this.targetLevel == null)
            {
                if (this.level.levelType == Level.LevelType.Island)
                {
                    this.sprite.AssignNewName(newAnimName: "blocked", checkForCollision: false);
                    this.isBlocked = true;
                }

                int levelSize = 6000 + (4000 * (this.level.depth - 1));

                this.targetLevel = new Level(type: this.levelType, world: this.world, seed: this.world.random.Next(1, 9999), width: levelSize, height: levelSize);
            }

            if (this.targetLevel.depth == 0 && Scene.GetTopSceneOfType(type: typeof(Menu), includeEndingScenes: true) == null)
            {
                var confirmationData = new Dictionary<string, Object> {
                            { "question", "Want to exit? You won't be able to come back." },
                            { "taskName", Scheduler.TaskName.UseEntrance },
                            { "executeHelper", this }, { "blocksUpdatesBelow", true } };

                MenuTemplate.CreateConfirmationMenu(confirmationData: confirmationData);
                return;
            }

            string levelText = this.targetLevel.depth == 0 ? "" : $" level {this.targetLevel.depth} seed {this.targetLevel.seed}";
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
            if (this.isBlocked)
            {
                new Yield().DropDebris(piece: this, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DustPuff }, particlesToEmit: 120);
                new Yield().DropDebris(piece: this, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.SmokePuff }, particlesToEmit: 40);

                new OpacityFade(sprite: this.sprite, destOpacity: 0, duration: 60 * 8, destroyPiece: true);
                this.activeState = State.Empty;
            }
        }
    }
}