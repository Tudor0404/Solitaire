using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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

            game = new Game(gameType_comboBox.SelectedIndex, resetTimer);
        }

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private DateTime startTime;
        private Game game;

        private void startButton_Click(object sender, RoutedEventArgs e) {
            game.gameSetup(gameType_comboBox.SelectedIndex);
        }

        private void resetTimer() {
            dispatcherTimer.Start();
            startTime = DateTime.Now;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            var newTime = DateTime.Now - startTime;
            timer_text.Text = String.Format("{0:00}:{1:00}", newTime.Minutes, newTime.Seconds);
        }
    }
}