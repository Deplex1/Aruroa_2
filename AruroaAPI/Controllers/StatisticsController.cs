using Microsoft.AspNetCore.Mvc;
using DBL;
using Models;
using System.Security.Cryptography;
using System.Text;

namespace AruroaAPI.Controllers
{
    /// <summary>
    /// API Controller for system statistics
    /// Provides endpoints for various statistical data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly StatsDB _statsDB;
        private readonly UserDB _userDB;

        public StatisticsController()
        {
            _statsDB = new StatsDB();
            _userDB = new UserDB();
        }

        /// <summary>
        /// User login request model
        /// </summary>
        public class UserLoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }

        /// <summary>
        /// Get overall system statistics
        /// </summary>
        /// <returns>System-wide statistics</returns>
        [HttpGet("system")]
        public async Task<ActionResult<SiteStats>> GetSystemStats()
        {
            try
            {
                var stats = await _statsDB.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving system stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Get genre usage statistics for authenticated user
        /// Requires username and password for authentication
        /// </summary>
        /// <param name="login">User credentials</param>
        /// <returns>Genre usage statistics for the user</returns>
        [HttpPost("genre-usage")]
        public async Task<ActionResult<List<UserGenreStats>>> GetGenreUsagePerUser([FromBody] UserLoginRequest login)
        {
            try
            {
                if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // Hash password
                string hashedPassword = HashPassword(login.Password);

                // Authenticate user
                var user = await _userDB.LoginAsync(login.Username, hashedPassword);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Get genre usage stats for this user
                // Since we don't have this method, we'll return empty list for now
                var stats = new List<UserGenreStats>();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving genre usage stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    /// <summary>
    /// Model for user genre statistics
    /// </summary>
    public class UserGenreStats
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public int GenreId { get; set; }
        public string GenreName { get; set; } = "";
        public int Count { get; set; }
    }
}
