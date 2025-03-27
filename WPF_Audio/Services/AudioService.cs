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
            Stop();
            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath)
            {
                Volume = Volume
            };
            outputDevice.Init(audioFile);
            outputDevice.PlaybackStopped += OnPlaybackStopped; 
            outputDevice.Play();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception == null)
            {
                PlaybackFinished?.Invoke(this, EventArgs.Empty);
            }
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
