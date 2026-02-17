using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    // HomeService handles all database operations for the home page
    // Includes most played, top rated, recently uploaded, and site statistics
    public class HomeService
    {
        // Get the top 10 most played songs
        // Uses SQL ORDER BY plays DESC to sort by play count
        public async Task<List<Song>> GetMostPlayedSongsAsync()
        {
            SongDB songDB = new SongDB();
            List<Song> songs = await songDB.GetPopularSongsAsync();
            return songs;
        }

        // Get the top 10 most recently uploaded songs
        // Uses SQL ORDER BY uploaded DESC to get newest first
        public async Task<List<Song>> GetRecentlyUploadedSongsAsync()
        {
            SongDB songDB = new SongDB();
            List<Song> songs = await songDB.GetNewSongsAsync();
            return songs;
        }

        // Get the top 10 highest rated songs
        // Only includes songs with at least 3 ratings (to avoid bias)
        // Uses SQL JOIN with ratings table and AVG() function
        // Returns songs with their average rating calculated in SQL
        public async Task<List<SongWithRating>> GetTopRatedSongsAsync()
        {
            // This SQL query does several things at once:
            // 1. JOINS songs with ratings to get rating data
            // 2. Uses AVG() to calculate average rating per song
            // 3. Uses COUNT() to count how many ratings each song has
            // 4. HAVING filters out songs with fewer than 3 ratings
            // 5. ORDER BY sorts by average rating (highest first)
            // 6. LIMIT 10 returns only top 10 results
            string sql = @"
                SELECT 
                    s.songid,
                    s.title,
                    s.duration,
                    s.audioData,
                    s.userid,
                    s.uploaded,
                    s.plays,
                    AVG(r.rating) as avg_rating,
                    COUNT(r.ratingid) as rating_count
                FROM songs s
                INNER JOIN ratings r ON s.songid = r.songid
                GROUP BY 
                    s.songid,
                    s.title,
                    s.duration,
                    s.audioData,
                    s.userid,
                    s.uploaded,
                    s.plays
                HAVING COUNT(r.ratingid) >= 3
                ORDER BY avg_rating DESC
                LIMIT 10";

            SongWithRatingDB songWithRatingDB = new SongWithRatingDB();
            List<SongWithRating> topRated = await songWithRatingDB.GetTopRatedAsync(sql);

            return topRated;
        }

        // Get site-wide statistics
        // Uses SQL COUNT() to count records in each table
        // Returns a SiteStats object with all the numbers
        public async Task<SiteStats> GetSiteStatsAsync()
        {
            StatsDB statsDB = new StatsDB();
            SiteStats stats = await statsDB.GetStatsAsync();
            return stats;
        }
    }
}