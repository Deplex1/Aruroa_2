using Google.Protobuf.WellKnownTypes;
using Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DBL
{
    // GenreRequest database operations
    // Handles storing and managing user requests for new genres
    public class GenreRequestDB : BaseDB<GenreRequest>
    {
        // Return the table name for this database class
        protected override string GetTableName()
        {
            return "genre_requests";
        }

        // Return the primary key column name
        protected override string GetPrimaryKeyName()
        {
            return "requestid";
        }

        // Create a GenreRequest object from a database row
        // The row array contains all columns from the genre_requests table
        protected override Task<GenreRequest> CreateModelAsync(object[] row)
        {
            GenreRequest request = new GenreRequest();

            // Column 0: requestid (INT)
            request.requestid = Convert.ToInt32(row[0]);

            // Column 1: userid (INT)
            request.userid = Convert.ToInt32(row[1]);

            // Column 2: genre_name (VARCHAR)
            request.genre_name = row[2].ToString();

            // Column 3: requested_date (DATETIME)
            request.requested_date = Convert.ToDateTime(row[3]);

            // Column 4: status (VARCHAR) - 'pending', 'approved', 'rejected'
            request.status = row[4].ToString();

            // Column 5: reviewed_by (INT, can be NULL)
            if (row[5] != null && row[5] != DBNull.Value)
            {
                request.reviewed_by = Convert.ToInt32(row[5]);
            }

            // Column 6: reviewed_date (DATETIME, can be NULL)
            if (row[6] != null && row[6] != DBNull.Value)
            {
                request.reviewed_date = Convert.ToDateTime(row[6]);
            }

            return Task.FromResult(request);
        }

        // Add a new genre request to the database
        public async Task<int> AddRequestAsync(int userId, string genreName)
        {
            // Create the dictionary with column names and values
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("userid", userId);
            values.Add("genre_name", genreName.Trim());
            values.Add("requested_date", DateTime.Now);
            values.Add("status", "pending");

            // Insert into database using BaseDB method
            // Returns number of rows affected (should be 1 if successful)
            return await InsertAsync(values);
        }

        // Get all pending genre requests (for admin to review)
        // Uses SQL ORDER BY for sorting much faster than sorting in C#
        public async Task<List<GenreRequest>> GetPendingRequestsAsync()
        {
            // Use custom SQL query with ORDER BY clause
            // DESC means descending order (newest first)
            string sql = "SELECT * FROM genre_requests WHERE status = @status ORDER BY requested_date DESC";

            // Create parameters for the WHERE clause
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("status", "pending");

            // Execute the query and return results
            // SQL does the sorting for us - much faster than sorting in C#
            List<GenreRequest> requests = await SelectAllAsync(sql, parameters);

            return requests;
        }


        


        // Get all requests submitted by a specific user
        // Uses SQL ORDER BY to return newest first
        public async Task<List<GenreRequest>> GetUserRequestsAsync(int userId)
        {
            // Use SQL with ORDER BY to sort by date
            string sql = "SELECT * FROM genre_requests WHERE userid = @userid ORDER BY requested_date DESC";

            // Create parameters
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userid", userId);

            // Execute query
            List<GenreRequest> requests = await SelectAllAsync(sql, parameters);

            return requests;
        }

        // Approve a genre request and create the genre
        // Returns true if successful, false if failed
        public async Task<bool> ApproveRequestAsync(int requestId, int adminUserId)
        {
            // STEP 1: Get the request to see what genre was requested
            Dictionary<string, object> whereClause = new Dictionary<string, object>();
            whereClause.Add("requestid", requestId);

            List<GenreRequest> requests = await SelectAllAsync(whereClause);

            if (requests.Count == 0)
            {
                // Request not found
                return false;
            }

            GenreRequest request = requests[0];

            // STEP 2: Check if this genre already exists using SQL LOWER() for case-insensitive comparison
            // This is MUCH better than loading all genres and looping in C#
            string checkSql = "SELECT COUNT(*) FROM genres WHERE LOWER(name) = LOWER(@name)";

            Dictionary<string, object> checkParams = new Dictionary<string, object>();
            checkParams.Add("name", request.genre_name);

            // Add parameter to command manually since we're using ExecScalarAsync
            cmd.Parameters.Clear();
            var param = cmd.CreateParameter();
            param.ParameterName = "@name";
            param.Value = request.genre_name;
            cmd.Parameters.Add(param);

            // Execute the COUNT query
            object countResult = await ExecScalarAsync(checkSql);

            int genreCount = 0;
            if (countResult != null && countResult != DBNull.Value)
            {
                genreCount = Convert.ToInt32(countResult);
            }

            bool genreExists = genreCount > 0;

            // STEP 3: If genre doesn't exist, create it
            if (genreExists == false)
            {
                GenreDB genreDB = new GenreDB();
                int result = await genreDB.AddGenreAsync(request.genre_name);
                if (result == 0)
                {
                    // Failed to create genre
                    return false;
                }
            }

            // STEP 4: Update the request status to 'approved'
            Dictionary<string, object> updateFields = new Dictionary<string, object>();
            updateFields.Add("status", "approved");
            updateFields.Add("reviewed_by", adminUserId);
            updateFields.Add("reviewed_date", DateTime.Now);

            Dictionary<string, object> whereUpdate = new Dictionary<string, object>();
            whereUpdate.Add("requestid", requestId);

            int rowsAffected = await UpdateAsync(updateFields, whereUpdate);

            // Return true if update was successful
            return rowsAffected > 0;
        }





        // Reject a genre request
        // Returns true if successful, false if failed
        public async Task<bool> RejectRequestAsync(int requestId, int adminUserId)
        {
            // Update the request status to 'rejected'
            Dictionary<string, object> updateFields = new Dictionary<string, object>();
            updateFields.Add("status", "rejected");
            updateFields.Add("reviewed_by", adminUserId);
            updateFields.Add("reviewed_date", DateTime.Now);

            Dictionary<string, object> whereClause = new Dictionary<string, object>();
            whereClause.Add("requestid", requestId);

            int rowsAffected = await UpdateAsync(updateFields, whereClause);

            // Return true if update was successful
            return rowsAffected > 0;
        }

        // Check if a genre name is already requested and pending
        // Uses SQL COUNT with case-insensitive comparison - much faster than looping
        public async Task<bool> IsGenreAlreadyRequestedAsync(string genreName)
        {
            // Use SQL to check if any pending request has this genre name (case-insensitive)
            // COUNT(*) returns the number of matching rows
            string sql = "SELECT COUNT(*) FROM genre_requests WHERE status = 'pending' AND LOWER(genre_name) = LOWER(@name)";

            // Add parameter manually
            cmd.Parameters.Clear();
            var param = cmd.CreateParameter();
            param.ParameterName = "@name";
            param.Value = genreName.Trim();
            cmd.Parameters.Add(param);

            // Execute the COUNT query
            object result = await ExecScalarAsync(sql);

            int count = 0;
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }

            // If count > 0, then a request with this name already exists
            return count > 0;
        }

        // Get count of pending requests (useful for admin dashboard notifications)
        // Pure SQL COUNT - no data transfer needed!
        public async Task<int> GetPendingRequestCountAsync()
        {
            // Use SQL COUNT to get number of pending requests
            // This is MUCH faster than loading all requests and counting in C#
            string sql = "SELECT COUNT(*) FROM genre_requests WHERE status = 'pending'";

            cmd.Parameters.Clear();

            object result = await ExecScalarAsync(sql);

            int count = 0;
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }

            return count;
        }

        
    }
}