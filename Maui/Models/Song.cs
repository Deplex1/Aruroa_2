namespace Maui.Models
{
    public class Song
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int UserId { get; set; }
        public DateTime Uploaded { get; set; }
        public int Plays { get; set; }

        public string DurationFormatted => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
        public string UploadedFormatted => Uploaded.ToString("dd/MM/yyyy");
    }
}
