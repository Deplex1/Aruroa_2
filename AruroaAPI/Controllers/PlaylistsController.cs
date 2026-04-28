using Microsoft.AspNetCore.Mvc;
using Models;
using DBL;

namespace AruroaAPI.Controllers
{
    /// <summary>
    /// API Controller for managing playlists
    /// Provides endpoints for playlist CRUD operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly PlaylistDB _playlistDB;
        private readonly PlaylistSongDB _playlistSongDB;

        public PlaylistsController()
        {
            _playlistDB = new PlaylistDB();
            _playlistSongDB = new PlaylistSongDB();
        }

        /// <summary>
        /// Get playlists by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's playlists</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Playlist>>> GetUserPlaylists(int userId)
        {
            try
            {
                var playlists = await _playlistDB.GetUserPlaylistsAsync(userId);
                if (playlists == null || playlists.Count == 0)
                {
                    return NotFound(new { message = $"No playlists found for user {userId}" });
                }
                return Ok(playlists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user playlists", error = ex.Message });
            }
        }

        /// <summary>
        /// Get playlist by ID
        /// </summary>
        /// <param name="id">Playlist ID</param>
        /// <returns>Playlist object</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> GetPlaylistById(int id)
        {
            try
            {
                var playlist = await _playlistDB.SelectByIdAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { message = $"Playlist with ID {id} not found" });
                }
                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving playlist", error = ex.Message });
            }
        }

        /// <summary>
        /// Get songs in a playlist
        /// </summary>
        /// <param name="id">Playlist ID</param>
        /// <returns>List of songs in the playlist</returns>
        [HttpGet("{id}/songs")]
        public async Task<ActionResult<List<Song>>> GetPlaylistSongs(int id)
        {
            try
            {
                var songs = await _playlistSongDB.GetSongsInPlaylistAsync(id);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving playlist songs", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new playlist
        /// </summary>
        /// <param name="playlist">Playlist object</param>
        /// <returns>Created playlist ID</returns>
        [HttpPost]
        public async Task<ActionResult> CreatePlaylist([FromBody] Playlist playlist)
        {
            try
            {
                if (playlist == null || string.IsNullOrEmpty(playlist.name))
                {
                    return BadRequest(new { message = "Playlist name is required" });
                }

                int newId = await _playlistDB.CreatePlaylistAsync(playlist);
                return Ok(new { playlistid = newId, message = "Playlist created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating playlist", error = ex.Message });
            }
        }

        /// <summary>
        /// Add song to playlist
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <param name="songId">Song ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<ActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            try
            {
                // Check if song already exists in playlist
                bool exists = await _playlistSongDB.SongExistsInPlaylistAsync(playlistId, songId);
                if (exists)
                {
                    return Conflict(new { message = "Song already exists in this playlist" });
                }

                // Get max position and add 1
                int maxPosition = await _playlistSongDB.GetMaxPositionInPlaylistAsync(playlistId);
                int newPosition = maxPosition + 1;

                int result = await _playlistSongDB.AddSongToPlaylistAsync(playlistId, songId, newPosition);
                if (result > 0)
                {
                    return Ok(new { message = "Song added to playlist successfully" });
                }
                return StatusCode(500, new { message = "Failed to add song to playlist" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding song to playlist", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove song from playlist
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <param name="songId">Song ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<ActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            try
            {
                int result = await _playlistSongDB.RemoveSongFromPlaylistAsync(playlistId, songId);
                if (result == 0)
                {
                    return NotFound(new { message = "Song not found in playlist" });
                }
                return Ok(new { message = "Song removed from playlist successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error removing song from playlist", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete playlist
        /// </summary>
        /// <param name="id">Playlist ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePlaylist(int id)
        {
            try
            {
                int result = await _playlistDB.DeletePlaylistAsync(id);
                if (result == 0)
                {
                    return NotFound(new { message = $"Playlist with ID {id} not found" });
                }
                return Ok(new { message = "Playlist deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting playlist", error = ex.Message });
            }
        }
    }
}
