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

        public DateTime uploaded { get; set; }
        public int plays { get; set; }

        // Used only in UI to play audio (not saved in DB)
        public string audioSource { get; set; }

        public Song()
        {
        }
    }
}
