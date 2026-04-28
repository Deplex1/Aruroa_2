using Maui.Services;
using Maui.Models;

namespace Maui
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;

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

                if (songs.Count > 0)
                {
                    SongsCollection.ItemsSource = songs;
                    StatusLabel.Text = $"✅ Loaded {songs.Count} songs";
                    StatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    StatusLabel.Text = "No songs found";
                    StatusLabel.TextColor = Colors.Orange;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"❌ Error: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
            }
            finally
            {
                LoadSongsBtn.IsEnabled = true;
            }
        }
    }
}
