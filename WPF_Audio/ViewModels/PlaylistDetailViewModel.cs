using System.Collections.ObjectModel;
using System.Linq;
using WPF_Audio.Models;

namespace WPF_Audio.ViewModels
{
    public class PlaylistDetailViewModel : BaseViewModel
    {
        public Playlist Playlist { get; set; }
        public ObservableCollection<AudioTrack> Tracks { get; set; }
        public PlaylistDetailViewModel(Playlist playlist)
        {
            if (string.IsNullOrWhiteSpace(playlist.Name))
            {
                playlist.Name = "Новый плейлист";
            }

            if (playlist.PlaylistTracks == null)
            {
                playlist.PlaylistTracks = new System.Collections.Generic.List<PlaylistTrack>();
            }

            Playlist = playlist;

            Tracks = new ObservableCollection<AudioTrack>(
                Playlist.PlaylistTracks.Select(pt => pt.AudioTrack)
            );
        }
    }
}
