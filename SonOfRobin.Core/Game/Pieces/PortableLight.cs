using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PortableLight : BoardPiece
    {
        private bool isOn;
        public readonly bool canBeUsedDuringRain;

        public PortableLight(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, List<Buff> buffList, string readableName, string description, Category category, bool canBeUsedDuringRain,
            byte animSize = 0, string animName = "off", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, byte stackSize = 1, Yield yield = null, int maxHitPoints = 1, int mass = 1, Scheduler.TaskName toolbarTask = Scheduler.TaskName.SwitchLightSource, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, bool rotatesWhenDropped = false, bool fadeInAnim = false) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassBySize: null, generation: generation, stackSize: stackSize, canBePickedUp: true, yield: yield, maxHitPoints: maxHitPoints, mass: mass, toolbarTask: toolbarTask, boardTask: boardTask, rotatesWhenDropped: rotatesWhenDropped, fadeInAnim: fadeInAnim, buffList: buffList, readableName: readableName, description: description, category: category, activeState: State.Empty)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireSmall, ignore3DAlways: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, ignore3DAlways: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Torch, maxPitchVariation: 0.5f, isLooped: true, ignore3DAlways: true));

            this.isOn = false;
            this.canBeUsedDuringRain = canBeUsedDuringRain;
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
                    foreach (BoardPiece piece in this.world.Player.ToolStorage.GetAllPieces())
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
                    this.world.Player.buffEngine.AddBuffs(world: this.world, this.buffList);
                    this.soundPack.Play(PieceSoundPack.Action.TurnOn);
                    this.soundPack.Play(action: PieceSoundPack.Action.IsOn);

                    var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };
                    new WorldEvent(eventName: WorldEvent.EventName.BurnOutLightSource, world: world, delay: 60, boardPiece: this, eventHelper: damageData);
                }
                else
                {
                    this.sprite.AssignNewName(animName: "off");
                    this.world.Player.buffEngine.RemoveBuffs(this.buffList);
                    this.soundPack.Stop(PieceSoundPack.Action.IsOn);
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff);
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