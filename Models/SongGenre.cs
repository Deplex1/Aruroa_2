namespace Models
{
    public class SongGenre
    {
        public int songid
        {
            get;
            set;
        }

        public int genreid
        {
            get;
            set;
        }

        public SongGenre()
        {
        }

        public SongGenre(int songid, int genreid)
        {
            this.songid = songid;
            this.genreid = genreid;
        }
    }
}
