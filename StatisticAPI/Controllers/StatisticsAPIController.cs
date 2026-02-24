using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace AuroraAPI.Controllers
{
    [Route("api/StatisticsAPI")]
    [ApiController]
    public class StatisticsAPIController : ControllerBase
    {
        private readonly ILogger<StatisticsAPIController> _logger;

        private readonly string _connectionString =
            "server=localhost;database=auroradb;user=root;password=josh17rog";

        public StatisticsAPIController(ILogger<StatisticsAPIController> logger)
        {
            _logger = logger;
        }

        // ----------------------------------------------------
        // DTO that receives username from POST body
        // ----------------------------------------------------
        public class UsernameDTO
        {
            public string Username { get; set; }
        }

        // ----------------------------------------------------
        // 1. GENRE COUNT PER PLAYLIST
        // ----------------------------------------------------
        [HttpPost("GenresPerPlaylist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenresPerPlaylist([FromBody] UsernameDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("Username is required");
            }

            List<object> result = new List<object>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT
                        p.playlistid,
                        p.name AS playlist_name,
                        g.name AS genre_name,
                        COUNT(*) AS times_appeared
                    FROM users u
                    INNER JOIN playlists p ON p.userid = u.userid
                    INNER JOIN playlistsongs ps ON ps.playlistid = p.playlistid
                    INNER JOIN songs s ON s.songid = ps.songid
                    INNER JOIN song_genres sg ON sg.songid = s.songid
                    INNER JOIN genres g ON g.genreid = sg.genreid
                    WHERE u.username = @username
                    GROUP BY p.playlistid, p.name, g.name
                    ORDER BY p.playlistid, times_appeared DESC;
                ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", dto.Username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                PlaylistId = reader.GetInt32(reader.GetOrdinal("playlistid")),
                                PlaylistName = reader.GetString(reader.GetOrdinal("playlist_name")),
                                Genre = reader.GetString(reader.GetOrdinal("genre_name")),
                                Count = reader.GetInt32(reader.GetOrdinal("times_appeared"))
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }

        // ----------------------------------------------------
        // 2. GENRE COUNT ACROSS ALL PLAYLISTS
        // ----------------------------------------------------
        [HttpPost("GenresTotal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenresTotal([FromBody] UsernameDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("Username is required");
            }

            List<object> result = new List<object>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT
                        g.name AS genre_name,
                        COUNT(*) AS times_appeared
                    FROM users u
                    INNER JOIN playlists p ON p.userid = u.userid
                    INNER JOIN playlistsongs ps ON ps.playlistid = p.playlistid
                    INNER JOIN songs s ON s.songid = ps.songid
                    INNER JOIN song_genres sg ON sg.songid = s.songid
                    INNER JOIN genres g ON g.genreid = sg.genreid
                    WHERE u.username = @username
                    GROUP BY g.name
                    ORDER BY times_appeared DESC;
                ";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", dto.Username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                Genre = reader.GetString(reader.GetOrdinal("genre_name")),
                                Count = reader.GetInt32(reader.GetOrdinal("times_appeared"))
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}