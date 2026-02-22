using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace BasicAPIpost.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private string connectionString = "server=localhost;database=auroradb;uid=root;pwd=1234;";

        // 1. Get how many times each genre appears in a playlist
        [HttpGet("{playlistId}")]
        public List<GenreCountDto> GenreUsageInPlaylist(int playlistId)
        {
            List<GenreCountDto> result = new List<GenreCountDto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT g.name AS GenreName, COUNT(*) AS UsageCount
                    FROM playlistsongs ps
                    JOIN song_genres sg ON ps.songid = sg.songid
                    JOIN genres g ON sg.genreid = g.genreid
                    WHERE ps.playlistid = @playlistId
                    GROUP BY g.name
                    ORDER BY UsageCount DESC;
                ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@playlistId", playlistId);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GenreCountDto dto = new GenreCountDto();
                    dto.GenreName = reader.GetString("GenreName");
                    dto.Count = reader.GetInt32("UsageCount");

                    result.Add(dto);
                }
            }

            return result;
        }

        // 2. Most popular genres in the whole system
        [HttpGet]
        public List<GenreCountDto> MostPopularGenres()
        {
            List<GenreCountDto> result = new List<GenreCountDto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT g.name AS GenreName, COUNT(*) AS UsageCount
                    FROM song_genres sg
                    JOIN genres g ON sg.genreid = g.genreid
                    GROUP BY g.name
                    ORDER BY UsageCount DESC;
                ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GenreCountDto dto = new GenreCountDto();
                    dto.GenreName = reader.GetString("GenreName");
                    dto.Count = reader.GetInt32("UsageCount");

                    result.Add(dto);
                }
            }

            return result;
        }

        // 3. Top played songs
        [HttpGet]
        public List<SongStatsDto> MostPlayedSongs()
        {
            List<SongStatsDto> result = new List<SongStatsDto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT title, plays
                    FROM songs
                    ORDER BY plays DESC
                    LIMIT 10;
                ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    SongStatsDto dto = new SongStatsDto();
                    dto.Title = reader.GetString("title");
                    dto.Plays = reader.GetInt32("plays");

                    result.Add(dto);
                }
            }

            return result;
        }

        // 4. Average rating per song
        [HttpGet("{songId}")]
        public double AverageRating(int songId)
        {
            double avg = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT AVG(rating) AS AvgRating
                    FROM ratings
                    WHERE songid = @songId;
                ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@songId", songId);

                object value = cmd.ExecuteScalar();

                if (value != DBNull.Value)
                {
                    avg = Convert.ToDouble(value);
                }
            }

            return avg;
        }
    }

    public class GenreCountDto
    {
        public string GenreName { get; set; }
        public int Count { get; set; }
    }

    public class SongStatsDto
    {
        public string Title { get; set; }
        public int Plays { get; set; }
    }
}