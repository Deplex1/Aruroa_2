using System.Text.Json.Serialization;

namespace Maui.Models
{
    public class Song
    {
        [JsonPropertyName("songID")]
        public int SongId { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        
        [JsonPropertyName("audioData")]
        public string? AudioData { get; set; }
        
        [JsonPropertyName("userID")]
        public int UserId { get; set; }
        
        [JsonPropertyName("uploaded")]
        public DateTime Uploaded { get; set; }
        
        [JsonPropertyName("plays")]
        public int Plays { get; set; }

        public string DurationFormatted => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
        public string UploadedFormatted => Uploaded.ToString("dd/MM/yyyy");
    }
}
