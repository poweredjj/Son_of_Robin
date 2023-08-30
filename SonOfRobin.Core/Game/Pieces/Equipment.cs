using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Equipment : BoardPiece
    {
        public enum EquipType : byte
        {
            None, // for compatibility with PieceInfo
            Head,
            Chest,
            Legs,
            Backpack,
            Belt,
            Accessory,
            Map,
        };

        public readonly EquipType equipType;

        public Equipment(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, List<Buff> buffList, string readableName, string description, EquipType equipType,
            byte animSize = 0, string animName = "default", int maxHitPoints = 1, bool rotatesWhenDropped = false, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, rotatesWhenDropped: rotatesWhenDropped, buffList: buffList, readableName: readableName, description: description, activeState: State.Empty, soundPack: soundPack)
        {
            this.equipType = equipType;
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // data to deserialize here
        }
    }
}