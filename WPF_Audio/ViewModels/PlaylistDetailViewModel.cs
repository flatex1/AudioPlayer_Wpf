using System.Collections.ObjectModel;
using System.Linq;
using WPF_Audio.Models;
using System.IO;
using System.Windows;
using WPF_Audio.Data;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;

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

        public void ScanTracks()
        {
            int processed = 0;
            foreach (var track in Tracks)
            {
                try
                {
                    string wavPath = track.FilePath;
                    if (!track.FilePath.ToLower().EndsWith(".wav"))
                    {
                        wavPath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");
                        using (var reader = new NAudio.Wave.AudioFileReader(track.FilePath))
                        using (var writer = new NAudio.Wave.WaveFileWriter(wavPath, reader.WaveFormat))
                        {
                            reader.CopyTo(writer);
                        }
                    }

                    float[] meanMfcc;
                    using (var stream = new FileStream(wavPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var wave = new WaveFile(stream);
                        var samples = wave[Channels.Left];
                        var extractor = new MfccExtractor(new NWaves.FeatureExtractors.Options.MfccOptions { SamplingRate = wave.WaveFmt.SamplingRate, FeatureCount = 13 });
                        var mfccs = extractor.ComputeFrom(samples);
                        meanMfcc = new float[mfccs[0].Length];
                        foreach (var frame in mfccs)
                            for (int i = 0; i < frame.Length; i++)
                                meanMfcc[i] += frame[i];
                        for (int i = 0; i < meanMfcc.Length; i++)
                            meanMfcc[i] /= mfccs.Count;
                    }
                    var mfccString = string.Join(",", meanMfcc.Select(f => f.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));

                    using (var db = new AudioDbContext())
                    {
                        var feature = db.TrackFeatures.FirstOrDefault(f => f.TrackId == track.Id);
                        if (feature == null)
                        {
                            db.TrackFeatures.Add(new TrackFeature { TrackId = track.Id, FeatureVector = mfccString });
                        }
                        else
                        {
                            feature.FeatureVector = mfccString;
                        }
                        db.SaveChanges();
                    }

                    if (wavPath != track.FilePath && File.Exists(wavPath))
                        File.Delete(wavPath);
                    processed++;
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при обработке трека {track.Title}:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            System.Windows.MessageBox.Show($"Сканирование завершено. Обработано: {processed} треков.", "Сканирование", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
