using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Media;
using System.Timers;
using System.Windows.Threading;
using System.Numerics;

namespace PRN221_CE160513_Assignment03
{
    public partial class AlarmWindow : Window
    {
        private DateTime alarmTime;
        private bool isAlarmSet;
        private Thread alarmThread;
        private DispatcherTimer timer;
        private volatile bool stopAlarmThread;
        private SoundPlayer player;

        public AlarmWindow()
        {
            InitializeComponent();

            // Set the current time label
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); 
            timer.Tick += Timer_Tick;
            timer.Start();

            InitializeComboBoxes();

            player = new SoundPlayer("Sounds/alarm.wav");

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // current time
            currentTimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
        }


        private void InitializeComboBoxes()
        {
            // Add hours (0 to 23) 
            for (int hour = 0; hour < 24; hour++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = hour.ToString("D2"), 
                    Tag = hour
                };
                hoursComboBox.Items.Add(item);
            }

            // Add minutes (0 to 59) 
            for (int minute = 0; minute < 60; minute++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = minute.ToString("D2"), 
                    Tag = minute
                };
                minutesComboBox.Items.Add(item);
            }

            hoursComboBox.SelectedIndex = DateTime.Now.Hour;
            minutesComboBox.SelectedIndex = DateTime.Now.Minute;
        }

        private void setAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAlarmSet)
            {
                int hours = Convert.ToInt32(((ComboBoxItem)hoursComboBox.SelectedItem).Tag);
                int minutes = Convert.ToInt32(((ComboBoxItem)minutesComboBox.SelectedItem).Tag);

                // Calculate the alarm time from the current time
                DateTime currentTime = DateTime.Now;
                DateTime selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hours, minutes, 0);

                // If the selected time is before the current time, add one day to the alarm time
                if (selectedTime <= currentTime)
                {
                    selectedTime = selectedTime.AddDays(1);
                }

                alarmTime = selectedTime;

                // Stop the alarm thread if it's running
                StopAlarmThread();

                // Start the new alarm thread
                alarmThread = new Thread(AlarmClock);
                stopAlarmThread = false;
                alarmThread.Start();

                hoursComboBox.IsEnabled = false;
                minutesComboBox.IsEnabled = false;
                setAlarmButton.IsEnabled = false;

                stopButton.IsEnabled = true;

                isAlarmSet = true;
            }
        }


        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAlarmSet)
            {
                stopAlarmThread = true;

                hoursComboBox.IsEnabled = true;
                minutesComboBox.IsEnabled = true;
                setAlarmButton.IsEnabled = true;

                // Disable the Stop button
                stopButton.IsEnabled = false;

                isAlarmSet = false;
            }
        }

        private void StopAlarmThread()
        {
            if (alarmThread != null && alarmThread.IsAlive)
            {
                alarmThread.Join();
            }
        }

        private void AlarmClock()
        {
            while (!stopAlarmThread)
            {
                // Check if it's time for the alarm
                if (DateTime.Now >= alarmTime)
                {
                    // Play the alarm sound
                    Thread soundThread = new Thread(PlayAlarmSound);
                    soundThread.Start();

                    MessageBox.Show("Alarm!");

                    player.Stop();

                    break;
                }

                Thread.Sleep(100);
            }
        }

        private void PlayAlarmSound()
        {
            player.PlayLooping();
        }
    }
}
