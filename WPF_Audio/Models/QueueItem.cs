using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Audio.Models
{
    public class QueueItem
    {
        public int Id { get; set; }
        public int AudioTrackId { get; set; }
        public AudioTrack AudioTrack { get; set; }
        public int Order { get; set; }
    }
}
