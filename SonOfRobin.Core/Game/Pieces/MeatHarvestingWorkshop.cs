using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeatHarvestingWorkshop : BoardPiece
    {
        private StorageSlot AnimalSlot
        { get { return this.PieceStorage.GetSlot(1, 0); } }

        private StorageSlot TriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private List<StorageSlot> meatSlots;

        public MeatHarvestingWorkshop(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, soundPack: soundPack)
        {
            this.PieceStorage = new PieceStorage(width: 4, height: 3, storagePiece: this, storageType: PieceStorage.StorageType.Processing);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot harvestTriggerSlot = this.TriggerSlot;
            harvestTriggerSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.MeatHarvestTrigger };
            BoardPiece harvestTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.MeatHarvestTrigger, world: this.world);
            harvestTriggerSlot.hidden = false;
            harvestTriggerSlot.locked = false;
            harvestTriggerSlot.AddPiece(harvestTrigger);
            harvestTriggerSlot.locked = true;

            StorageSlot animalSlot = this.AnimalSlot;
            animalSlot.hidden = false;
            animalSlot.locked = false;
            animalSlot.stackLimit = 1;
            animalSlot.label = "animal";
            animalSlot.allowedPieceNames = new List<PieceTemplate.Name>();
            foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
            {
                if (PieceInfo.TryToGetInfo(pieceName)?.type == typeof(Animal)) animalSlot.allowedPieceNames.Add(pieceName);
            }

            this.ConfigureMeatSlots();
        }

        private void ConfigureMeatSlots()
        {
            this.meatSlots = new List<StorageSlot>();

            for (int x = 0; x < this.PieceStorage.Width; x++)
            {
                for (int y = 1; y < this.PieceStorage.Height; y++)
                {
                    StorageSlot meatSlot = this.PieceStorage.GetSlot(x, y);
                    meatSlot.hidden = false;
                    meatSlot.locked = false;
                    this.meatSlots.Add(meatSlot);
                }
            }
            this.DisableMeatSlots();
        }

        private void EnableMeatSlots()
        {
            foreach (StorageSlot slot in this.meatSlots)
            {
                slot.allowedPieceNames = null;
            }
        }

        private void DisableMeatSlots()
        {
            foreach (StorageSlot slot in this.meatSlots)
            {
                slot.allowedPieceNames = new List<PieceTemplate.Name>();
            }
        }

        public void HarvestMeat()
        {
            // checking harvesting conditions

            Player player = this.world.Player;
            StorageSlot animalSlot = this.AnimalSlot;

            if (animalSlot.IsEmpty)
            {
                new TextWindow(text: "I need to place an animal first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            // checking if meat slots are empty

            foreach (StorageSlot meatSlot in this.meatSlots)
            {
                if (!meatSlot.IsEmpty)
                {
                    new TextWindow(text: "I need to take out the previous items first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                    return;
                }
            }

            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
            {
                new TextWindow(text: "I can't harvest meat with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 0, closingTask: Scheduler.TaskName.ShowTutorialInGame, closingTaskHelper: new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreDelay", true } }, animSound: world.DialogueSound);

                return;
            }

            if (!player.CanSeeAnything)
            {
                new HintMessage(text: "It is too dark to harvest meat.", boxType: HintMessage.BoxType.Dialogue, delay: 1, blockInput: false, animate: true, useTransition: false).ConvertToTask(storeForLaterUse: false); // converted to task for display in proper order (after other messages, not before)

                return;
            }

            if (player.IsVeryTired)
            {
                new HintMessage(text: "I'm too tired to harvest meat...", boxType: HintMessage.BoxType.Dialogue, delay: 1, blockInput: false, animate: true, useTransition: false).ConvertToTask(storeForLaterUse: false); // converted to task for display in proper order (after other messages, not before)

                return;
            }

            // starting harvesting

            player.Fatigue += 80f;
            this.world.islandClock.Advance(60 * 30);

            BoardPiece animalPiece = animalSlot.GetAllPieces(remove: true)[0];

            this.world.HintEngine.Disable(PieceHint.Type.HarvestingWorkshop);
            this.world.HintEngine.Disable(Tutorials.Type.HarvestMeat);

            this.TurnOn();
            this.world.worldEventManager.RemovePieceFromQueue(this); // clearing possible previous "turn off" instances
            new WorldEvent(eventName: WorldEvent.EventName.TurnOffHarvestingWorkshop, delay: 30 * 60, boardPiece: this, world: this.world);
            Sound.QuickPlay(SoundData.Name.KnifeSharpen);

            var basePieces = animalPiece.pieceInfo.Yield.GetAllPieces(piece: animalPiece);
            var bonusPieces = new List<BoardPiece>();
            var taskChain = new List<Object>();

            if (basePieces.Any())
            {
                // processing bonus pieces

                var bonusChanceByLevelDict = new Dictionary<int, int> {
                    { 1, 40 }, // 1, 40
                    { 2, 30 },
                    { 3, 20 },
                    { 4, 10 },
                    { 5, 5 },
                 };

                int bonusChance = bonusChanceByLevelDict[this.world.Player.HarvestLevel];

                foreach (BoardPiece meatPiece in basePieces)
                {
                    if (this.world.random.Next(bonusChance) == 0) bonusPieces.Add(PieceTemplate.Create(world: world, templateName: meatPiece.name));
                }

                if (bonusPieces.Any())
                {
                    var bonusPiecesDict = bonusPieces.GroupBy(piece => piece.name).ToDictionary(piece => piece.Key, piece => piece.Count());

                    var textLines = new List<string> { "Got bonus items:\n" };
                    var imageList = new List<Texture2D>();

                    foreach (var kvp in bonusPiecesDict)
                    {
                        PieceInfo.Info pieceInfo = PieceInfo.GetInfo(kvp.Key);
                        int count = kvp.Value;

                        if (count == 1) textLines.Add($"| {pieceInfo.readableName}");
                        else textLines.Add($"| {pieceInfo.readableName} x{count}");
                        imageList.Add(pieceInfo.texture);
                    }

                    taskChain.Add(new HintMessage(text: String.Join("\n", textLines), boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInput: false, useTransition: true, imageList: imageList, startingSound: SoundData.Name.Ding1).ConvertToTask());
                }

                // placing pieces inside storage

                var totalObtainedPieces = new List<BoardPiece>();
                totalObtainedPieces.AddRange(basePieces);
                totalObtainedPieces.AddRange(bonusPieces);

                new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 15, executeHelper: SoundData.Name.HitFlesh1);

                if (totalObtainedPieces.Count >= 2)
                {
                    new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 20, executeHelper: SoundData.Name.DestroyFlesh2);
                    new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 30, executeHelper: SoundData.Name.DropMeat3);
                }

                this.EnableMeatSlots();
                foreach (BoardPiece meatPiece in totalObtainedPieces)
                {
                    this.PieceStorage.AddPiece(piece: meatPiece, dropIfDoesNotFit: true);
                    if (!bonusPieces.Any()) this.world.HintEngine.CheckForPieceHintToShow(ignoreInputActive: true, newOwnedPieceNameToCheck: meatPiece.name);
                }
                this.DisableMeatSlots();
            }
            else
            {
                taskChain.Add(new HintMessage(text: String.Join("\n", "Could not harvest anything..."), boxType: HintMessage.BoxType.Dialogue, delay: 0, blockInput: false).ConvertToTask());
            }

            // registering stats and checking level up

            this.world.meatHarvestStats.RegisterMeatHarvest(animalPiece: animalPiece, obtainedBasePieces: basePieces, obtainedBonusPieces: bonusPieces);
            player.CheckForMeatHarvestingLevelUp();

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, executeHelper: taskChain);
            new RumbleEvent(force: 0.15f, bigMotor: true, fadeInSeconds: 0.2f, durationSeconds: 0, fadeOutSeconds: 0.2f);
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(newAnimName: "on");
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(newAnimName: "off");
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.ConfigureMeatSlots();
        }
    }
}