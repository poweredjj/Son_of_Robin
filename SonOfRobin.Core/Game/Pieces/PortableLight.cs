﻿using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PortableLight : BoardPiece
    {
        private bool isOn;
        public readonly bool canBeUsedDuringRain;
        public readonly PieceTemplate.Name convertsToWhenUsedUp;
        private readonly LightEngine storedLightEngine;

        public PortableLight(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool canBeUsedDuringRain, LightEngine storedLightEngine,
            byte animSize = 0, string animName = "off", bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, Scheduler.TaskName toolbarTask = Scheduler.TaskName.SwitchLightSource, bool rotatesWhenDropped = false, PieceTemplate.Name convertsToWhenUsedUp = PieceTemplate.Name.Empty, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, yield: yield, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty, soundPack: soundPack)
        {
            this.storedLightEngine = storedLightEngine;

            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireSmall, ignore3DAlways: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire, ignore3DAlways: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Torch, maxPitchVariation: 0.5f, isLooped: true, ignore3DAlways: true));

            this.isOn = false;
            this.canBeUsedDuringRain = canBeUsedDuringRain;
            this.convertsToWhenUsedUp = convertsToWhenUsedUp;
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
                    this.sprite.AssignNewName(newAnimName: "on");

                    Sprite playerSprite = this.world.Player.sprite;

                    playerSprite.lightEngine = this.storedLightEngine;
                    playerSprite.lightEngine.AssignSprite(playerSprite);
                    playerSprite.lightEngine.Activate();
                    this.soundPack.Play(PieceSoundPack.Action.TurnOn);
                    this.soundPack.Play(action: PieceSoundPack.Action.IsOn);

                    var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };
                    new WorldEvent(eventName: WorldEvent.EventName.BurnOutLightSource, world: world, delay: 60, boardPiece: this, eventHelper: damageData);
                }
                else
                {
                    this.sprite.AssignNewName(newAnimName: "off");
                    this.world.Player.sprite.lightEngine.Deactivate();
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