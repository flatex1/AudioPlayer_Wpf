using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using WPF_Audio.Models;
using WPF_Audio.Services;
using System.Linq;
using WPF_Audio.Data;
using FontAwesome.WPF;
using System.Collections.Generic;
using System.ComponentModel;

namespace WPF_Audio
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<AudioTrack> Playlist { get; set; } = new ObservableCollection<AudioTrack>();
        public int currentTrackIndex = -1;
        public AudioTrack currentTrack;

        public DispatcherTimer progressTimer;
        private bool isPaused = false;

        public ObservableCollection<AudioTrack> LikedTracks { get; set; } = new ObservableCollection<AudioTrack>();
        public ObservableCollection<AudioTrack> SimilarTracks { get; set; } = new ObservableCollection<AudioTrack>();

        private bool isRepeatOne = false;
        private Random random = new Random();

        private Dictionary<string, string> _hotkeys = new();
        private readonly string[] _actions = new[] { "PlayPause", "Next", "Prev", "Like", "VolumeUp", "VolumeDown" };

        private bool _isSimilarTracksExpanded = true;
        public bool IsSimilarTracksExpanded
        {
            get => _isSimilarTracksExpanded;
            set
            {
                _isSimilarTracksExpanded = value;
                OnPropertyChanged(nameof(IsSimilarTracksExpanded));
                if (ToggleSimilarTracksIcon != null)
                    ToggleSimilarTracksIcon.Text = value ? "▼" : "▲";
            }
        }

        private bool _isSimilarTracksVisible = false;
        public bool IsSimilarTracksVisible
        {
            get => _isSimilarTracksVisible;
            set
            {
                _isSimilarTracksVisible = value;
                OnPropertyChanged(nameof(IsSimilarTracksVisible));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            PageFrame.Navigate(new MainPage()); 
            PageFrame.Navigated += PageFrame_Navigated;
            UpdateLikedTracksPanelVisibility();
            SetGreeting();

            progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            progressTimer.Tick += ProgressTimer;
            VolumeSlider.Value = 0.5;
            LoadLikedTracks();
            AudioService.Instance.PlaybackFinished += AudioService_PlaybackFinished;

            LoadHotkeys();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void GoToLibraryPage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 1;
            PageFrame.Navigate(new Library());
        }

        private void GoToHomePage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 0;
            PageFrame.Navigate(new MainPage());
        }

        private void GoToPlayListsPage(object sender, RoutedEventArgs e)
        {
            Overlay.Opacity = 1;
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
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/file-music.png", UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                ControlPanel_photo.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при обновлении изображения:\n{ex.Message}");
            }
        }

        public void UpdateNowPlayingInfo(string title, string performer)
        {
            ControlPanelTitle.Text = title;
            ControlPanelPerformer.Text = performer;
            UpdateLikeIcon();
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
                UpdateLikeIcon();
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
                UpdateLikeIcon();
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

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrack == null) return;
            currentTrack.IsLiked = !currentTrack.IsLiked;
            // Сохраняем в базе данных
            using (var db = new AudioDbContext())
            {
                var track = db.AudioTracks.FirstOrDefault(t => t.Id == currentTrack.Id);
                if (track != null)
                {
                    track.IsLiked = currentTrack.IsLiked;
                    db.SaveChanges();
                }
            }
            UpdateLikeIcon();
            UpdateLikedTracks(currentTrack);
        }

        private void UpdateLikeIcon()
        {
            if (currentTrack != null && currentTrack.IsLiked)
            {
                LikeIcon.Icon = FontAwesomeIcon.Heart;
                LikeIcon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {
                LikeIcon.Icon = FontAwesomeIcon.HeartOutline;
                LikeIcon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(206, 206, 151)); // #cece97
            }
        }

        private void LoadLikedTracks()
        {
            LikedTracks.Clear();
            using (var db = new AudioDbContext())
            {
                var liked = db.AudioTracks.Where(t => t.IsLiked).ToList();
                foreach (var track in liked)
                    LikedTracks.Add(track);
            }
        }

        private void UpdateLikedTracks(AudioTrack track)
        {
            if (track.IsLiked)
            {
                if (!LikedTracks.Any(t => t.Id == track.Id))
                    LikedTracks.Add(track);
            }
            else
            {
                var toRemove = LikedTracks.FirstOrDefault(t => t.Id == track.Id);
                if (toRemove != null)
                    LikedTracks.Remove(toRemove);
            }
        }

        private void UpdateLikedTracksPanelVisibility()
        {
            if (PageFrame.Content is MainPage)
            {
                LikedTracksPanel.Visibility = Visibility.Visible;
                GreetingText.Visibility = Visibility.Visible;
            }
            else
            {
                LikedTracksPanel.Visibility = Visibility.Collapsed;
                GreetingText.Visibility = Visibility.Collapsed;
            }
        }

        private void PageFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateLikedTracksPanelVisibility();
        }

        private void SetGreeting()
        {
            string greeting;
            var hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12)
                greeting = "Доброе утро!";
            else if (hour >= 12 && hour < 18)
                greeting = "Добрый день!";
            else if (hour >= 18 && hour < 23)
                greeting = "Добрый вечер!";
            else
                greeting = "Доброй ночи!";
            GreetingText.Text = greeting;
        }

        private void LikedTracksListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LikedTracksListBox.SelectedItem is AudioTrack track)
            {
                Playlist.Clear();
                foreach (var t in LikedTracks)
                    Playlist.Add(t);
                currentTrackIndex = Playlist.IndexOf(track);
                currentTrack = track;
                AudioService.Instance.Play(currentTrack.FilePath);
                UpdateNowPlayingImage(currentTrack.Photo);
                UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist.Count > 1)
            {
                var tracks = Playlist.ToList();
                // Перемешиваем
                for (int i = tracks.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    var temp = tracks[i];
                    tracks[i] = tracks[j];
                    tracks[j] = temp;
                }
                Playlist.Clear();
                foreach (var t in tracks)
                    Playlist.Add(t);
                currentTrackIndex = 0;
                currentTrack = Playlist[0];
                AudioService.Instance.Play(currentTrack.FilePath);
                UpdateNowPlayingImage(currentTrack.Photo);
                UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            isRepeatOne = !isRepeatOne;
            if (isRepeatOne)
                RepeatIcon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            else
                RepeatIcon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGray);
        }

        private void AudioService_PlaybackFinished(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (isRepeatOne && currentTrack != null)
                {
                    AudioService.Instance.Play(currentTrack.FilePath);
                    UpdateNowPlayingImage(currentTrack.Photo);
                    UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
                }
                else
                {
                    NextButton_Click(null, null);
                }
            });
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow();
            win.Owner = this;
            win.ShowDialog();
            LoadHotkeys();
        }

        private void LoadHotkeys()
        {
            using var db = new AudioDbContext();
            var settings = db.HotkeySettings.ToList();
            foreach (var action in _actions)
            {
                var setting = settings.FirstOrDefault(s => s.Action == action);
                _hotkeys[action] = setting?.KeyGesture ?? string.Empty;
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string gesture = "";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
                gesture += "Ctrl+";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0)
                gesture += "Alt+";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0)
                gesture += "Shift+";
            gesture += e.Key.ToString();

            foreach (var pair in _hotkeys)
            {
                if (!string.IsNullOrEmpty(pair.Value) && gesture.Equals(pair.Value, System.StringComparison.OrdinalIgnoreCase))
                {
                    switch (pair.Key)
                    {
                        case "PlayPause":
                            PlayPauseButton_Click(null, null);
                            break;
                        case "Next":
                            NextButton_Click(null, null);
                            break;
                        case "Prev":
                            PrevButton_Click(null, null);
                            break;
                        case "Like":
                            LikeButton_Click(null, null);
                            break;
                        case "VolumeUp":
                            VolumeSlider.Value = Math.Min(1.0, VolumeSlider.Value + 0.05);
                            break;
                        case "VolumeDown":
                            VolumeSlider.Value = Math.Max(0.0, VolumeSlider.Value - 0.05);
                            break;
                    }
                    e.Handled = true;
                    break;
                }
            }
        }

        private void EnhanceButton_Click(object sender, RoutedEventArgs e)
        {
            SimilarTracks.Clear();
            if (currentTrack == null) return;
            float[] currentMfcc = GetTrackMfcc(currentTrack.Id);
            if (currentMfcc == null)
            {
                System.Windows.MessageBox.Show("Для текущего трека нет аудиопризнаков. Сначала выполните сканирование!", "Нет данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var db = new AudioDbContext())
            {
                var allFeatures = db.TrackFeatures.Where(f => f.TrackId != currentTrack.Id).ToList();
                var allTracks = db.AudioTracks.ToList();
                var results = new List<(AudioTrack, double)>();
                foreach (var feature in allFeatures)
                {
                    var mfcc = ParseMfcc(feature.FeatureVector);
                    if (mfcc == null) continue;
                    double sim = CosineSimilarity(currentMfcc, mfcc);
                    var track = allTracks.FirstOrDefault(t => t.Id == feature.TrackId);
                    if (track != null)
                        results.Add((track, sim));
                }
                foreach (var t in results.OrderByDescending(r => r.Item2).Take(5))
                    SimilarTracks.Add(t.Item1);
            }
            IsSimilarTracksVisible = SimilarTracks.Count > 0;
            IsSimilarTracksExpanded = true;
            System.Windows.MessageBox.Show($"Похожих треков: {SimilarTracks.Count}\nIsSimilarTracksVisible: {IsSimilarTracksVisible}");
        }

        private float[] GetTrackMfcc(int trackId)
        {
            using (var db = new AudioDbContext())
            {
                var feature = db.TrackFeatures.FirstOrDefault(f => f.TrackId == trackId);
                if (feature == null) return null;
                return ParseMfcc(feature.FeatureVector);
            }
        }

        private float[] ParseMfcc(string mfccString)
        {
            try
            {
                return mfccString.Split(',').Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            }
            catch { return null; }
        }

        private double CosineSimilarity(float[] v1, float[] v2)
        {
            double dot = 0, mag1 = 0, mag2 = 0;
            for (int i = 0; i < v1.Length && i < v2.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }
            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        private void SimilarTracksListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListBox lb && lb.SelectedItem is AudioTrack track)
            {
                Playlist.Clear();
                Playlist.Add(track);
                currentTrackIndex = 0;
                currentTrack = track;
                AudioService.Instance.Play(currentTrack.FilePath);
                UpdateNowPlayingImage(currentTrack.Photo);
                UpdateNowPlayingInfo(currentTrack.Title, currentTrack.Performer);
            }
        }

        private void ToggleSimilarTracksButton_Click(object sender, RoutedEventArgs e)
        {
            IsSimilarTracksExpanded = !IsSimilarTracksExpanded;
        }

        private void CloseSimilarTracksButton_Click(object sender, RoutedEventArgs e)
        {
            SimilarTracks.Clear();
            IsSimilarTracksVisible = false;
            IsSimilarTracksExpanded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
