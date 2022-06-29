using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceInfo
    {
        public static readonly Dictionary<PieceTemplate.Name, Info> info = new Dictionary<PieceTemplate.Name, Info> { };

        public struct Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public List<BuffEngine.Buff> buffList;
            public AnimFrame frame;
            public List<string> BuffDescList
            {
                get
                {
                    var buffDescList = new List<string> { };
                    if (buffList == null) return buffDescList;

                    foreach (var buff in buffList)
                    {
                        buffDescList.Add(buff.Description);
                    }
                    return buffDescList;
                }
            }

            public Info(BoardPiece piece)
            {
                this.name = piece.name;
                this.readableName = piece.readableName;
                this.description = piece.description;
                this.buffList = piece.buffList;
                this.frame = piece.sprite.frame;
            }

        }

        public static void CreateAllInfo(World world)
        {
            // creates one instance of every piece type - to get required info out of it

            foreach (PieceTemplate.Name name in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
            {
                BoardPiece piece = PieceTemplate.CreateOffBoard(templateName: name, world: world);
                info[name] = new Info(piece: piece);
            }
        }


    }
}
