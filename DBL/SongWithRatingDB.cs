using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // Database class for retrieving songs with their rating information
    // Uses JOIN queries to combine songs and ratings tables
    public class SongWithRatingDB : BaseDB<SongWithRating>
    {
        protected override string GetTableName()
        {
            return "songs";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        // Convert a database row into a SongWithRating object
        // The row contains columns from BOTH songs and ratings tables
        // because of the JOIN in our SQL query
        protected override Task<SongWithRating> CreateModelAsync(object[] row)
        {
            SongWithRating song = new SongWithRating();

            // Column 0: songid
            song.songID = Convert.ToInt32(row[0]);

            // Column 1: title
            song.title = row[1].ToString();

            // Column 2: duration
            song.duration = Convert.ToInt32(row[2]);

            // Column 3: audioData (binary MP3 data)
            if (row[3] != null && row[3] != DBNull.Value)
            {
                song.audioData = (byte[])row[3];
            }

            // Column 4: userid
            song.userid = Convert.ToInt32(row[4]);

            // Column 5: uploaded date
            if (row[5] != null && row[5] != DBNull.Value)
            {
                song.uploaded = Convert.ToDateTime(row[5]);
            }
            else
            {
                song.uploaded = DateTime.Now;
            }

            // Column 6: plays
            if (row[6] != null && row[6] != DBNull.Value)
            {
                song.plays = Convert.ToInt32(row[6]);
            }

            // Column 7: avg_rating (calculated by SQL AVG())
            if (row[7] != null && row[7] != DBNull.Value)
            {
                song.AvgRating = Convert.ToDouble(row[7]);
            }

            // Column 8: rating_count (calculated by SQL COUNT())
            if (row[8] != null && row[8] != DBNull.Value)
            {
                song.RatingCount = Convert.ToInt32(row[8]);
            }

            return Task.FromResult(song);
        }

        // Execute the top rated songs query and return results
        public async Task<List<SongWithRating>> GetTopRatedAsync(string sql)
        {
            // Empty parameters since the SQL has no parameters
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // SelectAllAsync will execute the SQL and use CreateModelAsync
            // to convert each row into a SongWithRating object
            List<SongWithRating> songs = await SelectAllAsync(sql, parameters);

            return songs;
        }
    }
}