using AruroaMusicPlayer.Models;
using Newtonsoft.Json;

namespace AruroaMusicPlayer.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5230";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        // Songs endpoints
        public async Task<List<Song>> GetAllSongsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Songs");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var songs = JsonConvert.DeserializeObject<List<Song>>(json);
                    return songs ?? new List<Song>();
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch songs: {ex.Message}", ex);
            }
        }

        public async Task<List<Song>> GetPopularSongsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Songs/popular");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var songs = JsonConvert.DeserializeObject<List<Song>>(json);
                    return songs ?? new List<Song>();
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch popular songs: {ex.Message}", ex);
            }
        }

        public async Task<List<Song>> GetNewSongsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Songs/new");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var songs = JsonConvert.DeserializeObject<List<Song>>(json);
                    return songs ?? new List<Song>();
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch new songs: {ex.Message}", ex);
            }
        }

        public async Task<List<Song>> SearchSongsAsync(string searchText)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Songs/search?searchText={Uri.EscapeDataString(searchText)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var songs = JsonConvert.DeserializeObject<List<Song>>(json);
                    return songs ?? new List<Song>();
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search songs: {ex.Message}", ex);
            }
        }

        public async Task<Song?> GetSongByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Songs/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Song>(json);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch song: {ex.Message}", ex);
            }
        }

        public async Task IncrementPlayCountAsync(int songId)
        {
            try
            {
                await _httpClient.PostAsync($"api/Songs/{songId}/play", null);
            }
            catch
            {
                // Silently fail - play count is not critical
            }
        }

        // Genres endpoints
        public async Task<List<Genre>> GetAllGenresAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Genres");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var genres = JsonConvert.DeserializeObject<List<Genre>>(json);
                    return genres ?? new List<Genre>();
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch genres: {ex.Message}", ex);
            }
        }

        public async Task<List<Genre>> GetSongGenresAsync(int songId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Genres/song/{songId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var genres = JsonConvert.DeserializeObject<List<Genre>>(json);
                    return genres ?? new List<Genre>();
                }
                
                return new List<Genre>();
            }
            catch
            {
                return new List<Genre>();
            }
        }
    }
}
