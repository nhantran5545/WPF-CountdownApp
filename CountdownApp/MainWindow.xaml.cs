using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PRN221_CE160513_Assignment03
{
    public partial class MainWindow : Window
    {
        private TimeSpan countdownTime;
        private DateTime startTime;
        private bool isCountingDown = false;
        private Thread countdownThread;
        private SoundPlayer player;
        private object countdownLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            AddTimeItems();
        }

        private void AddTimeItems()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                string hourValue = hour.ToString("00");
                ComboBoxItem hourItem = new ComboBoxItem
                {
                    Content = hourValue,
                    Tag = hour
                };
                hoursComboBox.Items.Add(hourItem);
            }

            for (int minute = 0; minute < 60; minute++)
            {
                string minuteValue = minute.ToString("00");
                ComboBoxItem minuteItem = new ComboBoxItem
                {
                    Content = minuteValue,
                    Tag = minute
                };
                minutesComboBox.Items.Add(minuteItem);
            }

            for (int second = 0; second < 60; second++)
            {
                string secondValue = second.ToString("00");
                ComboBoxItem secondItem = new ComboBoxItem
                {
                    Content = secondValue,
                    Tag = second
                };
                secondsComboBox.Items.Add(secondItem);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            lock (countdownLock)
            {
                if (!isCountingDown)
                {
                    // Check if the user has selected values for hours, minutes, and seconds
                    if (hoursComboBox.SelectedItem == null || minutesComboBox.SelectedItem == null || secondsComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a value for hours, minutes, and seconds.");
                        return;
                    }

                    int hours = Convert.ToInt32(((ComboBoxItem)hoursComboBox.SelectedItem).Tag);
                    int minutes = Convert.ToInt32(((ComboBoxItem)minutesComboBox.SelectedItem).Tag);
                    int seconds = Convert.ToInt32(((ComboBoxItem)secondsComboBox.SelectedItem).Tag);
                    countdownTime = new TimeSpan(hours, minutes, seconds);

                    // Start the countdown timer
                    startTime = DateTime.Now;
                    isCountingDown = true;

                    // Create and start a new thread for the countdown
                    countdownThread = new Thread(Countdown);
                    countdownThread.Start();

                    hoursComboBox.IsEnabled = false;
                    minutesComboBox.IsEnabled = false;
                    secondsComboBox.IsEnabled = false;
                    startButton.IsEnabled = false;
                    stopButton.IsEnabled = true;
                    alarmButton.IsEnabled = false;
                }
            }
        }


        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isCountingDown)
            {
                // Stop the countdown
                isCountingDown = false;

                hoursComboBox.IsEnabled = true;
                minutesComboBox.IsEnabled = true;
                secondsComboBox.IsEnabled = true;
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
                alarmButton.IsEnabled = true;
            }
        }

        private void Countdown()
        {
            while (isCountingDown)
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;
                TimeSpan remainingTime = countdownTime - elapsedTime;

                // Check if the countdown is complete
                if (remainingTime <= TimeSpan.Zero)
                {
                    // Stop the countdown
                    isCountingDown = false;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        hoursComboBox.IsEnabled = true;
                        minutesComboBox.IsEnabled = true;
                        secondsComboBox.IsEnabled = true;
                        startButton.IsEnabled = true;
                        stopButton.IsEnabled = false;
                        alarmButton.IsEnabled = true;
                    });

                    // Play the alarm sound
                    PlayAlarm();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        countdownLabel.Content = "00:00:00";
                    });

                    break;
                }

                // Update the countdown label on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    countdownLabel.Content = remainingTime.ToString(@"hh\:mm\:ss");
                });

                Thread.Sleep(100);
            }
        }

        private void PlayAlarm()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string soundPath = "Sounds/countdown.wav";
                MediaPlayer mediaPlayer = new MediaPlayer();

                mediaPlayer.Open(new Uri(soundPath, UriKind.Relative));

                mediaPlayer.Volume = 1.0;
                mediaPlayer.Play();

                Task.Run(() =>
                {
                    MessageBoxResult result = MessageBox.Show("Countdown Complete!", "Alarm", MessageBoxButton.OK);
                    if (result == MessageBoxResult.OK)
                    {
                        StopAlarm(mediaPlayer);
                    }
                });
            });
        }

        private void StopAlarm(MediaPlayer mediaPlayer)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
            });
        }

        private void alarmButton_Click(object sender, RoutedEventArgs e)
        {
            AlarmWindow alarmWindow = new AlarmWindow();
            alarmWindow.ShowDialog();
        }
    }
}
