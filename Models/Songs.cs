namespace Models
{
    public class Song
    {
        public int songID { get; set; }
        public string title { get; set; }
        public int duration { get; set; } // seconds
        public byte[] audioData { get; set; } // MP3 stored as BLOB
        public int userid { get; set; }
        public int genreID { get; set; }

        public Song()
        {
        }
    }
}
