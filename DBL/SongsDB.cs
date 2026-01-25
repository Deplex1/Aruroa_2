using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongDB : BaseDB<Song>
    {
        protected override string GetTableName()
        {
            return "songs";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        protected async override Task<Song> CreateModelAsync(object[] row)
        {
            Song s = new Song();

            // songid
            s.songID = int.Parse(row[0].ToString());

            // title
            s.title = row[1].ToString();

           
            // duration 
            s.duration = int.Parse(row[2].ToString());
            

            // audioData (BLOB)
            s.audioData = (byte[])row[3];
            

            // userid
            s.userid = int.Parse(row[4].ToString());

            // genreid
            s.genreID = int.Parse(row[5].ToString());

            return s;
        }

        // Get all songs
        public async Task<List<Song>> SelectAllSongsAsync()
        {
            return await SelectAllAsync();
        }

        // Search by title
        public async Task<List<Song>> SearchSongsAsync(string text)
        {
            string sql = "SELECT * FROM songs WHERE title LIKE @t";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("t", text);

            return await SelectAllAsync(sql, p);
        }

        // Get one song by id
        public async Task<Song> SelectSingleSongAsync(int id)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("songid", id);

            List<Song> list = await SelectAllAsync(p);

            if (list.Count == 1)
            {
                return list[0];
            }

            return null;
        }

        // Insert new song (BLOB + duration)
        public async Task<Song> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            values.Add("title", s.title);
            values.Add("duration", s.duration);
            values.Add("audioData", s.audioData);
            values.Add("userid", s.userid);
            values.Add("genreid", s.genreID);

            return await InsertGetObjAsync(values);
        }
    }
}
