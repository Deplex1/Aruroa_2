using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    public class RatingDB : BaseDB<Rating>
    {
        protected override string GetTableName()
        {
            return "ratings";
        }

        protected override string GetPrimaryKeyName()
        {
            return "ratingid";
        }

        protected async override Task<Rating> CreateModelAsync(object[] row)
        {
            Rating r = new Rating();
            r.ratingid = int.Parse(row[0].ToString());
            r.userid = int.Parse(row[1].ToString());
            r.songid = int.Parse(row[2].ToString());
            r.rating = int.Parse(row[3].ToString());
            
            if (row[4] == null)
            {
                r.daterated = null;
            }
            else
            {
                r.daterated = (System.DateTime?)row[4];
            }
            
            return r;
        }

        public async Task AddOrUpdateRatingAsync(int userId, int songId, int value)
        {
            string checkSql = "SELECT * FROM ratings WHERE userid=@u AND songid=@s";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);
            p.Add("s", songId);

            List<Rating> list = await SelectAllAsync(checkSql, p);

            if (list.Count == 0)
            {
                Dictionary<string, object> insert = new Dictionary<string, object>();
                insert.Add("userid", userId);
                insert.Add("songid", songId);
                insert.Add("rating", value);
                insert.Add("daterated", DateTime.Now);
                await InsertAsync(insert);
            }
            else
            {
                Dictionary<string, object> fields = new Dictionary<string, object>();
                fields.Add("rating", value);
                fields.Add("daterated", DateTime.Now);

                Dictionary<string, object> where = new Dictionary<string, object>();
                where.Add("ratingid", list[0].ratingid);

                await UpdateAsync(fields, where);
            }
        }

        public async Task SaveRatingAsync(int userId, int songId, int value)
        {
            await AddOrUpdateRatingAsync(userId, songId, value);
        }

        public async Task<List<Rating>> GetUserRatingsAsync(int userId)
        {
            string sql = "SELECT * FROM ratings WHERE userid=@u ORDER BY daterated DESC";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);

            return await SelectAllAsync(sql, p);
        }

        public async Task<List<Rating>> GetSongRatingsAsync(int songId)
        {
            string sql = "SELECT * FROM ratings WHERE songid=@s ORDER BY daterated DESC";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("s", songId);

            return await SelectAllAsync(sql, p);
        }

        public async Task<Rating?> GetUserRatingForSongAsync(int userId, int songId)
        {
            string sql = "SELECT * FROM ratings WHERE userid=@u AND songid=@s";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);
            p.Add("s", songId);

            var list = await SelectAllAsync(sql, p);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the average rating for a song using SQL AVG() function.
        /// This is much more efficient than loading all ratings and calculating in C#.
        /// Returns the average rating as a double, or 0 if no ratings exist.
        /// </summary>
        public async Task<double> GetAverageRatingAsync(int songId)
        {
            // Use SQL AVG() function to calculate average in database
            string sql = "SELECT AVG(CAST(rating AS FLOAT)) FROM ratings WHERE songid=@s";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("s", songId);

            // Execute query and get first row
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Check if result is empty (no ratings exist)
            if (rows.Count == 0 || rows[0][0] == null || rows[0][0] == DBNull.Value)
            {
                return 0;
            }

            // Convert result to double
            double average = Convert.ToDouble(rows[0][0]);
            return average;
        }

        /// <summary>
        /// Gets the count of ratings for a song using SQL COUNT() function.
        /// This is much more efficient than loading all ratings and counting in C#.
        /// Returns the count as an integer.
        /// </summary>
        public async Task<int> GetRatingCountAsync(int songId)
        {
            // Use SQL COUNT() function to count ratings in database
            string sql = "SELECT COUNT(*) FROM ratings WHERE songid=@s";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("s", songId);

            // Execute query and get first row
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Check if result is empty
            if (rows.Count == 0 || rows[0][0] == null || rows[0][0] == DBNull.Value)
            {
                return 0;
            }

            // Convert result to integer
            int count = Convert.ToInt32(rows[0][0]);
            return count;
        }

        /// <summary>
        /// Gets average ratings and counts for multiple songs in one query using SQL.
        /// Returns a dictionary mapping songId to a tuple of (average, count).
        /// This is MUCH more efficient than calling GetAverageRatingAsync for each song.
        /// </summary>
        public async Task<Dictionary<int, (double average, int count)>> GetRatingStatsForSongsAsync(List<int> songIds)
        {
            Dictionary<int, (double average, int count)> stats = new Dictionary<int, (double, int)>();

            // If no song IDs provided, return empty dictionary
            if (songIds.Count == 0)
            {
                return stats;
            }

            // Build SQL with GROUP BY to get all stats in one query
            // Use AVG() and COUNT() functions
            string sql = "SELECT songid, AVG(CAST(rating AS FLOAT)) as avg_rating, COUNT(*) as rating_count " +
                        "FROM ratings " +
                        "WHERE songid IN (";

            // Add placeholders for each song ID
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < songIds.Count; i = i + 1)
            {
                string paramName = "s" + i.ToString();
                sql = sql + "@" + paramName;
                
                if (i < songIds.Count - 1)
                {
                    sql = sql + ", ";
                }
                
                parameters.Add(paramName, songIds[i]);
            }

            sql = sql + ") GROUP BY songid";

            // Execute query using BaseDB method
            List<object[]> rows = await ExecuteQueryAsync(sql, parameters);

            // Process results
            for (int i = 0; i < rows.Count; i = i + 1)
            {
                object[] row = rows[i];
                
                int songId = Convert.ToInt32(row[0]);
                
                double average = 0;
                if (row[1] == DBNull.Value)
                {
                    average = 0;
                }
                else
                {
                    average = Convert.ToDouble(row[1]);
                }
                
                int count = Convert.ToInt32(row[2]);

                stats[songId] = (average, count);
            }

            return stats;
        }

        /// <summary>
        /// Gets all ratings for multiple songs in one SQL query.
        /// This is MUCH more efficient than calling GetSongRatingsAsync for each song.
        /// Returns a list of all Rating objects for the specified songs.
        /// </summary>
        public async Task<List<Rating>> GetRatingsForMultipleSongsAsync(List<int> songIds)
        {
            if (songIds.Count == 0)
            {
                return new List<Rating>();
            }

            // Build SQL with IN clause to get all ratings in one query
            string sql = "SELECT * FROM ratings WHERE songid IN (";

            // Add placeholders for each song ID
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < songIds.Count; i = i + 1)
            {
                string paramName = "s" + i.ToString();
                sql = sql + "@" + paramName;
                
                if (i < songIds.Count - 1)
                {
                    sql = sql + ", ";
                }
                
                parameters.Add(paramName, songIds[i]);
            }

            sql = sql + ") ORDER BY daterated DESC";

            // Execute query
            List<Rating> ratings = await SelectAllAsync(sql, parameters);
            return ratings;
        }
    }
}