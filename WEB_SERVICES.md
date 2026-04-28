# פרק 5: שכבת שירותי רשת (Web Services)

## תיאור כללי
הפרויקט כולל **REST API** מלא המבוסס על **ASP.NET Core Web API**. ה-API מאפשר גישה לכל פעולות המערכת דרך פרוטוקול HTTP, ומספק ממשק תקני לתקשורת בין שרת ללקוח.

---

## ארכיטקטורת REST API

```
┌─────────────────────────────────────────┐
│         Client (Blazor / Mobile)        │
└─────────────────────────────────────────┘
                    ↓ HTTP/HTTPS
┌─────────────────────────────────────────┐
│         REST API Controllers            │
│   SongsController, UsersController      │
│   PlaylistsController, GenresController │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      Data Access Layer (DBL)            │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         MySQL Database                  │
└─────────────────────────────────────────┘
```

---

## הגדרות רשת ו-Configuration

### 1. קובץ `Program.cs` - הגדרת השרת

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // הוספת Controllers
        builder.Services.AddControllers();

        // הגדרת CORS - מאפשר גישה מכל מקור
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()      // כל דומיין
                      .AllowAnyMethod()      // GET, POST, PUT, DELETE
                      .AllowAnyHeader();     // כל Headers
            });
        });

        // הוספת Swagger לתיעוד API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Aruroa Music API",
                Version = "v1",
                Description = "REST API for Aruroa Music Management System"
            });
        });

        var app = builder.Build();

        // טעינת Connection String
        var connectionString = builder.Configuration
            .GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            DB.SetConnectionString(connectionString);
        }

        // הפעלת Swagger בסביבת פיתוח
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", 
                                  "Aruroa Music API V1");
                c.RoutePrefix = string.Empty; // פתיחה ב-http://localhost:port/
            });
        }

        // הפעלת CORS
        app.UseCors("AllowAll");

        app.UseAuthorization();

        // מיפוי Controllers
        app.MapControllers();

        app.Run();
    }
}
```

**הסבר:**
- **CORS (Cross-Origin Resource Sharing)** - מאפשר לאפליקציית Blazor לגשת ל-API מדומיין אחר
- **Swagger** - יוצר תיעוד אינטראקטיבי אוטומטי של ה-API
- **Dependency Injection** - Controllers נוצרים אוטומטית על ידי המערכת

---

### 2. קובץ `appsettings.json` - הגדרות חיבור

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=auroradb;user=root;password=1234"
  }
}
```

**הסבר:**
- `ConnectionStrings` - מחרוזת החיבור למסד הנתונים
- `AllowedHosts: "*"` - מאפשר גישה מכל Host
- `Logging` - רמת פירוט של לוגים

---

## מבנה Controller

### דוגמה: `SongsController`

```csharp
[Route("api/[controller]")]  // נתיב: /api/songs
[ApiController]               // מסמן כ-API Controller
public class SongsController : ControllerBase
{
    private readonly SongDB _songDB;

    // Constructor - יוצר מופע של SongDB
    public SongsController()
    {
        _songDB = new SongDB();
    }

    // Endpoints מוגדרים כאן...
}
```

**הסבר:**
- `[Route("api/[controller]")]` - מגדיר את הנתיב הבסיסי (`/api/songs`)
- `[ApiController]` - מוסיף תכונות אוטומטיות:
  - Validation אוטומטי של Model
  - Binding אוטומטי של פרמטרים
  - תגובות HTTP סטנדרטיות
- `ControllerBase` - מחלקת בסיס ל-API Controllers (ללא View)

---

## מימוש CRUD - פעולות בסיסיות

### 1. CREATE - הוספת שיר חדש (POST)

```csharp
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
        // בדיקת תקינות
        if (song == null)
        {
            return BadRequest(new { message = "Song data is required" });
        }

        // הוספה למסד נתונים
        var newSong = await _songDB.InsertSongAsync(song);
        
        // החזרת תגובה 201 Created עם הנתיב לשיר החדש
        return CreatedAtAction(
            nameof(GetSongById),      // שם הפעולה לשליפה
            new { id = newSong.songID }, // פרמטרים לנתיב
            newSong                      // הגוף של התגובה
        );
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error adding song", 
            error = ex.Message 
        });
    }
}
```

