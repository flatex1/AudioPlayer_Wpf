using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPF_Audio.Models;
using WPF_Audio.ViewModels;
using WPF_Audio.Services;

namespace WPF_Audio
{
    /// <summary>
    /// Логика взаимодействия для Library.xaml
    /// </summary>
    public partial class Library : Page
    {
        public LibraryViewModel ViewModel { get; set; }

        public Library()
        {
            InitializeComponent();
            ViewModel = new LibraryViewModel();
            DataContext = ViewModel;
        }

        private void TracksListBoxSelect(object sender, SelectionChangedEventArgs e)
        {
            if (TracksListBox.SelectedItem is AudioTrack selectedTrack)
            {
                var view = CollectionViewSource.GetDefaultView(ViewModel.Tracks);
                var filteredList = new System.Collections.ObjectModel.ObservableCollection<AudioTrack>(
                    view.Cast<AudioTrack>()
                );

                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
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

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OnAddFolder();
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OnAddFile();
        }

        private void ScanTracksButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ScanTracks();
        }
    }
}
