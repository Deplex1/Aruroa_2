using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    // Service class that handles all genre request business logic
    // Keeps the Razor page clean with only UI logic
    public class GenreRequestService
    {
        // Get all pending genre requests for admin to review
        // Returns newest requests first (SQL handles sorting)
        public async Task<List<GenreRequest>> GetPendingRequestsAsync()
        {
            GenreRequestDB requestDB = new GenreRequestDB();
            List<GenreRequest> requests = await requestDB.GetPendingRequestsAsync();
            return requests;
        }

        // Get the total count of pending requests
        // Used to show notification badge on admin dashboard
        public async Task<int> GetPendingCountAsync()
        {
            GenreRequestDB requestDB = new GenreRequestDB();
            int count = await requestDB.GetPendingRequestCountAsync();
            return count;
        }

        // Approve a genre request
        // This will create the genre AND mark request as approved
        // Returns true if successful, false if something went wrong
        public async Task<bool> ApproveRequestAsync(int requestId, int adminUserId)
        {
            GenreRequestDB requestDB = new GenreRequestDB();
            bool success = await requestDB.ApproveRequestAsync(requestId, adminUserId);
            return success;
        }

        // Reject a genre request
        // Marks request as rejected without creating the genre
        // Returns true if successful, false if something went wrong
        public async Task<bool> RejectRequestAsync(int requestId, int adminUserId)
        {
            GenreRequestDB requestDB = new GenreRequestDB();
            bool success = await requestDB.RejectRequestAsync(requestId, adminUserId);
            return success;
        }

        // Get all requests made by a specific user
        // Used to show user their own request history
        public async Task<List<GenreRequest>> GetUserRequestsAsync(int userId)
        {
            GenreRequestDB requestDB = new GenreRequestDB();
            List<GenreRequest> requests = await requestDB.GetUserRequestsAsync(userId);
            return requests;
        }

        // Validate if an admin can perform actions
        // Returns error message if invalid, null if valid
        public string ValidateAdminAccess(User user)
        {
            // Check if user is logged in
            if (user == null)
            {
                return "You must be logged in to access this page.";
            }

            // Check if user is an admin
            if (user.IsAdmin != 1)
            {
                return "You do not have permission to access this page.";
            }

            // All checks passed
            return null;
        }

        // Remove an approved or rejected request from the pending list
        // Used to update UI after admin takes action
        public void RemoveRequestFromList(List<GenreRequest> requests, int requestId)
        {
            // Loop through requests and find the one to remove
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].requestid == requestId)
                {
                    requests.RemoveAt(i);
                    break;
                }
            }
        }
    }
}