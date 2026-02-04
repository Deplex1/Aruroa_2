using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBL
{
    public class PlaylistSongDB : BaseDB<PlaylistSong>
    {
        protected override string GetTableName() => "playlist_songs";
        protected override string GetPrimaryKeyName() => "playlistid"; // composite key might be playlistid+songid

        protected override async Task<PlaylistSong> CreateModelAsync(object[] row)
        {
            PlaylistSong ps = new PlaylistSong();
            ps.playlistid = int.Parse(row[0].ToString());
            ps.songid = int.Parse(row[1].ToString());
            ps.position = int.Parse(row[2].ToString());
            ps.dateadded = DateTime.Parse(row[3].ToString());
            return ps;
        }

        // Public wrapper to add a song to a playlist
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

        // Remove a song from a playlist
        public async Task<int> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            var filter = new Dictionary<string, object>
        {
            { "playlistid", playlistId },
            { "songid", songId }
        };
            return await DeleteAsync(filter);
        }

        // Get songs in a playlist, ordered by position
        public async Task<List<PlaylistSong>> GetSongsInPlaylistAsync(int playlistId)
        {
            var parameters = new Dictionary<string, object> { { "playlistid", playlistId } };
            var list = await SelectAllAsync(parameters);
            return list.OrderBy(ps => ps.position).ToList();
        }
    }

}
