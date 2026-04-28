using System.Net.Http.Json;
using Maui.Models;

namespace Maui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        
        // Use 10.0.2.2 for Android emulator, localhost for Windows
        private const string BaseUrl = 
#if ANDROID
            "http://10.0.2.2:5230/api";
#else
            "http://localhost:5230/api";
#endif

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<List<Song>> GetAllSongsAsync()
        {
            try
            {
                var songs = await _httpClient.GetFromJsonAsync<List<Song>>("songs");
                return songs ?? new List<Song>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching songs: {ex.Message}");
                return new List<Song>();
            }
        }

        public async Task<Song> GetSongByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Song>($"songs/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching song: {ex.Message}");
                return null;
            }
        }
    }
}
