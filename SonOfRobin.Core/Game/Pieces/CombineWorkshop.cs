using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class CombineWorkshop : BoardPiece
    {
        public CombineWorkshop(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, string readableName, string description, Category category,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack)
        {
            this.boardTask = Scheduler.TaskName.OpenContainer;

            this.pieceStorage = new PieceStorage(width: 3, height: 1, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Cooking, stackLimit: 1);

            StorageSlot combineTriggerSlot = this.pieceStorage.GetSlot(0, 0);
            BoardPiece combineTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.CombineTrigger, world: this.world);
            combineTriggerSlot.AddPiece(combineTrigger);
            combineTriggerSlot.locked = true;
        }

        public void Combine()
        {

            var piecesToCombine = this.pieceStorage.GetAllPieces();

            if (!piecesToCombine.Any())
            {
                new TextWindow(text: "I need to place some items first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (piecesToCombine.Count == 1)
            {
                new TextWindow(text: "I need to place 2 items first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }




            MessageLog.AddMessage(msgType: MsgType.User, message: $"Combining items...");
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