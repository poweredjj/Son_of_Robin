using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;


// TODO remove - kept for compatibility with old saves

namespace SonOfRobin
{
    public class UpgradeBench : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> mainNames = new List<PieceTemplate.Name>(); // to be updated during first use
        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.PotionGeneric };

        private StorageSlot UpgradeTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private StorageSlot MainSlot
        { get { return this.PieceStorage.GetSlot(1, 0); } }

        private StorageSlot BoosterSlot
        { get { return this.PieceStorage.GetSlot(2, 0); } }

        public UpgradeBench(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack)
        {
            this.CreateAllowedNames();

            this.PieceStorage = new PieceStorage(width: 3, height: 1, storagePiece: this, storageType: PieceStorage.StorageType.Processing);

            StorageSlot upgradeTriggerSlot = this.UpgradeTriggerSlot;
            BoardPiece upgradeTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.UpgradeTrigger, world: this.world);
            upgradeTriggerSlot.AddPiece(upgradeTrigger);
            upgradeTriggerSlot.locked = true;

            StorageSlot mainSlot = this.MainSlot;
            mainSlot.allowedPieceNames = mainNames;
            mainSlot.label = "main";

            StorageSlot boosterSlot = this.BoosterSlot;
            boosterSlot.allowedPieceNames = boosterNames;
            boosterSlot.stackLimit = 1;
            boosterSlot.label = "booster";
        }

        private void CreateAllowedNames()
        {
            if (mainNames.Any() && boosterNames.Any()) return;

            var typeList = new List<System.Type> { typeof(Tool), typeof(Projectile) };
            foreach (PieceInfo.Info info in PieceInfo.AllInfo)
            {
                if (typeList.Contains(info.type) && !info.shootsProjectile) mainNames.Add(info.name);
            }
        }

        public void Upgrade()
        {
            StorageSlot mainSlot = this.MainSlot;
            StorageSlot boosterSlot = this.BoosterSlot;

            // checking if slots are empty

            if (mainSlot.IsEmpty && boosterSlot.IsEmpty)
            {
                new TextWindow(text: "There are no items placed.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
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

            // checking inserted pieces

            List<BoardPiece> piecesToUpgrade = mainSlot.GetAllPieces(remove: false);
            BoardPiece boosterPiece = boosterSlot.GetAllPieces(remove: false)[0];

            if (boosterPiece.name == PieceTemplate.Name.EmptyBottle)
            {
                new TextWindow(text: "This | bottle is empty.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.EmptyBottle) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            foreach (BoardPiece pieceToUpgrade in piecesToUpgrade)
            {
                if (pieceToUpgrade.buffList.Any())
                {
                    new TextWindow(text: $"This | {pieceToUpgrade.readableName} has already been upgraded.", imageList: new List<Texture2D> { pieceToUpgrade.sprite.AnimFrame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                    return;
                }
            }

            // making new buffs (to ensure that every piece gets its own individual buffs)

            foreach (BoardPiece pieceToUpgrade in piecesToUpgrade)
            {
                foreach (Buff buff in boosterPiece.buffList)
                {
                    Buff newBuff = new Buff(type: buff.type, value: buff.value, autoRemoveDelay: buff.autoRemoveDelay, isPermanent: buff.isPermanent, canKill: buff.canKill, increaseIDAtEveryUse: true);
                    pieceToUpgrade.buffList.Add(newBuff);
                }
            }

            // removing booster

            if (boosterPiece.GetType() == typeof(Potion))
            {
                Potion potion = (Potion)boosterPiece;
                BoardPiece emptyContainter = PieceTemplate.Create(templateName: potion.convertsToWhenUsed, world: this.world);
                boosterSlot.DestroyPieceAndReplaceWithAnother(emptyContainter);
            }
            else boosterSlot.GetAllPieces(remove: true);

            Sound.QuickPlay(SoundData.Name.ItemUpgrade);
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(newAnimName: "on");
            this.sprite.lightEngine.Activate();
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(newAnimName: "off");
            this.sprite.lightEngine.Deactivate();
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }
    }
}