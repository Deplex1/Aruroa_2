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
            s.songID = int.Parse(row[0].ToString());
            s.title = row[1].ToString();
            s.duration = row[2] == null ? 0 : int.Parse(row[2].ToString());
            s.audioData = row[3] as byte[];
            s.userid = int.Parse(row[4].ToString());
            s.genreID = int.Parse(row[5].ToString());
            return s;
        }

        public async Task<List<Song>> SelectAllSongsAsync()
        {
            return await SelectAllAsync();
        }

        public async Task<List<Song>> SearchSongsAsync(string text)
        {
            string sql = "SELECT * FROM songs WHERE title LIKE @t";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("t", "%" + text + "%");

            return await SelectAllAsync(sql, p);
        }

        public async Task<Song> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("title", s.title);
            values.Add("duration", s.duration);
            values.Add("audiourl", s.audioData);
            values.Add("userid", s.userid);
            values.Add("genreid", s.genreID);

            return await InsertGetObjAsync(values);
        }
    }
}
