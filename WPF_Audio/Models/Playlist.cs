using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Audio.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<PlaylistTrack> PlaylistTracks { get; set; }

    }
}
