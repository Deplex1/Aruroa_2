using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace AuroraAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly string _connectionString =
            "server=localhost;database=auroradb;user=root;password=YOUR_PASSWORD";

        // Create a class to accept POST data
        public class UserLoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public IActionResult GetGenreUsagePerUser([FromBody] UserLoginRequest login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Username or password missing.");
            }

            List<UserGenreStats> result = new List<UserGenreStats>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // First, check if username and password match
                string authQuery = "SELECT userid FROM users WHERE username = @username AND password = @password;";
                MySqlCommand authCmd = new MySqlCommand(authQuery, conn);
                
                
                //hash
                using SHA256 sha = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(login.Password);
                byte[] hash = sha.ComputeHash(bytes);
                login.Password = Convert.ToBase64String(hash);

                authCmd.Parameters.AddWithValue("@username", login.Username);
                authCmd.Parameters.AddWithValue("@password", login.Password);



                object userIdObj = authCmd.ExecuteScalar();
                if (userIdObj == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                int userId = Convert.ToInt32(userIdObj);

                // Now get genre usage for this user only
                string query = @"
                    SELECT 
                        u.userid,
                        u.username,
                        g.genreid,
                        g.name AS genre_name,
                        COUNT(*) AS genre_count
                    FROM users u
                    JOIN playlists p ON p.userid = u.userid
                    JOIN playlistsongs ps ON ps.playlistid = p.playlistid
                    JOIN songs s ON s.songid = ps.songid
                    JOIN song_genres sg ON sg.songid = s.songid
                    JOIN genres g ON g.genreid = sg.genreid
                    WHERE u.userid = @userid
                    GROUP BY u.userid, u.username, g.genreid, g.name
                    ORDER BY genre_count DESC;";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userid", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserGenreStats stat = new UserGenreStats();
                        stat.UserId = reader.GetInt32("userid");
                        stat.Username = reader.GetString("username");
                        stat.GenreId = reader.GetInt32("genreid");
                        stat.GenreName = reader.GetString("genre_name");
                        stat.Count = reader.GetInt32("genre_count");

                        result.Add(stat);
                    }
                }
            }

            return Ok(result);
        }
    }

    
    public class UserGenreStats
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int Count { get; set; }
    }
}