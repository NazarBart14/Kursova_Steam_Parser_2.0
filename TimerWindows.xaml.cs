using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Kursova_Steam_Parser_2._0
{
    public partial class TimerWindows : Window
    {
        private DispatcherTimer timer;
        private TimeSpan timeElapsed;

        public TimerWindows()
        {
            InitializeComponent();

            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // Initial time value
            timeElapsed = TimeSpan.Zero;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Increase the elapsed time by 1 second
            timeElapsed = timeElapsed.Add(TimeSpan.FromSeconds(1));

            // Update the timer label on the screen
            timerLabel.Content = timeElapsed.ToString(@"hh\:mm\:ss");
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            // Start the timer
            timer.Start();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the timer
            timer.Stop();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to reset the timer?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Reset the timer to the initial value
                timeElapsed = TimeSpan.Zero;
                timerLabel.Content = timeElapsed.ToString(@"hh\:mm\:ss");
            }
        }
    }
}
