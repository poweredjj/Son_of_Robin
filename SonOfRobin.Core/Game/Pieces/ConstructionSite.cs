using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static SonOfRobin.Scene;

namespace SonOfRobin
{
    public class ConstructionSite : BoardPiece
    {
        private int constrLevel;

        private readonly BoardPiece triggerPiece;

        private readonly int maxConstrLevel;
        private readonly PieceTemplate.Name convertsIntoWhenFinished;
        private readonly Dictionary<int, Dictionary<PieceTemplate.Name, int>> materialsForLevels;
        private readonly Dictionary<int, string> descriptionsForLevels;

        private Dictionary<PieceTemplate.Name, int> CurrentMaterialsDict { get { return this.materialsForLevels[this.constrLevel]; } }
        private StorageSlot ConstructTriggerSlot { get { return this.PieceStorage.GetSlot(0, 0); } }

        public ConstructionSite(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, Dictionary<int, Dictionary<PieceTemplate.Name, int>> materialsForLevels, Dictionary<int, string> descriptionsForLevels, PieceTemplate.Name convertsIntoWhenFinished,
            byte animSize = 0) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.materialsForLevels = materialsForLevels;
            this.descriptionsForLevels = descriptionsForLevels;
            this.convertsIntoWhenFinished = convertsIntoWhenFinished;

            this.constrLevel = 0;
            this.maxConstrLevel = materialsForLevels.MaxBy(kvp => kvp.Key).Key;

            this.triggerPiece = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.ConstructTrigger, world: this.world);
            this.RefreshTriggerDescription();

            for (int i = 0; i <= this.maxConstrLevel; i++)
            {
                if (!this.materialsForLevels.ContainsKey(i)) throw new ArgumentOutOfRangeException($"No materials for level {i}.");
                if (!this.descriptionsForLevels.ContainsKey(i)) throw new ArgumentOutOfRangeException($"No description for level {i}.");
            }

            this.showStatBarsTillFrame = int.MaxValue;

