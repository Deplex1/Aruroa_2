using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    // Model for the API response
    // Matches the JSON structure you showed me
    public class GenrePlaylistStat
    {
        public int playlistId { get; set; }
        public string playlistName { get; set; }
        public string genre { get; set; }
        public int count { get; set; }

        public GenrePlaylistStat()
        {
            playlistName = "";
            genre = "";
        }
    }

    // Service class to call the Statistics API
    public class StatisticsService
    {
        // The base URL of your API
        private string apiBaseUrl = "http://localhost:5022/api/StatisticsAPI";

        // Get genres per playlist from the API
        // Returns a list of GenrePlaylistStat objects
        public async Task<List<GenrePlaylistStat>> GetGenresPerPlaylistAsync(string username)
        {
            try
            {
                // Create HTTP client
                using HttpClient client = new HttpClient();

                // Create the request
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    apiBaseUrl + "/GenresPerPlaylist"
                );

                // Create the JSON body
                string jsonBody = "{\"username\": \"" + username + "\"}";

                // Set the request content
                request.Content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json"
                );

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request);

                // Check if successful
                if (response.IsSuccessStatusCode == false)
                {
                    Console.WriteLine("API request failed: " + response.StatusCode);
                    return new List<GenrePlaylistStat>();
                }

                // Read the response as string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize JSON to C# objects
                List<GenrePlaylistStat> stats = JsonSerializer.Deserialize<List<GenrePlaylistStat>>(responseBody);

                // Return the stats (or empty list if null)
                if (stats == null)
                {
                    return new List<GenrePlaylistStat>();
                }

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling API: " + ex.Message);
                return new List<GenrePlaylistStat>();
            }
        }

        // Get total genres across all playlists
        // Groups the data by genre and sums the counts
        public async Task<Dictionary<string, int>> GetTotalGenresAsync(string username)
        {
            try
            {
                // Get the per-playlist data
                List<GenrePlaylistStat> stats = await GetGenresPerPlaylistAsync(username);

                // Create a dictionary to hold genre totals
                Dictionary<string, int> genreTotals = new Dictionary<string, int>();

                // Loop through all stats and sum by genre
                for (int i = 0; i < stats.Count; i = i + 1)
                {
                    string genre = stats[i].genre;
                    int count = stats[i].count;

                    // Check if genre already exists in dictionary
                    if (genreTotals.ContainsKey(genre))
                    {
                        // Add to existing count
                        genreTotals[genre] = genreTotals[genre] + count;
                    }
                    else
                    {
                        // Create new entry
                        genreTotals.Add(genre, count);
                    }
                }

                return genreTotals;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting total genres: " + ex.Message);
                return new Dictionary<string, int>();
            }
        }
    }
}