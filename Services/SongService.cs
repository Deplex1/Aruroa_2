using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Service class that handles all song browsing, rating, and queue management logic.
    /// Uses SQL-FIRST approach - all calculations done in database when possible.
    /// </summary>
    public class SongService
    {
        /// <summary>
        /// Loads all songs from the database.
        /// Returns a list of Song objects.
        /// </summary>
        public async Task<List<Song>> LoadAllSongsAsync()
        {
            SongDB db = new SongDB();
            List<Song> songs = await db.SelectAllSongsAsync();
            return songs;
        }

        /// <summary>
        /// Searches for songs based on search text.
        /// Returns a list of matching Song objects.
        /// </summary>
        public async Task<List<Song>> SearchSongsAsync(string searchText)
        {
            SongDB db = new SongDB();
            List<Song> songs = await db.SearchSongsAsync(searchText.Trim());
            return songs;
        }

        /// <summary>
        /// Loads all ratings for a specific song from the database.
        /// Returns a list of Rating objects.
        /// </summary>
        public async Task<List<Rating>> LoadSongRatingsAsync(int songId)
        {
            RatingDB ratingDB = new RatingDB();
            List<Rating> ratings = await ratingDB.GetSongRatingsAsync(songId);
            return ratings;
        }

        /// <summary>
        /// Loads all ratings for all songs in the provided list.
        /// Uses SQL IN clause to load all ratings in ONE query instead of N queries.
        /// Returns a combined list of all Rating objects.
        /// </summary>
        public async Task<List<Rating>> LoadAllRatingsForSongsAsync(List<Song> songs)
        {
            try
            {
                if (songs.Count == 0)
                {
                    return new List<Rating>();
                }

                // Build list of song IDs
                List<int> songIds = new List<int>();
                for (int i = 0; i < songs.Count; i = i + 1)
                {
                    songIds.Add(songs[i].songID);
                }

                // Get all ratings in ONE SQL query using IN clause
                RatingDB ratingDB = new RatingDB();
                List<Rating> allRatings = await ratingDB.GetRatingsForMultipleSongsAsync(songIds);
                
                return allRatings;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading ratings for songs: " + ex.Message);
                return new List<Rating>();
            }
        }

        /// <summary>
        /// Loads all ratings made by a specific user.
        /// Returns a dictionary mapping songId to rating value (1-5).
        /// </summary>
        public async Task<Dictionary<int, int>> LoadUserRatingsAsync(int userId)
        {
            RatingDB ratingDB = new RatingDB();
            List<Rating> ratings = await ratingDB.GetUserRatingsAsync(userId);

            Dictionary<int, int> userRatings = new Dictionary<int, int>();
            for (int i = 0; i < ratings.Count; i = i + 1)
            {
                userRatings[ratings[i].songid] = ratings[i].rating;
            }

            return userRatings;
        }

        /// <summary>
        /// Calculates the average rating for a specific song using SQL.
        /// Returns the average as a double, or 0 if no ratings exist.
        /// </summary>
        public async Task<double> CalculateAverageRatingAsync(int songId)
        {
            RatingDB ratingDB = new RatingDB();
            double average = await ratingDB.GetAverageRatingAsync(songId);
            return average;
        }

        /// <summary>
        /// Counts how many ratings a song has received using SQL.
        /// Returns the count as an integer.
        /// </summary>
        public async Task<int> CountRatingsAsync(int songId)
        {
            RatingDB ratingDB = new RatingDB();
            int count = await ratingDB.GetRatingCountAsync(songId);
            return count;
        }

        /// <summary>
        /// Gets rating statistics (average and count) for multiple songs in one SQL query.
        /// This is MUCH more efficient than calling CalculateAverageRatingAsync for each song.
        /// Returns a dictionary mapping songId to (average, count) tuple.
        /// </summary>
        public async Task<Dictionary<int, (double average, int count)>> GetRatingStatsForSongsAsync(List<Song> songs)
        {
            // Build list of song IDs
            List<int> songIds = new List<int>();
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                songIds.Add(songs[i].songID);
            }

            // Get all stats in one SQL query
            RatingDB ratingDB = new RatingDB();
            Dictionary<int, (double average, int count)> stats = await ratingDB.GetRatingStatsForSongsAsync(songIds);
            
            return stats;
        }

        /// <summary>
        /// Saves or updates a user's rating for a song.
        /// Returns true if successful.
        /// </summary>
        public async Task<bool> SaveRatingAsync(int userId, int songId, int ratingValue)
        {
            try
            {
                RatingDB ratingDB = new RatingDB();
                await ratingDB.SaveRatingAsync(userId, songId, ratingValue);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving rating: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Increments the play count for a song in the database.
        /// This is called when a song starts playing.
        /// </summary>
        public async Task IncrementPlayCountAsync(int songId)
        {
            try
            {
                SongDB db = new SongDB();
                await db.AddPlayAsync(songId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error incrementing play count: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if a song is already in the queue.
        /// Returns true if found, false otherwise.
        /// </summary>
        public bool IsSongInQueue(Song song, List<Song> queue)
        {
            for (int i = 0; i < queue.Count; i = i + 1)
            {
                if (queue[i].songID == song.songID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the index of a song in the queue by its ID.
        /// Returns the index, or -1 if not found.
        /// </summary>
        public int FindSongIndexInQueue(int songId, List<Song> queue)
        {
            for (int i = 0; i < queue.Count; i = i + 1)
            {
                if (queue[i].songID == songId)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the next song in the queue after the current index.
        /// Returns null if at end of queue or queue is empty.
        /// </summary>
        public Song? GetNextSongInQueue(List<Song> queue, int currentIndex)
        {
            if (queue.Count == 0)
            {
                return null;
            }

            int nextIndex = currentIndex + 1;
            if (nextIndex < queue.Count)
            {
                return queue[nextIndex];
            }

            return null;
        }

        /// <summary>
        /// Gets the previous song in the queue before the current index.
        /// Returns null if at start of queue or queue is empty.
        /// </summary>
        public Song? GetPreviousSongInQueue(List<Song> queue, int currentIndex)
        {
            if (queue.Count == 0)
            {
                return null;
            }

            int previousIndex = currentIndex - 1;
            if (previousIndex >= 0)
            {
                return queue[previousIndex];
            }

            return null;
        }

        /// <summary>
        /// Gets a song at a specific index in the queue.
        /// Returns null if index is out of bounds.
        /// </summary>
        public Song? GetSongAtQueueIndex(List<Song> queue, int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= queue.Count)
            {
                return null;
            }

            return queue[index];
        }

        /// <summary>
        /// Loads genres for multiple songs in ONE SQL query.
        /// Returns a dictionary mapping songId to a list of Genre objects.
        /// This is MUCH more efficient than loading genres for each song individually.
        /// </summary>
        public async Task<Dictionary<int, List<Genre>>> LoadSongGenresAsync(List<Song> songs)
        {
            Dictionary<int, List<Genre>> songGenres = new Dictionary<int, List<Genre>>();

            if (songs.Count == 0)
            {
                return songGenres;
            }

            // Build list of song IDs
            List<int> songIds = new List<int>();
            for (int i = 0; i < songs.Count; i = i + 1)
            {
                songIds.Add(songs[i].songID);
            }

            // Get all genre info in ONE SQL query using JOIN and IN clause
            SongGenreDB songGenreDB = new SongGenreDB();
            List<SongGenreInfo> genreInfoList = await songGenreDB.GetGenresForMultipleSongsAsync(songIds);

            // Group results by songId
            for (int i = 0; i < genreInfoList.Count; i = i + 1)
            {
                SongGenreInfo info = genreInfoList[i];

                // Create Genre object from info
                Genre genre = new Genre();
                genre.genreid = info.genreid;
                genre.name = info.name;

                // Add to dictionary
                if (songGenres.ContainsKey(info.songid))
                {
                    songGenres[info.songid].Add(genre);
                }
                else
                {
                    List<Genre> genreList = new List<Genre>();
                    genreList.Add(genre);
                    songGenres[info.songid] = genreList;
                }
            }

            return songGenres;
        }

        /// <summary>
        /// Filters songs by one or more genres using SQL JOIN and IN clause.
        /// If genreIds list is empty, returns all songs.
        /// Returns a list of Song objects that match the selected genres.
        /// </summary>
        public async Task<List<Song>> FilterSongsByGenresAsync(List<int> genreIds)
        {
            if (genreIds.Count == 0)
            {
                return await LoadAllSongsAsync();
            }

            SongDB songDB = new SongDB();
            List<Song> songs = await songDB.FilterByGenresAsync(genreIds);
            return songs;
        }
    }
}
