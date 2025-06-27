using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Audio.Models
{
    public class AudioTrack
    {
        public int Id { get; set; }
        public string Photo { get; set; } 
        public string Title { get; set; }
        public string Performer { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }
        public string FilePath { get; set; }
        public bool IsLiked { get; set; }
    }
}
