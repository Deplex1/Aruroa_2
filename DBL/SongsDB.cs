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

        /// <summary>
        /// Gets all songs that are NOT in a specific playlist.
        /// Uses SQL NOT IN clause to filter in database instead of C#.
        /// Returns a list of Song objects that can be added to the playlist.
        /// </summary>
        public async Task<List<Song>> GetSongsNotInPlaylistAsync(int playlistId)
        {
            // Use SQL NOT IN to filter songs in the database
            // This is MUCH more efficient than loading all songs and filtering in C#
            string sql = @"SELECT * FROM songs 
                          WHERE songid NOT IN (
                              SELECT songid FROM playlist_songs WHERE playlistid = @playlistId
                          )
                          ORDER BY title";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("playlistId", playlistId);

            return await SelectAllAsync(sql, parameters);
        }

        /// <summary>
        /// Gets user statistics using SQL COUNT and SUM.
        /// Returns total songs uploaded and total plays across all their songs.
        /// </summary>
        public async Task<(int totalSongs, int totalPlays)> GetUserStatsAsync(int userId)
        {
            // Get total songs count using SQL COUNT
            string countSql = "SELECT COUNT(*) FROM songs WHERE userid = @userid";
            Dictionary<string, object> countParams = new Dictionary<string, object>();
            countParams.Add("userid", userId);
            List<object[]> countRows = await ExecuteQueryAsync(countSql, countParams);

            int totalSongs = 0;
            if (countRows.Count > 0 && countRows[0][0] != null && countRows[0][0] != DBNull.Value)
            {
                totalSongs = Convert.ToInt32(countRows[0][0]);
            }

            // Get total plays using SQL SUM with COALESCE
            string playsSql = "SELECT COALESCE(SUM(plays), 0) FROM songs WHERE userid = @userid";
            Dictionary<string, object> playsParams = new Dictionary<string, object>();
            playsParams.Add("userid", userId);
            List<object[]> playsRows = await ExecuteQueryAsync(playsSql, playsParams);

            int totalPlays = 0;
            if (playsRows.Count > 0 && playsRows[0][0] != null && playsRows[0][0] != DBNull.Value)
            {
                totalPlays = Convert.ToInt32(playsRows[0][0]);
            }

            return (totalSongs, totalPlays);
        }

        /// <summary>
        /// Filters songs by one or more genres using SQL JOIN and IN clause.
        /// Returns a list of Song objects that match the selected genres.
        /// </summary>
        public async Task<List<Song>> FilterByGenresAsync(List<int> genreIds)
        {
            // Build SQL with JOIN and IN clause to filter in database
            string sql = @"SELECT DISTINCT s.* 
                          FROM songs s 
                          INNER JOIN song_genres sg ON s.songid = sg.songid 
                          WHERE sg.genreid IN (";

            // Add placeholders for each genre ID
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < genreIds.Count; i = i + 1)
            {
                string paramName = "g" + i.ToString();
                sql = sql + "@" + paramName;

                if (i < genreIds.Count - 1)
                {
                    sql = sql + ", ";
                }

                parameters.Add(paramName, genreIds[i]);
            }

            sql = sql + ") ORDER BY s.title";

            return await SelectAllAsync(sql, parameters);
        }
    }
}