using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using WPF_Audio.Models;
using WPF_Audio.Services;

namespace WPF_Audio
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<AudioTrack> Playlist { get; set; } = new ObservableCollection<AudioTrack>();
        public int currentTrackIndex = -1;
        public AudioTrack currentTrack;

        public DispatcherTimer progressTimer;
        private bool isPaused = false;

        public MainWindow()
        {
            InitializeComponent();
            PageFrame.Navigate(new MainPage()); 

            progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            progressTimer.Tick += ProgressTimer;
            VolumeSlider.Value = 0.5;
        }

        private void GoToLibraryPage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 1;
            NowPlayingPanel.Visibility = Visibility.Collapsed;
            PageFrame.Navigate(new Library());
        }

        private void GoToHomePage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 0;
            NowPlayingPanel.Visibility = Visibility.Visible;
            PageFrame.Navigate(new MainPage());
        }

        private void GoToPlayListsPage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 1;
            NowPlayingPanel.Visibility = Visibility.Collapsed;
            PageFrame.Navigate(new PlayLists());
        }

        public void UpdateNowPlayingImage(string photoData)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                if (!string.IsNullOrWhiteSpace(photoData) && !photoData.StartsWith("pack://"))
                {
                    byte[] imageBytes = Convert.FromBase64String(photoData);
                    using (var stream = new System.IO.MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
                else
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/default1.jpg", UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                NowPlayingImage.Source = bitmap;
                BackGround_photo.Source = bitmap;
                ControlPanel_photo.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при обновлении изображения:\n{ex.Message}");
            }
        }

        public void UpdateNowPlayingInfo(string title, string performer)
        {
            NowPlayingTitle.Text = title;
            NowPlayingPerformer.Text = performer;
            ControlPanelTitle.Text = title;
            ControlPanelPerformer.Text = performer;
        }

        private void ProgressTimer(object sender, EventArgs e)
        {
            double currentSec = AudioService.Instance.GetCurrentTime();
            double totalSec = AudioService.Instance.GetTotalTime();
            ProgressSlider.Minimum = 0;
            ProgressSlider.Maximum = totalSec;
            ProgressSlider.Value = currentSec;
            CurrentTimeText.Text = TimeSpan.FromSeconds(currentSec).ToString(@"mm\:ss");
            TotalTimeText.Text = TimeSpan.FromSeconds(totalSec).ToString(@"mm\:ss");
        }

        private void VolumeSlider_Value(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioService.Instance.SetVolume(VolumeSlider.Value);
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist != null && Playlist.Count > 0 && currentTrackIndex > 0)
            {
                currentTrackIndex--;
                currentTrack = Playlist[currentTrackIndex];
                AudioService.Instance.Play(currentTrack.FilePath);
                UpdateNowPlayingImage(currentTrack.Photo);
                UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist != null && Playlist.Count > 0 && currentTrackIndex < Playlist.Count - 1)
            {
                currentTrackIndex++;
                currentTrack = Playlist[currentTrackIndex];
                AudioService.Instance.Play(currentTrack.FilePath);
                UpdateNowPlayingImage(currentTrack.Photo);
                UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                AudioService.Instance.Resume();
                PlayPauseButton.Tag = "Play";
                isPaused = false;
            }
            else
            {
                AudioService.Instance.Pause();
                PlayPauseButton.Tag = "Stop";
                isPaused = true;
            }
        }

        private void ProgressSlider_Value(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ProgressSlider.IsMouseCaptureWithin)
            {
                AudioService.Instance.Seek(TimeSpan.FromSeconds(ProgressSlider.Value));
            }
        }
    }
}
