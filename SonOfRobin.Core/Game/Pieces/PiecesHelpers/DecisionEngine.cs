using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class DecisionEngine
    {
        public enum Action
        {
            Eat,
            Mate,
            Flee
        }

        public class Choice
        {
            public Action action;
            public float priority;
            public BoardPiece piece;

            public Choice(float priority, BoardPiece piece, Action action)
            {
                this.priority = priority;
                this.piece = piece;
                this.action = action;
            }
        }

        private List<Choice> allChoices;

        public DecisionEngine()
        { this.allChoices = new List<Choice> { }; }

        public void AddChoice(float priority, BoardPiece piece, Action action)
        {
            this.allChoices.Add(new Choice(priority: priority, piece: piece, action: action));
        }

        public Choice GetBestChoice()
        {
            var possibleChoices = this.allChoices.Where(choice => choice.priority > 0 && choice.piece != null).OrderByDescending(choice => choice.priority).ToList();
            if (possibleChoices.Count > 0) return possibleChoices[0];
            else return null;
        }
    }
}