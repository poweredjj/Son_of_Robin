using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class ConstructionSite : BoardPiece
    {
        private int constrLevel;
        private int neededMaterialsCount;

        private readonly int maxConstrLevel;
        private readonly PieceTemplate.Name convertsIntoWhenFinished;
        private readonly Dictionary<int, Dictionary<PieceTemplate.Name, int>> materialsForLevels;

        private Dictionary<PieceTemplate.Name, int> CurrentMaterialsDict { get { return this.materialsForLevels[this.constrLevel]; } }

        private StorageSlot ConstructTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        public ConstructionSite(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, Dictionary<int, Dictionary<PieceTemplate.Name, int>> materialsForLevels, PieceTemplate.Name convertsIntoWhenFinished,
            byte animSize = 0) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.materialsForLevels = materialsForLevels;
            this.convertsIntoWhenFinished = convertsIntoWhenFinished;

            this.constrLevel = 0;
            this.maxConstrLevel = materialsForLevels.MaxBy(kvp => kvp.Key).Key;

            for (int i = 0; i <= this.maxConstrLevel; i++)
            {
                if (!this.materialsForLevels.ContainsKey(i)) throw new ArgumentOutOfRangeException($"No materials for level {i}.");
            }

            this.showStatBarsTillFrame = int.MaxValue;

            this.ClearAndConfigureStorage();
        }

        private void ClearAndConfigureStorage()
        {
            Dictionary<PieceTemplate.Name, int> currentMaterialsDict = this.CurrentMaterialsDict;

            int storageWidth = currentMaterialsDict.MaxBy(kvp => kvp.Value).Value;

            this.neededMaterialsCount = 0;
            foreach (var kvp in currentMaterialsDict)
            {
                PieceTemplate.Name materialName = kvp.Key;
                int stackCount = kvp.Value;

                this.neededMaterialsCount += PieceInfo.GetInfo(materialName).stackSize * stackCount;
            }

            this.PieceStorage = new PieceStorage(width: (byte)storageWidth, height: (byte)(currentMaterialsDict.Count + 1), storagePiece: this, storageType: PieceStorage.StorageType.Construction);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot constructTriggerSlot = this.ConstructTriggerSlot;
            constructTriggerSlot.locked = false;
            constructTriggerSlot.hidden = false;
            constructTriggerSlot.AddPiece(PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.ConstructTrigger, world: this.world));
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

            if (missingMaterials.Count > 0)
            {
                var textList = new List<string>();
                var imageList = new List<Texture2D>();

                foreach (var kvp in missingMaterials)
                {
                    PieceTemplate.Name materialName = kvp.Key;
                    int missingCount = kvp.Value;

                    PieceInfo.Info pieceInfo = PieceInfo.GetInfo(materialName);

                    textList.Add($"|  {pieceInfo.readableName} x{missingCount}");
                    imageList.Add(pieceInfo.CroppedFrame.texture);
                }

                string materialsList = String.Join("\n", textList);

                new TextWindow(text: $"Some materials are missing:\n\n{materialsList}", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);

                return;
            }

            if (this.world.Player.FatiguePercent > 0.5f)
            {
                new TextWindow(text: "I'm | too tired to work on this now...", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.Bed) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            var allowedPartsOfDay = new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Morning, IslandClock.PartOfDay.Noon };

            if (!allowedPartsOfDay.Contains(this.world.islandClock.CurrentPartOfDay))
            {
                new TextWindow(text: "It is too late to work on this today...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);

            int newConstructionLevel = this.constrLevel + 1;

            bool buildingFinished = newConstructionLevel > this.maxConstrLevel;

            BoardPiece nextLevelPiece;

            if (buildingFinished)
            {
                nextLevelPiece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.convertsIntoWhenFinished, world: this.world, position: this.sprite.position, ignoreCollisions: true, createdByPlayer: true);
            }
            else
            {
                nextLevelPiece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.name, world: this.world, position: this.sprite.position, ignoreCollisions: true, createdByPlayer: true);

                ConstructionSite newConstrSite = (ConstructionSite)nextLevelPiece;
                newConstrSite.constrLevel = newConstructionLevel;
                newConstrSite.sprite.AssignNewSize(newAnimSize: (byte)newConstructionLevel, checkForCollision: false);
                newConstrSite.ClearAndConfigureStorage();
            }

            World world = this.world;
            Player player = this.world.Player;

            world.touchLayout = TouchLayout.Empty;
            world.tipsLayout = ControlTips.TipsLayout.Empty;
            player.activeState = State.PlayerWaitForBuilding;
            player.buildDurationForOneFrame = 16;
            player.buildFatigueForOneFrame = 6;

            Sound.QuickPlay(SoundData.Name.Sawing);
            Sound.QuickPlay(SoundData.Name.Hammering);

            int buildDuration = 60 * 3;

            this.PieceStorage.DestroyAllPieces(); // to avoid dropping "used" materials on the ground

            nextLevelPiece.sprite.opacity = 0f;

            new OpacityFade(sprite: nextLevelPiece.sprite, destOpacity: 1f, duration: buildDuration);
            new OpacityFade(sprite: this.sprite, destOpacity: 0f, destroyPiece: true, duration: buildDuration);

            var taskChain = new List<Object>();

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ChangeActiveState, delay: 60 * 3, executeHelper: new Dictionary<string, Object> { { "boardPiece", player }, { "state", State.PlayerControlledWalking } }, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ChangeTipsLayout, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", world }, { "layout", ControlTips.TipsLayout.WorldMain } }, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ChangeTouchLayout, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", world }, { "layout", TouchLayout.WorldMain } }, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
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
        }

        public override void DrawStatBar()
        {
            new StatBar(label: "", value: this.PieceStorage.StoredPiecesCount - 1, valueMax: this.neededMaterialsCount, colorMin: new Color(0, 152, 163), colorMax: new Color(0, 216, 232), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.croppedFramesForPkgs[AnimData.PkgName.Hammer].texture);

            base.DrawStatBar();
        }
    }
}