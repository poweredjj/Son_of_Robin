using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceInfo
    {
        public static readonly Dictionary<PieceTemplate.Name, Info> info = new Dictionary<PieceTemplate.Name, Info> { };

        public class Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public List<BuffEngine.Buff> buffList;
            public AnimFrame frame;
            public List<PieceTemplate.Name> eats;
            public List<PieceTemplate.Name> isEatenBy;

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
                if (piece.GetType() == typeof(Animal)) this.eats = ((Animal)piece).eats;
                this.isEatenBy = new List<PieceTemplate.Name> { };

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

            // getting isEatenBy data

            foreach (Info potentialPrey in info.Values)
            {
                if (potentialPrey.eats != null)
                {
                    // animal will either hunt the player or run away
                    if (!potentialPrey.eats.Contains(PieceTemplate.Name.Player)) potentialPrey.isEatenBy.Add(PieceTemplate.Name.Player);

                    foreach (Info potentialPredator in info.Values)
                    {
                        if (potentialPredator.eats != null && potentialPredator.eats.Contains(potentialPrey.name))
                            potentialPrey.isEatenBy.Add(potentialPredator.name);
                    }
                }
            }
        }

        public static List<PieceTemplate.Name> GetIsEatenBy(PieceTemplate.Name name)
        {
            if (info.ContainsKey(name) && info[name].isEatenBy != null) return info[name].isEatenBy;

            return new List<PieceTemplate.Name> { };
        }


    }
}
