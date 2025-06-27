using System.ComponentModel.DataAnnotations;

namespace WPF_Audio.Models
{
    public class HotkeySetting
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; } // PlayPause, Next, Prev, Like, VolumeUp, VolumeDown
        public string KeyGesture { get; set; } // Например, "Ctrl+P"
    }
} 