# Security Analysis - API Authentication Issues

## ⚠️ CRITICAL SECURITY VULNERABILITY

### Issue: Unauthenticated DELETE Endpoints

**All DELETE operations in the API are publicly accessible without any authentication or authorization checks.**

---

## Affected Endpoints

### 1. **DELETE /api/Users/{id}** ⚠️
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    await _userDB.DeleteUserAsync(id);
    return Ok(new { message = "User deleted successfully" });
}
```
**Risk:** Anyone can delete any user account, including admins!

### 2. **DELETE /api/Songs/{id}** ⚠️
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteSong(int id)
{
    await _songDB.DeleteSongAsync(id);
    return Ok(new { message = "Song deleted successfully" });
}
```
**Risk:** Anyone can delete any song, even if they don't own it!

### 3. **DELETE /api/Playlists/{id}** ⚠️
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeletePlaylist(int id)
{
    await _playlistDB.DeletePlaylistAsync(id);
    return Ok(new { message = "Playlist deleted successfully" });
}
```
**Risk:** Anyone can delete anyone's playlists!

### 4. **DELETE /api/Genres/{id}** ⚠️
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteGenre(int id)
{
    await _genreDB.DeleteGenreAsync(id);
    return Ok(new { message = "Genre deleted successfully" });
}
```
**Risk:** Anyone can delete genres, breaking song categorization!

### 5. **DELETE /api/Ratings/user/{userId}/song/{songId}** ⚠️
```csharp
[HttpDelete("user/{userId}/song/{songId}")]
public async Task<ActionResult> DeleteRating(int userId, int songId)
{
    await _ratingDB.DeleteRatingAsync(userId, songId);
    return Ok(new { message = "Rating deleted successfully" });
}
```
**Risk:** Anyone can delete anyone's ratings!

---

## How to Exploit (For Demonstration)

### Using Postman or cURL:
```bash
# Delete user with ID 1 (no authentication needed!)
curl -X DELETE http://localhost:5230/api/Users/1

# Delete song with ID 5
curl -X DELETE http://localhost:5230/api/Songs/5

# Delete playlist with ID 10
curl -X DELETE http://localhost:5230/api/Playlists/10
```

**Result:** Data is deleted immediately, no questions asked!

---

## Why This Happened

### Current Architecture
The Blazor web app handles authentication:
```csharp
// In Blazor pages
@if (user == null || user.IsAdmin == 0)
{
    <p>You are not an admin.</p>
}
else
{
    // Show delete button
}
```

**Problem:** This only protects the UI, not the API!

### The Gap
```
┌─────────────────────────────────────┐
│   Blazor Web App (Protected)       │
│   - Checks if user is admin         │
│   - Only shows delete button        │
└──────────────┬──────────────────────┘
               │
               │ HTTP Request
               ▼
┌─────────────────────────────────────┐
│   API (UNPROTECTED!)                │
│   - No authentication check         │
│   - Accepts any request             │
│   - Deletes data immediately        │
└─────────────────────────────────────┘
```

**Anyone can bypass the Blazor UI and call the API directly!**

---

## Solutions (For Your Presentation)

### Solution 1: Add Admin Check to Each Endpoint

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteSong(int id, [FromHeader] int userId)
{
    // 1. Verify user exists and is admin
    var user = await _userDB.GetUserByIdAsync(userId);
    
    if (user == null)
    {
        return Unauthorized(new { message = "User not found" });
    }
    
    if (user.IsAdmin == 0)
    {
        return Forbid(new { message = "Admin access required" });
    }
    
    // 2. Proceed with deletion
    await _songDB.DeleteSongAsync(id);
    return Ok(new { message = "Song deleted successfully" });
}
```

**How to call:**
```bash
curl -X DELETE http://localhost:5230/api/Songs/5 \
  -H "userId: 1"
```

### Solution 2: Check Ownership (For User-Owned Resources)

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteSong(int id, [FromHeader] int userId)
{
    // 1. Get the song
    var song = await _songDB.GetSongByIdAsync(id);
    
    if (song == null)
    {
        return NotFound(new { message = "Song not found" });
    }
    
    // 2. Get the user
    var user = await _userDB.GetUserByIdAsync(userId);
    
    if (user == null)
    {
        return Unauthorized(new { message = "User not found" });
    }
    
    // 3. Check if user owns the song OR is admin
    if (song.userid != userId && user.IsAdmin == 0)
    {
        return Forbid(new { message = "You can only delete your own songs" });
    }
    
    // 4. Proceed with deletion
    await _songDB.DeleteSongAsync(id);
    return Ok(new { message = "Song deleted successfully" });
}
```

**Business Rule:** Users can delete their own songs, admins can delete any song.

### Solution 3: JWT Token Authentication (Production-Ready)

```csharp
// 1. Add JWT authentication in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AruroaAPI",
            ValidAudience = "AruroaUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSecretKeyHere"))
        };
    });

// 2. Add authorization
builder.Services.AddAuthorization();

// 3. Use authentication middleware
app.UseAuthentication();
app.UseAuthorization();
```

