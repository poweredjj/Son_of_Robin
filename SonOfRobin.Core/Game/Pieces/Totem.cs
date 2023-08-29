using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Totem : BoardPiece
    {
        private DateTime lockedUntilIslandDateTime;
        public bool CanBeUsedNow { get { return this.world.islandClock.IslandDateTime >= this.lockedUntilIslandDateTime; } }
        public TimeSpan TimeUntilCanBeUsed { get { return this.lockedUntilIslandDateTime - this.world.islandClock.IslandDateTime; } }

        public Totem(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
             int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, soundPack: soundPack, lightEngine: new LightEngine(size: 410, opacity: 0.7f, colorActive: true, color: new Color(71, 255, 255) * 1.2f, isActive: false, castShadows: true))
        {
            this.PieceStorage = new PieceStorage(width: 3, height: 4, storagePiece: this, storageType: PieceStorage.StorageType.Totem);
            this.lockedUntilIslandDateTime = DateTime.MinValue;

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot offerTriggerSlot = this.PieceStorage.GetSlot(1, 0);
            offerTriggerSlot.allowedPieceNames = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.OfferTrigger };
            BoardPiece offerTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.OfferTrigger, world: this.world);
            offerTriggerSlot.hidden = false;
            offerTriggerSlot.locked = false;
            offerTriggerSlot.AddPiece(offerTrigger);
            offerTriggerSlot.locked = true;

            for (int x = 0; x < 3; x++)
            {
                StorageSlot slot = this.PieceStorage.GetSlot(x, 3);
                slot.locked = false;
                slot.hidden = false;
                slot.stackLimit = 1;
            }
        }

        public void Offer()
        {
            // checking offered pieces

            var offeredPieces = new List<BoardPiece>();

            foreach (StorageSlot slot in this.PieceStorage.OccupiedSlots)
            {
                if (!slot.locked) offeredPieces.AddRange(slot.GetAllPieces(remove: true));
            }

            if (offeredPieces.Count == 0)
            {
                new TextWindow(text: "There is nothing to offer.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            // exiting inventory

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);

            // calculating good offering mass

            float goodOfferingMass = 0;
            foreach (BoardPiece offeredPiece in offeredPieces)
            {
                bool isFood = offeredPiece.pieceInfo.toolbarTask == Scheduler.TaskName.GetEaten;

                if (isFood) goodOfferingMass += offeredPiece.Mass;
                else if (offeredPiece.GetType() == typeof(Animal)) goodOfferingMass += 100;
                else goodOfferingMass -= 50;
            }
            if (goodOfferingMass == 0) goodOfferingMass = -50; // to make sure that mass is always positive or negative

            // calculating variables

            int burnDurationFrames = 60 * 3;
            int eventsDelay = burnDurationFrames + 60;
            bool offeringAccepted = goodOfferingMass > 0;
            TimeSpan weatherChangeDelay = IslandClock.ConvertUpdatesCountToTimeSpan(eventsDelay);
            TimeSpan weatherChangeDuration = GetWeatherChangeDuration(goodOfferingMass);
            this.lockedUntilIslandDateTime = this.world.islandClock.IslandDateTime + weatherChangeDuration;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"goodOfferingMass {goodOfferingMass} delay {weatherChangeDelay} timespan {weatherChangeDuration}");

            // first phase - burning the offering

            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.BurnFlame, duration: burnDurationFrames, particlesToEmit: 2);
            this.sprite.lightEngine.Activate();
            Sound.QuickPlay(SoundData.Name.FireBurst);
            Sound.QuickPlay(SoundData.Name.Bonfire);

            SolidColor colorOverlay = new(color: offeringAccepted ? new Color(179, 255, 255) : Color.Black, viewOpacity: 0.0f);
            colorOverlay.transManager.AddTransition(new Transition(transManager: colorOverlay.transManager, outTrans: true, startDelay: burnDurationFrames, duration: 60 * 9, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.75f, endRemoveScene: true));
            this.world.solidColorManager.Add(colorOverlay);

            this.world.worldEventManager.RemovePieceFromQueue(this); // just in case

            new WorldEvent(eventName: WorldEvent.EventName.TotemAffectWeather, world: this.world, delay: IslandClock.ConvertTimeSpanToUpdates(weatherChangeDelay), boardPiece: this, eventHelper: goodOfferingMass);

            new WorldEvent(eventName: WorldEvent.EventName.SwitchLightEngine, world: this.world, delay: IslandClock.ConvertTimeSpanToUpdates(weatherChangeDuration + weatherChangeDelay), boardPiece: this, eventHelper: false);
        }

        public void AffectWeather(float goodOfferingMass)
        {
            // second phase - affecting weather

            bool offeringAccepted = goodOfferingMass > 0;
            TimeSpan weatherChangeDuration = GetWeatherChangeDuration(goodOfferingMass);

            Sound.QuickPlay(offeringAccepted ? SoundData.Name.AngelChorus : SoundData.Name.CreepyCave);

            if (offeringAccepted) this.world.weather.RemoveAllEventsForDuration(weatherChangeDuration);
            else this.world.weather.CreateVeryBadWeatherForDuration(weatherChangeDuration);
        }

        private static TimeSpan GetWeatherChangeDuration(float goodOfferingMass)
        {
            return TimeSpan.FromMinutes(Math.Max(Math.Abs(goodOfferingMass) * 1.6f, 15));
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["totem_lockedUntilIslandDateTime"] = this.lockedUntilIslandDateTime;
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.lockedUntilIslandDateTime = (DateTime)pieceData["totem_lockedUntilIslandDateTime"];
        }
    }
}