using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Solitaire {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            // adding game types to combo box            
            gameType_comboBox.ItemsSource = Game.gameRules.Select(c => c.name);
            gameType_comboBox.SelectedIndex = 0;

            // initializing timer
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        protected bool isDragging;
        private Point clickPosition;
        private TranslateTransform originTT;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DateTime startTime;


        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var draggableControl = sender as Canvas;
            originTT = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
            isDragging = true;
            clickPosition = e.GetPosition(this);
            draggableControl.CaptureMouse();
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            isDragging = false;
            var draggable = sender as Canvas;
            draggable.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            var draggableControl = sender as Canvas;
            if (isDragging && draggableControl != null) {
                Point currentPosition = e.GetPosition(this);
                var transform = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
                transform.X = originTT.X + (currentPosition.X - clickPosition.X);
                transform.Y = originTT.Y + (currentPosition.Y - clickPosition.Y);
                draggableControl.RenderTransform = new TranslateTransform(transform.X, transform.Y);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e) {
            var tempCard = new Card(0, 0, true);
            var displayCard = tempCard.cardDraw();
            displayCard.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            displayCard.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            displayCard.MouseMove += Canvas_MouseMove;

            main_canvas.Children.Add(displayCard);
            Canvas.SetLeft(displayCard, 30);
            Canvas.SetTop(displayCard, 30);

            dispatcherTimer.Start();
            startTime = DateTime.Now;
        }

        void dispatcherTimer_Tick(object sender, EventArgs e) {
            if (startTime != null) {
                var newTime = DateTime.Now - startTime;
                timer_text.Text = String.Format("{0:00}:{1:00}", newTime.Minutes, newTime.Seconds);
            } else {
                timer_text.Text = "00:00";
            }
        }
    }
}