**בקשה לדוגמה:**
```http
POST /api/songs HTTP/1.1
Content-Type: application/json

{
  "title": "Bohemian Rhapsody",
  "duration": 354,
  "audioData": "base64_encoded_audio...",
  "userid": 1
}
```

**תגובה:**
```http
HTTP/1.1 201 Created
Location: /api/songs/42

{
  "songID": 42,
  "title": "Bohemian Rhapsody",
  "duration": 354,
  "userid": 1,
  "uploaded": "2026-04-28T10:30:00",
  "plays": 0
}
```

**הסבר:**
- `[HttpPost]` - מגדיר שהפעולה מגיבה לבקשות POST
- `[FromBody]` - מציין שהנתונים מגיעים בגוף הבקשה (JSON)
- `CreatedAtAction` - מחזיר קוד 201 עם נתיב לשיר החדש
- `try-catch` - תפיסת שגיאות והחזרת תגובה מתאימה

---

### 2. READ - שליפת נתונים (GET)

#### 2.1 שליפת כל השירים

```csharp
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
        return Ok(songs);  // קוד 200 OK
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error retrieving songs", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
GET /api/songs HTTP/1.1
```

**תגובה:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "songID": 1,
    "title": "Imagine",
    "duration": 183,
    "plays": 1250
  },
  {
    "songID": 2,
    "title": "Stairway to Heaven",
    "duration": 482,
    "plays": 2100
  }
]
```

---

#### 2.2 שליפת שיר לפי ID

```csharp
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
        
        // בדיקה אם השיר נמצא
        if (song == null)
        {
            return NotFound(new { 
                message = $"Song with ID {id} not found" 
            });
        }
        
        return Ok(song);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error retrieving song", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
GET /api/songs/42 HTTP/1.1
```

**תגובה (נמצא):**
```http
HTTP/1.1 200 OK

{
  "songID": 42,
  "title": "Bohemian Rhapsody",
  "duration": 354,
  "plays": 5000
}
```

**תגובה (לא נמצא):**
```http
HTTP/1.1 404 Not Found