```csharp
// 4. Protect endpoints with [Authorize] attribute
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]  // Only admins can access
public async Task<ActionResult> DeleteSong(int id)
{
    await _songDB.DeleteSongAsync(id);
    return Ok(new { message = "Song deleted successfully" });
}
```

**How it works:**
1. User logs in → API returns JWT token
2. Client stores token
3. Client sends token with every request
4. API validates token before allowing access

---

## Comparison of Solutions

| Solution | Pros | Cons | Best For |
|----------|------|------|----------|
| **Manual Check** | Simple, easy to implement | Must add to every endpoint | Learning/Demo |
| **Ownership Check** | Fine-grained control | More complex logic | User-owned resources |
| **JWT Tokens** | Industry standard, secure | More setup required | Production apps |

---

## How to Present This

### 1. **Acknowledge the Issue**
"During development, I focused on functionality first. I implemented authentication in the Blazor UI, but I discovered that the API endpoints themselves are not protected."

### 2. **Demonstrate the Vulnerability**
- Show Swagger UI
- Show how DELETE endpoints can be called without authentication
- Explain the security risk

### 3. **Explain Why It Happened**
"This is a common mistake in web development - protecting the UI but forgetting to protect the API. The UI authentication only prevents honest users from accessing features, but anyone can bypass the UI and call the API directly."

### 4. **Show Your Solution**
"Here's how I would fix this in production..."
- Show one of the solutions above
- Explain the authentication flow
- Discuss trade-offs

### 5. **Lessons Learned**
"This taught me that:
- Security must be implemented at every layer
- Never trust the client (UI)
- Always validate on the server (API)
- Authentication and authorization are different things"

---

## Quick Fix for Presentation

If you want to quickly fix this before your presentation, here's the minimal change:

### Add to UsersController.cs:
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id, [FromHeader] int requestingUserId)
{
    // Get requesting user
    var requestingUser = await _userDB.GetUserByIdAsync(requestingUserId);
    
    // Check if admin
    if (requestingUser == null || requestingUser.IsAdmin == 0)
    {
        return StatusCode(403, new { message = "Admin access required" });
    }
    
    // Prevent self-deletion
    if (id == requestingUserId)
    {
        return BadRequest(new { message = "Cannot delete yourself" });
    }
    
    await _userDB.DeleteUserAsync(id);
    return Ok(new { message = "User deleted successfully" });
}
```

### Update Blazor to send userId:
```csharp
// In UsersAdmin.razor
private async Task DeleteUser(int userId)
{
    var httpClient = HttpClientFactory.CreateClient();
    httpClient.DefaultRequestHeaders.Add("requestingUserId", user.userid.ToString());
    
    var response = await httpClient.DeleteAsync($"http://localhost:5230/api/Users/{userId}");
    
    if (response.IsSuccessStatusCode)
    {
        await LoadUsers();
    }
}
```

---

## Other Security Considerations

### 1. **Password Storage** ✅ GOOD
```csharp
// Passwords are hashed with SHA256
string hashedPassword = HashPassword(password);
```
**Good:** Passwords are not stored in plain text.

### 2. **SQL Injection** ✅ GOOD
```csharp
// Using parameterized queries
command.Parameters.AddWithValue("@username", username);
```
**Good:** Protected against SQL injection.

### 3. **File Upload Validation** ✅ GOOD
```csharp
// Checking file size and type
if (file.Size > 10485760) // 10MB
    return "File too large";
if (!file.ContentType.Contains("audio"))
    return "Must be audio file";
```
**Good:** Basic validation in place.

### 4. **CORS** ⚠️ CHECK
```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});
```
**Warning:** `AllowAnyOrigin()` is too permissive for production.

**Better:**
```csharp
options.AddPolicy("AllowSpecific",
    builder => builder.WithOrigins("http://localhost:5000")
                     .AllowAnyMethod()
                     .AllowAnyHeader());
```

---

## Conclusion

### Current State
- ✅ UI is protected (Blazor checks admin status)
- ❌ API is NOT protected (anyone can call DELETE)
- ✅ Passwords are hashed
- ✅ SQL injection protected
- ⚠️ CORS too permissive

### For Production
1. Implement JWT authentication
2. Add [Authorize] attributes to all sensitive endpoints
3. Implement role-based access control (RBAC)
4. Add rate limiting
5. Add logging and monitoring
6. Restrict CORS to specific origins
7. Add input validation on all endpoints
8. Implement HTTPS only

### For Your Presentation
**Be honest:** "This is a learning project. I identified this security issue and here's how I would fix it in production."

**Show understanding:** Explain the difference between authentication (who you are) and authorization (what you can do).

**Demonstrate solutions:** Show at least one of the fixes above.

This shows maturity and understanding - much better than pretending the issue doesn't exist!
