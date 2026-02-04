namespace Models
{
    public class Playlist
    {
        public int playlistid { get; set; }
        public string name { get; set; }
        public int userid { get; set; }
        public bool ispublic { get; set; } // true = public, false = private

        public Playlist()
        {
        }
    }
}
