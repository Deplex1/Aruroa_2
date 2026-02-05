using Models;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;



namespace DBL

{

    public class PlaylistSongDB : BaseDB<PlaylistSong>

    {

        protected override string GetTableName()

        {

            return "playlistsongs";

        }



        protected override string GetPrimaryKeyName()

        {

            return "playlistid";

        }



        protected override async Task<PlaylistSong> CreateModelAsync(object[] row)

        {

            PlaylistSong ps = new PlaylistSong();



            ps.playlistid = int.Parse(row[0].ToString());

            ps.songid = int.Parse(row[1].ToString());

            ps.position = int.Parse(row[2].ToString());

            ps.dateadded = DateTime.Parse(row[3].ToString());



            return ps;

        }



        public async Task<int> AddSongToPlaylistAsync(PlaylistSong ps)

        {

            var values = new Dictionary<string, object>

    {

        { "playlistid", ps.playlistid },

        { "songid", ps.songid },

        { "position", ps.position },

        { "dateadded", ps.dateadded }

    };



            return await InsertAsync(values);

        }



        // Overload: simpler version

        public async Task<int> AddSongToPlaylistAsync(int playlistId, int songId, int position)

        {

            var values = new Dictionary<string, object>

    {

        { "playlistid", playlistId },

        { "songid", songId },

        { "position", position },

        { "dateadded", DateTime.Now }

    };



            int rows = await InsertAsync(values);
            if (rows == 1)
            {
                return 1;
            }

            // BaseDB may swallow SQL exceptions and return 0.
            // Verify if the row exists anyway.
            bool exists = await SongExistsInPlaylistAsync(playlistId, songId);
            return exists ? 1 : 0;

        }



        public async Task<int> RemoveSongFromPlaylistAsync(int playlistId, int songId)

        {

            Dictionary<string, object> filter = new Dictionary<string, object>();

            filter.Add("playlistid", playlistId);

            filter.Add("songid", songId);



            return await DeleteAsync(filter);

        }



        public async Task<List<PlaylistSong>> GetSongsInPlaylistAsync(int playlistId)

        {

            string sql = "SELECT * FROM playlistsongs WHERE playlistid=@playlistid ORDER BY position";



            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("playlistid", playlistId);



            return await SelectAllAsync(sql, parameters);

        }



        public async Task<bool> SongExistsInPlaylistAsync(int playlistId, int songId)

        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("playlistid", playlistId);

            parameters.Add("songid", songId);



            List<PlaylistSong> list = await SelectAllAsync(parameters);



            return list.Any(ps => ps.songid == songId);

        }



        // Get the count of songs in a playlist

        public async Task<int> GetSongCountAsync(int playlistId)

        {

            string sql = "SELECT COUNT(*) FROM playlistsongs WHERE playlistid=@playlistid";



            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("playlistid", playlistId);



            var result = await SelectAllAsync(sql, parameters);

            

            // For now, return the count of songs we can get

            try

            {

                var songs = await GetSongsInPlaylistAsync(playlistId);

                return songs.Count;

            }

            catch

            {

                return 0;

            }

        }



        // Search for songs in playlists by title - returns Song objects

        public async Task<List<Song>> SearchSongsInPlaylistsAsync(string searchText)

        {

            if (string.IsNullOrWhiteSpace(searchText))

                return new List<Song>();



            // Use SongsDB to search songs, then filter by playlist membership

            SongDB songDB = new SongDB();

            var allSongs = await songDB.SelectAllSongsAsync();

            

            // Get all song IDs that are in playlists

            string playlistSongIdsSql = "SELECT DISTINCT songid FROM playlistsongs";

            var playlistSongIds = await SelectAllAsync(playlistSongIdsSql);

            var songIdsInPlaylists = playlistSongIds.Select(ps => ps.songid).ToHashSet();

            

            // Filter songs by search text and playlist membership

            var filteredSongs = allSongs

                .Where(s => s.title.Contains(searchText, StringComparison.OrdinalIgnoreCase))

                .Where(s => songIdsInPlaylists.Contains(s.songID))

                .ToList();

            

            return filteredSongs;

        }



        // Get playlists containing a specific song
        public async Task<List<PlaylistSong>> GetPlaylistsContainingSongAsync(int songId)
        {
            string sql = @"
                SELECT DISTINCT ps.*
                FROM playlistsongs ps
                WHERE ps.songid = @songid
                ORDER BY ps.playlistid";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("songid", songId);

            return await SelectAllAsync(sql, parameters);
        }



        // Update song position in playlist

        public async Task UpdateSongPositionAsync(int playlistId, int songId, int newPosition)

        {

            Dictionary<string, object> fields = new Dictionary<string, object>();

            fields.Add("position", newPosition);



            Dictionary<string, object> where = new Dictionary<string, object>();

            where.Add("playlistid", playlistId);

            where.Add("songid", songId);



            await UpdateAsync(fields, where);

        }



        private async Task<int> GetNextPositionAsync(int playlistId)

        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("playlistid", playlistId);



            List<PlaylistSong> songs = await SelectAllAsync(parameters);



            if (songs.Count == 0)

            {

                return 1;

            }



            return songs.Max(ps => ps.position) + 1;

        }

    }

}

