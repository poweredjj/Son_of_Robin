using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class UpgradeWorkshop : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> mainNames = new List<PieceTemplate.Name>(); // to be updated during first use
        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BottleOfPoison, PieceTemplate.Name.PotionMaxHP, PieceTemplate.Name.PotionStrength, PieceTemplate.Name.PotionMaxStamina, PieceTemplate.Name.EmptyBottle };

        private StorageSlot CombineTriggerSlot { get { return this.pieceStorage.GetSlot(0, 0); } }
        private StorageSlot MainSlot { get { return this.pieceStorage.GetSlot(1, 0); } }
        private StorageSlot BoosterSlot { get { return this.pieceStorage.GetSlot(2, 0); } }


        public UpgradeWorkshop(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, string readableName, string description, Category category,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack)
        {

            this.CreateMainNames();

            this.boardTask = Scheduler.TaskName.OpenContainer;

            this.pieceStorage = new PieceStorage(width: 3, height: 1, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Cooking, stackLimit: 1);

            StorageSlot combineTriggerSlot = this.CombineTriggerSlot;
            BoardPiece combineTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.UpgradeTrigger, world: this.world);
            combineTriggerSlot.AddPiece(combineTrigger);
            combineTriggerSlot.locked = true;

            StorageSlot mainSlot = this.MainSlot;
            mainSlot.allowedPieceNames = mainNames;
            mainSlot.label = "main";

            StorageSlot boosterSlot = this.BoosterSlot;
            boosterSlot.allowedPieceNames = boosterNames;
            boosterSlot.label = "booster";
        }

        private void CreateMainNames()
        {
            if (mainNames.Any()) return;

            var typeList = new List<System.Type> { typeof(Tool), typeof(Equipment), typeof(Projectile) };
            foreach (PieceInfo.Info info in PieceInfo.AllInfo)
            {
                if (typeList.Contains(info.type)) mainNames.Add(info.name);
            }
        }

        public void Upgrade()
        {
            StorageSlot mainSlot = this.MainSlot;
            StorageSlot boosterSlot = this.BoosterSlot;

            if (mainSlot.IsEmpty && boosterSlot.IsEmpty)
            {
                new TextWindow(text: "I need to place some items first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (mainSlot.IsEmpty)
            {
                new TextWindow(text: "Main slot is empty.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (boosterSlot.IsEmpty)
            {
                new TextWindow(text: "Booster slot is empty.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            BoardPiece mainPiece = mainSlot.GetAllPieces(remove: false)[0];
            BoardPiece boosterPiece = boosterSlot.GetAllPieces(remove: false)[0];

            if (boosterPiece.name == PieceTemplate.Name.EmptyBottle)
            {
                new TextWindow(text: "This | bottle is empty.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.EmptyBottle) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (mainPiece.buffList.Any())
            {
                new TextWindow(text: $"This | '{mainPiece.name}' has already been upgraded.", imageList: new List<Texture2D> { mainPiece.sprite.frame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }




            mainPiece.buffList.AddRange(boosterPiece.buffList);

            if (boosterPiece.GetType() == typeof(Potion))
            {
                Potion potion = (Potion)boosterPiece;

                BoardPiece emptyContainter = PieceTemplate.Create(templateName: potion.convertsToWhenUsed, world: this.world);
                boosterSlot.DestroyPieceAndReplaceWithAnother(emptyContainter);
            }
            else boosterSlot.GetAllPieces(remove: true);
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(animName: "on");
            this.sprite.lightEngine.Activate();
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(animName: "off");
            this.sprite.lightEngine.Deactivate();
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }


    }
}