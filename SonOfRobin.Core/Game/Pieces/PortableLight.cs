using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PortableLight : BoardPiece
    {
        private bool isOn;
        public readonly bool canBeUsedDuringRain;
        public readonly PieceTemplate.Name convertsToWhenUsedUp;
        private readonly LightEngine storedLightEngine;

        public PortableLight(World world, int id, AnimDataNew.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, bool canBeUsedDuringRain, LightEngine storedLightEngine,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, bool rotatesWhenDropped = false, PieceTemplate.Name convertsToWhenUsedUp = PieceTemplate.Name.Empty) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty)
        {
            this.storedLightEngine = storedLightEngine;

            this.isOn = false;
            this.canBeUsedDuringRain = canBeUsedDuringRain;
            this.convertsToWhenUsedUp = convertsToWhenUsedUp;
        }

        public bool CanBurnNow
        {
            get
            {
                bool cannotBurnNow =
                    this.world.Player == null ||
                    !this.world.Player.alive ||
                    !this.world.Player.exists ||
                    this.world.Player.sprite.IsInWater ||
                    this.world.Player.sleepMode != Player.SleepMode.Awake ||
                    !this.IsOnPlayersToolbar ||
                    (!this.canBeUsedDuringRain && this.world.weather.IsRaining);

                return !cannotBurnNow;
            }
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
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOn);
                    this.activeSoundPack.Play(action: PieceSoundPackTemplate.Action.IsOn);

                    var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };
                    new LevelEvent(eventName: LevelEvent.EventName.BurnOutLightSource, level: this.level, delay: 60, boardPiece: this, eventHelper: damageData);
                }
                else
                {
                    this.sprite.AssignNewName(newAnimName: "off");
                    this.world.Player.sprite.lightEngine.Deactivate();
                    this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.IsOn);
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOff);
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