using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPF_Audio.Data;
using WPF_Audio.Models;
using WPF_Audio.ViewModels;
using WPF_Audio.Services;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace WPF_Audio
{
    public partial class PlaylistDetailPage : Page
    {
        public PlaylistDetailViewModel ViewModel { get; set; }

        public PlaylistDetailPage(Playlist playlist)
        {
            InitializeComponent();
            ViewModel = new PlaylistDetailViewModel(playlist);
            DataContext = ViewModel;
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            var selectWindow = new SelectTrack();
            selectWindow.Owner = System.Windows.Application.Current.MainWindow;
            if (selectWindow.ShowDialog() == true)
            {
                var selectedTrack = selectWindow.SelectedTrack;
                if (selectedTrack != null)
                {
                    AddTrackToPlaylist(selectedTrack);
                }
            }
        }

        private void AddTrackToPlaylist(AudioTrack track)
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    if (db.PlaylistTracks.Any(pt => pt.PlaylistId == ViewModel.Playlist.Id && pt.AudioTrackId == track.Id))
                    {
                        System.Windows.MessageBox.Show("Этот трек уже есть в плейлисте.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var newPt = new PlaylistTrack
                    {
                        PlaylistId = ViewModel.Playlist.Id,
                        AudioTrackId = track.Id,
                        Order = db.PlaylistTracks.Where(pt => pt.PlaylistId == ViewModel.Playlist.Id).Count() + 1
                    };
                    db.PlaylistTracks.Add(newPt);
                    db.SaveChanges();
                }
                ViewModel.Tracks.Add(track);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка добавления трека в плейлист: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Вы уверены, что хотите удалить этот плейлист?",
                                "Удаление плейлиста", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AudioDbContext())
                    {
                        var playlistToDelete = db.Playlists.FirstOrDefault(p => p.Id == ViewModel.Playlist.Id);
                        if (playlistToDelete != null)
                        {
                            var relatedTracks = db.PlaylistTracks.Where(pt => pt.PlaylistId == playlistToDelete.Id);
                            db.PlaylistTracks.RemoveRange(relatedTracks);
                            db.Playlists.Remove(playlistToDelete);
                            db.SaveChanges();
                        }
                    }
                    NavigationService.Navigate(new PlayLists());
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении плейлиста: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void TracksListBoxSelect(object sender, SelectionChangedEventArgs e)
        {
            if (TracksListBox.SelectedItem is AudioTrack selectedTrack)
            {
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var view = CollectionViewSource.GetDefaultView(ViewModel.Tracks);
                    var filteredList = new ObservableCollection<AudioTrack>(view.Cast<AudioTrack>());

                    mainWindow.Playlist = filteredList;
                    int idx = mainWindow.Playlist.IndexOf(selectedTrack);
                    mainWindow.currentTrackIndex = idx;
                    mainWindow.currentTrack = selectedTrack;

                    mainWindow.UpdateNowPlayingImage(selectedTrack.Photo);
                    mainWindow.UpdateNowPlayingInfo(selectedTrack.Title, selectedTrack.Performer);
                    AudioService.Instance.Play(selectedTrack.FilePath);
                    mainWindow.progressTimer.Start();
                }

                TracksListBox.SelectedItem = null;
            }
        }

        private void DeleteTrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is AudioTrack track)
            {
                if (System.Windows.MessageBox.Show($"Удалить трек \"{track.Title}\" из плейлиста?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new AudioDbContext())
                        {
                            var ptRecord = db.PlaylistTracks.FirstOrDefault(pt => pt.PlaylistId == ViewModel.Playlist.Id && pt.AudioTrackId == track.Id);
                            if (ptRecord != null)
                            {
                                db.PlaylistTracks.Remove(ptRecord);
                                db.SaveChanges();
                            }
                        }
                        ViewModel.Tracks.Remove(track);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при удалении трека из плейлиста: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