            this.ClearAndConfigureStorage();
            this.RefreshTriggerDescription();
        }

        private int NeededMaterialsCount
        {
            get
            {
                int neededMaterialsCount = 0;
                foreach (var kvp in this.CurrentMaterialsDict)
                {
                    PieceTemplate.Name materialName = kvp.Key;
                    int stackCount = kvp.Value;

                    neededMaterialsCount += PieceInfo.GetInfo(materialName).stackSize * stackCount;
                }

                return neededMaterialsCount;
            }
        }

        private void ClearAndConfigureStorage()
        {
            Dictionary<PieceTemplate.Name, int> currentMaterialsDict = this.CurrentMaterialsDict;

            int storageWidth = currentMaterialsDict.MaxBy(kvp => kvp.Value).Value;

            this.PieceStorage = new PieceStorage(width: (byte)storageWidth, height: (byte)(currentMaterialsDict.Count + 1), storagePiece: this, storageType: PieceStorage.StorageType.Construction);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot constructTriggerSlot = this.ConstructTriggerSlot;
            constructTriggerSlot.locked = false;
            constructTriggerSlot.hidden = false;
            constructTriggerSlot.AddPiece(this.triggerPiece);
            constructTriggerSlot.locked = true;

            for (int i = 1; i < storageWidth; i++)
            {
                StorageSlot slot = this.PieceStorage.GetSlot(i, 0);
                slot.locked = true;
                slot.hidden = true;
            }

            int storageY = 1;
            foreach (var kvp in currentMaterialsDict)
            {
                PieceTemplate.Name materialName = kvp.Key;
                int slotCount = kvp.Value;

                for (int i = 0; i < slotCount; i++)
                {
                    StorageSlot slot = this.PieceStorage.GetSlot(i, storageY);

                    slot.locked = false;
                    slot.hidden = false;
                    slot.pieceTextureShownWhenEmpty = materialName;
                    slot.allowedPieceNames = new HashSet<PieceTemplate.Name> { materialName };
                }

                storageY++;
            }
        }

        public void Construct()
        {
            World world = this.world;
            Player player = this.world.Player;

            var foundMaterials = new Dictionary<PieceTemplate.Name, int>();

            foreach (BoardPiece material in this.PieceStorage.GetAllPieces())
            {
                if (!foundMaterials.ContainsKey(material.name)) foundMaterials[material.name] = 0;
                foundMaterials[material.name]++;
            }

            var missingMaterials = new Dictionary<PieceTemplate.Name, int>();

            foreach (var kvp in this.CurrentMaterialsDict)
            {
                PieceTemplate.Name materialName = kvp.Key;
                int stackCount = kvp.Value;
                int neededCount = PieceInfo.GetInfo(materialName).stackSize * stackCount;

                int foundCount = foundMaterials.ContainsKey(materialName) ? foundMaterials[materialName] : 0;
                if (foundCount < neededCount) missingMaterials[materialName] = neededCount - foundCount;
            }

            if (missingMaterials.Count > 0 && !Preferences.debugAllowConstructionWithoutMaterials)
            {
                var textList = new List<string>();
                var imageList = new List<ImageObj>();

                foreach (var kvp in missingMaterials)
                {
                    PieceTemplate.Name materialName = kvp.Key;
                    int missingCount = kvp.Value;

                    PieceInfo.Info pieceInfo = PieceInfo.GetInfo(materialName);

                    textList.Add($"| {pieceInfo.readableName} x{missingCount}");
                    imageList.Add(pieceInfo.ImageObj);
                }

                string materialsList = String.Join("\n", textList);

                new TextWindow(text: $"Some materials are missing:\n\n{materialsList}", imageList: imageList, imageAlignX: Helpers.AlignX.Left, minMarkerWidthMultiplier: 2f, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);

                return;
            }

            if (player.FatiguePercent > 0.5f)
            {
                new TextWindow(text: "I'm | too tired to work on this now...", imageList: new List<ImageObj> { TextureBank.GetImageObj(TextureBank.TextureName.Bed) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            var allowedPartsOfDay = new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Morning, IslandClock.PartOfDay.Noon, IslandClock.PartOfDay.Afternoon };

            if (!allowedPartsOfDay.Contains(this.world.islandClock.CurrentPartOfDay))
            {
                new TextWindow(text: "It is too late to work on this today...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
            {
                Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.KeepingAnimalsAway, world: world, ignoreDelay: true); };

                new TextWindow(text: "I cannot work on this with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ExecuteDelegate, closingTaskHelper: showTutorialDlgt, animSound: world.DialogueSound);
                return;
            }

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);

            int newConstructionLevel = this.constrLevel + 1;

            bool buildingFinished = newConstructionLevel > this.maxConstrLevel;

            BoardPiece nextLevelPiece;

            if (buildingFinished)
            {
                nextLevelPiece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.convertsIntoWhenFinished, world: this.world, position: this.sprite.position, ignoreCollisions: true, createdByPlayer: true);
                new LevelEvent(eventName: LevelEvent.EventName.CheckForPieceHints, level: this.world.ActiveLevel, delay: 60 * 1, boardPiece: null, eventHelper: new Dictionary<string, Object> { { "fieldPiece", nextLevelPiece.name } });
            }
            else
            {
                nextLevelPiece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.name, world: this.world, position: this.sprite.position, ignoreCollisions: true, createdByPlayer: true);

                ConstructionSite newConstrSite = (ConstructionSite)nextLevelPiece;
                newConstrSite.constrLevel = newConstructionLevel;
                newConstrSite.ClearAndConfigureStorage();
                newConstrSite.RefreshTriggerDescription();
                newConstrSite.sprite.AssignNewSize(newAnimSize: (byte)newConstructionLevel, checkForCollision: false);
            }

            world.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
            world.ActiveLevel.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);
            world.touchLayout = TouchLayout.Empty;
            world.tipsLayout = ControlTips.TipsLayout.Empty;

            player.activeState = State.PlayerWaitForBuilding;
            player.buildDurationForOneFrame = 90;
            player.buildFatigueForOneFrame = 6;

            Sound.QuickPlay(SoundData.Name.Sawing);
            Sound.QuickPlay(SoundData.Name.Hammering);

            int buildDuration = 60 * 3;

            var emptyContainersToDrop = new List<BoardPiece>();
            foreach (BoardPiece piece in this.PieceStorage.GetAllPieces())
            {
                if (piece.pieceInfo.convertsToWhenUsed != PieceTemplate.Name.Empty)
                {
                    BoardPiece emptyContainer = PieceTemplate.CreatePiece(world: this.world, templateName: piece.pieceInfo.convertsToWhenUsed);
                    if (!player.PieceStorage.AddPiece(emptyContainer)) emptyContainersToDrop.Add(emptyContainer);
                }
            }

            if (emptyContainersToDrop.Count > 0)
            {
                Vector2 containerPlacementPos = player.sprite.position + (player.sprite.position - this.sprite.position); // to avoid containers "falling under" construction site

                foreach (BoardPiece piece in emptyContainersToDrop)
                {
                    piece.PlaceOnBoard(randomPlacement: false, position: containerPlacementPos + new Vector2(this.world.random.Next(-15, 15), this.world.random.Next(-15, 15)), closestFreeSpot: true);
                    piece.sprite.rotation = this.world.random.NextSingle() * (float)Math.PI;
                }
            }

            this.PieceStorage.DestroyAllPieces(); // to avoid dropping "used" materials on the ground

            nextLevelPiece.sprite.opacity = 0f;

            new OpacityFade(sprite: nextLevelPiece.sprite, destOpacity: 1f, duration: buildDuration);
            new OpacityFade(sprite: this.sprite, destOpacity: 0f, destroyPiece: true, duration: buildDuration);

            var taskChain = new List<Object>();

            Scheduler.ExecutionDelegate enterLevelDelegate = () => { if (!this.world.HasBeenRemoved) FinishConstructionAnimation(nextLevelPiece); };
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: buildDuration, executeHelper: enterLevelDelegate, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }

        private void RefreshTriggerDescription()
        {
            var descriptionLines = new List<string> { this.constrLevel == this.maxConstrLevel ? "Construct final level\n" : "Construct next level\n" };

            foreach (var kvp in this.descriptionsForLevels)
            {
                int constrLevel = kvp.Key;
                string constrLevelName = kvp.Value;

                string doneTxt = "";
                if (constrLevel < this.constrLevel) doneTxt = " - done";
                else if (constrLevel == this.constrLevel) doneTxt = " - next";

                descriptionLines.Add($"{constrLevelName}{doneTxt}");
            }

            this.triggerPiece.description = String.Join("\n", descriptionLines);
        }

        public static void FinishConstructionAnimation(BoardPiece nextLevelPiece)
        {
            bool buildingFinished = nextLevelPiece.GetType() != typeof(ConstructionSite);

            World world = nextLevelPiece.world;

            world.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
            world.ActiveLevel.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
            world.Player.activeState = State.PlayerControlledWalking;
            world.touchLayout = TouchLayout.WorldMain;
            world.tipsLayout = ControlTips.TipsLayout.WorldMain;

            if (buildingFinished)
            {
                int flashDuration = 60 * 2;

                nextLevelPiece.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Yellow, framesLeft: flashDuration, fadeFramesLeft: flashDuration));

                // separate particle emitter is needed to draw particles under new piece
                BoardPiece particleEmitter = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(nextLevelPiece.sprite.position.X, nextLevelPiece.sprite.position.Y - 1), templateName: PieceTemplate.Name.ParticleEmitterEnding, precisePlacement: true);

                Yield starYield = new(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStarBig });
                starYield.DropDebris(piece: particleEmitter);

                Sound.QuickPlay(name: SoundData.Name.Chime, pitchChange: -0.35f);
            }
            else Sound.QuickPlay(name: SoundData.Name.Ding1);

            string constructionMessage;
            var imageList = new List<ImageObj>();

            if (buildingFinished)
            {
                constructionMessage = $"| {Helpers.FirstCharToUpperCase(nextLevelPiece.readableName)} construction finished!";
                imageList.Add(nextLevelPiece.pieceInfo.ImageObj);
            }
            else
            {
                ConstructionSite constructionSite = (ConstructionSite)nextLevelPiece;

                string constrLevelName = constructionSite.descriptionsForLevels[constructionSite.constrLevel - 1];

                constructionMessage = $"{Helpers.FirstCharToUpperCase(constrLevelName)} complete!\n{Helpers.FirstCharToUpperCase(constructionSite.readableName)} level up {constructionSite.constrLevel - 1} -> {constructionSite.constrLevel}.";
            }

            new TextWindow(text: constructionMessage, imageList: imageList, textColor: buildingFinished ? Color.PaleGoldenrod : Color.White, bgColor: buildingFinished ? Color.DarkGoldenrod : Color.Green, useTransition: true, animate: true, blockInputDuration: 60 * 2); // blockInputDuration is necessary, to prevent from accessing "old" construction site
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["constructionSite_currentLevel"] = this.constrLevel;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.constrLevel = (int)(Int64)pieceData["constructionSite_currentLevel"];

            // to avoid having deserialized trigger, instead of this.triggerPiece
            this.ConstructTriggerSlot.locked = false;
            this.ConstructTriggerSlot.GetAllPieces(remove: true);
            this.ConstructTriggerSlot.AddPiece(this.triggerPiece);
            this.ConstructTriggerSlot.locked = true;
            this.RefreshTriggerDescription();
        }

        public override void DrawStatBar()
        {
            new StatBar(label: "", value: this.PieceStorage.StoredPiecesCount - 1, valueMax: this.NeededMaterialsCount, colorMin: new Color(0, 152, 163), colorMax: new Color(0, 216, 232), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.GetCroppedFrameForPackage(AnimData.PkgName.Hammer).Texture);

            base.DrawStatBar();
        }
    }
}