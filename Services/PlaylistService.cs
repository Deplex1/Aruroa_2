using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Service class that handles all playlist management business logic.
    /// Manages playlist creation, deletion, and song management within playlists.
    /// </summary>
    public class PlaylistService
    {
        /// <summary>
        /// Loads all playlists created by a specific user.
        /// Returns a list of Playlist objects.
        /// </summary>
        public async Task<List<Playlist>> LoadUserPlaylistsAsync(int userId)
        {
            try
            {
                PlaylistDB playlistDB = new PlaylistDB();
                List<Playlist> playlists = await playlistDB.GetUserPlaylistsAsync(userId);
                return playlists;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading playlists: " + ex.Message);
                return new List<Playlist>();
            }
        }

        /// <summary>
        /// Loads all songs from the database.
        /// Returns a list of Song objects.
        /// </summary>
        public async Task<List<Song>> LoadAllSongsAsync()
        {
            try
            {
                SongDB songDB = new SongDB();
                List<Song> songs = await songDB.SelectAllSongsAsync();
                return songs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading songs: " + ex.Message);
                return new List<Song>();
            }
        }

        /// <summary>
        /// Creates a new playlist for a user.
        /// Returns the number of rows affected (1 if successful, 0 if failed).
        /// </summary>
        public async Task<int> CreatePlaylistAsync(int userId, string playlistName, bool isPublic)
        {
            try
            {
                PlaylistDB playlistDB = new PlaylistDB();
                Playlist newPlaylist = new Playlist();
                newPlaylist.name = playlistName.Trim();
                newPlaylist.userid = userId;
                newPlaylist.ispublic = isPublic;

                int result = await playlistDB.AddPlaylistAsync(newPlaylist);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating playlist: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Finds a playlist by its ID in a list of playlists.
        /// Returns the Playlist object if found, null otherwise.
        /// </summary>
        public Playlist FindPlaylistById(int playlistId, List<Playlist> playlists)
        {
            for (int i = 0; i < playlists.Count; i = i + 1)
            {
                if (playlists[i].playlistid == playlistId)
                {
                    return playlists[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Loads all songs that belong to a specific playlist using SQL JOIN.
        /// This is MUCH more efficient than loading relationships then loading each song.
        /// Returns a list of Song objects in the playlist.
        /// </summary>
        public async Task<List<Song>> LoadPlaylistSongsAsync(int playlistId, List<Song> allSongs)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                
                // Use SQL JOIN to get all songs in one query
                List<Song> playlistSongs = await playlistSongDB.GetPlaylistSongsWithDetailsAsync(playlistId);
                
                return playlistSongs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading playlist songs: " + ex.Message);
                return new List<Song>();
            }
        }

        /// <summary>
        /// Deletes a playlist from the database.
        /// Returns true if successful.
        /// </summary>
        public async Task<bool> DeletePlaylistAsync(int playlistId)
        {
            try
            {
                PlaylistDB playlistDB = new PlaylistDB();
                await playlistDB.DeletePlaylistAsync(playlistId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting playlist: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Removes a song from a playlist.
        /// Returns true if successful.
        /// </summary>
        public async Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                await playlistSongDB.RemoveSongFromPlaylistAsync(playlistId, songId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing song from playlist: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets the count of songs in a playlist.
        /// Returns the count as an integer.
        /// </summary>
        public async Task<int> GetSongCountAsync(int playlistId)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                int count = await playlistSongDB.GetSongCountAsync(playlistId);
                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Loads song counts for all playlists at once using SQL GROUP BY.
        /// This is MUCH more efficient than calling GetSongCountAsync for each playlist.
        /// Returns a dictionary mapping playlistId to song count.
        /// </summary>
        public async Task<Dictionary<int, int>> LoadSongCountsAsync(List<Playlist> playlists)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                
                // Get all counts in one SQL query using GROUP BY
                Dictionary<int, int> allCounts = await playlistSongDB.GetAllPlaylistSongCountsAsync();
                
                // For playlists with no songs, add them with count 0
                for (int i = 0; i < playlists.Count; i = i + 1)
                {
                    int playlistId = playlists[i].playlistid;
                    
                    // If playlist not in dictionary, it has 0 songs
                    if (allCounts.ContainsKey(playlistId) == false)
                    {
                        allCounts[playlistId] = 0;
                    }
                }
                
                return allCounts;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading song counts: " + ex.Message);
                return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Checks if a song already exists in a playlist.
        /// Returns true if the song is already in the playlist.
        /// </summary>
        public async Task<bool> SongExistsInPlaylistAsync(int playlistId, int songId)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                bool exists = await playlistSongDB.SongExistsInPlaylistAsync(playlistId, songId);
                return exists;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a song to a playlist at a specific position.
        /// Returns the number of rows affected (1 if successful).
        /// </summary>
        public async Task<int> AddSongToPlaylistAsync(int playlistId, int songId, int position)
        {
            try
            {
                PlaylistSongDB playlistSongDB = new PlaylistSongDB();
                int rows = await playlistSongDB.AddSongToPlaylistAsync(playlistId, songId, position);
                return rows;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding song to playlist: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Checks if a song is already in the playlist songs list.
        /// Returns true if found.
        /// </summary>
        public bool IsSongInPlaylist(int songId, List<Song> playlistSongs)
        {
            for (int i = 0; i < playlistSongs.Count; i = i + 1)
            {
                if (playlistSongs[i].songID == songId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
