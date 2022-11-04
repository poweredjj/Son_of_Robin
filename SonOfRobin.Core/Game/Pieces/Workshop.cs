using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public class Workshop : BoardPiece
    {
        public readonly MenuTemplate.Name craftMenuTemplate;
        public readonly bool emitsLightWhenCrafting;

        public Workshop(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, Dictionary<byte, int> maxMassBySize, MenuTemplate.Name craftMenuTemplate, string readableName, string description, Category category,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false, bool emitsLightWhenCrafting = false, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, readableName: readableName, description: description, category: category, lightEngine: lightEngine, activeState: State.Empty)
        {
            this.boardTask = Scheduler.TaskName.OpenCraftMenu;
            this.craftMenuTemplate = craftMenuTemplate;
            this.emitsLightWhenCrafting = emitsLightWhenCrafting;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // data to deserialize here
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(animName: "on");
            if (this.sprite.lightEngine != null) this.sprite.lightEngine.Activate();
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(animName: "off");
            if (this.sprite.lightEngine != null) this.sprite.lightEngine.Deactivate();
        }

    }
}