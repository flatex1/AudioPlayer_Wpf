using System.Collections.ObjectModel;
using System.Windows;
using WPF_Audio.Models;
using WPF_Audio.Data;
using System.Linq;

namespace WPF_Audio
{
    public partial class SelectTrack : Window
    {
        public AudioTrack SelectedTrack { get; set; }
        public ObservableCollection<AudioTrack> AvailableTracks { get; set; } = new ObservableCollection<AudioTrack>();

        public SelectTrack()
        {
            InitializeComponent();
            LoadLibraryTracks();
            TracksListBox.ItemsSource = AvailableTracks;
        }

        private void LoadLibraryTracks()
        {
            using (var db = new AudioDbContext())
            {
                var tracks = db.AudioTracks.ToList();
                foreach (var track in tracks)
                {
                    AvailableTracks.Add(track);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListBox.SelectedItem is AudioTrack track)
            {
                SelectedTrack = track;
                DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите трек.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
