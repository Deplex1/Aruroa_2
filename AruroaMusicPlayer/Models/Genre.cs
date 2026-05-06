using Newtonsoft.Json;

namespace AruroaMusicPlayer.Models
{
    public class Genre
    {
        [JsonProperty("genreID")]
        public int GenreId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
