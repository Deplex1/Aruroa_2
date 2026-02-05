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
            if (rows == 1) return 1;

            bool exists = await SongExistsInPlaylistAsync(playlistId, songId);
            return exists ? 1 : 0;
        }

        public async Task<int> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            var filter = new Dictionary<string, object>
            {
                { "playlistid", playlistId },
                { "songid", songId }
            };

            return await DeleteAsync(filter);
        }

        public async Task<List<PlaylistSong>> GetSongsInPlaylistAsync(int playlistId)
        {
            string sql = "SELECT * FROM playlistsongs WHERE playlistid=@playlistid ORDER BY position";
            var parameters = new Dictionary<string, object> { { "playlistid", playlistId } };
            return await SelectAllAsync(sql, parameters);
        }

        public async Task<bool> SongExistsInPlaylistAsync(int playlistId, int songId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "playlistid", playlistId },
                { "songid", songId }
            };
            var list = await SelectAllAsync(parameters);
            return list.Any(ps => ps.songid == songId);
        }

        public async Task UpdateSongPositionAsync(int playlistId, int songId, int newPosition)
        {
            var fields = new Dictionary<string, object> { { "position", newPosition } };
            var where = new Dictionary<string, object>
            {
                { "playlistid", playlistId },
                { "songid", songId }
            };
            await UpdateAsync(fields, where);
        }

        // --- New Methods ---

        // Get the maximum position in a playlist
        public async Task<int> GetMaxPositionInPlaylistAsync(int playlistId)
        {
            var songs = await GetSongsInPlaylistAsync(playlistId);
            return songs.Count == 0 ? 0 : songs.Max(s => s.position);
        }

        // Swap the positions of two songs in the playlist
        public async Task SwapSongPositionsAsync(int playlistId, int songId1, int songId2)
        {
            var songs = await GetSongsInPlaylistAsync(playlistId);
            var s1 = songs.FirstOrDefault(s => s.songid == songId1);
            var s2 = songs.FirstOrDefault(s => s.songid == songId2);

            if (s1 == null || s2 == null) return;

            int tempPos = s1.position;
            await UpdateSongPositionAsync(playlistId, songId1, s2.position);
            await UpdateSongPositionAsync(playlistId, songId2, tempPos);
        }
    }
}
