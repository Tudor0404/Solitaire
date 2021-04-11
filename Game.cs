using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Solitaire {

    partial class Game {

        #region properties and variables

        private MainWindow Form = Application.Current.Windows[0] as MainWindow;
        private List<Set> stacks = new List<Set>();
        private int gameIndex;

        private int colCount = 8;
        private int rowCount = 3;

        private int cardWidth = 100;
        private int cardHeight = 146;

        private int templateWidth = 120;
        private int templateHeight = 180;

        static private Random rnd = new Random();
        static private MediaPlayer mediaPlayer = new MediaPlayer();

        private List<Set> tableauStacks {
            get {
                return stacks.FindAll(c => c.stackType == GameRules.StackType.tableau);
            }
        }

        private List<Set> foundationStacks {
            get {
                return stacks.FindAll(c => c.stackType == GameRules.StackType.foundation);
            }
        }

        private List<Set> cellStacks {
            get {
                return stacks.FindAll(c => c.stackType == GameRules.StackType.cell);
            }
        }

        private Set drawStack {
            get {
                return stacks.Find(c => c.stackType == GameRules.StackType.draw);
            }
        }

        private Set wasteStack {
            get {
                return stacks.Find(c => c.stackType == GameRules.StackType.waste);
            }
        }

        private Action resetClock;

        #endregion properties and variables

        /// <summary>
        /// Initializing a game
        /// </summary>
        public Game(int index, Action resetClock_) {
            resetClock = resetClock_;
            mediaPlayer.Volume = 300;
        }

        /// <summary>
        /// sets/resets the board with chosen game type
        /// </summary>
        /// /// <param name="index">Index of 'gameRules', what game is to be played</param>
        public void gameSetup(int index) {
            gameIndex = index;

            stacks.Clear();
            foreach (var item in gameRules[gameIndex].cardStackLayout) {
                switch (item.Item3) {
                    case GameRules.StackType.foundation:
                        stacks.Add(new Set(item.Item2, item.Item1, false, stackType_: item.Item3));
                        break;

                    case GameRules.StackType.tableau:
                        stacks.Add(new Set(item.Item2, item.Item1, true, 13, item.Item3));
                        break;

                    case GameRules.StackType.draw:
                        stacks.Add(new Set(item.Item2, item.Item1, 1, false, stackType_: item.Item3));
                        break;

                    case GameRules.StackType.waste:
                        stacks.Add(new Set(item.Item2, item.Item1, true, 3, item.Item3));
                        break;

                    case GameRules.StackType.cell:
                        stacks.Add(new Set(item.Item2, item.Item1, false, stackType_: item.Item3));
                        break;

                    case GameRules.StackType.undefined:
                        stacks.Add(new Set(item.Item2, item.Item1, false, stackType_: item.Item3));
                        break;
                }
            }
            RemoveChildren(true);
            drawStack.Shuffle();
            InitialDistributeCards();
            DrawStacks(true);

            Form.moves_text.Text = "0";
            resetClock();
        }

        /// <summary>
        /// resets the board
        /// </summary>
        /// <param name="templates">reset templates too?</param>
        private void DrawStacks(bool templates = false) {
            RemoveChildren(templates);
            // draw templates if specified
            if (templates) {
                double templateColWidth = Form.main_canvas.ActualWidth / colCount;
                double templateRowHeight = Form.main_canvas.ActualHeight / rowCount;

                foreach (var item in gameRules[gameIndex].cardStackLayout) {
                    var canvas = new Canvas() { Width = templateWidth, Height = templateHeight };
                    ImageBrush brush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"assets\playingCards\" + item.Item3 + "_template.png", UriKind.Relative)) };
                    canvas.Background = brush;
                    canvas.Name = $"_{item.Item1}_{item.Item2}_{0}";

                    if (item.Item3 == GameRules.StackType.draw) {
                        canvas.MouseDown += Draw;
                    }

                    Form.main_canvas.Children.Add(canvas);
                    Canvas.SetLeft(canvas, (item.Item1 * templateColWidth) + (templateColWidth - templateWidth) / 2);
                    Canvas.SetTop(canvas, (item.Item2 * templateRowHeight) + (templateRowHeight - templateHeight) / 2);
                    UIElementExtensions.SetGroupID(canvas, 12); // 12 = template group
                }
            }

            double cardColWidth = Form.main_canvas.ActualWidth / colCount;
            double cardRowHeight = Form.main_canvas.ActualHeight / rowCount;

            foreach (var item in stacks) {
                double leftSet = (item.gridCol * cardColWidth) + (cardColWidth - cardWidth) / 2;
                double topSet = (item.gridRow * cardRowHeight) + (cardRowHeight - cardHeight) / 2;
                item.canvasLeft = leftSet;
                item.canvasTop = topSet;
                foreach (var card in item.drawSet()) {
                    Form.main_canvas.Children.Add(card.Item1);

                    Canvas.SetLeft(card.Item1, leftSet);
                    Canvas.SetTop(card.Item1, topSet + card.Item2);
                    Canvas.SetZIndex(card.Item1, card.Item3);
                    card.Item1.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
                    card.Item1.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
                    card.Item1.MouseMove += Canvas_MouseMove;

                    if (item.stackType == GameRules.StackType.draw)
                        card.Item1.MouseDown += Draw;

                    UIElementExtensions.SetGroupID(card.Item1, 11); // 11 = card group
                }
            }
        }

        /// <summary>
        /// removes the children of main_canvas
        /// </summary>
        /// <param name="templateClear">if to clear templates too</param>
        private void RemoveChildren(bool templateClear) {
            // selects all items within group 11 (cards) and 12 (templates if selected)
            var childrenToRemove =
                (Form.main_canvas.Children.OfType<UIElement>().Where(c => UIElementExtensions.GetGroupID(c) == 11))
                .Concat(templateClear ? Form.main_canvas.Children.OfType<UIElement>()
                .Where(c => UIElementExtensions.GetGroupID(c) == 12) : Enumerable.Empty<UIElement>()).ToList();

            foreach (var child in childrenToRemove) {
                Form.main_canvas.Children.Remove(child);
            }
        }

        private void InitialDistributeCards() {
            for (int i = 0; i < gameRules[gameIndex].initalCardDeal; i++) {
                for (int j = 0; j < gameRules[gameIndex].layoutStart.Count; j++) {
                    int foundIndex = gameRules[gameIndex].layoutStart[j].FindIndex(c => c.Item1 == i);
                    if (foundIndex != -1) {
                        var tempTableau = tableauStacks[j];
                        drawStack.TransferCard(ref tempTableau);
                        tempTableau.cards[tempTableau.cards.Count - 1].flipped = gameRules[gameIndex].layoutStart[j][foundIndex].Item2;
                        tableauStacks[j] = tempTableau;
                    }
                }
            }
            if (gameRules[gameIndex].cellsFilled) {
                for (int i = 0; i < cellStacks.Count; i++) {
                    var tempCell = cellStacks[i];
                    drawStack.TransferCard(ref tempCell);
                    tempCell.cards[tempCell.cards.Count - 1].flipped = false;
                    cellStacks[i] = tempCell;
                }
            }
        }

        /// <summary>
        /// Animates transform of a card
        /// </summary>
        /// <param name="card">canvas refernce to the card being animated</param>
        /// <param name="toX">final X position</param>
        /// <param name="toY">final Y position</param>
        /// <param name="milliseconds">total time for animation</param>
        /// <param name="resetAfterAnimation">if to call DrawStacks() after animation</param>
        /// <param name="zIndexSet">if resetAfterAnimation is not called, resets Zindex of card</param>
        private void AnimateMove(ref Canvas card, double toX, double toY, int milliseconds = 300, bool resetAfterAnimation = false, int zIndexSet = -1) {
            double fromX = Canvas.GetLeft(card);
            double fromY = Canvas.GetTop(card);
            Duration time = TimeSpan.FromMilliseconds(milliseconds);
            var XTranslate = new DoubleAnimation(fromValue: fromX, toValue: toX, duration: time);
            var YTranslate = new DoubleAnimation(fromValue: fromY, toValue: toY, duration: time);

            XTranslate.FillBehavior = FillBehavior.Stop;
            YTranslate.FillBehavior = FillBehavior.Stop;

            XTranslate.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn };
            YTranslate.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn };

            string[] cardData = card.Name.Split('_');

            cardData[3] = "0";
            card.Name = String.Join('_', cardData);

            bool alreadyReset = false;

            // cant put these in a separate function because it uses variables in this scope
            XTranslate.Completed += (s, e) => {
                try {
                    if (resetAfterAnimation) {
                        if (alreadyReset == false) {
                            DrawStacks();
                            alreadyReset = true;
                        }
                    } else {
                        var aCard = Form.main_canvas.Children.OfType<Canvas>().Where(c => c.Name == String.Join('_', cardData)).Single();
                        string[] aCardData = aCard.Name.Split('_');
                        aCardData[3] = "1";

                        if (zIndexSet != -1)
                            Canvas.SetZIndex(aCard, zIndexSet);

                        aCard.Name = String.Join('_', aCardData);
                    }
                } catch {
                }
            };
            YTranslate.Completed += (s, e) => {
                try {
                    if (resetAfterAnimation) {
                        if (alreadyReset == false) {
                            DrawStacks();
                            alreadyReset = true;
                        }
                    } else {
                        var aCard = Form.main_canvas.Children.OfType<Canvas>().Where(c => c.Name == String.Join('_', cardData)).Single();
                        string[] aCardData = aCard.Name.Split('_');
                        aCardData[3] = "1";

                        if (zIndexSet != -1)
                            Canvas.SetZIndex(aCard, zIndexSet);

                        aCard.Name = String.Join('_', aCardData);
                    }
                } catch {
                }
            };

            card.BeginAnimation(Canvas.LeftProperty, XTranslate);
            card.BeginAnimation(Canvas.TopProperty, YTranslate);
        }

        /// <summary>
        /// Functions to handle user interaction with the board
        /// </summary>

        #region ControlHandlers

        private bool isDragging;
        private Point dragOffset;
        private double prevLeft;
        private double prevTop;
        private int gridColStart;
        private int gridRowStart;
        private int gridColOver;
        private int gridRowOver;
        private int amountCardsDragging;
        private List<Canvas> cardsDragging = new List<Canvas>();

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var draggable = sender as Canvas;

            string[] cardData = draggable.Name.Split('_');

            if (cardData[4] == "0" && cardData[3] == "1") {
                gridColStart = Int32.Parse(cardData[1]);
                gridRowStart = Int32.Parse(cardData[2]);

                // check if dragging multiple cards
                int placeInSet = stacks.Where(c => {
                    if (c.gridCol == gridColStart && c.gridRow == gridRowStart)
                        return true;
                    return false;
                }).Single().cards.FindIndex(c => {
                    if (c.ZIndex == Int32.Parse(cardData[7]))
                        return true;
                    return false;
                });
                int countSet = stacks.Where(c => {
                    if (c.gridCol == gridColStart && c.gridRow == gridRowStart)
                        return true;
                    return false;
                }).Single().cards.Count;
                amountCardsDragging = countSet - placeInSet;

                cardsDragging = Form.main_canvas.Children.OfType<Canvas>()
                    .Where(c => UIElementExtensions.GetGroupID(c) == 11)
                    .Where(c => {
                        var tempCard = c.Name.Split('_');
                        if (tempCard[1] == cardData[1] && tempCard[2] == cardData[2])
                            return true;
                        return false;
                    }
                    ).OrderBy(
                    c => {
                        var tempCard = c.Name.Split('_');
                        return Int32.Parse(tempCard[7]);
                    }
                    ).ToList();

                if (cardsDragging.Count > amountCardsDragging)
                    cardsDragging = cardsDragging.Skip(Math.Max(0, cardsDragging.Count - amountCardsDragging)).ToList();

                for (int i = 0; i < amountCardsDragging; i++) {
                    Canvas.SetZIndex(cardsDragging[i], 100 + i);
                }

                bool success = true;

                // validate allowGroupMoveFromWaste rule
                if (wasteStack != null)
                    if (amountCardsDragging > 1 && gameRules[gameIndex].allowGroupMoveFromWaste == false && (wasteStack.gridCol == gridColStart && wasteStack.gridRow == gridRowStart))
                        success = false;

                //validate maxGroupMove rule
                if (success && gameRules[gameIndex].groupMoveMax < amountCardsDragging)
                    success = false;

                //validate groupMoveRule
                if (success) {
                    switch (gameRules[gameIndex].groupMoveRules) {
                        case GameRules.GroupMoveRules.normal:
                            break;

                        case GameRules.GroupMoveRules.none:
                            if (amountCardsDragging > 1)
                                success = false;
                            break;

                        case GameRules.GroupMoveRules.onePlusFreeCells:
                            try {
                                if (amountCardsDragging > cellStacks.Where(c => c.cards.Count == 0).Count() + 1)
                                    success = false;
                            } catch { }
                            break;
                    }
                }

                //validate tableauStackingRule
                if (success && amountCardsDragging > 1 && gameRules[gameIndex].strictTableauRule) {
                    switch (gameRules[gameIndex].tableauStackingRule) {
                        case GameRules.TableauStackingRule.highToLow:
                            for (int i = 1; i < amountCardsDragging; i++) {
                                var tempCard = cardsDragging[i].Name.Split('_');
                                var tempCardLow = cardsDragging[i - 1].Name.Split('_');
                                if (Int32.Parse(tempCard[5]) != Int32.Parse(tempCardLow[5]) - 1) {
                                    success = false;
                                    break;
                                }
                            }
                            break;

                        case GameRules.TableauStackingRule.lowToHigh:
                            for (int i = 1; i < amountCardsDragging; i++) {
                                var tempCard = cardsDragging[i].Name.Split('_');
                                var tempCardLow = cardsDragging[i - 1].Name.Split('_');
                                if (Int32.Parse(tempCard[5]) != Int32.Parse(tempCardLow[5]) + 1) {
                                    success = false;
                                    break;
                                }
                            }
                            break;

                        case GameRules.TableauStackingRule.None:
                            break;
                    }
                }

                //validate tableauColourRule
                if (success && amountCardsDragging > 1 && gameRules[gameIndex].strictTableauRule) {
                    var tempCard = cardsDragging[1].Name.Split('_');
                    int baseSuit = Int32.Parse(tempCard[6]);
                    int otherSuit;
                    int otherSuit2;

                    switch (gameRules[gameIndex].tableauColourRule) {
                        case GameRules.TableauColourRule.noRule:
                            break;

                        case GameRules.TableauColourRule.alternatingColours:
                            for (int i = 1; i < amountCardsDragging; i++) {
                                tempCard = cardsDragging[i].Name.Split('_');
                                baseSuit = Int32.Parse(tempCard[6]);

                                var tempCardLow = cardsDragging[i - 1].Name.Split('_');
                                otherSuit = baseSuit == 0 || baseSuit == 3 ? 1 : 0;
                                otherSuit2 = baseSuit == 0 || baseSuit == 3 ? 2 : 3;

                                if (Int32.Parse(tempCardLow[6]) != otherSuit && Int32.Parse(tempCardLow[6]) != otherSuit2) {
                                    success = false;
                                    break;
                                }
                            }
                            break;

                        case GameRules.TableauColourRule.sameColour:
                            otherSuit = baseSuit == 0 || baseSuit == 3 ? 0 : 1;
                            otherSuit2 = baseSuit == 0 || baseSuit == 3 ? 3 : 2;

                            for (int i = 1; i < amountCardsDragging; i++) {
                                var tempCardLow = cardsDragging[i - 1].Name.Split('_');
                                if (Int32.Parse(tempCardLow[6]) != otherSuit && Int32.Parse(tempCardLow[6]) != otherSuit2) {
                                    success = false;
                                    break;
                                }
                            }
                            break;

                        case GameRules.TableauColourRule.sameSuit:
                            for (int i = 1; i < amountCardsDragging; i++) {
                                var tempCardLow = cardsDragging[i - 1].Name.Split('_');
                                if (baseSuit != Int32.Parse(tempCardLow[6])) {
                                    success = false;
                                    break;
                                }
                            }
                            break;
                    }
                }

                if (!success) {
                    isDragging = false;
                    gridColStart = -1;
                    gridRowStart = -1;
                    cardsDragging.Clear();
                    amountCardsDragging = -1;
                    return;
                }

                isDragging = true;
                Point mousePosition = (Point)e.GetPosition(Form.main_canvas);

                prevLeft = Canvas.GetLeft(draggable);
                prevTop = Canvas.GetTop(draggable);
                dragOffset = new Point(mousePosition.X - prevLeft, mousePosition.Y - prevTop);

                gridColOver = -1;
                gridRowOver = -1;

                draggable.CaptureMouse();

                mediaPlayer.Open(new Uri(@"assets\sfx\deal_" + rnd.Next(1, 6).ToString() + ".wav", UriKind.Relative));
                mediaPlayer.Play();
            } else {
                isDragging = false;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (isDragging) {
                isDragging = false;
                var draggable = sender as Canvas;

                bool positionValid = gridColOver != -1 && (gridColOver != gridColStart || gridRowOver != gridRowStart);

                if (positionValid) {
                    bool tableauColourValid = true;
                    bool tableauOrderValid = true;
                    bool tableauEmptyCheck = true;

                    bool foundationOrderCheck = true;

                    bool cellEmptyCheck = true;

                    int newTableauIndex = -1;
                    int newFoundationIndex = -1;
                    int newCellIndex = -1;
                    string[] cardData = draggable.Name.Split('_');
                    int tempSuit = Int32.Parse(cardData[6]);
                    int tempRank = Int32.Parse(cardData[5]);

                    /* Order of tableau checks:
                     1. check if position in grid is not original/invalid
                     2. colour rule
                     3. stacking rule
                     3. empty tableau rule
                    */
                    newTableauIndex = tableauStacks.FindIndex(c => c.gridCol == gridColOver && c.gridRow == gridRowOver);
                    if (newTableauIndex != -1) {
                        switch (gameRules[gameIndex].tableauColourRule) {
                            case GameRules.TableauColourRule.noRule:
                                break;

                            case GameRules.TableauColourRule.alternatingColours:
                                if (tableauStacks[newTableauIndex].cards.Count == 0)
                                    break;
                                else if ((tempSuit == 0 || tempSuit == 3) &&
                                     (tableauStacks[newTableauIndex].cards.Last().suit == 1 ||
                                     tableauStacks[newTableauIndex].cards.Last().suit == 2))
                                    break;
                                else if ((tempSuit == 1 || tempSuit == 2) &&
                                    (tableauStacks[newTableauIndex].cards.Last().suit == 0 ||
                                    tableauStacks[newTableauIndex].cards.Last().suit == 3))
                                    break;
                                else
                                    tableauColourValid = false;
                                break;

                            case GameRules.TableauColourRule.sameColour:
                                if (tableauStacks[newTableauIndex].cards.Count == 0)
                                    break;
                                else if ((tempSuit == 0 || tempSuit == 3) &&
                                   (tableauStacks[newTableauIndex].cards.Last().suit == 0 ||
                                   tableauStacks[newTableauIndex].cards.Last().suit == 3))
                                    break;
                                else if ((tempSuit == 1 || tempSuit == 2) &&
                                    (tableauStacks[newTableauIndex].cards.Last().suit == 1 ||
                                    tableauStacks[newTableauIndex].cards.Last().suit == 2))
                                    break;
                                else
                                    tableauColourValid = false;

                                break;

                            case GameRules.TableauColourRule.sameSuit:
                                if (tableauStacks[newTableauIndex].cards.Count == 0)
                                    break;
                                else if (tempSuit == tableauStacks[newTableauIndex].cards.Last().suit)
                                    break;
                                else
                                    tableauColourValid = false;

                                break;
                        }

                        if (tableauColourValid) {
                            switch (gameRules[gameIndex].tableauStackingRule) {
                                case GameRules.TableauStackingRule.highToLow:
                                    if (tableauStacks[newTableauIndex].cards.Count == 0)
                                        break;
                                    else if (tableauStacks[newTableauIndex].cards.Last().rank - 1 != Int32.Parse(cardData[5]))
                                        tableauOrderValid = false;
                                    break;

                                case GameRules.TableauStackingRule.lowToHigh:
                                    if (tableauStacks[newTableauIndex].cards.Count == 0)
                                        break;
                                    else if (tableauStacks[newTableauIndex].cards.Last().rank + 1 != Int32.Parse(cardData[5]))
                                        tableauOrderValid = false;
                                    break;

                                case GameRules.TableauStackingRule.None:
                                    break;
                            }
                        }

                        switch (gameRules[gameIndex].emptyTableauRule) {
                            case GameRules.EmptyTableauRules.KingOnly:
                                if (tableauStacks[newTableauIndex].cards.Count == 0) {
                                    if (tempRank == 12)
                                        break;
                                    else
                                        tableauEmptyCheck = false;
                                }
                                break;

                            case GameRules.EmptyTableauRules.Any:
                                break;
                        }
                    }

                    /* Order of foundation checks:
                      1. Empty foundation check / Order check
                      3. Suit check
                     */
                    newFoundationIndex = foundationStacks.FindIndex(c => c.gridCol == gridColOver && c.gridRow == gridRowOver);
                    if (newFoundationIndex != -1) {
                        /* Order of foundation checks:
                        * 1. Empty foundation check / Order check
                        * 3. Suit check
                        */
                        switch (gameRules[gameIndex].foundationOrderRule) {
                            case GameRules.FoundationOrderRules.HighToLow:
                                if (foundationStacks[newFoundationIndex].cards.Count == 0) {
                                    if (tempRank == 12)
                                        break;
                                    else
                                        foundationOrderCheck = false;
                                } else {
                                    if (foundationStacks[newFoundationIndex].cards[foundationStacks[newFoundationIndex].cards.Count - 1].suit != tempSuit ||
                                        foundationStacks[newFoundationIndex].cards[foundationStacks[newFoundationIndex].cards.Count - 1].rank != tempRank + 1)
                                        foundationOrderCheck = false;
                                }
                                break;

                            case GameRules.FoundationOrderRules.LowToHigh:
                                if (foundationStacks[newFoundationIndex].cards.Count == 0) {
                                    if (tempRank == 0)
                                        break;
                                    else
                                        foundationOrderCheck = false;
                                } else {
                                    if (foundationStacks[newFoundationIndex].cards[foundationStacks[newFoundationIndex].cards.Count - 1].suit != tempSuit ||
                                        foundationStacks[newFoundationIndex].cards[foundationStacks[newFoundationIndex].cards.Count - 1].rank != tempRank - 1)
                                        foundationOrderCheck = false;
                                }
                                break;
                        }
                    }

                    //check if cell is empty
                    newCellIndex = cellStacks.FindIndex(c => c.gridCol == gridColOver && c.gridRow == gridRowOver);
                    if (newCellIndex != -1) {
                        if (cellStacks[newCellIndex].cards.Count != 0)
                            cellEmptyCheck = false;
                    }

                    bool success = false;
                    bool allowTableau = tableauOrderValid && tableauColourValid && tableauEmptyCheck && newTableauIndex != -1;
                    bool allowCell = cellEmptyCheck && newCellIndex != -1;
                    bool allowFoundation = foundationOrderCheck && newFoundationIndex != -1;

                    if (allowTableau) {
                        var tempTableau = tableauStacks[newTableauIndex];
                        massTransfer(ref tempTableau);
                        tableauStacks[newTableauIndex] = tempTableau;
                        success = true;
                    } else if (allowCell) {
                        var tempCell = cellStacks[newCellIndex];
                        massTransfer(ref tempCell);
                        cellStacks[newCellIndex] = tempCell;
                        success = true;
                    } else if (allowFoundation) {
                        var tempFoundation = foundationStacks[newFoundationIndex];
                        massTransfer(ref tempFoundation);
                        foundationStacks[newFoundationIndex] = tempFoundation;
                        success = true;
                    }

                    if (success) {
                        Form.moves_text.Text = (Int32.Parse(Form.moves_text.Text) + 1).ToString();

                        ResetPositions(gridColOver, gridRowOver, true);
                        ResetPositions(gridColStart, gridRowStart);
                        if (CheckWin()) {
                            MessageBox.Show($"You won the game in {Form.timer_text.Text} and {Form.moves_text.Text} moves!", "You won!", MessageBoxButton.OK);
                            gameSetup(gameIndex);
                        }
                    } else {
                        ResetPositions(gridColStart, gridRowStart);
                    }
                } else {
                    ResetPositions(gridColStart, gridRowStart);
                }

                mediaPlayer.Open(new Uri(@"assets\sfx\fall_" + rnd.Next(1, 6).ToString() + ".wav", UriKind.Relative));
                mediaPlayer.Play();
                draggable.ReleaseMouseCapture();
            }
            amountCardsDragging = -1;
            cardsDragging.Clear();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            var draggable = sender as Canvas;
            if (isDragging && draggable != null) {
                var cardData = draggable.Name.Split('_');

                Point p = e.GetPosition(Form.main_canvas);

                double leftSet = p.X - dragOffset.X;
                double topSet = p.Y - dragOffset.Y;

                for (int i = 0; i < amountCardsDragging; i++) {
                    Canvas.SetLeft(cardsDragging[i], leftSet);
                    Canvas.SetTop(cardsDragging[i], topSet + 20 * i);

                    // prevent card from going out of main_canvas
                    if (Canvas.GetLeft(cardsDragging[i]) < 0)
                        Canvas.SetLeft(cardsDragging[i], 0);
                    else if (Canvas.GetLeft(cardsDragging[i]) > Form.main_canvas.ActualWidth - cardsDragging[i].ActualWidth)
                        Canvas.SetLeft(cardsDragging[i], Form.main_canvas.ActualWidth - cardsDragging[i].ActualWidth);

                    if (Canvas.GetTop(cardsDragging[i]) < 0)
                        Canvas.SetTop(cardsDragging[i], 0);
                    else if (Canvas.GetTop(cardsDragging[i]) > Form.main_canvas.ActualHeight - cardsDragging[i].ActualHeight)
                        Canvas.SetTop(cardsDragging[i], Form.main_canvas.ActualHeight - cardsDragging[i].ActualHeight);
                }

                // loop through all cards except one being dragged, check if the mouse is within bounds, return first card
                try {
                    foreach (var card in Form.main_canvas.Children.OfType<Canvas>().Where(c => !cardsDragging.Contains(c))) {
                        cardData = card.Name.Split('_');
                        if (p.X > Canvas.GetLeft(card) && p.X < Canvas.GetLeft(card) + card.ActualWidth && p.Y > Canvas.GetTop(card) && p.Y < Canvas.GetTop(card) + card.ActualHeight) {
                            gridColOver = Int32.Parse(cardData[1]);
                            gridRowOver = Int32.Parse(cardData[2]);
                            break;
                        }
                    }
                } catch {
                    gridColOver = -1;
                    gridRowOver = -1;
                }
            }
        }

        /// <summary>
        /// Draw cards form pile, shuffle when empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        #endregion ControlHandlers

        /// <summary>
        /// Transfers {amountCardsDragging} over to a new set
        /// </summary>
        /// <param name="toSet">recieving set</param>
        private void massTransfer(ref Set toSet) {
            Set stack = stacks.Where(c => c.gridCol == gridColStart && c.gridRow == gridRowStart).Single();
            Set tempSet = new Set(stack.cards.TakeLast(amountCardsDragging).Reverse().ToList(), stack.gridRow, stack.gridCol, stack.cardsExpanded, stack.maxCardExpandable, stack.stackType);

            for (int i = amountCardsDragging - 1; i >= 0; i--) {
                tempSet.TransferCard(ref toSet, true);
            }

            stack.cards.RemoveRange(stack.cards.Count - amountCardsDragging, amountCardsDragging);
            stack.FlipCard();
            stacks[stacks.FindIndex(c => c.gridCol == gridColStart && c.gridRow == gridRowStart)] = stack;
        }

        private void Draw(object sender, MouseButtonEventArgs e) {
            if (drawStack.cards.Count != 0) {
                switch (gameRules[gameIndex].dealingRule) {
                    case GameRules.DealingRules.DrawToWaste:
                        var tempWaste = wasteStack;
                        for (int i = 0; i < gameRules[gameIndex].cardsOnDraw; i++) {
                            try {
                                drawStack.cards[drawStack.cards.Count - 1].flipped = false;
                                drawStack.TransferCard(ref tempWaste, true);
                                mediaPlayer.Open(new Uri(@"assets\sfx\deal_" + rnd.Next(1, 6).ToString() + ".wav", UriKind.Relative));
                                mediaPlayer.Play();
                            } catch { }
                        }
                        Form.moves_text.Text = (Int32.Parse(Form.moves_text.Text) + 1).ToString();
                        int index = stacks.FindIndex(c => c.stackType == GameRules.StackType.waste);
                        stacks[index] = tempWaste;
                        break;

                    case GameRules.DealingRules.DrawToTableaus:
                        for (int i = 0; i < tableauStacks.Count; i++) {
                            try {
                                var tempTableau = tableauStacks[i];
                                drawStack.TransferCard(ref tempTableau);
                                tempTableau.FlipCard();
                                tableauStacks[i] = tempTableau;
                                mediaPlayer.Open(new Uri(@"assets\sfx\deal_" + rnd.Next(1, 6).ToString() + ".wav", UriKind.Relative));
                                mediaPlayer.Play();
                                Form.moves_text.Text = (Int32.Parse(Form.moves_text.Text) + 1).ToString();
                            } catch { }
                        }
                        break;

                    case GameRules.DealingRules.None:
                        break;
                }
            } else if (wasteStack.cards.Count != 0 && gameRules[gameIndex].allowRestock) {
                try {
                    var tempDraw = drawStack;

                    // go through all the cards in the waste stack, in reverse, and flip them and move them to draw
                    for (int i = wasteStack.cards.Count - 1; i >= 0; i--) {
                        wasteStack.cards[i].flipped = true;
                        wasteStack.TransferCard(ref tempDraw, wasteStack.cards[i], true);
                    }
                    int index = stacks.FindIndex(c => c.stackType == GameRules.StackType.draw);
                    stacks[index] = tempDraw;
                    mediaPlayer.Open(new Uri(@"assets\sfx\shuffle.wav", UriKind.Relative));
                    mediaPlayer.Play();
                    Form.moves_text.Text = (Int32.Parse(Form.moves_text.Text) + 1).ToString();
                } catch { }
            }
            ResetPositions(drawStack.gridCol, drawStack.gridRow);
            ResetPositions(wasteStack.gridCol, wasteStack.gridRow);
        }

        private bool CheckWin() {
            foreach (var stack in foundationStacks) {
                if (stack.cards.Count != 13)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// reset the positions of cards on the canvas by animating them to the correct place then reseting the
        /// </summary>
        /// <param name="gridCol"></param>
        /// <param name="gridRow"></param>
        /// <param name="invertDraggingPositions">used for when moving cards</param>
        public void ResetPositions(int gridCol, int gridRow, bool invertDraggingPositions = false) {
            // get all the cards in the set with the given col/row, ordered by index
            List<Canvas> selectedCards = Form.main_canvas.Children.OfType<Canvas>()
                .Where(c => UIElementExtensions.GetGroupID(c) == 11)
                .Where(
                c => {
                    var cardData = c.Name.Split('_');

                    if (Int32.Parse(cardData[1]) == gridCol && Int32.Parse(cardData[2]) == gridRow)
                        return true;
                    return false;
                }
                ).OrderBy(
                c => {
                    var cardData = c.Name.Split('_');
                    return Int32.Parse(cardData[7]);
                }
                ).ToList();

            // fixes bug where the top new cards are not going in the inverse position, eg. top will go at bottom during animation, this took me too long to fix :(
            // I still don't know why this happens, probably because I invert the cards when transferring them, somehow
            if (invertDraggingPositions && amountCardsDragging > 0) {
                List<Canvas> bottomCards = new List<Canvas>();
                bottomCards = selectedCards.Take(selectedCards.Count - amountCardsDragging).ToList();
                List<Canvas> topCards = new List<Canvas>();
                topCards = selectedCards.TakeLast(amountCardsDragging).Reverse().ToList();
                bottomCards.AddRange(topCards);
                selectedCards = bottomCards;
            }

            Set tempStack = stacks.Where(c => c.gridCol == gridCol && c.gridRow == gridRow).Single();

            double cardColWidth = Form.main_canvas.ActualWidth / colCount;
            double cardRowHeight = Form.main_canvas.ActualHeight / rowCount;

            var cardList = tempStack.drawSet().Select(c => c.Item2).ToList();

            for (int i = 0; i < selectedCards.Count; i++) {
                var tempCard = selectedCards[i];
                double topCanvas = tempStack.canvasTop + cardList[i];
                AnimateMove(ref tempCard, tempStack.canvasLeft, topCanvas, 200, true);
            }
        }

        public static readonly GameRules[] gameRules = new GameRules[] {
            new GameRules {
            name = "Klondike - 1 draw",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 1,
            cells = 0,
            cardsOnDraw = 1,
            groupMoveMax = 13,
            initalCardDeal = 28,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            allowRestock = true,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.KingOnly,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.normal,

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
        }, // Klondike - 1 draw
            new GameRules {
            name = "Klondike - 3 draw",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 1,
            cells = 0,
            cardsOnDraw = 3,
            groupMoveMax = 13,
            initalCardDeal = 28,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            allowRestock = true,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.KingOnly,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.normal,

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
        }, // Klondike - 3 draw
            new GameRules {
            name = "Strict Klondike",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 1,
            cells = 0,
            cardsOnDraw = 1,
            groupMoveMax = 13,
            initalCardDeal = 28,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            allowRestock = false,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.KingOnly,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.normal,

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
        }, // Strict Klondike
            new GameRules {
            name = "Yukon",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 0,
            cells = 0,
            cardsOnDraw = 1,
            groupMoveMax = 13,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            allowRestock = true,
            strictTableauRule = false,

            emptyTableauRule = GameRules.EmptyTableauRules.KingOnly,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.normal,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false) },
                new List<(int index, bool flipped)>{ (1, true), (7, false), (13, false), (19, false), (25, false), (31, false) },
                new List<(int index, bool flipped)>{ (2, true), (8, true),  (14, false), (20, false), (26, false), (32, false), (37, false) },
                new List<(int index, bool flipped)>{ (3, true), (9, true),  (15, true),  (21, false), (27, false), (33, false), (38, false), (42, false) },
                new List<(int index, bool flipped)>{ (4, true), (10, true), (16, true),  (22, true),  (28, false), (34, false), (39, false), (43, false), (46, false) },
                new List<(int index, bool flipped)>{ (5, true), (11, true), (17, true),  (23, true),  (29, true),  (35, false), (40, false), (44, false), (47, false), (49, false) },
                new List<(int index, bool flipped)>{ (6, true), (12, true), (18, true),  (24, true),  (30, true),  (36, true),  (41, false), (45, false), (48, false), (50, false), (51, false) },
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (100,100, GameRules.StackType.draw),
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
        }, // Yukon
            new GameRules {
            name = "Strict Yukon (Russian Solitaire)",
            packs = 1,
            foundations = 4,
            tableaus = 7,
            waste = 0,
            cells = 0,
            cardsOnDraw = 1,
            groupMoveMax = 13,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            allowRestock = true,
            strictTableauRule = false,

            emptyTableauRule = GameRules.EmptyTableauRules.KingOnly,
            tableauColourRule = GameRules.TableauColourRule.sameSuit,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.DrawToWaste,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.normal,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false) },
                new List<(int index, bool flipped)>{ (1, true), (7, false), (13, false), (19, false), (25, false), (31, false) },
                new List<(int index, bool flipped)>{ (2, true), (8, true),  (14, false), (20, false), (26, false), (32, false), (37, false) },
                new List<(int index, bool flipped)>{ (3, true), (9, true),  (15, true),  (21, false), (27, false), (33, false), (38, false), (42, false) },
                new List<(int index, bool flipped)>{ (4, true), (10, true), (16, true),  (22, true),  (28, false), (34, false), (39, false), (43, false), (46, false) },
                new List<(int index, bool flipped)>{ (5, true), (11, true), (17, true),  (23, true),  (29, true),  (35, false), (40, false), (44, false), (47, false), (49, false) },
                new List<(int index, bool flipped)>{ (6, true), (12, true), (18, true),  (24, true),  (30, true),  (36, true),  (41, false), (45, false), (48, false), (50, false), (51, false) },
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (100,100, GameRules.StackType.draw),
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
        }, // Strict Yukon (Russian Solitaire)
            new GameRules {
            name = "Freecell",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 4,
            cardsOnDraw = 0,
            groupMoveMax = 5,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.Any,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.onePlusFreeCells,

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
                (100,100, GameRules.StackType.draw),
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
        }, // Freecell
            new GameRules {
            name = "Baker's Game",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 4,
            cardsOnDraw = 0,
            groupMoveMax = 5,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.Any,
            tableauColourRule = GameRules.TableauColourRule.sameSuit,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.onePlusFreeCells,

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
                (100,100, GameRules.StackType.draw),
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
        }, // Baker's game
            new GameRules {
            name = "Forecell",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 4,
            cardsOnDraw = 0,
            groupMoveMax = 5,
            initalCardDeal = 48,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = true,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.Any,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.onePlusFreeCells,

            layoutStart = new List<List<(int index, bool flipped)>> {
                new List<(int index, bool flipped)>{ (0, false), (8, false),  (16, false), (24, false), (32, false), (40, false) },
                new List<(int index, bool flipped)>{ (1, false), (9, false),  (17, false), (25, false), (33, false), (41, false) },
                new List<(int index, bool flipped)>{ (2, false), (10, false), (18, false), (26, false), (34, false), (42, false) },
                new List<(int index, bool flipped)>{ (3, false), (11, false), (19, false), (27, false), (35, false), (43, false) },
                new List<(int index, bool flipped)>{ (4, false), (12, false), (20, false), (28, false), (36, false), (44, false) },
                new List<(int index, bool flipped)>{ (5, false), (13, false), (21, false), (29, false), (37, false), (45, false) },
                new List<(int index, bool flipped)>{ (6, false), (14, false), (22, false), (30, false), (38, false), (46, false) },
                new List<(int index, bool flipped)>{ (7, false), (15, false), (23, false), (31, false), (39, false), (47, false) }
            },
            cardStackLayout = new List<(int, int, GameRules.StackType)> {
                (100,100, GameRules.StackType.draw),
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
        }, // Forecell
            new GameRules {
            name = "Twocell",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 2,
            cardsOnDraw = 0,
            groupMoveMax = 5,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.Any,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.onePlusFreeCells,

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
                (100,100, GameRules.StackType.draw),
                (0,0, GameRules.StackType.cell),
                (1,0, GameRules.StackType.cell),
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
        }, // Twocell
            new GameRules {
            name = "Threecell",
            packs = 1,
            foundations = 4,
            tableaus = 8,
            waste = 0,
            cells = 3,
            cardsOnDraw = 0,
            groupMoveMax = 5,
            initalCardDeal = 52,

            allowGroupMoveFromWaste = false,
            shuffleOnRestock = false,
            strictFoundationRule = true,
            cellsFilled = false,
            strictTableauRule = true,

            emptyTableauRule = GameRules.EmptyTableauRules.Any,
            tableauColourRule = GameRules.TableauColourRule.alternatingColours,
            tableauStackingRule = GameRules.TableauStackingRule.highToLow,
            foundationOrderRule = GameRules.FoundationOrderRules.LowToHigh,
            dealingRule = GameRules.DealingRules.None,
            foundationMoveRule = GameRules.FoundationMoveRules.Single,
            groupMoveRules = GameRules.GroupMoveRules.onePlusFreeCells,

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
                (100,100, GameRules.StackType.draw),
                (0,0, GameRules.StackType.cell),
                (1,0, GameRules.StackType.cell),
                (2,0, GameRules.StackType.cell),
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
        }, // Threecell
        };
    }
}