namespace Models
{
    /// <summary>
    /// Model class that represents genre information for a song.
    /// Used when loading genres for multiple songs in one query.
    /// </summary>
    public class SongGenreInfo
    {
        public int songid { get; set; }
        public int genreid { get; set; }
        public string name { get; set; }

        public SongGenreInfo()
        {
        }
    }
}
