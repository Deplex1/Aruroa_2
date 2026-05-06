using NAudio.Wave;
using AruroaMusicPlayer.Models;

namespace AruroaMusicPlayer.Services
{
    public class AudioPlayerService : IDisposable
    {
        private WaveOutEvent? _waveOut;
        private Mp3FileReader? _mp3Reader;
        private string? _currentTempFile;

        public event EventHandler? PlaybackStopped;
        public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => _waveOut?.PlaybackState == PlaybackState.Paused;

        public void PlaySong(Song song)
        {
            try
            {
                // Stop current playback
                Stop();

                if (string.IsNullOrEmpty(song.AudioData))
                {
                    throw new Exception("No audio data available");
                }

                // Convert base64 to bytes
                var audioBytes = Convert.FromBase64String(song.AudioData);

                // Create temp file
                _currentTempFile = Path.Combine(Path.GetTempPath(), $"song_{song.SongId}.mp3");
                File.WriteAllBytes(_currentTempFile, audioBytes);

                // Initialize audio
                _mp3Reader = new Mp3FileReader(_currentTempFile);
                _waveOut = new WaveOutEvent();
                _waveOut.PlaybackStopped += (s, e) => PlaybackStopped?.Invoke(this, EventArgs.Empty);
                _waveOut.Init(_mp3Reader);
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to play song: {ex.Message}", ex);
            }
        }

        public void Pause()
        {
            _waveOut?.Pause();
        }

        public void Resume()
        {
            _waveOut?.Play();
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _mp3Reader?.Dispose();
            _waveOut?.Dispose();

            if (_currentTempFile != null && File.Exists(_currentTempFile))
            {
                try
                {
                    File.Delete(_currentTempFile);
                }
                catch { }
            }

            _mp3Reader = null;
            _waveOut = null;
            _currentTempFile = null;
        }

        public TimeSpan GetCurrentTime()
        {
            return _mp3Reader?.CurrentTime ?? TimeSpan.Zero;
        }

        public TimeSpan GetTotalTime()
        {
            return _mp3Reader?.TotalTime ?? TimeSpan.Zero;
        }

        public void SetPosition(TimeSpan position)
        {
            if (_mp3Reader != null)
            {
                _mp3Reader.CurrentTime = position;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
