using System;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // Database class for retrieving site-wide statistics
    // Uses SQL COUNT() and SUM() functions for efficiency
    public class StatsDB : BaseDB<SiteStats>
    {
        protected override string GetTableName()
        {
            return "songs";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        // We don't need CreateModelAsync for this class
        // because we use ExecScalarAsync directly
        protected override Task<SiteStats> CreateModelAsync(object[] row)
        {
            return Task.FromResult(new SiteStats());
        }

        // Get all site statistics in separate SQL queries
        // Each query uses COUNT() or SUM() which is much faster
        // than loading all records and counting in C#
        public async Task<SiteStats> GetStatsAsync()
        {
            SiteStats stats = new SiteStats();

            // Query 1: Count total songs
            // COUNT(*) counts ALL rows in the songs table
            object songCount = await ExecScalarAsync("SELECT COUNT(*) FROM songs");
            if (songCount != null && songCount != DBNull.Value)
            {
                stats.TotalSongs = Convert.ToInt32(songCount);
            }

            // Query 2: Count total users
            object userCount = await ExecScalarAsync("SELECT COUNT(*) FROM users");
            if (userCount != null && userCount != DBNull.Value)
            {
                stats.TotalUsers = Convert.ToInt32(userCount);
            }

            // Query 3: Count total playlists
            object playlistCount = await ExecScalarAsync("SELECT COUNT(*) FROM playlists");
            if (playlistCount != null && playlistCount != DBNull.Value)
            {
                stats.TotalPlaylists = Convert.ToInt32(playlistCount);
            }

            // Query 4: Sum all play counts
            // SUM(plays) adds up the plays column from every song
            // This gives us total plays across the whole platform
            object totalPlays = await ExecScalarAsync("SELECT SUM(plays) FROM songs");
            if (totalPlays != null && totalPlays != DBNull.Value)
            {
                stats.TotalPlays = Convert.ToInt32(totalPlays);
            }

            return stats;
        }
    }
}