using Newtonsoft.Json;

namespace AruroaMusicPlayer.Models
{
    public class Song
    {
        [JsonProperty("songID")]
        public int SongId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("audioData")]
        public string? AudioData { get; set; }

        [JsonProperty("userID")]
        public int UserId { get; set; }

        [JsonProperty("uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonProperty("plays")]
        public int Plays { get; set; }

        // Display properties
        public string DurationFormatted => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
        public string UploadedFormatted => Uploaded.ToString("dd/MM/yyyy");
    }
}
