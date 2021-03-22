using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace Solitaire {
    class Game {
        // access main controls to access canvas
        MainWindow Form = Application.Current.Windows[0] as MainWindow;

        /// <summary>
        /// Initializing a game
        /// </summary>
        /// <param name="index">Index of 'gameRules', what game is to be played</param>
        public Game(int index) {
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Game() {

        }

        public static readonly GameRules[] gameRules = new GameRules[] {
            new GameRules {
            name = "Classic Klondike - 1 draw",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 1,
            cells = 0,
            cardsOnDraw = 1,
            groupMoveMax = 13,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,

            emptyFoundationRule = GameRules.EmptyFoundationRules.KingOnly,
            stackingRule = GameRules.StackingRules.alternatingColours,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false) },
                new List<(int index, bool flipped)>{ (1, true), (7, false) },
                new List<(int index, bool flipped)>{ (2, true), (8, true),  (13, false) },
                new List<(int index, bool flipped)>{ (3, true), (9, true),  (14, true), (18, false) },
                new List<(int index, bool flipped)>{ (4, true), (10, true), (15, true), (19, true), (22, false) },
                new List<(int index, bool flipped)>{ (5, true), (11, true), (16, true), (20, true), (23, true), (25, false) },
                new List<(int index, bool flipped)>{ (6, true), (12, true), (17, true), (21, true), (24, true), (26, true), (27, false) }
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (0,0, GameRules.StackType.draw),
                (1,0, GameRules.StackType.waste),
                (4,0, GameRules.StackType.foundation),
                (5,0, GameRules.StackType.foundation),
                (6,0, GameRules.StackType.foundation),
                (7,0, GameRules.StackType.foundation),
                (1,1, GameRules.StackType.tableau),
                (2,1, GameRules.StackType.tableau),
                (3,1, GameRules.StackType.tableau),
                (4,1, GameRules.StackType.tableau),
                (5,1, GameRules.StackType.tableau),
                (6,1, GameRules.StackType.tableau),
                (7,1, GameRules.StackType.tableau),
            }
        },
            new GameRules {
            name = "Classic Klondike - 3 draw",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 1,
            cells = 0,
            cardsOnDraw = 3,
            groupMoveMax = 13,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,

            emptyFoundationRule = GameRules.EmptyFoundationRules.KingOnly,
            stackingRule = GameRules.StackingRules.alternatingColours,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false) },
                new List<(int index, bool flipped)>{ (1, true), (7, false) },
                new List<(int index, bool flipped)>{ (2, true), (8, true),  (13, false) },
                new List<(int index, bool flipped)>{ (3, true), (9, true),  (14, true), (18, false) },
                new List<(int index, bool flipped)>{ (4, true), (10, true), (15, true), (19, true), (22, false) },
                new List<(int index, bool flipped)>{ (5, true), (11, true), (16, true), (20, true), (23, true), (25, false) },
                new List<(int index, bool flipped)>{ (6, true), (12, true), (17, true), (21, true), (24, true), (26, true), (27, false) }
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (0,0, GameRules.StackType.draw),
                (1,0, GameRules.StackType.waste),
                (4,0, GameRules.StackType.foundation),
                (5,0, GameRules.StackType.foundation),
                (6,0, GameRules.StackType.foundation),
                (7,0, GameRules.StackType.foundation),
                (1,1, GameRules.StackType.tableau),
                (2,1, GameRules.StackType.tableau),
                (3,1, GameRules.StackType.tableau),
                (4,1, GameRules.StackType.tableau),
                (5,1, GameRules.StackType.tableau),
                (6,1, GameRules.StackType.tableau),
                (7,1, GameRules.StackType.tableau),
            }
        },
            new GameRules {
            name = "Classic Freecell - 4 cells",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 4,
            cardsOnDraw = 0,
            groupMoveMax = 5,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,

            emptyFoundationRule = GameRules.EmptyFoundationRules.Any,
            stackingRule = GameRules.StackingRules.alternatingColours,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false), (8, false),  (16, false), (24, false), (32, false), (40, false), (48, false) },
                new List<(int index, bool flipped)>{ (1, false), (9, false),  (17, false), (25, false), (33, false), (41, false), (49, false) },
                new List<(int index, bool flipped)>{ (2, false), (10, false), (18, false), (26, false), (34, false), (42, false), (50, false) },
                new List<(int index, bool flipped)>{ (3, false), (11, false), (19, false), (27, false), (35, false), (43, false), (51, false) },
                new List<(int index, bool flipped)>{ (4, false), (12, false), (20, false), (28, false), (36, false), (44, false) },
                new List<(int index, bool flipped)>{ (5, false), (13, false), (21, false), (29, false), (37, false), (45, false) },
                new List<(int index, bool flipped)>{ (6, false), (14, false), (22, false), (30, false), (38, false), (46, false) },
                new List<(int index, bool flipped)>{ (7, false), (15, false), (23, false), (31, false), (39, false), (47, false) }
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (0,0, GameRules.StackType.cell),
                (1,0, GameRules.StackType.cell),
                (2,0, GameRules.StackType.cell),
                (3,0, GameRules.StackType.cell),
                (4,0, GameRules.StackType.foundation),
                (5,0, GameRules.StackType.foundation),
                (6,0, GameRules.StackType.foundation),
                (7,0, GameRules.StackType.foundation),
                (0,1, GameRules.StackType.tableau),
                (1,1, GameRules.StackType.tableau),
                (2,1, GameRules.StackType.tableau),
                (3,1, GameRules.StackType.tableau),
                (4,1, GameRules.StackType.tableau),
                (5,1, GameRules.StackType.tableau),
                (6,1, GameRules.StackType.tableau),
                (7,1, GameRules.StackType.tableau),
            }
        },
        };

    }

    /// <summary>
    /// Defining different game rules
    /// </summary>
    public struct GameRules {
        public enum StackingRules {
            noRule,
            alternatingColours,
            sameSuit
        };
        public enum EmptyFoundationRules {
            KingOnly,
            Any
        };
        public enum FoundationOrderRules {
            HighToLow,
            LowToHigh,
            SameSuit
        };
        public enum DealingRules {
            /// <summary>
            /// Draw directly to waste pile
            /// </summary>
            DrawToWaste,
            /// <summary>
            /// Deal {cardsOnDraw} amount to each foundation
            /// </summary>
            DrawToTableaus,
            None
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
        }

        public string name;

        public int packs;
        public int foundations;
        public int tableaus;
        public int waste;
        public int cells;
        public int cardsOnDraw;
        public int groupMoveMax;

        public bool allowGroupMoveFromWaste;
        public bool shuffleOnRestock;
        public bool strictFoundationRule;

        public EmptyFoundationRules emptyFoundationRule;
        public StackingRules stackingRule;
        public FoundationOrderRules foundationOrderRule;
        public DealingRules dealingRule;
        public FoundationMoveRules foundationMoveRule;

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
