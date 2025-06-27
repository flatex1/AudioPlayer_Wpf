using System.ComponentModel.DataAnnotations;

namespace WPF_Audio.Models
{
    public class TrackFeature
    {
        [Key]
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string FeatureVector { get; set; } // MFCC или fingerprint в виде строки
    }
} 