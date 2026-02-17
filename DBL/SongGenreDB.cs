using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongGenreDB : BaseDB<SongGenre>
    {
        protected override string GetTableName()
        {
            return "song_genres";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        protected override async Task<SongGenre> CreateModelAsync(object[] row)
        {
            SongGenre sg = new SongGenre();
            sg.songid = int.Parse(row[0].ToString());
            sg.genreid = int.Parse(row[1].ToString());
            return sg;
        }

        public async Task<int> AddSongGenreAsync(int songId, int genreId)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("songid", songId);
            values.Add("genreid", genreId);

            return await InsertAsync(values);
        }

        public async Task<List<SongGenre>> GetGenresForSongAsync(int songId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("songid", songId);

            return await SelectAllAsync(parameters);
        }

        public async Task<List<int>> GetGenreIdsForSongAsync(int songId)
        {
            List<SongGenre> songGenres = await GetGenresForSongAsync(songId);
            List<int> genreIds = new List<int>();

            for (int i = 0; i < songGenres.Count; i++)
            {
                genreIds.Add(songGenres[i].genreid);
            }

            return genreIds;
        }

        public async Task<int> DeleteGenresForSongAsync(int songId)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter.Add("songid", songId);

            return await DeleteAsync(filter);
        }

        public async Task UpdateSongGenresAsync(int songId, List<int> genreIds)
        {
            await DeleteGenresForSongAsync(songId);

            for (int i = 0; i < genreIds.Count; i++)
            {
                await AddSongGenreAsync(songId, genreIds[i]);
            }
        }

        /// <summary>
        /// Gets genres for multiple songs in ONE SQL query using JOIN and IN clause.
        /// This is MUCH more efficient than calling GetGenresForSongAsync for each song.
        /// Returns a list of SongGenreInfo objects containing songid, genreid, and genre name.
        /// </summary>
        public async Task<List<SongGenreInfo>> GetGenresForMultipleSongsAsync(List<int> songIds)
        {
            List<SongGenreInfo> result = new List<SongGenreInfo>();

            if (songIds.Count == 0)
            {
                return result;
            }

            // Build SQL with JOIN and IN clause to get all genres in one query
            string sql = @"SELECT sg.songid, g.genreid, g.name 
                          FROM genres g 
                          INNER JOIN song_genres sg ON g.genreid = sg.genreid 
                          WHERE sg.songid IN (";

            // Add placeholders for each song ID
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < songIds.Count; i = i + 1)
            {
                string paramName = "s" + i.ToString();
                sql = sql + "@" + paramName;

                if (i < songIds.Count - 1)
                {
                    sql = sql + ", ";
                }

                parameters.Add(paramName, songIds[i]);
            }

            sql = sql + ") ORDER BY g.name";

            // Execute query
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Process results
            for (int i = 0; i < rows.Count; i = i + 1)
            {
                object[] row = rows[i];

                SongGenreInfo info = new SongGenreInfo();
                info.songid = int.Parse(row[0].ToString());
                info.genreid = int.Parse(row[1].ToString());
                info.name = row[2].ToString();

                result.Add(info);
            }

            return result;
        }
    }
}