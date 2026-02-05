using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class PlaylistDB : BaseDB<Playlist>
    {
        protected override string GetTableName()
        {
            return "playlists";
        }

        protected override string GetPrimaryKeyName()
        {
            return "playlistid";
        }

        protected override async Task<Playlist> CreateModelAsync(object[] row)
        {
            Playlist p = new Playlist();

            p.playlistid = int.Parse(row[0].ToString());
            p.name = row[1].ToString();
            p.userid = int.Parse(row[2].ToString());
            p.ispublic = Convert.ToBoolean(row[3]);
            
            // Handle created field (might be null)
            if (row.Length > 4 && row[4] != null && row[4] != DBNull.Value)
            {
                p.created = Convert.ToDateTime(row[4]);
            }
            
            return p;
        }

        public async Task<int> CreatePlaylistAsync(Playlist playlist)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            values.Add("name", playlist.name);
            values.Add("userid", playlist.userid);
            values.Add("ispublic", playlist.ispublic);
            values.Add("created", DateTime.Now);

            return await InsertAsync(values);
        }

        // Alias for CreatePlaylistAsync to match the method name used in Playlists.razor
        public async Task<int> AddPlaylistAsync(Playlist playlist)
        {
            return await CreatePlaylistAsync(playlist);
        }

        public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userid", userId);

            return await SelectAllAsync(parameters);
        }

        public async Task<Playlist> SelectByIdAsync(int playlistId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("playlistid", playlistId);

            List<Playlist> result = await SelectAllAsync(parameters);

            if (result.Count == 1)
            {
                return result[0];
            }

            return null;
        }

        public async Task<int> UpdatePlaylistAsync(int playlistId, Dictionary<string, object> values)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter.Add("playlistid", playlistId);

            return await UpdateAsync(values, filter);
        }

        public async Task<int> DeletePlaylistAsync(int playlistId)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter.Add("playlistid", playlistId);

            return await DeleteAsync(filter);
        }
    }
}
