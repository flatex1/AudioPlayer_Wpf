using NAudio.Wave;
using System;

namespace WPF_Audio.Services
{
    public class AudioService
    {
        private static AudioService _instance;
        public static AudioService Instance => _instance ??= new AudioService();

        private IWavePlayer outputDevice;
        private AudioFileReader audioFile;
        public event EventHandler PlaybackFinished;

        public float Volume { get; private set; } = 0.5f;

        private bool isManualStop = false;

        public void SetVolume(double volume)
        {
            Volume = (float)volume;
            if (audioFile != null)
            {
                audioFile.Volume = Volume;
            }
        }

        public void Play(string filePath)
        {
            isManualStop = false;
            if (outputDevice != null)
            {
                outputDevice.PlaybackStopped -= OnPlaybackStopped;
            }
            Stop();
            try
            {
                outputDevice = new WaveOutEvent();
                audioFile = new AudioFileReader(filePath)
                {
                    Volume = Volume
                };
                outputDevice.Init(audioFile);
                outputDevice.PlaybackStopped += OnPlaybackStopped;
                outputDevice.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка воспроизведения аудио:\n{ex.Message}", "Ошибка NAudio", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Stop();
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!isManualStop && e.Exception == null)
            {
                PlaybackFinished?.Invoke(this, EventArgs.Empty);
            }
            isManualStop = false;
        }

        public void Pause()
        {
            if (outputDevice?.PlaybackState == PlaybackState.Playing)
                outputDevice.Pause();
        }

        public void Resume()
        {
            if (outputDevice?.PlaybackState == PlaybackState.Paused)
                outputDevice.Play();
        }

        public double GetCurrentTime()
        {
            return audioFile?.CurrentTime.TotalSeconds ?? 0;
        }

        public double GetTotalTime()
        {
            return audioFile?.TotalTime.TotalSeconds ?? 0;
        }

        public PlaybackState InstancePlaybackState => outputDevice?.PlaybackState ?? PlaybackState.Stopped;

        public void Seek(TimeSpan position)
        {
            if (audioFile != null)
            {
                audioFile.CurrentTime = position;
            }
        }

        public void Stop()
        {
            isManualStop = true;
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }
        }
    }
}
