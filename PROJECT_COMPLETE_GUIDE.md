# Aruroa Music Platform - Complete Project Guide

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Technologies Used](#technologies-used)
4. [Project Structure](#project-structure)
5. [Database Layer (DBL)](#database-layer)
6. [Business Logic Layer (Services)](#business-logic-layer)
7. [API Layer (AruroaAPI)](#api-layer)
8. [Web Application (AruroaBlazor)](#web-application)
9. [Desktop Application (AruroaMusicPlayer)](#desktop-application)
10. [Security Concerns](#security-concerns)

---

## Project Overview

**Aruroa** is a full-stack music streaming platform with three main components:
1. **REST API** - Backend service for data management
2. **Blazor Web App** - Admin panel and user interface
3. **WinForms Desktop App** - Music player application

### Key Features
- User authentication and authorization
- Song upload and management
- Playlist creation and management
- Genre management with user requests
- Rating system
- Statistics dashboard with charts
- Audio playback
- Admin panel for content moderation

---

## Architecture

### Three-Tier Architecture

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (Blazor Web App + WinForms Desktop)    │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         API Layer (REST API)            │
│         (AruroaAPI Controllers)         │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Business Logic Layer               │
│         (Services Project)              │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Data Access Layer                  │
│         (DBL Project)                   │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Database (MySQL)                │
│         (auroradb)                      │
└─────────────────────────────────────────┘
```

---

## Technologies Used

### Backend
- **C# .NET 9.0** - Programming language and framework
- **ASP.NET Core** - Web API framework
- **MySQL** - Relational database
- **MySql.Data** - MySQL connector for .NET

### Frontend
- **Blazor Server** - Web UI framework (C# instead of JavaScript)
- **Bootstrap 5** - CSS framework for styling
- **ApexCharts** - Interactive charts library
- **WinForms** - Desktop application framework

### Audio
- **NAudio** - Audio playback library for WinForms

### Tools
- **Swagger/OpenAPI** - API documentation
- **Visual Studio 2022** - IDE

---

## Project Structure

```
Aruroa/
├── Models/              # Data models (shared across all projects)
├── DBL/                 # Database Layer (data access)
├── Services/            # Business Logic Layer
├── AruroaAPI/           # REST API
├── AruroaBlazor/        # Web Application
├── AruroaMusicPlayer/   # Desktop Application
└── SongManagment/       # Helper utilities
```

---

## Database Layer (DBL)

### Purpose
Handles ALL database operations - no SQL queries exist outside this layer.

### Key Files

#### **DB.cs** - Base Database Class
```csharp
public class DB
{
    private static string connectionString;
    protected MySqlConnection connection;
    protected MySqlCommand command;
    protected MySqlDataReader reader;
}
```
**What it does:**
- Stores the MySQL connection string
- Provides connection management
- Base class for all database classes

#### **UserDB.cs** - User Database Operations
```csharp
public async Task<User> LoginAsync(string username, string hashedPassword)
```
**What it does:**
- Handles user login (checks username and password)
- Registers new users
- Updates user profiles
- Deletes users
- Gets user statistics

**Key Methods:**
- `LoginAsync()` - Authenticates user
- `RegisterAsync()` - Creates new user account
- `GetAllUsersAsync()` - Gets all users (for admin)
- `UpdateUserAsync()` - Updates user fields
- `DeleteUserAsync()` - Removes user from database

#### **SongDB.cs** - Song Database Operations
**What it does:**
- Stores songs with audio data (MP3 files as BLOB)
- Retrieves songs by ID, user, or search criteria
- Tracks play counts
- Manages song metadata

**Key Methods:**
- `InsertSongAsync()` - Uploads new song
- `SelectAllSongsAsync()` - Gets all songs
- `GetSongByIdAsync()` - Gets specific song
- `SearchSongsAsync()` - Searches by title
- `IncrementPlayCountAsync()` - Tracks plays
- `DeleteSongAsync()` - Removes song

#### **PlaylistDB.cs** - Playlist Operations
**What it does:**
- Creates and manages playlists
- Links playlists to users
- Tracks public/private status

#### **GenreDB.cs** - Genre Management
**What it does:**
- Manages music genres
- Handles genre requests from users
- Links songs to multiple genres (many-to-many)

#### **RatingDB.cs** - Rating System
**What it does:**
- Stores user ratings for songs (1-5 stars)
- Calculates average ratings
- Prevents duplicate ratings

---

## Business Logic Layer (Services)

### Purpose
Contains business rules and validation - separates logic from data access.

### Key Services

#### **HomeService.cs**
```csharp
public async Task<List<Song>> LoadPopularSongsAsync()
public async Task<List<Song>> LoadNewSongsAsync()
```
**What it does:**
- Loads songs for home page
- Gets popular songs (sorted by plays)
- Gets new songs (sorted by upload date)
- Loads site statistics

#### **SongUploadService.cs**
```csharp
public string ValidateUpload(User user, string title, List<int> selectedGenreIds, IBrowserFile selectedFile)
```
**What it does:**
- Validates song uploads
- Checks file size (max 10MB)
- Checks file type (MP3 only)
- Ensures title is provided
- Ensures at least one genre selected
- Converts browser file to byte array

**Validation Rules:**
- User must be logged in
- Title required (not empty)
- At least 1 genre required
- File must be selected
- File must be MP3
- File must be ≤ 10MB

#### **PlaylistService.cs**
```csharp
public async Task<List<Playlist>> LoadUserPlaylistsAsync(int userId)
public async Task<int> CreatePlaylistAsync(int userId, string playlistName, bool isPublic)
```
**What it does:**
- Manages playlist CRUD operations
- Adds/removes songs from playlists
- Handles song ordering in playlists
- Validates playlist operations

#### **UserAdminService.cs**
```csharp
public string ValidateUserDeletion(User targetUser, User currentUser)
public async Task<bool> ToggleAdminStatusAsync(User targetUser)
```
**What it does:**
- Admin user management
- Validates admin operations
- Prevents self-deletion
- Prevents self-demotion
- Toggles admin status

**Business Rules:**
- Admin cannot delete themselves
- Admin cannot remove their own admin status
- Only admins can access these functions

---

## API Layer (AruroaAPI)

### Purpose
Exposes HTTP endpoints for external access (REST API).

### Key Controllers

#### **UsersController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
```

**Endpoints:**
- `POST /api/Users/login` - User login
- `POST /api/Users/register` - User registration
- `GET /api/Users` - Get all users
- `GET /api/Users/{id}` - Get user by ID
- `DELETE /api/Users/{id}` - Delete user ⚠️ **NO AUTH**
- `GET /api/Users/{id}/stats` - Get user statistics

**How it works:**
```csharp
[HttpPost("login")]
public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
{
    // 1. Hash the password
    string hashedPassword = HashPassword(request.Password);
    
    // 2. Call database to check credentials
    var user = await _userDB.LoginAsync(request.Username, hashedPassword);
    
    // 3. Return user or 401 Unauthorized
    if (user == null)
        return Unauthorized(new { message = "Invalid credentials" });
    
    return Ok(user);
}
```

#### **SongsController.cs**
**Endpoints:**
- `GET /api/Songs` - Get all songs
- `POST /api/Songs` - Upload new song
- `GET /api/Songs/{id}` - Get song by ID
- `DELETE /api/Songs/{id}` - Delete song ⚠️ **NO AUTH**
- `GET /api/Songs/search?query=...` - Search songs
- `GET /api/Songs/popular` - Get popular songs
- `GET /api/Songs/new` - Get new songs
- `POST /api/Songs/{id}/play` - Increment play count

#### **PlaylistsController.cs**
**Endpoints:**
- `GET /api/Playlists/user/{userId}` - Get user playlists
- `GET /api/Playlists/{id}` - Get playlist by ID
- `DELETE /api/Playlists/{id}` - Delete playlist ⚠️ **NO AUTH**
- `POST /api/Playlists` - Create playlist
- `POST /api/Playlists/{playlistId}/songs/{songId}` - Add song
- `DELETE /api/Playlists/{playlistId}/songs/{songId}` - Remove song

#### **GenresController.cs**
**Endpoints:**
- `GET /api/Genres` - Get all genres
- `POST /api/Genres` - Create genre
- `DELETE /api/Genres/{id}` - Delete genre ⚠️ **NO AUTH**
- `POST /api/Genres/request` - Request new genre
- `GET /api/Genres/requests/pending` - Get pending requests
- `POST /api/Genres/requests/{id}/approve` - Approve request
- `POST /api/Genres/requests/{id}/reject` - Reject request

#### **StatisticsController.cs**
**Endpoints:**
- `GET /api/Statistics/system` - Get system statistics
- `POST /api/Statistics/genre-usage` - Get genre usage (requires auth)

**How it works:**
```csharp
[HttpGet("system")]
public async Task<ActionResult<SiteStats>> GetSystemStats()
{
    var stats = await _statsDB.GetStatsAsync();
    return Ok(stats);
}
```

Returns:
```json
{
  "totalSongs": 150,
  "totalUsers": 25,
  "totalPlaylists": 45,
  "totalPlays": 3200
}
```

---

## Web Application (AruroaBlazor)

### Purpose
Admin panel and user interface built with Blazor Server.

### Technology: Blazor Server
**What is Blazor?**
- Write web UI in C# instead of JavaScript
- Server-side rendering with SignalR for real-time updates
- Component-based architecture (like React, but C#)

### Key Pages

#### **Home.razor**
```razor
@page "/"
@using Models
@using Services
```
**What it does:**
- Landing page
- Shows popular songs
- Shows new songs
- Displays site statistics
- Audio player integration

**How Blazor works:**
```razor
@code {
    private List<Song> popularSongs = new List<Song>();
    
    protected override async Task OnInitializedAsync()
    {
        // This runs when page loads
        popularSongs = await homeService.LoadPopularSongsAsync();
    }
}
```

#### **Login.razor**
```razor
@page "/login"
```
**What it does:**
- User login form
- Password hashing (SHA256)
- Stores user in session storage
- Redirects to home on success

**Session Storage:**
```csharp
await Storage.SetAsync("user", user);
```
This stores the logged-in user in browser session.

#### **Upload.razor**
**What it does:**
- Song upload form
- File selection (MP3 only)
- Genre selection (multi-select)
- Validation
- Progress indication

**File Upload Process:**
```csharp
// 1. User selects file
IBrowserFile file = e.File;

// 2. Read file into memory
using var stream = file.OpenReadStream(maxAllowedSize: 10485760); // 10MB
byte[] audioData = new byte[stream.Length];
await stream.ReadAsync(audioData);

// 3. Create song object
Song song = new Song {
    title = title,
    audioData = audioData,
    userid = user.userid
};

// 4. Save to database
await songDB.InsertSongAsync(song);
```

#### **Admin Pages**

##### **AdminDashboard.razor**
- Central hub for admin functions
- Buttons to navigate to:
  - Manage Genres
  - Manage Genre Requests
  - Manage Users
  - Manage Songs
  - Manage Playlists
  - Statistics Dashboard

##### **UsersAdmin.razor**
**What it does:**
- Lists all users
- Toggle admin status
- Delete users
- View user details

**Admin Check:**
```csharp
@if (user == null)
{
    <p>You must be logged in.</p>
}
else if (user.IsAdmin == 0)
{
    <p>You are not an admin.</p>
}
else
{
    // Show admin content
}
```

##### **Statistics.razor** (with ApexCharts)
**What it does:**
- Displays system statistics
- Interactive pie chart (Users, Songs, Playlists, Plays)
- Interactive bar chart (Averages)
- Calculates engagement metrics

**Chart Configuration:**
```csharp
private ApexChartOptions<StatItem> pieOptions = new ApexChartOptions<StatItem>
{
    Legend = new Legend { Position = LegendPosition.Bottom },
    Colors = new List<string> { "#007bff", "#28a745", "#17a2b8", "#ffc107" }
};
```

**Chart Data:**
```csharp
pieData = new List<StatItem>
{
    new StatItem { Label = "Users", Value = stats.TotalUsers },
    new StatItem { Label = "Songs", Value = stats.TotalSongs },
    new StatItem { Label = "Playlists", Value = stats.TotalPlaylists }
};
```

#### **PlaylistPage.razor** (Enhanced)
**What it does:**
- View playlist songs
- Add songs with search and multi-select
- Reorder songs (up/down)
- Remove songs
- Shuffle playlist

**Multi-Select Feature:**
```csharp
// User can select multiple songs
private List<int> selectedSongIds = new List<int>();

// Add all selected songs at once
private async Task AddSelectedSongs()
{
    int position = await playlistService.GetMaxPositionAsync(playlistId);
    
    foreach (int songId in selectedSongIds)
    {
        await playlistService.AddSongToPlaylistAsync(playlistId, songId, position);
        position++;
    }
}
```

---

## Desktop Application (AruroaMusicPlayer)

### Purpose
Standalone Windows application for playing music from the API.

### Technology: WinForms
- Traditional Windows desktop UI
- Event-driven programming
- Native Windows controls

### Key Components

#### **Form1.cs** - Main Window
**What it does:**
- Main UI with DataGridView for songs
- Playback controls (Play, Pause, Stop, Next, Previous)
- Volume control
- Progress bar
- Search functionality
- Genre filter

**UI Controls:**
```csharp
private DataGridView dgvSongs;      // Song list
private Button btnPlay;             // Play button
private Button btnPause;            // Pause button
private TrackBar volumeSlider;      // Volume control
private ProgressBar progressBar;    // Playback progress
private TextBox txtSearch;          // Search box
private ComboBox cmbGenre;          // Genre filter
```

#### **ApiService.cs** - API Communication
```csharp
public async Task<List<Song>> GetAllSongsAsync()
{
    var response = await _httpClient.GetAsync($"{_baseUrl}/api/Songs");
    
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Song>>(json);
    }
    
    return new List<Song>();
}
```

**What it does:**
- Makes HTTP requests to API
- Deserializes JSON responses
- Handles errors gracefully

#### **AudioPlayerService.cs** - Audio Playback
```csharp
public void Play(byte[] audioData)
{
    // 1. Create memory stream from byte array
    var stream = new MemoryStream(audioData);
    
    // 2. Create MP3 reader
    var reader = new Mp3FileReader(stream);
    
    // 3. Create output device
    _outputDevice = new WaveOutEvent();
    _outputDevice.Init(reader);
    
    // 4. Start playback
    _outputDevice.Play();
}
```

**What it does:**
- Plays MP3 audio from byte arrays
- Controls playback (play, pause, stop)
- Manages volume
- Tracks playback position

**How NAudio Works:**
1. **Input**: Byte array (MP3 data from API)
2. **MemoryStream**: Wraps byte array as stream
3. **Mp3FileReader**: Decodes MP3 format
4. **WaveOutEvent**: Outputs to speakers
5. **Playback**: Audio plays through speakers

---

## Security Concerns ⚠️

### CRITICAL ISSUE: No Authentication on DELETE Endpoints

**Problem:**
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteSong(int id)
{
    // ⚠️ ANYONE can call this!
    await _songDB.DeleteSongAsync(id);
    return Ok();
}
```

**Who can delete?**
- Anyone with the API URL
- No login required
- No admin check
- No ownership check

**Affected Endpoints:**
- `DELETE /api/Users/{id}`
- `DELETE /api/Songs/{id}`
- `DELETE /api/Playlists/{id}`
- `DELETE /api/Genres/{id}`

### How to Fix (For Your Presentation)

**Option 1: Add Authorization Attribute**
```csharp
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")] // Only admins can delete
public async Task<IActionResult> DeleteSong(int id)
{
    await _songDB.DeleteSongAsync(id);
    return Ok();
}
```

**Option 2: Manual Check**
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteSong(int id, [FromHeader] int userId)
{
    // 1. Get user from database
    var user = await _userDB.GetUserByIdAsync(userId);
    
    // 2. Check if admin
    if (user == null || user.IsAdmin == 0)
    {
        return Unauthorized(new { message = "Admin access required" });
    }
    
    // 3. Proceed with deletion
    await _songDB.DeleteSongAsync(id);
    return Ok();
}
```

**Option 3: Check Ownership**
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteSong(int id, [FromHeader] int userId)
{
    // 1. Get the song
    var song = await _songDB.GetSongByIdAsync(id);
    
    // 2. Check if user owns the song OR is admin
    var user = await _userDB.GetUserByIdAsync(userId);
    
    if (song.userid != userId && user.IsAdmin == 0)
    {
        return Forbidden(new { message = "You can only delete your own songs" });
    }
    
    // 3. Proceed with deletion
    await _songDB.DeleteSongAsync(id);
    return Ok();
}
```

---

## Key Concepts to Explain in Presentation

### 1. **Three-Tier Architecture**
- **Presentation**: What users see (Blazor/WinForms)
- **Business Logic**: Rules and validation (Services)
- **Data Access**: Database operations (DBL)

**Why?**
- Separation of concerns
- Easier to maintain
- Can swap out layers (e.g., change database)

### 2. **REST API**
- **RE**presentational **S**tate **T**ransfer
- Uses HTTP methods: GET, POST, PUT, DELETE
- Stateless (no session on server)
- Returns JSON data

### 3. **Blazor Server**
- C# instead of JavaScript
- Server-side rendering
- Real-time updates via SignalR
- Component-based

### 4. **Async/Await**
```csharp
public async Task<List<Song>> GetSongsAsync()
{
    // await = wait for this to finish without blocking
    var songs = await _songDB.SelectAllSongsAsync();
    return songs;
}
```
**Why?**
- Non-blocking operations
- Better performance
- Responsive UI

### 5. **Dependency Injection**
```csharp
public class HomeService
{
    private readonly SongDB _songDB;
    
    public HomeService()
    {
        _songDB = new SongDB(); // Create dependency
    }
}
```

### 6. **Password Hashing**
```csharp
private string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```
**Why?**
- Never store plain text passwords
- One-way encryption
- Even if database is compromised, passwords are safe

---

## Database Schema

### Users Table
```sql
CREATE TABLE users (
    userid INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,  -- Hashed
    email VARCHAR(100) UNIQUE NOT NULL,
    profilepicture BLOB,
    IsAdmin INT DEFAULT 0,           -- 0 = user, 1 = admin
    ResetCode VARCHAR(50)
);
```

### Songs Table
```sql
CREATE TABLE songs (
    songID INT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(255) NOT NULL,
    duration INT NOT NULL,           -- Seconds
    audioData LONGBLOB NOT NULL,     -- MP3 file
    userid INT NOT NULL,             -- Who uploaded
    uploaded DATETIME DEFAULT CURRENT_TIMESTAMP,
    plays INT DEFAULT 0,
    FOREIGN KEY (userid) REFERENCES users(userid)
);
```

### Playlists Table
```sql
CREATE TABLE playlists (
    playlistid INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(255) NOT NULL,
    userid INT NOT NULL,
    ispublic BOOLEAN DEFAULT FALSE,
    created DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (userid) REFERENCES users(userid)
);
```

### Many-to-Many Relationships

**song_genres** (Songs can have multiple genres)
```sql
CREATE TABLE song_genres (
    songid INT,
    genreid INT,
    PRIMARY KEY (songid, genreid),
    FOREIGN KEY (songid) REFERENCES songs(songID),
    FOREIGN KEY (genreid) REFERENCES genres(genreid)
);
```

**playlist_songs** (Playlists can have multiple songs)
```sql
CREATE TABLE playlist_songs (
    playlistid INT,
    songid INT,
    position INT,                    -- Order in playlist
    PRIMARY KEY (playlistid, songid),
    FOREIGN KEY (playlistid) REFERENCES playlists(playlistid),
    FOREIGN KEY (songid) REFERENCES songs(songID)
);
```

---

## Common Questions for Presentation

### Q: Why use Blazor instead of React/Angular?
**A:** 
- Full-stack C# (no context switching)
- Type safety across entire stack
- Shared models between frontend and backend
- Server-side rendering (better for SEO)
- Real-time updates built-in

### Q: Why store audio in database instead of file system?
**A:**
- Easier backup (one database backup includes everything)
- Atomic transactions (song + metadata together)
- Easier to manage permissions
- Simpler deployment (no file system dependencies)

**Downside:**
- Database size grows quickly
- Slower than file system for large files

### Q: How does the audio player work?
**A:**
1. API returns song with audioData (byte array)
2. WinForms creates MemoryStream from bytes
3. NAudio decodes MP3 format
4. WaveOutEvent outputs to speakers
5. User hears music!

### Q: What's the difference between DBL and Services?
**A:**
- **DBL**: Pure data access (SQL queries only)
- **Services**: Business logic (validation, calculations, orchestration)

Example:
- DBL: "Insert this song into database"
- Services: "Validate file size, check user permissions, then insert song"

### Q: How do you prevent SQL injection?
**A:**
```csharp
// BAD (vulnerable)
string query = "SELECT * FROM users WHERE username = '" + username + "'";

// GOOD (parameterized)
command.CommandText = "SELECT * FROM users WHERE username = @username";
command.Parameters.AddWithValue("@username", username);
```

### Q: Why is there no authentication on DELETE endpoints?
**A:**
- **Honest answer**: Security oversight that needs to be fixed
- **How to fix**: Add authorization checks (shown above)
- **For production**: Would implement JWT tokens or session-based auth

---

## Demo Flow for Presentation

### 1. **Show Database** (MySQL Workbench)
- Show tables
- Show sample data
- Explain relationships

### 2. **Show API** (Swagger)
- Open http://localhost:5230/swagger
- Test GET /api/Songs
- Test POST /api/Users/login
- Show JSON responses

### 3. **Show Blazor Web App**
- Login as regular user
- Upload a song
- Create a playlist
- Add songs to playlist
- Rate a song

### 4. **Show Admin Panel**
- Login as admin
- Show statistics dashboard with charts
- Manage users (toggle admin)
- Approve genre requests
- Delete inappropriate content

### 5. **Show WinForms App**
- Load songs from API
- Search for songs
- Filter by genre
- Play a song
- Show volume control

### 6. **Show Code**
- Walk through one complete flow (e.g., song upload)
- Show how data flows through layers
- Explain key code sections

---

## Conclusion

This is a **full-stack, three-tier music streaming platform** with:
- ✅ REST API backend
- ✅ Web admin panel
- ✅ Desktop music player
- ✅ User authentication
- ✅ File upload/storage
- ✅ Real-time statistics
- ✅ Interactive charts
- ⚠️ Security improvements needed (DELETE endpoints)

**Technologies mastered:**
- C# / .NET
- ASP.NET Core Web API
- Blazor Server
- WinForms
- MySQL
- REST architecture
- Async programming
- Three-tier architecture
