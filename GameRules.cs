using System.Collections.Generic;

namespace Solitaire {

    /// <summary>
    /// Defining different game rules
    /// </summary>
    public struct GameRules {

        public enum TableauColourRule {
            noRule,
            alternatingColours,
            sameColour,
            sameSuit
        };

        public enum TableauStackingRule {
            highToLow,
            lowToHigh,
            None
        }

        public enum EmptyTableauRules {
            KingOnly,
            Any
        };

        public enum FoundationOrderRules {
            HighToLow,
            LowToHigh,
        };

        public enum DealingRules {
            DrawToWaste,

            /// <summary>
            /// Deal 1 to each foundation
            /// </summary>
            DrawToTableaus,

            None
        }

        public enum GroupMoveRules {
            normal,
            none,
            onePlusFreeCells
        }

        public enum FoundationMoveRules {
            None,
            Single,
            Group
        }

        public enum StackType {
            foundation,
            tableau,
            draw,
            waste,
            cell,
            undefined
        }

        public string name;

        public int packs;
        public int foundations;
        public int tableaus;
        public int waste;
        public int cells;
        public int cardsOnDraw;
        public int groupMoveMax;
        public int initalCardDeal;

        public bool cellsFilled;
        public bool allowGroupMoveFromWaste;
        public bool shuffleOnRestock;
        public bool allowRestock;
        public bool strictFoundationRule;
        public bool strictTableauRule;

        public EmptyTableauRules emptyTableauRule;
        public TableauColourRule tableauColourRule;
        public TableauStackingRule tableauStackingRule;
        public FoundationOrderRules foundationOrderRule;
        public DealingRules dealingRule;
        public FoundationMoveRules foundationMoveRule;
        public GroupMoveRules groupMoveRules;

        /// <summary>
        /// how cards are dealt, index and if it is flipped or not
        /// </summary>
        public List<List<(int, bool)>> layoutStart;

        /// <summary>
        /// Where the stacks are placed on the 8x3 board, indexed at 0; col, row, stack type
        /// </summary>
        public List<(int, int, StackType)> cardStackLayout;
    }
}