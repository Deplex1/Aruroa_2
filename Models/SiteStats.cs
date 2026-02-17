namespace Models
{
    // Simple model to hold site-wide statistics
    // Used on the home page to show totals
    public class SiteStats
    {
        // Total number of songs uploaded to the platform
        public int TotalSongs { get; set; }

        // Total number of registered users
        public int TotalUsers { get; set; }

        // Total number of playlists created
        public int TotalPlaylists { get; set; }

        // Total number of times songs have been played
        public int TotalPlays { get; set; }

        // Empty constructor with default values
        public SiteStats()
        {
            TotalSongs = 0;
            TotalUsers = 0;
            TotalPlaylists = 0;
            TotalPlays = 0;
        }
    }
}