using Maui.Services;
using Maui.Models;
using CommunityToolkit.Maui.Views;

namespace Maui
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private Song? _currentSong;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnLoadSongsClicked(object sender, EventArgs e)
        {
            try
            {
                // Disable button and show loading
                LoadSongsBtn.IsEnabled = false;
                StatusLabel.Text = "Loading songs...";
                StatusLabel.TextColor = Colors.Blue;

                // Fetch songs from API
                var songs = await _apiService.GetAllSongsAsync();

                if (songs != null && songs.Count > 0)
                {
                    SongsCollection.ItemsSource = songs;
                    StatusLabel.Text = $"✅ Loaded {songs.Count} songs";
                    StatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    StatusLabel.Text = "No songs found or API error";
                    StatusLabel.TextColor = Colors.Orange;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"❌ Error: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                
                // Show detailed error in console
                System.Diagnostics.Debug.WriteLine($"Full error: {ex}");
            }
            finally
            {
                LoadSongsBtn.IsEnabled = true;
            }
        }

        private void OnSongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                _currentSong = selectedSong;
                PlaySong(selectedSong);
            }
        }

        private void PlaySong(Song song)
        {
            try
            {
                if (string.IsNullOrEmpty(song.AudioData))
                {
                    StatusLabel.Text = "❌ No audio data available";
                    StatusLabel.TextColor = Colors.Red;
                    return;
                }

                // Convert base64 audio data to byte array
                var audioBytes = Convert.FromBase64String(song.AudioData);
                
                // Create a temporary file to play the audio
                var tempFile = Path.Combine(FileSystem.CacheDirectory, $"temp_audio_{song.SongId}.mp3");
                File.WriteAllBytes(tempFile, audioBytes);

                // Set the media source
                AudioPlayer.Source = MediaSource.FromFile(tempFile);
                AudioPlayer.Play();

                // Show player and update label
                PlayerFrame.IsVisible = true;
                NowPlayingLabel.Text = $"Now Playing: {song.Title}";
                StatusLabel.Text = $"▶️ Playing: {song.Title}";
                StatusLabel.TextColor = Colors.Green;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"❌ Playback error: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                System.Diagnostics.Debug.WriteLine($"Playback error: {ex}");
            }
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            AudioPlayer.Play();
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            AudioPlayer.Pause();
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            AudioPlayer.Stop();
            PlayerFrame.IsVisible = false;
        }
    }
}
