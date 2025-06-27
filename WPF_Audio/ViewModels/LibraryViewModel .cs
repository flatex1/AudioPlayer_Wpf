using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WPF_Audio.Data;
using WPF_Audio.Models;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Transforms;
using NWaves.Utils;

namespace WPF_Audio.ViewModels
{
    public class LibraryViewModel : BaseViewModel
    {
        public ObservableCollection<AudioTrack> Tracks { get; set; } = new ObservableCollection<AudioTrack>();
        public ICollectionView TracksView { get; set; }
        public ObservableCollection<string> SortOptions { get; set; } = new ObservableCollection<string>
        {
            "А-Я", "Исполнитель", "Альбом", "Год выпуска"
        };

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged();
                    ApplySorting();
                }
            }
        }

        public ObservableCollection<string> Genres { get; set; } = new ObservableCollection<string>();

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (_selectedGenre != value)
                {
                    _selectedGenre = value;
                    OnPropertyChanged();
                    TracksView.Refresh();
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    TracksView?.Refresh();
                }
            }
        }

        public string CurrentFolder { get; private set; } = null;

        public LibraryViewModel()
        {
            UpdateLibrary();

            TracksView = CollectionViewSource.GetDefaultView(Tracks);
            TracksView.Filter = FilterTracks;
            SelectedSortOption = SortOptions.First();
            SelectedGenre = "Все";

        }

        public void UpdateLibrary()
        {
            if (!string.IsNullOrWhiteSpace(CurrentFolder) && !Directory.Exists(CurrentFolder))
            {
                ClearLibrary();
            }
            else
            {
                RemoveMissingFiles();
            }
            LoadTracks();
        }

        public void RemoveMissingFiles()
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    var toRemove = db.AudioTracks.ToList()
                        .Where(t => !File.Exists(t.FilePath))
                        .ToList();
                    foreach (var track in toRemove)
                    {
                        db.AudioTracks.Remove(track);
                        var uiTrack = Tracks.FirstOrDefault(x => x.FilePath == track.FilePath);
                        if (uiTrack != null)
                            Tracks.Remove(uiTrack);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при проверке файлов:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadTracks()
        {
            try
            {
                Tracks.Clear();
                using (var db = new AudioDbContext())
                {
                    var tracksFromDb = db.AudioTracks.ToList();
                    foreach (var track in tracksFromDb)
                    {
                        Tracks.Add(track);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке треков:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterTracks(object item)
        {
            if (item is AudioTrack track)
            {
                if (!string.IsNullOrEmpty(SearchText) &&
                    !(track.Title?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                      track.Performer?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                      track.Album?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
                    return false;
                if (string.IsNullOrEmpty(SelectedGenre) || SelectedGenre == "Все")
                    return true;
                return track.Genre.Equals(SelectedGenre, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        private void ApplySorting()
        {
            using (TracksView.DeferRefresh())
            {
                TracksView.SortDescriptions.Clear();
                switch (SelectedSortOption)
                {
                    case "А-Я":
                        TracksView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(AudioTrack.Title), System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case "Исполнитель":
                        TracksView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(AudioTrack.Performer), System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case "Альбом":
                        TracksView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(AudioTrack.Album), System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case "Год выпуска":
                        TracksView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(AudioTrack.Year), System.ComponentModel.ListSortDirection.Ascending));
                        break;
                }
            }
        }

        private void ClearLibrary()
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    var all = db.AudioTracks.ToList();
                    foreach (var t in all)
                    {
                        db.AudioTracks.Remove(t);
                    }
                    db.SaveChanges();
                }
                Tracks.Clear();
                CurrentFolder = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при очистке библиотеки:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnAddFolder()
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folder = folderDialog.SelectedPath;

                    if (CurrentFolder == null || !CurrentFolder.Equals(folder, StringComparison.OrdinalIgnoreCase))
                    {
                        ClearLibrary();
                        CurrentFolder = folder;
                    }

                    var supportedExt = new string[] { ".mp3", ".wav", ".flac", ".aac", ".ogg" };
                    var files = Directory.GetFiles(folder)
                        .Where(f => supportedExt.Contains(Path.GetExtension(f).ToLower()))
                        .ToList();

                    using (var db = new AudioDbContext())
                    {
                        var dbTracks = db.AudioTracks.Where(t => t.FilePath.StartsWith(folder)).ToList();
                        foreach (var t in dbTracks)
                        {
                            if (!files.Contains(t.FilePath))
                                db.AudioTracks.Remove(t);
                        }
                        db.SaveChanges();
                    }

                    foreach (var file in files)
                        AddTrackFromFile(file);

                }
            }
        }

        public void OnAddFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Аудиофайлы|*.mp3;*.wav;*.flac;*.aac;*.ogg",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                AddTrackFromFile(dialog.FileName);
            }
        }

        private void AddTrackFromFile(string file)
        {
            try
            {
                using (var db = new AudioDbContext())
                {
                    if (db.AudioTracks.Any(x => x.FilePath == file))
                        return;
                }

                var tagFile = TagLib.File.Create(file);
                var track = new AudioTrack
                {
                    Title = string.IsNullOrEmpty(tagFile.Tag.Title) ? Path.GetFileNameWithoutExtension(file) : tagFile.Tag.Title,
                    Performer = tagFile.Tag.FirstPerformer ?? "Неизвестный",
                    Album = tagFile.Tag.Album ?? "Неизвестный",
                    Year = (int)tagFile.Tag.Year,
                    Genre = tagFile.Tag.FirstGenre ?? "Неизвестный",
                    Duration = tagFile.Properties.Duration,
                    FilePath = file,
                    Photo = "" 
                };

                if (tagFile.Tag.Pictures.Length > 0)
                {
                    var picData = tagFile.Tag.Pictures[0].Data.Data;
                    track.Photo = Convert.ToBase64String(picData);
                }

                using (var db = new AudioDbContext())
                {
                    db.AudioTracks.Add(track);
                    db.SaveChanges();
                }
                Tracks.Add(track);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при обработке файла {Path.GetFileName(file)}:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ScanTracks()
        {
            int processed = 0;
            foreach (var track in Tracks)
            {
                try
                {
                    // Преобразуем в WAV, если нужно (NAudio)
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

                    // Извлекаем MFCC
                    float[] meanMfcc;
                    using (var stream = new FileStream(wavPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var wave = new WaveFile(stream);
                        var samples = wave[Channels.Left];
                        var mfccExtractor = new MfccExtractor(new NWaves.FeatureExtractors.Options.MfccOptions { SamplingRate = wave.WaveFmt.SamplingRate, FeatureCount = 13 });
                        var mfccs = mfccExtractor.ComputeFrom(samples);
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
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при анализе трека {track.Title}:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            System.Windows.MessageBox.Show($"Сканирование завершено. Обработано треков: {processed}", "Сканирование", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
