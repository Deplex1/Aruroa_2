using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Handles admin operations related to songs.
    /// Keeps database logic away from UI layer.
    /// </summary>
    public class SongAdminService
    {
        /// <summary>
        /// Loads all songs from database.
        /// </summary>
        public async Task<List<Song>> LoadAllSongsAsync()
        {
            try
            {
                SongDB sDB = new SongDB();
                return await sDB.SelectAllSongsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading songs: " + ex.Message);
                return new List<Song>();
            }
        }

        /// <summary>
        /// Deletes a song by ID.
        /// </summary>
        public async Task<bool> DeleteSongAsync(int songId)
        {
            try
            {
                SongDB sDB = new SongDB();
                await sDB.DeleteSongAsync(songId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting song: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Basic validation to prevent nonsense operations.
        /// </summary>
        public string ValidateSong(Song song)
        {
            if (song == null)
            {
                return "Invalid song selection.";
            }

            if (song.songID <= 0)
            {
                return "Song ID is invalid.";
            }

            return null;
        }
    }
}
