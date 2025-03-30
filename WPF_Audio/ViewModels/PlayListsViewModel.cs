using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WPF_Audio.Data;
using WPF_Audio.Models;

namespace WPF_Audio.ViewModels
{
    public class PlaylistsViewModel : BaseViewModel
    {
        public ObservableCollection<Playlist> Playlists { get; set; } = new ObservableCollection<Playlist>();

        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
            }
        }

        public PlaylistsViewModel()
        {
            LoadPlaylists();
        }

        public void LoadPlaylists()
        {
            Playlists.Clear();
            try
            {
                using (var db = new AudioDbContext())
                {
                    var list = db.Playlists.Include(p => p.PlaylistTracks)
                                           .ThenInclude(pt => pt.AudioTrack)
                                           .ToList();
                    foreach (var pl in list)
                    {
                        Playlists.Add(pl);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки плейлистов: {ex.Message}");
            }
        }

        public void CreatePlaylist(string name)
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    var newPlaylist = new Playlist { Name = name };
                    db.Playlists.Add(newPlaylist);
                    db.SaveChanges();
                    Playlists.Add(newPlaylist);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка создания плейлиста: {ex.Message}");
            }
        }

        public void AddTrackToPlaylist(int playlistId, AudioTrack track)
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    if (db.PlaylistTracks.Any(pt => pt.PlaylistId == playlistId && pt.AudioTrackId == track.Id))
                        return;
                    var ptRecord = new PlaylistTrack
                    {
                        PlaylistId = playlistId,
                        AudioTrackId = track.Id,
                        Order = 0
                    };
                    db.PlaylistTracks.Add(ptRecord);
                    db.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка добавления трека в плейлист: {ex.Message}");
            }
        }
    }
}
