# Aruroa Music API

REST API for Aruroa Music Management System built with ASP.NET Core Web API.

## 🚀 Features

- **RESTful Architecture** - Stateless API following REST principles
- **Swagger Documentation** - Interactive API documentation at root URL
- **CORS Enabled** - Allows cross-origin requests from Blazor app
- **Async Operations** - All database operations are asynchronous
- **Comprehensive Endpoints** - Full CRUD operations for all entities

## 📋 API Endpoints

### Songs (`/api/songs`)
- `GET /api/songs` - Get all songs
- `GET /api/songs/{id}` - Get song by ID
- `GET /api/songs/search?searchText={text}` - Search songs
- `GET /api/songs/user/{userId}` - Get songs by user
- `GET /api/songs/popular` - Get top 10 popular songs
- `GET /api/songs/new` - Get latest 10 songs
- `POST /api/songs` - Add new song
- `POST /api/songs/{id}/play` - Increment play count
- `DELETE /api/songs/{id}` - Delete song
- `POST /api/songs/filter-by-genres` - Filter songs by genres (AND logic)
- `GET /api/songs/not-in-playlist/{playlistId}` - Get songs not in playlist

### Users (`/api/users`)
- `POST /api/users/login` - User login
- `POST /api/users/register` - User registration
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/{id}/stats` - Get user statistics
- `DELETE /api/users/{id}` - Delete user

### Playlists (`/api/playlists`)
- `GET /api/playlists/public` - Get all public playlists
- `GET /api/playlists/user/{userId}` - Get user's playlists
- `GET /api/playlists/{id}` - Get playlist by ID
- `GET /api/playlists/{id}/songs` - Get songs in playlist
- `POST /api/playlists` - Create new playlist
- `POST /api/playlists/{playlistId}/songs/{songId}` - Add song to playlist
- `DELETE /api/playlists/{playlistId}/songs/{songId}` - Remove song from playlist
- `PUT /api/playlists/{id}` - Update playlist
- `DELETE /api/playlists/{id}` - Delete playlist

### Genres (`/api/genres`)
- `GET /api/genres` - Get all genres
- `GET /api/genres/{id}` - Get genre by ID
- `GET /api/genres/song/{songId}` - Get genres for song
- `POST /api/genres` - Add new genre (Admin)
- `PUT /api/genres/{id}` - Update genre (Admin)
- `DELETE /api/genres/{id}` - Delete genre (Admin)
- `POST /api/genres/request` - Request new genre
- `GET /api/genres/requests/pending` - Get pending requests (Admin)
- `POST /api/genres/requests/{requestId}/approve` - Approve request (Admin)
- `POST /api/genres/requests/{requestId}/reject` - Reject request (Admin)
- `POST /api/genres/song/{songId}/genre/{genreId}` - Add genre to song
- `DELETE /api/genres/song/{songId}/genre/{genreId}` - Remove genre from song

### Ratings (`/api/ratings`)
- `GET /api/ratings/song/{songId}` - Get rating stats for song
- `GET /api/ratings/user/{userId}/song/{songId}` - Get user's rating for song
- `GET /api/ratings/user/{userId}` - Get all user's ratings
- `POST /api/ratings` - Add or update rating
- `DELETE /api/ratings/user/{userId}/song/{songId}` - Delete rating
- `GET /api/ratings/top?limit={n}` - Get top rated songs

### Statistics (`/api/statistics`)
- `GET /api/statistics/system` - Get system-wide statistics
- `POST /api/statistics/genre-usage` - Get genre usage for authenticated user
- `GET /api/statistics/active-users?limit={n}` - Get most active users
- `GET /api/statistics/popular-genres?limit={n}` - Get most popular genres

## 🔧 Configuration

### Connection String
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=auroradb;user=root;password=YOUR_PASSWORD"
  }
}
```

### CORS
CORS is configured to allow all origins. For production, update in `Program.cs`:
```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

## 🏃 Running the API

### Development
```bash
cd AruroaAPI
dotnet run
```

The API will start at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Swagger UI
Navigate to the root URL to access interactive API documentation:
- `http://localhost:5000`
- `https://localhost:5001`

## 📦 Dependencies

- **Microsoft.AspNetCore.OpenApi** (9.0.13) - OpenAPI support
- **MySql.Data** (9.6.0) - MySQL database connectivity
- **Newtonsoft.Json** (13.0.4) - JSON serialization
- **Swashbuckle.AspNetCore** (10.1.4) - Swagger documentation

## 🔐 Security Features

- **Password Hashing** - SHA256 hashing for passwords
- **SQL Injection Protection** - Parameterized queries throughout
- **CORS** - Configurable cross-origin resource sharing
- **Input Validation** - Request validation on all endpoints

## 📝 Request/Response Examples

### Login
**Request:**
```http
POST /api/users/login
Content-Type: application/json

{
  "username": "john",
  "password": "password123"
}
```

**Response:**
```json
{
  "userid": 1,
  "username": "john",
  "email": "john@example.com",
  "isAdmin": 0
}
```

### Add Song to Playlist
**Request:**
```http
POST /api/playlists/5/songs/10
```

**Response:**
```json
{
  "message": "Song added to playlist successfully"
}
```

### Search Songs
**Request:**
```http
GET /api/songs/search?searchText=rock
```

**Response:**
```json
[
  {
    "songID": 1,
    "title": "Rock Song",
    "duration": 240,
    "userid": 1,
    "plays": 150,
    "uploaded": "2024-01-15T10:30:00"
  }
]
```

## 🎯 Architecture

The API follows a layered architecture:

```
AruroaAPI (Web API Layer)
    ↓
Controllers (HTTP Endpoints)
    ↓
DBL (Data Access Layer)
    ↓
Models (Data Models)
    ↓
MySQL Database
```

## 🧪 Testing

Use Swagger UI for interactive testing, or use tools like:
- **Postman** - Import OpenAPI spec from `/swagger/v1/swagger.json`
- **curl** - Command-line testing
- **HTTPie** - User-friendly HTTP client

## 📄 License

Part of Aruroa Music Management System project.
