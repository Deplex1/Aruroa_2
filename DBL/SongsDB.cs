using Models;
using System;
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

        public async Task<List<Song>> SelectAllSongsAsync()
        {
            return await base.SelectAllAsync();
        }

        public async Task<int> AddSongAsync(Song song)
        {
            var values = new Dictionary<string, object>
            {
                { "title", song.title },
                { "audioData", song.audioData },
                { "userid", song.userid },
                { "uploaded", song.uploaded },
                { "duration", song.duration },
                { "plays", song.plays }
            };

            Console.WriteLine($"=== DATABASE INSERT ===");
            Console.WriteLine($"Inserting song: {song.title}");
            Console.WriteLine($"Fields: title, audioData, userid, uploaded, duration, plays");

            return await InsertAsync(values);
        }

        public async Task<Song> SelectByIdAsync(int id)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { GetPrimaryKeyName(), id }
            };

            List<Song> list = await SelectAllAsync(parameters);

            if (list.Count == 1)
                return list[0];

            return null;
        }

        protected async override Task<Song> CreateModelAsync(object[] row)
        {
            Song s = new Song();

            s.songID = int.Parse(row[0].ToString());
            s.title = row[1].ToString();
            s.duration = int.Parse(row[2].ToString());
            s.audioData = (byte[])row[3];
            s.userid = int.Parse(row[4].ToString());

            if (row[5] != null)
            {
                s.uploaded = DateTime.Parse(row[5].ToString());
            }
            else
            {
                s.uploaded = DateTime.Now;
            }

            if (row[6] != null)
            {
                s.plays = int.Parse(row[6].ToString());
            }
            else
            {
                s.plays = 0;
            }

            return s;
        }

        public async Task<List<Song>> SearchSongsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return await SelectAllSongsAsync();
            }

            string sql = "SELECT * FROM songs WHERE title LIKE @t ORDER BY title";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("t", $"%{text}%");

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

        public async Task<Song> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            values.Add("title", s.title);
            values.Add("duration", s.duration);
            values.Add("audioData", s.audioData);
            values.Add("userid", s.userid);

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

        // This method increments the play count by 1 whenever a song is played
        // We use a direct SQL UPDATE statement to increment the counter
        public async Task AddPlayAsync(int songId)
        {
            // Create the UPDATE query
            // plays + 1 means: take the current value and add 1 to it
            string sql = "UPDATE songs SET plays = plays + 1 WHERE songid = @songid";

            // Create parameters dictionary to safely pass the songid
            // This prevents SQL injection attacks
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("songid", songId);

            // Add parameters to the command
            // We need to manually add them because ExecNonQueryAsync doesn't do it
            cmd.Parameters.Clear();
            var param = cmd.CreateParameter();
            param.ParameterName = "@songid";
            param.Value = songId;
            cmd.Parameters.Add(param);

            // Execute the UPDATE query
            // ExecNonQueryAsync returns the number of rows affected (should be 1)
            await ExecNonQueryAsync(sql);
        }

        public async Task<int> DeleteSongAsync(int songId)
        {
            Dictionary<string, object> where = new Dictionary<string, object>
            {
                { "songid", songId }
            };

            return await DeleteAsync(where);
        }
    }
}