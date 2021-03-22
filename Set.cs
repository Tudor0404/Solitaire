using System;
using System.Collections.Generic;
using System.Text;

namespace Solitaire {
    class Set {

        public List<Card> cards { get; set; }

        /// <summary>
        /// Creates 
        /// </summary>
        /// <param name="cards_"></param>
        public Set(List<Card> cards_) {
            cards = cards_;
        }

        /// <summary>
        /// Creates a number of decks of 52 cards
        /// </summary>
        public Set(int numDecks = 1) {
            cards.Clear();
            for (int i = 0; i < numDecks; i++) {
                for (int s = 0; s < 4; s++) {
                    for (int r = 0; r < 13; r++) {
                        cards.Add(new Card(r, s));
                    }
                }
            }
        }

        public void Shuffle() {
            Random rnd = new Random();

            int n = cards.Count;
            for (int i = n - 1; i > 0; i--) {
                int tempIndex = rnd.Next(0, i + 1);
                Card tempCard = cards[i];
                cards[i] = cards[tempIndex];
                cards[tempIndex] = tempCard;
            }
        }
    }
}
