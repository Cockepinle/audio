using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
namespace AudioPlayerr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer timer;
        public bool buttonsEnabled = false;
        private string selectedDirectory;
        public TimeSpan savedPosition;
        private List<string> playlist;
        private int index = 0;
        private Stack<int> playBack = new Stack<int>();
        private bool isRepeating = false;
        private bool isRepeatingTrack = false;
        private RoutedEventHandler mediaEndedHandler;
        private bool isShuffled = false;
        private List<string> originalPlaylist;


        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            mediaElement.MediaEnded += (sender, e) =>
            {
                index = 0; 
                PlayTrack(); 
                playBack.Push(index);
            };
        }


        private void open_Click(object sender, RoutedEventArgs e)
        { 
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                selectedDirectory = dialog.FileName;
                string[] files = Directory.GetFiles(selectedDirectory, "*.mp3");
                if (files.Length > 0)
                {
                    playlist = new List<string>(files);
                    listBox.ItemsSource = playlist.Select(file => Path.GetFileName(file));
                    PlayTrack(); 
                }
              
            }
        }
        private void repeat_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;
            isRepeatingTrack = isRepeating;
            if (isRepeatingTrack)
            {
                mediaElement.MediaEnded += mediaEndedHandler; 
            }
            else
            {
                mediaElement.MediaEnded -= mediaEndedHandler; 
            }
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            if (index > 0)
            {
                index--;
                mediaElement.MediaEnded -= mediaEndedHandler; 
                PlayTrack();
                playBack.Push(index);
            }
        }
     

        private void forward_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.MediaEnded += mediaEndedHandler;
            if (index < playlist.Count - 1)
            {
                index++;
                PlayTrack();
                playBack.Push(index);
            }
            else
            {
                index = 0; 
                PlayTrack();
                playBack.Push(index);
            }
        }

        private void PlayTrack()
        {
            if (index >= 0 && index < playlist.Count && playlist.Count > 0)
            {
                mediaElement.Stop();
                mediaElement.Source = new Uri(playlist[index]);
                StartUpdatingTime();

                if (mediaEndedHandler != null)
                {
                    mediaElement.MediaEnded -= mediaEndedHandler;
                }

                mediaEndedHandler = (sender, e) =>
                {
                    if (isRepeatingTrack)
                    {
                        mediaElement.Position = TimeSpan.Zero;
                        mediaElement.Play();
                    }
                    else
                    {
                        if (index < playlist.Count - 1) 
                        {
                            index++; 
                            PlayTrack(); 
                            playBack.Push(index);
                        }
                        else
                        {
                            index = 0; 
                            PlayTrack();
                            playBack.Push(index);
                        }
                    }
                };
                mediaElement.MediaEnded += mediaEndedHandler;

                mediaElement.Play();
            }
        }


        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (index < playlist.Count - 1)
            {
                index++;
                PlayTrack();
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                string selectedFile = Path.Combine(selectedDirectory, (string)listBox.SelectedItem);
                mediaElement.Source = new Uri(selectedFile); 
            }
        }

        private void history_Click(object sender, RoutedEventArgs e)
        {
            MyNewWindow myNewWindow = new MyNewWindow(this);
            List<string> tracks = GetListenedTracks();
            myNewWindow.AddListenedTracks(tracks);
            if (myNewWindow.ShowDialog() == true) 

            myNewWindow.Close();
        }
        private List<string> GetListenedTracks()
        {
            List<string> listenedTracks = new List<string>();
            foreach (int playedIndex in playBack)
            {
                if (playedIndex >= 0 && playedIndex < playlist.Count)
                {
                    string trackName = Path.GetFileName(playlist[playedIndex]);
                   
                    listenedTracks.Add(trackName);
                }
            }
            return listenedTracks;
        }
        public void PlaySelectedTrack(string selectedTrack)
        {
            if (selectedTrack != null)
            {
                string selectedFile = Path.Combine(selectedDirectory, selectedTrack);
                mediaElement.Source = new Uri(selectedFile);
                mediaElement.Play();
            }
        }

        private void random_Click(object sender, RoutedEventArgs e)
        {
            if (!isShuffled)
            {
                Random rnd = new Random();
                originalPlaylist = new List<string>(playlist);
                playlist = playlist.OrderBy(x => rnd.Next()).ToList(); 
                index = 0;
                PlayTrack(); 
                isShuffled = true;
            }
            else
            {
                playlist = new List<string>(originalPlaylist); 
                index = 0; 
                PlayTrack(); 
                isShuffled = false;
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(slider.Value - mediaElement.Position.TotalSeconds) > 1)
            {
                mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
            }
        }
        private void slidersound_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = slidersound.Value; 
        }
        private void UpdateSliderPosition(object sender, EventArgs e)
        {
            if (mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                slider.Value = mediaElement.Position.TotalSeconds;
            }
        }
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            CompositionTarget.Rendering += UpdateSliderPosition;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                nachaloTimeTextBlock.Text = mediaElement.Position.ToString(@"mm\:ss");
                if (mediaElement.NaturalDuration.HasTimeSpan)
                {
                    endTimeTextBlock.Text = mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }
            };
            timer.Start();
        }

            private async void play_Click(object sender, RoutedEventArgs e)
        {
            if (play.Content.ToString() == "Играть" && mediaElement.Source != null)
            {
                if (savedPosition != TimeSpan.Zero)
                {
                    mediaElement.Position = savedPosition;
                    savedPosition = TimeSpan.Zero;
                }
                else
                {
                    savedPosition = mediaElement.Position;
                }

                var newSource = new Uri(mediaElement.Source.ToString());

                if (mediaElement.Source != newSource)
                {
                    mediaElement.Source = newSource;
                }

                mediaElement.Play();
                StartUpdatingTime();

                play.Content = "Пауза";
                buttonsEnabled = true;
            }
            else if (play.Content.ToString() == "Пауза")
            {
                savedPosition = mediaElement.Position;
                mediaElement.Pause();
                StopUpdatingTime();
                play.Content = "Играть";
                buttonsEnabled = false;
            }

            UpdatePosition();
        }
        private void StartUpdatingTime()
        {
            CompositionTarget.Rendering += UpdateMediaTime;
        }

        private void StopUpdatingTime()
        {
            CompositionTarget.Rendering -= UpdateMediaTime;
        }

        private void UpdateMediaTime(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                nachaloTimeTextBlock.Text = mediaElement.Position.ToString(@"mm\:ss");
                endTimeTextBlock.Text = (mediaElement.NaturalDuration.TimeSpan - mediaElement.Position).ToString(@"mm\:ss");
            }
        }
        public TimeSpan CurrentPosition
        {
            get { return (TimeSpan)GetValue(CurrentPositionProperty); }
            set { SetValue(CurrentPositionProperty, value); }
        }

        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register("CurrentPosition", typeof(TimeSpan), typeof(MainWindow), new PropertyMetadata(TimeSpan.Zero));

        private void UpdatePosition()
        {
            CurrentPosition = mediaElement.Position;
        }

    }
}
