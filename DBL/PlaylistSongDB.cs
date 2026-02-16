using Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
            if (rows == 1)
            {
                return 1;
            }

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
            var parameters = new Dictionary<string, object>
            {
                { "playlistid", playlistId }
            };

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
            
            // Check if song exists using explicit for loop
            for (int i = 0; i < list.Count; i = i + 1)
            {
                if (list[i].songid == songId)
                {
                    return true;
                }
            }
            
            return false;
        }

        public async Task UpdateSongPositionAsync(int playlistId, int songId, int newPosition)
        {
            var fields = new Dictionary<string, object>
            {
                { "position", newPosition }
            };

            var where = new Dictionary<string, object>
            {
                { "playlistid", playlistId },
                { "songid", songId }
            };

            await UpdateAsync(fields, where);
        }

        public async Task<int> GetMaxPositionInPlaylistAsync(int playlistId)
        {
            var songs = await GetSongsInPlaylistAsync(playlistId);
            if (songs.Count == 0)
            {
                return 0;
            }

            // Find max position using explicit for loop
            int maxPosition = 0;
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                if (songs[i].position > maxPosition)
                {
                    maxPosition = songs[i].position;
                }
            }

            return maxPosition;
        }

        public async Task SwapSongPositionsAsync(int playlistId, int songId1, int songId2)
        {
            var songs = await GetSongsInPlaylistAsync(playlistId);

            // Find song 1 using explicit for loop
            PlaylistSong s1 = null;
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                if (songs[i].songid == songId1)
                {
                    s1 = songs[i];
                    break;
                }
            }

            // Find song 2 using explicit for loop
            PlaylistSong s2 = null;
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                if (songs[i].songid == songId2)
                {
                    s2 = songs[i];
                    break;
                }
            }

            if (s1 == null || s2 == null)
            {
                return;
            }

            int temp = s1.position;

            await UpdateSongPositionAsync(playlistId, songId1, s2.position);
            await UpdateSongPositionAsync(playlistId, songId2, temp);
        }

        public async Task<int> GetSongCountAsync(int playlistId)
        {
            var songs = await GetSongsInPlaylistAsync(playlistId);
            return songs.Count;
        }

        /// <summary>
        /// Gets song counts for ALL playlists in one SQL query using GROUP BY.
        /// This is MUCH more efficient than calling GetSongCountAsync for each playlist.
        /// Returns a dictionary mapping playlistId to song count.
        /// </summary>
        public async Task<Dictionary<int, int>> GetAllPlaylistSongCountsAsync()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            // Use SQL GROUP BY to get all counts in one query
            string sql = "SELECT playlistid, COUNT(*) as song_count " +
                        "FROM playlistsongs " +
                        "GROUP BY playlistid";

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Execute query and get raw rows
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Process results
            for (int i = 0; i < rows.Count; i = i + 1)
            {
                object[] row = rows[i];
                
                int playlistId = Convert.ToInt32(row[0]);
                int count = Convert.ToInt32(row[1]);

                counts[playlistId] = count;
            }

            return counts;
        }

        /// <summary>
        /// Gets songs in a playlist with full song details using SQL JOIN.
        /// This is MUCH more efficient than loading playlist-song relationships
        /// and then loading each song individually (N+1 query problem).
        /// Returns a list of Song objects in the correct order.
        /// </summary>
        public async Task<List<Song>> GetPlaylistSongsWithDetailsAsync(int playlistId)
        {
            // Use SQL JOIN to get all song data in one query
            // ORDER BY position to maintain playlist order
            string sql = "SELECT s.songID, s.title, s.audioData, s.userid, s.uploaded, s.duration, s.plays " +
                        "FROM songs s " +
                        "INNER JOIN playlistsongs ps ON s.songID = ps.songid " +
                        "WHERE ps.playlistid = @playlistid " +
                        "ORDER BY ps.position";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("playlistid", playlistId);

            // Execute query and get raw rows
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Convert rows to Song objects
            List<Song> songs = new List<Song>();
            for (int i = 0; i < rows.Count; i = i + 1)
            {
                object[] row = rows[i];

                Song song = new Song();
                song.songID = Convert.ToInt32(row[0]);
                song.title = row[1].ToString();
                
                // Handle audioData (can be null or byte array)
                if (row[2] != null && row[2] != DBNull.Value)
                {
                    song.audioData = (byte[])row[2];
                }
                else
                {
                    song.audioData = null;
                }

                song.userid = Convert.ToInt32(row[3]);
                song.uploaded = Convert.ToDateTime(row[4]);
                song.duration = Convert.ToInt32(row[5]);
                song.plays = Convert.ToInt32(row[6]);

                songs.Add(song);
            }

            return songs;
        }

        // ⭐ NEW: Remove song and fix positions automatically
        public async Task RemoveSongAndReorderAsync(int playlistId, int songId)
        {
            await RemoveSongFromPlaylistAsync(playlistId, songId);

            var songs = await GetSongsInPlaylistAsync(playlistId);

            int position = 1;
            
            // Reorder using explicit for loop
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                if (songs[i].position != position)
                {
                    await UpdateSongPositionAsync(playlistId, songs[i].songid, position);
                }

                position = position + 1;
            }
        }
    }
}
