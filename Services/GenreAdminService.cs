using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Service class that handles all genre administration business logic.
    /// Manages genre creation, deletion, and validation for admin users.
    /// </summary>
    public class GenreAdminService
    {
        /// <summary>
        /// Loads all genres from the database.
        /// Returns a list of Genre objects.
        /// </summary>
        public async Task<List<Genre>> LoadGenresAsync()
        {
            try
            {
                GenreDB genreDB = new GenreDB();
                List<Genre> dbGenres = await genreDB.SelectAllGenresAsync();

                List<Genre> genres = new List<Genre>();
                for (int i = 0; i < dbGenres.Count; i = i + 1)
                {
                    genres.Add(dbGenres[i]);
                }

                return genres;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading genres: " + ex.Message);
                return new List<Genre>();
            }
        }

        /// <summary>
        /// Validates a genre name before adding it to the database.
        /// Returns an error message if validation fails, or null if valid.
        /// </summary>
        public string ValidateGenreName(string genreName, List<Genre> existingGenres)
        {
            // Check if name is empty
            if (string.IsNullOrWhiteSpace(genreName))
            {
                return "Genre name cannot be empty";
            }

            string trimmedName = genreName.Trim();

            // Check for duplicates (case-insensitive)
            bool isDuplicate = false;
            for (int i = 0; i < existingGenres.Count; i = i + 1)
            {
                if (existingGenres[i].name.ToLower() == trimmedName.ToLower())
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate)
            {
                return "Genre '" + trimmedName + "' already exists!";
            }

            // Validate length
            if (trimmedName.Length < 2)
            {
                return "Genre name must be at least 2 characters long";
            }

            if (trimmedName.Length > 50)
            {
                return "Genre name must be less than 50 characters";
            }

            // All validations passed
            return null;
        }

        /// <summary>
        /// Adds a new genre to the database.
        /// Returns the number of rows affected (1 if successful, 0 if failed).
        /// </summary>
        public async Task<int> AddGenreAsync(string genreName)
        {
            try
            {
                GenreDB genreDB = new GenreDB();
                int rows = await genreDB.AddGenreAsync(genreName.Trim());
                return rows;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding genre: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Deletes a genre from the database.
        /// Returns the number of rows affected (greater than 0 if successful).
        /// </summary>
        public async Task<int> DeleteGenreAsync(int genreId)
        {
            try
            {
                GenreDB genreDB = new GenreDB();
                int rowsAffected = await genreDB.DeleteGenreAsync(genreId);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting genre: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Finds a genre in the list by its ID.
        /// Returns the Genre object if found, null otherwise.
        /// </summary>
        public Genre FindGenreById(int genreId, List<Genre> genres)
        {
            for (int i = 0; i < genres.Count; i = i + 1)
            {
                if (genres[i].genreid == genreId)
                {
                    return genres[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Removes a genre from a list by its ID.
        /// Returns true if the genre was found and removed.
        /// </summary>
        public bool RemoveGenreFromList(int genreId, List<Genre> genres)
        {
            for (int i = 0; i < genres.Count; i = i + 1)
            {
                if (genres[i].genreid == genreId)
                {
                    genres.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}
