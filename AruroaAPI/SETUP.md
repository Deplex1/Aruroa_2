# Aruroa API - Setup and Running Instructions

## Prerequisites

- .NET 9.0 SDK
- MySQL Server running on localhost
- Database: `auroradb`

## Configuration

1. **Update Connection String**
   
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;database=auroradb;user=root;password=YOUR_PASSWORD"
     }
   }
   ```

2. **Database Setup**
   
   Make sure your MySQL database `auroradb` is created and contains all required tables:
   - users
   - songs
   - playlists
   - playlist_songs (or playlistsongs)
   - genres
   - song_genres
   - ratings
   - genre_requests

## Running the API

### Option 1: Using Visual Studio
1. Open `Aruroa.sln` in Visual Studio
2. Set `AruroaAPI` as the startup project
3. Press F5 or click "Run"

### Option 2: Using Command Line
```bash
cd AruroaAPI
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Accessing Swagger Documentation

Once the API is running, navigate to:
- `http://localhost:5000`
- `https://localhost:5001`

You'll see the interactive Swagger UI with all available endpoints.

## Available Endpoints

### Songs
- `GET /api/songs` - Get all songs
- `GET /api/songs/{id}` - Get song by ID
- `GET /api/songs/search?searchText={text}` - Search songs
- `GET /api/songs/user/{userId}` - Get songs by user
- `GET /api/songs/popular` - Get popular songs
- `GET /api/songs/new` - Get new songs
- `POST /api/songs` - Add new song
- `POST /api/songs/{id}/play` - Increment play count
- `DELETE /api/songs/{id}` - Delete song
- `POST /api/songs/filter-by-genres` - Filter by genres
- `GET /api/songs/not-in-playlist/{playlistId}` - Get songs not in playlist

### Users
- `POST /api/users/login` - User login
- `POST /api/users/register` - User registration
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/{id}/stats` - Get user statistics
- `DELETE /api/users/{id}` - Delete user

### Playlists
- `GET /api/playlists/user/{userId}` - Get user's playlists
- `GET /api/playlists/{id}` - Get playlist by ID
- `GET /api/playlists/{id}/songs` - Get songs in playlist
- `POST /api/playlists` - Create new playlist
- `POST /api/playlists/{playlistId}/songs/{songId}` - Add song to playlist
- `DELETE /api/playlists/{playlistId}/songs/{songId}` - Remove song from playlist
- `DELETE /api/playlists/{id}` - Delete playlist

### Genres
- `GET /api/genres` - Get all genres
- `GET /api/genres/{id}` - Get genre by ID
- `GET /api/genres/song/{songId}` - Get genres for song
- `POST /api/genres` - Add new genre
- `DELETE /api/genres/{id}` - Delete genre
- `POST /api/genres/request` - Request new genre
- `GET /api/genres/requests/pending` - Get pending requests
- `POST /api/genres/requests/{requestId}/approve` - Approve request
- `POST /api/genres/requests/{requestId}/reject` - Reject request
- `POST /api/genres/song/{songId}/genre/{genreId}` - Add genre to song
- `DELETE /api/genres/song/{songId}/genre/{genreId}` - Remove genre from song

### Ratings
- `GET /api/ratings/song/{songId}` - Get rating stats for song
- `GET /api/ratings/user/{userId}/song/{songId}` - Get user's rating for song
- `GET /api/ratings/user/{userId}` - Get all user's ratings
- `POST /api/ratings` - Add or update rating
- `DELETE /api/ratings/user/{userId}/song/{songId}` - Delete rating

### Statistics
- `GET /api/statistics/system` - Get system-wide statistics
- `POST /api/statistics/genre-usage` - Get genre usage for authenticated user

## Testing the API

### Using Swagger UI
1. Navigate to the API root URL
2. Click on any endpoint to expand it
3. Click "Try it out"
4. Fill in the required parameters
5. Click "Execute"

### Using Postman
1. Import the OpenAPI spec from `/swagger/v1/swagger.json`
2. All endpoints will be automatically configured

### Example: Login Request
```http
POST http://localhost:5000/api/users/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

### Example: Get All Songs
```http
GET http://localhost:5000/api/songs
```

### Example: Search Songs
```http
GET http://localhost:5000/api/songs/search?searchText=rock
```

## CORS Configuration

The API is configured to allow all origins for development. For production, update `Program.cs`:

```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

## Security Features

- **Password Hashing**: SHA256 hashing for all passwords
- **SQL Injection Protection**: Parameterized queries throughout
- **Input Validation**: Request validation on all endpoints
- **CORS**: Configurable cross-origin resource sharing

## Troubleshooting

### Connection Issues
- Verify MySQL is running
- Check connection string in `appsettings.json`
- Ensure database `auroradb` exists

### Port Already in Use
Edit `Properties/launchSettings.json` to change ports:
```json
"applicationUrl": "https://localhost:5002;http://localhost:5001"
```

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

## Architecture

```
AruroaAPI (REST API Layer)
    ↓
Controllers (HTTP Endpoints)
    ↓
Services (Business Logic)
    ↓
DBL (Data Access Layer)
    ↓
Models (Data Models)
    ↓
MySQL Database
```

## Project Structure

```
AruroaAPI/
├── Controllers/          # API Controllers
│   ├── SongsController.cs
│   ├── UsersController.cs
│   ├── PlaylistsController.cs
│   ├── GenresController.cs
│   ├── RatingsController.cs
│   └── StatisticsController.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json     # Configuration
├── Program.cs           # Application entry point
├── README.md            # API documentation
└── SETUP.md             # This file
```

## Next Steps

1. Test all endpoints using Swagger UI
2. Integrate with Blazor frontend
3. Deploy to production server
4. Configure production CORS settings
5. Set up SSL certificates for HTTPS

## Support

For issues or questions, refer to the main project documentation.
