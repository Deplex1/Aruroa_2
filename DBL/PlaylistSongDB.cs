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
            return "playlist_songs";
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

            return await InsertAsync(values);
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
            string sql = "SELECT * FROM playlist_songs WHERE playlistid=@playlistid ORDER BY position";

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
