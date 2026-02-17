using Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBL
{
    public class UserDB : BaseDB<User>
    {
        protected override string GetTableName()
        {
            return "users";
        }

        protected override string GetPrimaryKeyName()
        {
            return "userid";
        }

        protected async override Task<User> CreateModelAsync(object[] row)
        {
            User u = new User();
            u.userid = int.Parse(row[0].ToString());
            u.username = row[1].ToString();
            u.password = row[2].ToString();
            u.email = row[3].ToString();
            u.profilepicture = row[4] as byte[];
            u.IsAdmin = int.Parse(row[5].ToString());
            u.ResetCode = row[6]?.ToString();
            return u;
        }

        // ------------------- REGISTRATION -------------------
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                return (false, "All fields are required.");

            if (!IsValidEmail(email))
                return (false, "Invalid email format.");

            // Check for duplicates
            if (await UsernameExistsAsync(username))
                return (false, "Username already taken.");

            if (await EmailExistsAsync(email))
                return (false, "Email already registered.");

            // Hash password
            string hashedPassword = HashPassword(password);

            // Insert user
            Dictionary<string, object> values = new Dictionary<string, object>
            {
                { "username", username },
                { "password", hashedPassword },
                { "email", email },
                { "IsAdmin", 0 }
            };

            int rows = await InsertAsync(values);

            if (rows == 1)
                return (true, "Registration successful.");

            return (false, "Registration failed due to unknown error.");
        }

        // ------------------- LOGIN -------------------
        public async Task<User> LoginAsync(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            string sql = "SELECT * FROM users WHERE username=@username AND password=@password";
            var parameters = new Dictionary<string, object>
            {
                { "username", username },
                { "password", hashedPassword }
            };

            List<User> list = await SelectAllAsync(sql, parameters);
            if (list.Count == 1)
                return list[0];

            return null;
        }

        // ------------------- HELPERS -------------------
        private async Task<bool> UsernameExistsAsync(string username)
        {
            string sql = "SELECT * FROM users WHERE username=@username";
            List<User> list = await SelectAllAsync(sql, new Dictionary<string, object> { { "username", username } });
            return list.Count > 0;
        }

        private async Task<bool> EmailExistsAsync(string email)
        {
            string sql = "SELECT * FROM users WHERE email=@email";
            List<User> list = await SelectAllAsync(sql, new Dictionary<string, object> { { "email", email } });
            return list.Count > 0;
        }

        private bool IsValidEmail(string email)
        {
            // Simple regex for email validation
            string pattern = @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ------------------- UPDATES -------------------

        public async Task<int> UpdateUserAsync(int userId, Dictionary<string, object> fieldsToUpdate)
        {
            // 'fieldsToUpdate' is a dictionary like { "profilepicture", imageBytes }
            Dictionary<string, object> where = new Dictionary<string, object>
            {
                { "userid", userId }
            };

            // Call the BaseDB.UpdateAsync method
            return await UpdateAsync(fieldsToUpdate, where);
        }

        // ------------------- WRAPPERS -------------------

        public async Task<List<User>> GetAllUsersAsync()
        {
            // Calls the protected SelectAllAsync() from BaseDB
            return await SelectAllAsync();
        }

        public async Task<User> SelectByIdAsync(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userid", userId);

            List<User> result = await SelectAllAsync(parameters);

            if (result.Count == 1)
            {
                return result[0];
            }

            return null;
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            // Prepare the filter
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter.Add("userid", userId);

            // Calls the protected DeleteAsync() from BaseDB
            return await DeleteAsync(filter);
        }
    }
}
