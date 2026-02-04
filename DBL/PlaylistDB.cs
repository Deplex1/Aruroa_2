using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBL
{
    public class PlaylistDB : BaseDB<Playlist>
    {
        protected override string GetTableName() => "playlists";
        protected override string GetPrimaryKeyName() => "playlistid";

        protected override async Task<Playlist> CreateModelAsync(object[] row)
        {
            Playlist p = new Playlist();
            p.playlistid = int.Parse(row[0].ToString());
            p.name = row[1].ToString();
            p.userid = int.Parse(row[2].ToString());
            p.ispublic = bool.Parse(row[3].ToString());
            return p;
        }

        // Public wrapper to insert new playlist
        public async Task<int> CreatePlaylistAsync(Playlist playlist)
        {
            var values = new Dictionary<string, object>
        {
            { "name", playlist.name },
            { "userid", playlist.userid },
            { "isPublic", playlist.ispublic }
        };
            return await InsertAsync(values);
        }

        public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId)
        {
            var parameters = new Dictionary<string, object> { { "userid", userId } };
            return await SelectAllAsync(parameters);
        }

        public async Task<int> UpdatePlaylistAsync(int playlistId, Dictionary<string, object> values)
        {
            var filter = new Dictionary<string, object> { { "playlistid", playlistId } };
            return await UpdateAsync(values, filter);
        }

        public async Task<int> DeletePlaylistAsync(int playlistId)
        {
            var filter = new Dictionary<string, object> { { "playlistid", playlistId } };
            return await DeleteAsync(filter);
        }
    }


}
