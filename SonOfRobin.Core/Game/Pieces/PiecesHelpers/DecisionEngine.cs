using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class DecisionEngine
    {
        public enum Action : byte
        {
            Eat,
            Mate,
            Flee,
        }

        public class Choice
        {
            public Action Action { get; private set; }
            public float Priority { get; private set; }
            public BoardPiece Piece { get; private set; }

            public Choice(float priority, BoardPiece piece, Action action)
            {
                this.Priority = priority;
                this.Piece = piece;
                this.Action = action;
            }
        }

        private readonly List<Choice> allChoices;

        public DecisionEngine()
        {
            this.allChoices = new List<Choice> { };
        }

        public void AddChoice(float priority, BoardPiece piece, Action action)
        {
            this.allChoices.Add(new Choice(priority: priority, piece: piece, action: action));
        }

        public Choice GetBestChoice()
        {
            var possibleChoices = this.allChoices
                .Where(choice => choice.Priority > 0 && choice.Piece != null)
                .OrderByDescending(choice => choice.Priority);

            return possibleChoices.Count() > 0 ? possibleChoices.First() : null;
        }
    }
}