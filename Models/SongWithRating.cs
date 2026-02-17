using System;

namespace Models
{
    // Extended Song model that includes rating information
    // Used for the top rated songs section on the home page
    // We need this because regular Song model doesn't have rating fields
    public class SongWithRating
    {
        // All the regular song fields
        public int songID { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public byte[] audioData { get; set; }
        public int userid { get; set; }
        public DateTime uploaded { get; set; }
        public int plays { get; set; }

        // Extra fields from the JOIN with ratings table
        // avg_rating is calculated by SQL AVG() function
        public double AvgRating { get; set; }

        // rating_count is calculated by SQL COUNT() function
        public int RatingCount { get; set; }

        public SongWithRating()
        {
            AvgRating = 0;
            RatingCount = 0;
        }
    }
}