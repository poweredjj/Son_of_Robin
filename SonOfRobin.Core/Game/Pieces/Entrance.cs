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
             byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false) :

             base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, rotatesWhenDropped: rotatesWhenDropped)
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

                this.targetLevel = new Level(type: this.levelType, world: this.world, seed: this.level.random.Next(1, 100000), width: 5000, height: 5000);
                Grid grid = new Grid(level: this.targetLevel, resDivider: this.world.resDivider);
                this.targetLevel.AssignGrid(grid);
            }

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
    }
}