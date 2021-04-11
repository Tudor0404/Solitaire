using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Solitaire {

    internal class Set {
        private MainWindow Form = Application.Current.Windows[0] as MainWindow;
        public List<Card> cards = new List<Card>();
        public GameRules.StackType stackType;

        public bool cardsExpanded;
        public int maxCardExpandable;

        public int gridRow;
        public int gridCol;
        public double canvasTop;
        public double canvasLeft;
        private int newCardOffset { get { return cards.Count > 9 ? 24 : 30; } }

        public List<int> topOffset {
            // allows only cards which are in the last {maxExpandablecards} to be expanded
            // idk even know anymore about this equation, could be better, don't touch it
            get {
                var marginList = new List<int>();

                for (int i = 0; i < 100; i++) {
                    int margin = (i + (maxCardExpandable - cards.Count + (cards.Count - maxCardExpandable < 1 ? cards.Count - maxCardExpandable : 0))) * newCardOffset;
                    marginList.Add(margin < 0 ? 0 : margin);
                }

                return marginList;
            }
        }

        static public int AssumedTopOffset(int numCards, int index, int maxCardExpandable) {
            int newCardOffset = numCards > 9 ? 24 : 30;
            var marginList = new List<int>();
            for (int i = 0; i < numCards; i++) {
                int margin = (i + (maxCardExpandable - numCards + (numCards - maxCardExpandable < 1 ? numCards - maxCardExpandable : 0))) * newCardOffset;
                marginList.Add(margin < 0 ? 0 : margin);
            }
            return marginList[index];
        }

        /// <summary>
        /// Creates a custom set of cards
        /// </summary>
        /// <param name="cards_">custom set of cards</param>
        /// <param name="cardsexpandable_">If the cards are to be displayed lower than the next one up</param>
        /// <param name="stackType_">Type of stack this set is</param>
        public Set(List<Card> cards_, int gridRow_, int gridCol_, bool cardsexpandable_ = false, int maxCardExpandable_ = 0, GameRules.StackType stackType_ = GameRules.StackType.undefined) {
            cards = cards_;
            cardsExpanded = cardsexpandable_;
            maxCardExpandable = maxCardExpandable_;
            stackType = stackType_;
            gridCol = gridCol_;
            gridRow = gridRow_;
        }

        /// <summary>
        /// Creates a number of decks of 52 cards
        /// </summary>
        /// <param name="cardsexpandable_">If the cards are to be displayed lower than the next one up</param>
        /// <param name="numDecks">How many full 52 cards to create</param>
        /// <param name="stackType_">Type of stack this set is</param>
        public Set(int gridRow_, int gridCol_, int numDecks = 1, bool cardsexpandable_ = false, int maxCardExpandable_ = 0, GameRules.StackType stackType_ = GameRules.StackType.undefined) {
            cards.Clear();
            cardsExpanded = cardsexpandable_;
            maxCardExpandable = maxCardExpandable_;
            stackType = stackType_;
            gridCol = gridCol_;
            gridRow = gridRow_;
            for (int i = 0; i < numDecks; i++) {
                for (int s = 0; s < 4; s++) {
                    for (int r = 0; r < 13; r++) {
                        cards.Add(new Card(r, s, gridCol, gridRow, true));
                    }
                }
            }
        }

        /// <summary>
        /// Creates an empty set
        /// </summary>
        /// <param name="cardsexpandable_">If the cards are to be displayed lower than the next one up</param>
        /// <param name="numDecks">How many full 52 cards to create</param>
        /// <param name="stackType_">Type of stack this set is</param>
        public Set(int gridRow_, int gridCol_, bool cardsexpandable_ = false, int maxCardExpandable_ = 0, GameRules.StackType stackType_ = GameRules.StackType.undefined) {
            cards.Clear();
            cardsExpanded = cardsexpandable_;
            maxCardExpandable = maxCardExpandable_;
            stackType = stackType_;
            gridCol = gridCol_;
            gridRow = gridRow_;
        }

        /// <summary>
        /// Transfers from host set to other, by object
        /// </summary>
        /// <param name="otherSet">reference of set obtaining the card</param>
        /// <param name="card">card to be transfered</param>
        public void TransferCard(ref Set otherSet, Card card, bool resetName = false) {
            cards.Remove(card);
            card.setCol = otherSet.gridCol;
            card.setRow = otherSet.gridRow;
            card.ZIndex = otherSet.cards.Count + 1;
            otherSet.cards.Add(card);

            if (resetName) {
                try {
                    Form.main_canvas.Children.OfType<Canvas>()
                        .Where(
                        c => {
                            var cardData = c.Name.Split('_');

                            if (Int32.Parse(cardData[1]) == gridCol && Int32.Parse(cardData[2]) == gridRow) {
                                return true;
                            }
                            return false;
                        }).Last().Name = otherSet.cards[otherSet.cards.Count - 1].name;
                } catch {
                }
            }

            if (stackType == GameRules.StackType.tableau) {
                FlipCard();
            }
        }

        /// <summary>
        /// Transfers the top card from host set to other
        /// </summary>
        /// <param name="otherSet">reference of set obtaining the card</param>
        public void TransferCard(ref Set otherSet, bool resetName = false) {
            Card tempCard = cards[cards.Count - 1];
            tempCard.setCol = otherSet.gridCol;
            tempCard.setRow = otherSet.gridRow;
            tempCard.ZIndex = otherSet.cards.Count + 1;
            otherSet.cards.Add(tempCard);
            cards.RemoveAt(cards.Count - 1);

            if (resetName) {
                try {
                    Form.main_canvas.Children.OfType<Canvas>()
                        .Where(
                        c => {
                            var cardData = c.Name.Split('_');

                            if (Int32.Parse(cardData[1]) == gridCol && Int32.Parse(cardData[2]) == gridRow) {
                                return true;
                            }
                            return false;
                        }).Last().Name = otherSet.cards[otherSet.cards.Count - 1].name;
                } catch {
                }
            }

            if (stackType == GameRules.StackType.tableau) {
                FlipCard();
            }
        }

        /// <summary>
        /// flips card by index to desired mode
        /// </summary>
        /// <param name="index">leave empty for top card</param>
        /// <param name="setState">sets if the card is flipped or not</param>
        public void FlipCard(int index = -1, bool setState = false) {
            try {
                if (index == -1)
                    index = cards.Count - 1;

                cards[index].flipped = setState;
            } catch { }
        }

        /// <summary>
        /// Shuffles deck
        /// </summary>
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

        /// <summary>
        /// returns a list of tuples which is used to display the cards
        /// </summary>
        /// <returns> card Canvas, top offset, zIndex</returns>
        public List<(Canvas, int, int)> drawSet() {
            var cardList = new List<(Canvas, int, int)>();

            int curIndex = 0;

            for (int i = 0; i < cards.Count; i++) {
                cards[i].ZIndex = curIndex;
                cardList.Add((cards[i].cardDraw(), topOffset[i], curIndex));
                curIndex++;
            }

            return cardList;
        }
    }
}