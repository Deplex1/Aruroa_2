using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Service class that handles individual playlist page business logic.
    /// Manages playlist loading, song ordering, and playlist manipulation.
    /// </summary>
    public class PlaylistPageService
    {
        /// <summary>
        /// Loads a playlist by its ID from the database.
        /// Returns the Playlist object if found, null otherwise.
        /// </summary>
        public async Task<Playlist> LoadPlaylistByIdAsync(int playlistId)
        {
            try
            {
                PlaylistDB playlistDB = new PlaylistDB();
                Playlist playlist = await playlistDB.SelectByIdAsync(playlistId);
                return playlist;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading playlist: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Loads all songs that belong to a specific playlist using SQL JOIN.
        /// This is MUCH more efficient than loading relationships then loading each song.
        /// Returns a list of Song objects in the correct order.
        /// </summary>
        public async Task<List<Song>> LoadPlaylistSongsAsync(int playlistId)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                
                // Use SQL JOIN to get all songs in one query
                List<Song> songs = await psDB.GetPlaylistSongsWithDetailsAsync(playlistId);
                
                return songs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading playlist songs: " + ex.Message);
                return new List<Song>();
            }
        }

        /// <summary>
        /// Loads all songs that are NOT already in the playlist.
        /// Uses SQL NOT IN clause to filter in database instead of C#.
        /// Returns a list of Song objects that can be added.
        /// </summary>
        public async Task<List<Song>> LoadAvailableSongsAsync(int playlistId)
        {
            try
            {
                SongDB songDB = new SongDB();
                List<Song> songs = await songDB.GetSongsNotInPlaylistAsync(playlistId);
                return songs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading available songs: " + ex.Message);
                return new List<Song>();
            }
        }

        /// <summary>
        /// Gets the maximum position number in a playlist.
        /// Returns the highest position value.
        /// </summary>
        public async Task<int> GetMaxPositionAsync(int playlistId)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                int maxPos = await psDB.GetMaxPositionInPlaylistAsync(playlistId);
                return maxPos;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting max position: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Adds a song to a playlist at a specific position.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> AddSongToPlaylistAsync(int playlistId, int songId, int position)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                await psDB.AddSongToPlaylistAsync(playlistId, songId, position);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding song to playlist: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Removes a song from a playlist.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                await psDB.RemoveSongFromPlaylistAsync(playlistId, songId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing song from playlist: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Swaps the positions of two songs in a playlist.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> SwapSongPositionsAsync(int playlistId, int songId1, int songId2)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                await psDB.SwapSongPositionsAsync(playlistId, songId1, songId2);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error swapping song positions: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates a song's position in a playlist.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> UpdateSongPositionAsync(int playlistId, int songId, int newPosition)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();
                await psDB.UpdateSongPositionAsync(playlistId, songId, newPosition);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating song position: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Shuffles a list of songs using the Fisher-Yates algorithm.
        /// Modifies the list in place and returns it.
        /// </summary>
        public List<Song> ShuffleSongs(List<Song> songs)
        {
            Random rnd = new Random();

            for (int i = songs.Count - 1; i > 0; i = i - 1)
            {
                int swapIndex = rnd.Next(i + 1);

                Song temp = songs[i];
                songs[i] = songs[swapIndex];
                songs[swapIndex] = temp;
            }

            return songs;
        }

        /// <summary>
        /// Updates all song positions in a playlist based on the current list order.
        /// Returns true if all updates were successful.
        /// </summary>
        public async Task<bool> UpdateAllSongPositionsAsync(int playlistId, List<Song> songs)
        {
            try
            {
                PlaylistSongDB psDB = new PlaylistSongDB();

                for (int i = 0; i < songs.Count; i = i + 1)
                {
                    await psDB.UpdateSongPositionAsync(playlistId, songs[i].songID, i + 1);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating all song positions: " + ex.Message);
                return false;
            }
        }
    }
}
