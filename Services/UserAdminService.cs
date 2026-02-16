using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBL;
using Models;

namespace Services
{
    /// <summary>
    /// Service class that handles all user administration business logic.
    /// Manages user loading, admin status toggling, and user deletion for admin users.
    /// </summary>
    public class UserAdminService
    {
        /// <summary>
        /// Loads all users from the database.
        /// Returns a list of User objects.
        /// </summary>
        public async Task<List<User>> LoadAllUsersAsync()
        {
            try
            {
                UserDB uDB = new UserDB();
                List<User> users = await uDB.GetAllUsersAsync();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading users: " + ex.Message);
                return new List<User>();
            }
        }

        /// <summary>
        /// Validates if a user can be deleted.
        /// Returns an error message if validation fails, or null if valid.
        /// </summary>
        public string ValidateUserDeletion(User targetUser, User currentUser)
        {
            if (targetUser == null)
            {
                return "Invalid user selection!";
            }

            if (targetUser.userid == currentUser.userid)
            {
                return "You cannot delete yourself!";
            }

            return null;
        }

        /// <summary>
        /// Deletes a user from the database.
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                UserDB uDB = new UserDB();
                await uDB.DeleteUserAsync(userId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates if a user's admin status can be toggled.
        /// Returns an error message if validation fails, or null if valid.
        /// </summary>
        public string ValidateAdminToggle(User targetUser, User currentUser)
        {
            if (targetUser == null)
            {
                return "Invalid user selection!";
            }

            if (targetUser.userid == currentUser.userid)
            {
                return "You cannot change your own admin status!";
            }

            return null;
        }

        /// <summary>
        /// Toggles a user's admin status (admin to regular user, or vice versa).
        /// Returns true if successful, false if failed.
        /// </summary>
        public async Task<bool> ToggleAdminStatusAsync(User targetUser)
        {
            try
            {
                Dictionary<string, object> fieldsToUpdate = new Dictionary<string, object>();

                if (targetUser.IsAdmin == 1)
                {
                    fieldsToUpdate.Add("IsAdmin", 0);
                }
                else
                {
                    fieldsToUpdate.Add("IsAdmin", 1);
                }

                UserDB uDB = new UserDB();
                await uDB.UpdateUserAsync(targetUser.userid, fieldsToUpdate);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating user: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets a user from a list by index.
        /// Returns the User object if index is valid, null otherwise.
        /// </summary>
        public User GetUserByIndex(int index, List<User> users)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= users.Count)
            {
                return null;
            }

            return users[index];
        }
    }
}
