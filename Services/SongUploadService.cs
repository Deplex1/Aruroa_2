using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DBL;
using Models;
using Microsoft.AspNetCore.Components.Forms;
using SongManagment;

namespace Services
{
    /// <summary>
    /// Service class that handles all song upload business logic and database operations.
    /// This keeps the Razor page clean and focused on UI concerns only.
    /// </summary>
    public class SongUploadService
    {
        /// <summary>
        /// Validates all upload requirements before processing.
        /// Returns an error message if validation fails, or null if everything is valid.
        /// </summary>
        public string ValidateUpload(User user, string title, List<int> selectedGenreIds, IBrowserFile selectedFile)
        {
            // VALIDATION STEP 1: Check if user is logged in
            if (user == null)
            {
                return "You must be logged in to upload songs.";
            }

            // VALIDATION STEP 2: Check if title is provided
            if (string.IsNullOrWhiteSpace(title))
            {
                return "Please enter a song title.";
            }

            // VALIDATION STEP 3: Check if at least one genre is selected
            if (selectedGenreIds.Count == 0)
            {
                return "Please select at least one genre.";
            }

            // VALIDATION STEP 4: Check if file is selected
            if (selectedFile == null)
            {
                return "Please select an audio file.";
            }

            // VALIDATION STEP 5: Check file type (must be audio)
            if (selectedFile.ContentType.StartsWith("audio/") == false)
            {
                return "Please select a valid audio file.";
            }

            // VALIDATION STEP 6: Check file size (maximum 20MB)
            long maxFileSize = 20 * 1024 * 1024; // 20MB in bytes
            if (selectedFile.Size > maxFileSize)
            {
                return "File size must be less than 20MB.";
            }

            // All validations passed
            return null;
        }

        /// <summary>
        /// Uploads a song to the database with all its metadata and genre relationships.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> UploadSongAsync(User user, string title, List<int> selectedGenreIds, IBrowserFile selectedFile)
        {
            try
            {
                // Log upload details for debugging
                Console.WriteLine("=== UPLOAD START ===");
                Console.WriteLine("User ID: " + user.userid.ToString());
                Console.WriteLine("Title: '" + title + "'");
                Console.WriteLine("Selected Genres: " + string.Join(", ", selectedGenreIds));
                Console.WriteLine("File: " + selectedFile.Name + " (" + selectedFile.Size.ToString() + " bytes)");
                Console.WriteLine("Content Type: " + selectedFile.ContentType);

                // STEP 1: Read the audio file into memory
                // OpenReadStream allows us to read files up to 20MB
                using (Stream stream = selectedFile.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);

                        // Convert the file to a byte array for database storage
                        byte[] audioData = ms.ToArray();
                        Console.WriteLine("Audio data size: " + audioData.Length.ToString() + " bytes");

                        // STEP 2: Calculate the duration of the audio file
                        // We use TagLib library (in AudioHelper) to read MP3 metadata
                        int duration = await AudioHelper.GetMp3DurationInSeconds(audioData);
                        Console.WriteLine("Calculated duration: " + duration.ToString() + " seconds");

                        // STEP 3: Create a new Song object
                        Song song = new Song();
                        song.title = title.Trim();
                        song.audioData = audioData;
                        song.userid = user.userid;
                        song.uploaded = DateTime.Now;
                        song.duration = duration;
                        song.plays = 0;

                        Console.WriteLine("=== SAVING TO DATABASE ===");
                        Console.WriteLine("Song object created successfully");

                        // STEP 4: Insert the song into the database
                        SongDB songDB = new SongDB();
                        int rows = await songDB.AddSongAsync(song);

                        Console.WriteLine("Database insert result: " + rows.ToString() + " rows affected");

                        // Check if the insert was successful
                        if (rows == 1)
                        {
                            // STEP 5: Get the ID of the song we just inserted
                            // We need this to create the genre relationships
                            List<Song> allSongs = await songDB.SelectAllSongsAsync();

                            // Find the song we just added (it should be the last one)
                            int newSongId = 0;
                            for (int i = 0; i < allSongs.Count; i = i + 1)
                            {
                                if (allSongs[i].title == song.title && allSongs[i].userid == user.userid)
                                {
                                    newSongId = allSongs[i].songID;
                                }
                            }

                            // STEP 6: Add genre relationships to song_genres table
                            if (newSongId > 0)
                            {
                                SongGenreDB songGenreDB = new SongGenreDB();

                                // Loop through all selected genres and create relationships
                                for (int i = 0; i < selectedGenreIds.Count; i = i + 1)
                                {
                                    int genreId = selectedGenreIds[i];
                                    await songGenreDB.AddSongGenreAsync(newSongId, genreId);
                                    Console.WriteLine("Added genre " + genreId.ToString() + " to song " + newSongId.ToString());
                                }
                            }

                            Console.WriteLine("=== UPLOAD SUCCESS ===");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("=== UPLOAD FAILED: Database returned 0 rows ===");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If anything goes wrong, log the error
                Console.WriteLine("=== UPLOAD FAILED ===");
                Console.WriteLine("Exception: " + ex.GetType().Name);
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                Console.WriteLine("=== END OF ERROR ===");
                return false;
            }
        }

        /// <summary>
        /// Loads all available genres from the database.
        /// Returns a list of Genre objects.
        /// </summary>
        public async Task<List<Genre>> LoadGenresAsync()
        {
            try
            {
                GenreDB genreDB = new GenreDB();
                List<Genre> genres = await genreDB.SelectAllGenresAsync();
                return genres;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading genres: " + ex.Message);
                return new List<Genre>();
            }
        }
    }
}
