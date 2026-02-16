using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DBL;
using Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Services
{
    /// <summary>
    /// Service class that handles user profile business logic.
    /// Manages user data loading, song management, and profile picture updates.
    /// </summary>
    public class ProfileService
    {
        /// <summary>
        /// Loads all songs uploaded by a specific user.
        /// Returns a list of Song objects.
        /// </summary>
        public async Task<List<Song>> LoadUserSongsAsync(int userId)
        {
            try
            {
                SongDB songDB = new SongDB();
                List<Song> songs = await songDB.SelectSongsByUserIDAsync(userId);

                if (songs != null)
                {
                    return songs;
                }
                else
                {
                    return new List<Song>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading user songs: " + ex.Message);
                return new List<Song>();
            }
        }

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
                Console.WriteLine("Error loading user playlists: " + ex.Message);
                return new List<Playlist>();
            }
        }

        /// <summary>
        /// Deletes a song from the database.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> DeleteSongAsync(int songId)
        {
            try
            {
                SongDB songDB = new SongDB();
                await songDB.DeleteSongAsync(songId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting song: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Removes a song from a list by its ID.
        /// Returns true if the song was found and removed.
        /// </summary>
        public bool RemoveSongFromList(int songId, List<Song> songs)
        {
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                if (songs[i].songID == songId)
                {
                    songs.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Processes an uploaded profile picture file.
        /// Returns the image as a byte array, or null if processing failed.
        /// </summary>
        public async Task<byte[]> ProcessProfileImageAsync(IBrowserFile file)
        {
            try
            {
                if (file == null)
                {
                    return null;
                }

                using (Stream stream = file.OpenReadStream(5 * 1024 * 1024))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);

                        byte[] imageBytes = ms.ToArray();
                        return imageBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing profile image: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates a user's profile picture in the database.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> UpdateProfilePictureAsync(int userId, byte[] imageBytes)
        {
            try
            {
                UserDB userDB = new UserDB();
                Dictionary<string, object> updates = new Dictionary<string, object>();
                updates.Add("profilepicture", imageBytes);
                
                await userDB.UpdateUserAsync(userId, updates);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating profile picture: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Converts image byte array to a base64 data URL for display.
        /// Returns the data URL string, or null if imageData is null/empty.
        /// </summary>
        public string GetProfileImageDataUrl(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            return "data:image/png;base64," + Convert.ToBase64String(imageData);
        }
    }
}
