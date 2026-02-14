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
    }
}