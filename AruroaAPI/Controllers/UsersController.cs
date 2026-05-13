using Microsoft.AspNetCore.Mvc;
using Models;
using DBL;
using System.Security.Cryptography;
using System.Text;

namespace AruroaAPI.Controllers
{
    /// <summary>
    /// API Controller for managing users
    /// Provides endpoints for user authentication and management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserDB _userDB;

        public UsersController()
        {
            _userDB = new UserDB();
        }

        /// <summary>
        /// User login request model
        /// </summary>
        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }

        /// <summary>
        /// User registration request model
        /// </summary>
        public class RegisterRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string Email { get; set; } = "";
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>User object if successful</returns>
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // Hash the password
                string hashedPassword = HashPassword(request.Password);

                var user = await _userDB.LoginAsync(request.Username, hashedPassword);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Don't return password in response
                user.password = "";
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during login", error = ex.Message });
            }
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="request">Registration data</param>
        /// <returns>Created user</returns>
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // Check if username already exists
                var existingUsers = await _userDB.GetAllUsersAsync();
                var existingUser = existingUsers.FirstOrDefault(u => u.username == request.Username);
                if (existingUser != null)
                {
                    return Conflict(new { message = "Username already exists" });
                }

                // Hash the password
                string hashedPassword = HashPassword(request.Password);

                var newUser = new User
                {
                    username = request.Username,
                    password = hashedPassword,
                    email = request.Email,
                    IsAdmin = 0
                };

                // Use RegisterAsync which returns a tuple
                var result = await _userDB.RegisterAsync(request.Username, request.Password, request.Email);
                
                if (result.Success)
                {
                    return Ok(new { message = "User registered successfully" });
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during registration", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _userDB.GetAllUsersAsync();
                
                // Remove passwords from response
                foreach (var user in users)
                {
                    user.password = "";
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User object</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userDB.SelectByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                // Don't return password
                user.password = "";
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User statistics</returns>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult> GetUserStats(int id)
        {
            try
            {
                var songDB = new SongDB();
                var stats = await songDB.GetUserStatsAsync(id);

                return Ok(new
                {
                    userId = id,
                    totalSongs = stats.totalSongs,
                    totalPlays = stats.totalPlays
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <param name="requestingUserId">ID of user making the request (from header)</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id, [FromHeader(Name = "X-User-Id")] int requestingUserId)
        {
            try
            {
                // 1. Verify requesting user exists and is admin
                var requestingUser = await _userDB.SelectByIdAsync(requestingUserId);
                
                if (requestingUser == null)
                {
                    return Unauthorized(new { message = "Authentication required. User not found." });
                }
                
                if (requestingUser.IsAdmin == 0)
                {
                    return StatusCode(403, new { message = "Forbidden. Admin access required." });
                }
                
                // 2. Prevent self-deletion
                if (id == requestingUserId)
                {
                    return BadRequest(new { message = "Cannot delete yourself" });
                }
                
                // 3. Proceed with deletion
                int result = await _userDB.DeleteUserAsync(id);
                if (result == 0)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }
                
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
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
}
