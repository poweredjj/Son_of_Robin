using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class CoolingManager
    {
        private readonly World world;
        private readonly Dictionary<string, BoardPiece> pieceByID;

        public CoolingManager(World world)
        {
            this.world = world;
            this.pieceByID = new Dictionary<string, BoardPiece>();
        }

        public void AddPiece(BoardPiece boardPiece)
        {
            if (!this.pieceByID.ContainsKey(boardPiece.id)) this.pieceByID[boardPiece.id] = boardPiece;
        }

        public void Update()
        {
            if (this.world.CurrentUpdate % 6 != 0) return;

            var idsToRemove = new List<string>();

            foreach (BoardPiece boardPiece in this.pieceByID.Values)
            {
                bool isCold = this.CoolBoardPiece(boardPiece);
                if (isCold) idsToRemove.Add(boardPiece.id);
            }

            foreach (string idToRemove in idsToRemove)
            {
                this.pieceByID.Remove(idToRemove);
            }
        }

        private bool CoolBoardPiece(BoardPiece boardPiece)
        {
            if (!boardPiece.sprite.IsOnBoard ||
                boardPiece.sprite.IsInWater ||
                !boardPiece.exists)
            {
                boardPiece.BurnLevel = 0;
                return true;
            }

            boardPiece.BurnLevel -= this.world.weather.IsRaining ? 0.02f : 0.0035f;

            return boardPiece.BurnLevel == 0;
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> coolingData = new Dictionary<string, object>
            {
               { "pieceIDList", this.pieceByID.Keys.ToList() },
            };

            return coolingData;
        }

        public void Deserialize(Dictionary<string, Object> coolingData)
        {
            this.pieceByID.Clear();

            if (coolingData.ContainsKey("pieceIDList")) // for compatibility with older saves
            {
                List<string> idList = (List<string>)coolingData["pieceIDList"];

                foreach (string pieceID in idList)
                {
                    if (!this.world.piecesByOldID.ContainsKey(pieceID))
                    {
                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"CoolingData - cannot find boardPiece id {pieceID}.", color: Color.Orange);
                    }
                    else this.pieceByID[pieceID] = this.world.piecesByOldID[pieceID];
                }
            }
        }
    }
}