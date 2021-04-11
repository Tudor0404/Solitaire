using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Solitaire {

    /// <summary>
    /// Colour of the card
    /// </summary>
    internal enum cardColour { red, black };

    /// <summary>
    /// Card suits
    /// </summary>
    internal enum cardSuit { clubs, diamonds, hearts, spades };

    internal enum cardRank { ace, two, three, four, five, six, seven, eight, nine, ten, jack, queen, king };

    internal class Card {

        /// <summary>
        /// card class initializer
        /// </summary>
        /// <param name="r">Rank</param>
        /// <param name="s">Suit</param>
        /// <param name="f">Flipped status</param>
        public Card(int r, int s, int setCol_ = -1, int setRow_ = -1, bool f = false) {
            rank = r;
            suit = s;
            flipped = f;
            setCol = setCol_;
            setRow = setRow_;
        }

        public int rank { private set; get; }
        public int suit { private set; get; }

        public bool flipped;
        public int setCol;
        public int setRow;
        public int ZIndex = 0;
        public Canvas canvasCard;

        public string name { get { return $"_{setCol}_{setRow}_{1}_{(flipped ? "1" : "0")}_{rank}_{suit}_{ZIndex}"; } } // 1 represents it is a card, not a template

        public cardColour getCardColour {
            get {
                switch (getCardSuit) {
                    case cardSuit.clubs:
                    case cardSuit.spades:
                        return cardColour.black;

                    default:
                        return cardColour.red;
                }
            }
        }

        /// <summary>
        /// return enum reference of rank
        /// </summary>
        public cardRank getCardRank {
            get {
                return (cardRank)rank;
            }
        }

        /// <summary>
        /// return enum value of suit
        /// </summary>
        public cardSuit getCardSuit {
            get {
                return (cardSuit)suit;
            }
        }

        /// <summary>
        /// returns the path of the image associated to the card, relative to root
        /// </summary>
        public string cardImagePath {
            get {
                string path = @"assets\playingCards\back.png";

                if (!flipped) {
                    string[] rankPath = new string[] { "ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "jack", "queen", "king" };
                    string? addition = rank > 9 ? "2" : null;
                    path = @"assets\playingCards\" + $"{rankPath[rank]}_of_{getCardSuit}{addition}.png";
                }

                return path;
            }
        }

        /// <summary>
        /// For debug reasons
        /// </summary>
        /// <returns></returns>
        public static string getStringFromName(string name) {
            var cardData = name.Split('_');
            return $"{(cardRank)Int32.Parse(cardData[5])} of {(cardSuit)Int32.Parse(cardData[6])}";
        }

        public override string ToString() {
            return $"{getCardRank} of {getCardSuit}";
        }

        /// <summary>
        /// returns a canvas with the image of the card inside
        /// </summary>
        public Canvas cardDraw() {
            int width = 100;
            int height = 146;

            var canvas = new Canvas() { Width = width, Height = height };
            ImageBrush brush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(cardImagePath, UriKind.Relative)) };
            canvas.Background = brush;
            canvas.Name = name;
            canvasCard = canvas;
            return canvas;
        }

        public void renameCard() {
            MainWindow Form = Application.Current.Windows[0] as MainWindow;
            Form.main_canvas.Children.OfType<Canvas>().Where(c => c == canvasCard).First().Name = name;
            canvasCard.Name = name;
        }
    }
}