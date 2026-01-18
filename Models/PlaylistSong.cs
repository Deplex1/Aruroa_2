using System;

namespace Models
{
    public class PlaylistSong
    {
        public int playlistid { get; set; }
        public int songid { get; set; }
        public int position { get; set; }
        public DateTime dateadded { get; set; }

        public PlaylistSong()
        {
        }
    }
}
