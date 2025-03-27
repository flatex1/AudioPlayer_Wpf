using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Audio.Models;
using WPF_Audio.ViewModels;

namespace WPF_Audio
{
    public partial class PlayLists : Page
    {
        public PlaylistsViewModel ViewModel { get; set; }
        public PlayLists()
        {
            InitializeComponent();
            ViewModel = new PlaylistsViewModel();
            DataContext = ViewModel;
        }

        private void Playlists_TwoClick(object sender, MouseButtonEventArgs e)
        {
            var playlist = ((FrameworkElement)e.OriginalSource).DataContext as Playlist;
            if (playlist != null)
            {
                NavigationService.Navigate(new PlaylistDetailPage(playlist));
            }
        }

        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new NamePlaylist("Введите название плейлиста:", "Новый плейлист");
            if (inputDialog.ShowDialog() == true)
            {
                string playlistName = inputDialog.ResponseText;
                if (!string.IsNullOrWhiteSpace(playlistName))
                {
                    ViewModel.CreatePlaylist(playlistName);
                }
            }
        }
    }
}
