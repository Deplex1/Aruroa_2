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
                System.Diagnostics.Debug.WriteLine($"Calling API: {_httpClient.BaseAddress}Songs");
                
                var response = await _httpClient.GetAsync("Songs");
                var content = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    var songs = await response.Content.ReadFromJsonAsync<List<Song>>();
                    return songs ?? new List<Song>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode}");
                    return new List<Song>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Full Exception: {ex}");
                return new List<Song>();
            }
        }

        public async Task<Song> GetSongByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Song>($"Songs/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching song: {ex.Message}");
                return null;
            }
        }
    }
}