{
  "message": "Song with ID 42 not found"
}
```

**הסבר:**
- `{id}` בנתיב - פרמטר דינמי
- `NotFound()` - מחזיר קוד 404
- `Ok()` - מחזיר קוד 200

---

#### 2.3 חיפוש שירים

```csharp
/// <summary>
/// Search songs by title
/// </summary>
/// <param name="searchText">Search text</param>
/// <returns>List of matching songs</returns>
[HttpGet("search")]
public async Task<ActionResult<List<Song>>> SearchSongs(
    [FromQuery] string searchText)
{
    try
    {
        var songs = await _songDB.SearchSongsAsync(searchText);
        return Ok(songs);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error searching songs", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
GET /api/songs/search?searchText=queen HTTP/1.1
```

**תגובה:**
```http
HTTP/1.1 200 OK

[
  {
    "songID": 42,
    "title": "Bohemian Rhapsody",
    "duration": 354
  },
  {
    "songID": 43,
    "title": "We Are The Champions",
    "duration": 179
  }
]
```

**הסבר:**
- `[FromQuery]` - הפרמטר מגיע מ-Query String (`?searchText=...`)
- נתיב מותאם: `/api/songs/search` (לא `/api/songs/{searchText}`)

---

### 3. UPDATE - עדכון נתונים (PUT/POST)

```csharp
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
        return Ok(new { 
            message = "Play count incremented successfully" 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error incrementing play count", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
POST /api/songs/42/play HTTP/1.1
```

**תגובה:**
```http
HTTP/1.1 200 OK

{
  "message": "Play count incremented successfully"
}
```

**הסבר:**
- `[HttpPost("{id}/play")]` - נתיב מותאם: `/api/songs/42/play`
- פעולה פשוטה - רק עדכון מונה, ללא גוף בקשה

---

### 4. DELETE - מחיקת נתונים

```csharp
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
        
        // בדיקה אם השיר נמחק
        if (result == 0)
        {
            return NotFound(new { 
                message = $"Song with ID {id} not found" 
            });
        }
        
        return Ok(new { 
            message = "Song deleted successfully" 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error deleting song", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
DELETE /api/songs/42 HTTP/1.1
```

**תגובה (הצלחה):**
```http
HTTP/1.1 200 OK

{
  "message": "Song deleted successfully"
}
```

**תגובה (לא נמצא):**
```http
HTTP/1.1 404 Not Found

{
  "message": "Song with ID 42 not found"
}
```

**הסבר:**
- `[HttpDelete]` - מגדיר פעולת DELETE
- `result == 0` - אם לא נמחקו שורות, השיר לא נמצא

---

## פעולות מתקדמות

### 1. סינון לפי מספר ז'אנרים (POST עם Body)

```csharp
/// <summary>
/// Filter songs by genres (AND logic)
/// </summary>
/// <param name="genreIds">List of genre IDs</param>
/// <returns>List of songs matching all genres</returns>
[HttpPost("filter-by-genres")]
public async Task<ActionResult<List<Song>>> FilterByGenres(
    [FromBody] List<int> genreIds)
{
    try
    {
        if (genreIds == null || genreIds.Count == 0)
        {
            return BadRequest(new { 
                message = "Genre IDs are required" 
            });
        }

        var songs = await _songDB.FilterByGenresAsync(genreIds);
        return Ok(songs);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error filtering songs", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
POST /api/songs/filter-by-genres HTTP/1.1
Content-Type: application/json

[1, 5, 12]
```

**תגובה:**
```http
HTTP/1.1 200 OK

[
  {
    "songID": 15,
    "title": "Electronic Rock Fusion",
    "duration": 240
  }
]
```

**הסבר:**
- משתמש ב-POST (לא GET) כי יש גוף בקשה מורכב
- `[FromBody] List<int>` - רשימת מספרים מ-JSON

---

### 2. שליפת שירים לפי משתמש

```csharp
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
            return NotFound(new { 
                message = $"No songs found for user {userId}" 
            });
        }
        
        return Ok(songs);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error retrieving user songs", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
GET /api/songs/user/5 HTTP/1.1
```

---

## ניהול משתמשים - UsersController

### 1. התחברות (Login)

```csharp
public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

[HttpPost("login")]
public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
{
    try
    {
        // בדיקת תקינות
        if (string.IsNullOrEmpty(request.Username) || 
            string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { 
                message = "Username and password are required" 
            });
        }

        // הצפנת סיסמה
        string hashedPassword = HashPassword(request.Password);

        // בדיקה מול מסד נתונים
        var user = await _userDB.LoginAsync(request.Username, hashedPassword);
        
        if (user == null)
        {
            return Unauthorized(new { 
                message = "Invalid username or password" 
            });
        }

        // הסרת סיסמה מהתגובה
        user.password = "";
        return Ok(user);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error during login", 
            error = ex.Message 
        });
    }
}

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

**בקשה:**
```http
POST /api/users/login HTTP/1.1
Content-Type: application/json

{
  "username": "john_doe",
  "password": "mypassword123"
}
```

**תגובה (הצלחה):**
```http
HTTP/1.1 200 OK

{
  "userid": 5,
  "username": "john_doe",
  "email": "john@example.com",
  "IsAdmin": 0,
  "password": ""
}
```

**תגובה (כישלון):**
```http
HTTP/1.1 401 Unauthorized

{
  "message": "Invalid username or password"
}
```

**הסבר:**
- `Unauthorized()` - קוד 401 לכישלון אימות
- הסיסמה מוצפנת לפני השוואה
- הסיסמה לא מוחזרת בתגובה (אבטחה)

---

### 2. הרשמה (Register)

```csharp
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Email { get; set; } = "";
}

[HttpPost("register")]
public async Task<ActionResult> Register([FromBody] RegisterRequest request)
{
    try
    {
        if (string.IsNullOrEmpty(request.Username) || 
            string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { 
                message = "Username and password are required" 
            });
        }

        // בדיקת כפילות
        var existingUsers = await _userDB.GetAllUsersAsync();
        var existingUser = existingUsers
            .FirstOrDefault(u => u.username == request.Username);
            
        if (existingUser != null)
        {
            return Conflict(new { 
                message = "Username already exists" 
            });
        }

        // הרשמה
        var result = await _userDB.RegisterAsync(
            request.Username, 
            request.Password, 
            request.Email
        );
        
        if (result.Success)
        {
            return Ok(new { 
                message = "User registered successfully" 
            });
        }
        else
        {
            return BadRequest(new { message = result.Message });
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            message = "Error during registration", 
            error = ex.Message 
        });
    }
}
```

**בקשה:**
```http
POST /api/users/register HTTP/1.1
Content-Type: application/json

{
  "username": "new_user",
  "password": "securepass123",
  "email": "user@example.com"
}
```

**תגובה (הצלחה):**
```http
HTTP/1.1 200 OK

{
  "message": "User registered successfully"
}
```

**תגובה (שם משתמש תפוס):**
```http
HTTP/1.1 409 Conflict

{
  "message": "Username already exists"
}
```

**הסבר:**
- `Conflict()` - קוד 409 לכפילות
- בדיקת קיום לפני הוספה

---

## קודי תגובה HTTP

| קוד | שם | שימוש |
|-----|-----|-------|
| 200 | OK | פעולה הצליחה (GET, PUT, DELETE) |
| 201 | Created | נוצר משאב חדש (POST) |
| 400 | Bad Request | נתונים לא תקינים |
| 401 | Unauthorized | אימות נכשל |
| 404 | Not Found | משאב לא נמצא |
| 409 | Conflict | כפילות (שם משתמש קיים) |
| 500 | Internal Server Error | שגיאת שרת |

---

## Swagger - תיעוד אוטומטי

Swagger יוצר ממשק אינטראקטיבי לבדיקת ה-API:

**גישה:**
```
http://localhost:5230/
```

**תכונות:**
- ✅ רשימת כל ה-Endpoints
- ✅ תיאור כל פעולה
- ✅ דוגמאות בקשות ותגובות
- ✅ אפשרות לבצע בקשות ישירות מהדפדפן
- ✅ תיעוד פרמטרים ומודלים

---

## אבטחה ב-API

### 1. הצפנת סיסמאות
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

### 2. הסרת סיסמאות מתגובות
```csharp
user.password = "";  // לפני החזרת User ללקוח
```

### 3. Validation
```csharp
if (string.IsNullOrEmpty(request.Username))
{
    return BadRequest(new { message = "Username is required" });
}
```

### 4. Error Handling
```csharp
try
{
    // פעולה
}
catch (Exception ex)
{
    return StatusCode(500, new { 
        message = "Error", 
        error = ex.Message 
    });
}
```

---

## סיכום Endpoints

### Songs API (`/api/songs`)
| Method | Endpoint | תיאור |
|--------|----------|-------|
| GET | `/api/songs` | כל השירים |
| GET | `/api/songs/{id}` | שיר לפי ID |
| GET | `/api/songs/search?searchText=...` | חיפוש שירים |
| GET | `/api/songs/user/{userId}` | שירים של משתמש |
| GET | `/api/songs/popular` | שירים פופולריים |
| GET | `/api/songs/new` | שירים חדשים |
| POST | `/api/songs` | הוספת שיר |
| POST | `/api/songs/{id}/play` | עדכון מונה השמעות |
| POST | `/api/songs/filter-by-genres` | סינון לפי ז'אנרים |
| DELETE | `/api/songs/{id}` | מחיקת שיר |

### Users API (`/api/users`)
| Method | Endpoint | תיאור |
|--------|----------|-------|
| POST | `/api/users/login` | התחברות |
| POST | `/api/users/register` | הרשמה |
| GET | `/api/users` | כל המשתמשים |
| GET | `/api/users/{id}` | משתמש לפי ID |
| GET | `/api/users/{id}/stats` | סטטיסטיקות משתמש |
| DELETE | `/api/users/{id}` | מחיקת משתמש |

---

## שירותי רשת חיצוניים

**הערה:** הפרויקט הנוכחי **לא משתמש** בשירותי רשת חיצוניים (External APIs).  
כל הפונקציונליות מבוססת על שרת פנימי ומסד נתונים מקומי.

**אפשרויות להרחבה עתידית:**
- 🎵 **Spotify API** - שילוב מטא-דאטה של שירים
- 🎤 **Lyrics API** - הצגת מילות שירים
- 🎨 **Last.fm API** - המלצות מוזיקה
- ☁️ **Cloud Storage** - אחסון קבצי אודיו (AWS S3, Azure Blob)

---

**סיום פרק 5**
