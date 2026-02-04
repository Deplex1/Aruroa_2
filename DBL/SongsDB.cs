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

        //get all songs
        public async Task<List<Song>> SelectAllSongsAsync()
        {
            
            return await base.SelectAllAsync(); 
        }
        public async Task<int> AddSongAsync(Song song)
        {
        // Create dictionary for DB insert
        var values = new Dictionary<string, object>
        {
            { "title", song.title },
            { "audioData", song.audioData },
            { "userid", song.userid },
            { "uploaded", song.uploaded }
        };

        // Call the protected InsertAsync from BaseDB
        return await InsertAsync(values);
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

            DateTime time = new DateTime();

            if (row[6] != null)
            {
                s.uploaded = DateTime.Parse(row[6].ToString());
            }
            else
            {
                s.uploaded = DateTime.Now;
            }

            if (row[7] != null)
            {
                s.plays = int.Parse(row[7].ToString());
            }
            else
            {
                s.plays = 0;
            }
                return s;
        }

        
        

        // Search by title
        public async Task<List<Song>> SearchSongsAsync(string text)
        {
            string sql = "SELECT * FROM songs WHERE title LIKE @t";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("t", text);

            return await SelectAllAsync(sql, p);
        }

        public async Task<List<Song>> SelectSongsByUserIDAsync(int id)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("userid", id);

            List<Song> list = await SelectAllAsync(p);

            if (list.Count != 0)
            {
                return list;
            }

            return null;
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

        public async Task<List<Song>> GetPopularSongsAsync()
        {
            string sql = "SELECT * FROM songs ORDER BY plays DESC LIMIT 10";
            return await SelectAllAsync(sql);
        }

        public async Task<List<Song>> GetNewSongsAsync()
        {
            string sql = "SELECT * FROM songs ORDER BY uploaded DESC LIMIT 10";
            return await SelectAllAsync(sql);
        }

        public async Task AddPlayAsync(int songId)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields.Add("plays", "plays + 1"); // special handling later

            Dictionary<string, object> where = new Dictionary<string, object>();
            where.Add("songid", songId);

            string sql = "UPDATE songs SET plays = plays + 1 WHERE songid = @songid";
            await SelectAllAsync(sql, where);
        }

    }
}
