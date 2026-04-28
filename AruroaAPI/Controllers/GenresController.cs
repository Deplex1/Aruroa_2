using Microsoft.AspNetCore.Mvc;
using Models;
using DBL;

namespace AruroaAPI.Controllers
{
    /// <summary>
    /// API Controller for managing genres
    /// Provides endpoints for genre operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly GenreDB _genreDB;
        private readonly GenreRequestDB _genreRequestDB;
        private readonly SongGenreDB _songGenreDB;

        public GenresController()
        {
            _genreDB = new GenreDB();
            _genreRequestDB = new GenreRequestDB();
            _songGenreDB = new SongGenreDB();
        }

        /// <summary>
        /// Get all genres
        /// </summary>
        /// <returns>List of all genres</returns>
        [HttpGet]
        public async Task<ActionResult<List<Genre>>> GetAllGenres()
        {
            try
            {
                var genres = await _genreDB.SelectAllGenresAsync();
                return Ok(genres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving genres", error = ex.Message });
            }
        }

        /// <summary>
        /// Get genre by ID
        /// </summary>
        /// <param name="id">Genre ID</param>
        /// <returns>Genre object</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenreById(int id)
        {
            try
            {
                var genres = await _genreDB.SelectAllGenresAsync();
                var genre = genres.FirstOrDefault(g => g.genreid == id);
                if (genre == null)
                {
                    return NotFound(new { message = $"Genre with ID {id} not found" });
                }
                return Ok(genre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving genre", error = ex.Message });
            }
        }

        /// <summary>
        /// Get genres for a specific song
        /// </summary>
        /// <param name="songId">Song ID</param>
        /// <returns>List of genres for the song</returns>
        [HttpGet("song/{songId}")]
        public async Task<ActionResult<List<Genre>>> GetGenresBySongId(int songId)
        {
            try
            {
                var songGenres = await _songGenreDB.GetGenresForSongAsync(songId);
                var genreIds = songGenres.Select(sg => sg.genreid).ToList();
                
                var allGenres = await _genreDB.SelectAllGenresAsync();
                var genres = allGenres.Where(g => genreIds.Contains(g.genreid)).ToList();
                
                return Ok(genres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving song genres", error = ex.Message });
            }
        }

        /// <summary>
        /// Add new genre (Admin only)
        /// </summary>
        /// <param name="genre">Genre object</param>
        /// <returns>Created genre ID</returns>
        [HttpPost]
        public async Task<ActionResult> AddGenre([FromBody] Genre genre)
        {
            try
            {
                if (genre == null || string.IsNullOrEmpty(genre.name))
                {
                    return BadRequest(new { message = "Genre name is required" });
                }

                int newId = await _genreDB.AddGenreAsync(genre.name);
                return Ok(new { genreid = newId, message = "Genre added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding genre", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete genre (Admin only)
        /// </summary>
        /// <param name="id">Genre ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGenre(int id)
        {
            try
            {
                int result = await _genreDB.DeleteGenreAsync(id);
                if (result == 0)
                {
                    return NotFound(new { message = $"Genre with ID {id} not found" });
                }
                return Ok(new { message = "Genre deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting genre", error = ex.Message });
            }
        }

        /// <summary>
        /// Request new genre
        /// </summary>
        /// <param name="request">Genre request</param>
        /// <returns>Created genre request ID</returns>
        [HttpPost("request")]
        public async Task<ActionResult> RequestGenre([FromBody] GenreRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.genre_name))
                {
                    return BadRequest(new { message = "Genre name is required" });
                }

                int newId = await _genreRequestDB.AddRequestAsync(request.userid, request.genre_name);
                return Ok(new { requestid = newId, message = "Genre request submitted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting genre request", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all pending genre requests (Admin only)
        /// </summary>
        /// <returns>List of pending requests</returns>
        [HttpGet("requests/pending")]
        public async Task<ActionResult<List<GenreRequest>>> GetPendingRequests()
        {
            try
            {
                var requests = await _genreRequestDB.GetPendingRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving genre requests", error = ex.Message });
            }
        }

        /// <summary>
        /// Approve genre request (Admin only)
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="adminId">Admin user ID</param>
        /// <returns>Success message</returns>
        [HttpPost("requests/{requestId}/approve")]
        public async Task<ActionResult> ApproveGenreRequest(int requestId, [FromQuery] int adminId)
        {
            try
            {
                bool result = await _genreRequestDB.ApproveRequestAsync(requestId, adminId);
                if (!result)
                {
                    return NotFound(new { message = $"Genre request with ID {requestId} not found" });
                }
                return Ok(new { message = "Genre request approved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error approving genre request", error = ex.Message });
            }
        }

        /// <summary>
        /// Reject genre request (Admin only)
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="adminId">Admin user ID</param>
        /// <returns>Success message</returns>
        [HttpPost("requests/{requestId}/reject")]
        public async Task<ActionResult> RejectGenreRequest(int requestId, [FromQuery] int adminId)
        {
            try
            {
                bool result = await _genreRequestDB.RejectRequestAsync(requestId, adminId);
                if (!result)
                {
                    return NotFound(new { message = $"Genre request with ID {requestId} not found" });
                }
                return Ok(new { message = "Genre request rejected successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error rejecting genre request", error = ex.Message });
            }
        }

        /// <summary>
        /// Add genre to song
        /// </summary>
        /// <param name="songId">Song ID</param>
        /// <param name="genreId">Genre ID</param>
        /// <returns>Success message</returns>
        [HttpPost("song/{songId}/genre/{genreId}")]
        public async Task<ActionResult> AddGenreToSong(int songId, int genreId)
        {
            try
            {
                int result = await _songGenreDB.AddSongGenreAsync(songId, genreId);
                if (result > 0)
                {
                    return Ok(new { message = "Genre added to song successfully" });
                }
                return StatusCode(500, new { message = "Failed to add genre to song" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding genre to song", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove genre from song
        /// </summary>
        /// <param name="songId">Song ID</param>
        /// <param name="genreId">Genre ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("song/{songId}/genre/{genreId}")]
        public async Task<ActionResult> RemoveGenreFromSong(int songId, int genreId)
        {
            try
            {
                // Delete all genres for song then check if any were deleted
                var genresBefore = await _songGenreDB.GetGenresForSongAsync(songId);
                var hadGenre = genresBefore.Any(sg => sg.genreid == genreId);
                
                if (!hadGenre)
                {
                    return NotFound(new { message = "Genre not found for this song" });
                }
                
                // Delete by creating a filter
                await _songGenreDB.DeleteGenresForSongAsync(songId);
                
                // Re-add all genres except the one we want to remove
                foreach (var sg in genresBefore.Where(sg => sg.genreid != genreId))
                {
                    await _songGenreDB.AddSongGenreAsync(sg.songid, sg.genreid);
                }
                
                return Ok(new { message = "Genre removed from song successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error removing genre from song", error = ex.Message });
            }
        }
    }
}
