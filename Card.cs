﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Solitaire {

    /// <summary>
    /// Colour of the card
    /// </summary>
    enum cardColour { red, black };

    /// <summary>
    /// Card suits
    /// </summary>
    enum cardSuit { clubs, diamonds, hearts, spades };
    enum cardRank { ace, two, three, four, five, six, seven, eight, nine, ten, jack, queen, king };
    class Card {
        /// <summary>
        /// card class initializer
        /// </summary>
        /// <param name="r">Rank</param>
        /// <param name="s">Suit</param>
        /// <param name="f">Flipped status</param>
        public Card(int r, int s, bool f = false) {
            rank = r;
            suit = s;
            flipped = f;
        }

        public int rank { private set; get; }
        public int suit { private set; get; }

        public bool flipped { set; get; }

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

        public Brush getCardColourBrush {
            get {
                if (getCardColour == cardColour.black) {
                    return new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else {
                    return new SolidColorBrush(Color.FromRgb(209, 45, 54));
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

        public override string ToString() {
            return $"{getCardRank} of {suit}";
        }

        public Canvas cardDraw() {
            Random rnd = new Random();

            // cards are at a ratio of 2:3
            var canvas = new Canvas() { Width = 90, Height = 135 };

            List<SolidColorBrush> colours = new List<SolidColorBrush> {
                new SolidColorBrush(Color.FromRgb(81, 24, 69)),
                new SolidColorBrush(Color.FromRgb(144, 12, 63)),
                new SolidColorBrush(Color.FromRgb(199, 0, 57)),
                new SolidColorBrush(Color.FromRgb(255, 87, 51))};

            if (flipped) {
                int baseSquareSize = 15;
                for (int c = 0; c < 6; c++) {
                    for (int r = 0; r < 9; r++) {
                        var tempRect = new Rectangle() { Width = baseSquareSize, Height = baseSquareSize, Fill = colours[rnd.Next(0, colours.Count)] };
                        tempRect.SnapsToDevicePixels = true;
                        canvas.Children.Add(tempRect);
                        Canvas.SetTop(tempRect, r * baseSquareSize);
                        Canvas.SetLeft(tempRect, c * baseSquareSize);
                    }
                }
            }
            else {
                var cardGrid = new Grid();

                // define the grid
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                cardGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                cardGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) });
                cardGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });


                // sets the rank content and colour
                var tempLabelTopLeft = new Label();
                var tempLabelMiddle = new Label();
                var tempLabelBottomRight = new Label();

                tempLabelTopLeft.Content = getCardRank;
                tempLabelTopLeft.Foreground = getCardColourBrush;

                tempLabelMiddle.Content = getCardRank;
                tempLabelMiddle.Foreground = getCardColourBrush;

                tempLabelBottomRight.Content = getCardRank;
                tempLabelBottomRight.Foreground = getCardColourBrush;

                // sets the suit icon and colour
                var tempIcon = new PackIcon();
                switch (getCardSuit) {
                    case cardSuit.clubs:
                        tempIcon.Kind = PackIconKind.CardsClub;
                        break;
                    case cardSuit.diamonds:
                        tempIcon.Kind = PackIconKind.CardsDiamond;
                        break;
                    case cardSuit.hearts:
                        tempIcon.Kind = PackIconKind.CardsHeart;
                        break;
                    case cardSuit.spades:
                        tempIcon.Kind = PackIconKind.CardsSpade;
                        break;
                    default:
                        break;
                }
                tempIcon.Foreground = getCardColourBrush;
            }

            // card border
            var border = new Border() { Width = canvas.Width, Height = canvas.Height, BorderBrush = colours[0], BorderThickness = new Thickness(3), CornerRadius = new CornerRadius(10) };
            border.SnapsToDevicePixels = true;
            canvas.Children.Add(border);

            // mask to round the corners
            var canvasMask = new Canvas() { Width = 90, Height = 135 };
            var mask = new Border() { Width = 90, Height = 135, CornerRadius = new CornerRadius(13), Background = Brushes.Black, BorderThickness = new Thickness(1) };
            mask.SnapsToDevicePixels = true;
            canvasMask.Children.Add(mask);
            Canvas.SetTop(mask, 0);
            Canvas.SetLeft(mask, 0);

            // add mask
            var b = new VisualBrush();
            b.Visual = canvasMask;
            canvas.OpacityMask = b;

            return canvas;
        }


    }
}
