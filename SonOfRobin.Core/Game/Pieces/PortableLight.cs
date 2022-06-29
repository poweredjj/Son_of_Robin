using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PortableLight : BoardPiece
    {
        private bool isOn;

        public PortableLight(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, List<BuffEngine.Buff> buffList, string readableName, string description, Category category,
            byte animSize = 0, string animName = "off", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, Yield yield = null, int maxHitPoints = 1, int mass = 1, Scheduler.TaskName toolbarTask = Scheduler.TaskName.SwitchLightSource, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, bool rotatesWhenDropped = false, bool fadeInAnim = false, bool placeAtBeachEdge = false) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, checksFullCollisions: false, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, mass: mass, toolbarTask: toolbarTask, boardTask: boardTask, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, placeAtBeachEdge: placeAtBeachEdge, isShownOnMiniMap: true, buffList: buffList, readableName: readableName, description: description, category: category, activeState: State.Empty)
        {
            this.isOn = false;
        }

        public bool IsOn
        {
            get { return this.isOn; }
            set
            {
                if (this.isOn == value) return;
                this.isOn = value;

                if (this.isOn)
                {
                    PortableLight portableLight;
                    foreach (BoardPiece piece in this.world.player.toolStorage.GetAllPieces())
                    {
                        // making sure that no light source is on
                        if (piece.GetType() == typeof(PortableLight) && piece.id != this.id)
                        {
                            portableLight = (PortableLight)piece;
                            portableLight.IsOn = false;
                        }
                    }

                    // turning on this piece
                    this.sprite.AssignNewName(animName: "on");
                    this.world.player.buffEngine.AddBuffs(world: this.world, this.buffList);

                    var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };
                    new WorldEvent(eventName: WorldEvent.EventName.BurnOutLightSource, world: world, delay: 60, boardPiece: this, eventHelper: damageData);
                }
                else
                {
                    this.sprite.AssignNewName(animName: "off");
                    this.world.player.buffEngine.RemoveBuffs(this.buffList);
                }
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["lightsource_isOn"] = this.isOn;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.isOn = (bool)pieceData["lightsource_isOn"];
        }

    }
}