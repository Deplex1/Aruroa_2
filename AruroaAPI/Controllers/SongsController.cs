using Microsoft.AspNetCore.Mvc;
using Models;
using DBL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AruroaAPI.Controllers
{
    /// <summary>
    /// API Controller for managing songs
    /// Provides endpoints for CRUD operations on songs
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly SongDB _songDB;

        public SongsController()
        {
            _songDB = new SongDB();
        }

        /// <summary>
        /// Get all songs
        /// </summary>
        /// <returns>List of all songs</returns>
        [HttpGet]
        public async Task<ActionResult<List<Song>>> GetAllSongs()
        {
            try
            {
                var songs = await _songDB.SelectAllSongsAsync();
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific song by ID
        /// </summary>
        /// <param name="id">Song ID</param>
        /// <returns>Song object</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Song>> GetSongById(int id)
        {
            try
            {
                var song = await _songDB.SelectByIdAsync(id);
                if (song == null)
                {
                    return NotFound(new { message = $"Song with ID {id} not found" });
                }
                return Ok(song);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving song", error = ex.Message });
            }
        }

        /// <summary>
        /// Search songs by title
        /// </summary>
        /// <param name="searchText">Search text</param>
        /// <returns>List of matching songs</returns>
        [HttpGet("search")]
        public async Task<ActionResult<List<Song>>> SearchSongs([FromQuery] string searchText)
        {
            try
            {
                var songs = await _songDB.SearchSongsAsync(searchText);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get songs by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of songs uploaded by the user</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Song>>> GetSongsByUserId(int userId)
        {
            try
            {
                var songs = await _songDB.SelectSongsByUserIDAsync(userId);
                if (songs == null || songs.Count == 0)
                {
                    return NotFound(new { message = $"No songs found for user {userId}" });
                }
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get popular songs (top 10 by plays)
        /// </summary>
        /// <returns>List of popular songs</returns>
        [HttpGet("popular")]
        public async Task<ActionResult<List<Song>>> GetPopularSongs()
        {
            try
            {
                var songs = await _songDB.GetPopularSongsAsync();
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving popular songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get new songs (latest 10 uploads)
        /// </summary>
        /// <returns>List of new songs</returns>
        [HttpGet("new")]
        public async Task<ActionResult<List<Song>>> GetNewSongs()
        {
            try
            {
                var songs = await _songDB.GetNewSongsAsync();
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving new songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a new song
        /// </summary>
        /// <param name="song">Song object</param>
        /// <returns>Created song with ID</returns>
        [HttpPost]
        public async Task<ActionResult<Song>> AddSong([FromBody] Song song)
        {
            try
            {
                if (song == null)
                {
                    return BadRequest(new { message = "Song data is required" });
                }

                var newSong = await _songDB.InsertSongAsync(song);
                return CreatedAtAction(nameof(GetSongById), new { id = newSong.songID }, newSong);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding song", error = ex.Message });
            }
        }

        /// <summary>
        /// Increment play count for a song
        /// </summary>
        /// <param name="id">Song ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/play")]
        public async Task<ActionResult> AddPlay(int id)
        {
            try
            {
                await _songDB.AddPlayAsync(id);
                return Ok(new { message = "Play count incremented successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error incrementing play count", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a song
        /// </summary>
        /// <param name="id">Song ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSong(int id)
        {
            try
            {
                var result = await _songDB.DeleteSongAsync(id);
                if (result == 0)
                {
                    return NotFound(new { message = $"Song with ID {id} not found" });
                }
                return Ok(new { message = "Song deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting song", error = ex.Message });
            }
        }

        /// <summary>
        /// Filter songs by genres (AND logic)
        /// </summary>
        /// <param name="genreIds">List of genre IDs</param>
        /// <returns>List of songs matching all genres</returns>
        [HttpPost("filter-by-genres")]
        public async Task<ActionResult<List<Song>>> FilterByGenres([FromBody] List<int> genreIds)
        {
            try
            {
                if (genreIds == null || genreIds.Count == 0)
                {
                    return BadRequest(new { message = "Genre IDs are required" });
                }

                var songs = await _songDB.FilterByGenresAsync(genreIds);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error filtering songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get songs not in a specific playlist
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <returns>List of songs not in the playlist</returns>
        [HttpGet("not-in-playlist/{playlistId}")]
        public async Task<ActionResult<List<Song>>> GetSongsNotInPlaylist(int playlistId)
        {
            try
            {
                var songs = await _songDB.GetSongsNotInPlaylistAsync(playlistId);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving songs", error = ex.Message });
            }
        }
    }
}
