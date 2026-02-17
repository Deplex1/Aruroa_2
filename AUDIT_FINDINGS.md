# Project Audit Findings and Fixes

## TASK 1: SQL-FIRST VIOLATIONS FOUND

### 🔴 CRITICAL: SongService.LoadAllRatingsForSongsAsync()
**Location:** Services/SongService.cs, lines 52-68
**Problem:** N+1 query problem - loads ratings for each song in a loop
**Current Code:**
```csharp
for (int i = 0; i < songs.Count; i = i + 1)
{
    List<Rating> songRatings = await ratingDB.GetSongRatingsAsync(songs[i].songID);
    // adds to list
}
```
**Fix:** Create SQL query that loads all ratings for multiple songs in ONE query
**SQL Solution:**
```sql
SELECT * FROM ratings WHERE songid IN (@id1, @id2, @id3, ...) ORDER BY daterated DESC
```

### 🔴 CRITICAL: PlaylistPageService.FilterAvailableSongs()
**Location:** Services/PlaylistPageService.cs, lines 80-106
**Problem:** Filters songs in C# with nested loops instead of SQL
**Current Code:**
```csharp
for (int i = 0; i < allSongs.Count; i = i + 1)
{
    for (int j = 0; j < playlistSongs.Count; j = j + 1)
    {
        // checks if song exists
    }
}
```
**Fix:** Use SQL NOT IN or LEFT JOIN to filter in database
**SQL Solution:**
```sql
SELECT s.* FROM songs s 
WHERE s.songID NOT IN (
    SELECT songid FROM playlist_songs WHERE playlistid = @playlistId
)
ORDER BY s.title
```

## TASK 2: MEMORY LEAK FOUND

### 🔴 CRITICAL: Songs.razor Event Subscription
**Location:** AruroaBlazor/Components/Pages/Songs.razor
**Problem:** Subscribes to `playerService.OnStateChanged` but never unsubscribes
**Current Code:**
```csharp
playerService.OnStateChanged += OnPlayerStateChanged;
// NO Dispose() method!
```
**Fix:** Add `@implements IDisposable` and Dispose() method
**Solution:**
```csharp
@implements IDisposable

public void Dispose()
{
    playerService.OnStateChanged -= OnPlayerStateChanged;
}
```

## TASK 3: ERROR HANDLING VIOLATIONS

### ⚠️ Services Missing Try/Catch:
Most service methods have try/catch, but need to verify ALL database calls are wrapped.

**Services to check:**
- ✅ SongUploadService - Has try/catch
- ✅ SongService - Has try/catch on IncrementPlayCountAsync
- ⚠️ SongService.LoadAllRatingsForSongsAsync - NO try/catch
- ⚠️ PlaylistPageService.FilterAvailableSongs - NO try/catch (but will be replaced with SQL)
- ✅ GenreAdminService - Has try/catch
- ✅ UserAdminService - Has try/catch
- ✅ ProfileService - Has try/catch
- ✅ PlaylistService - Has try/catch
- ✅ PlaylistPageService - Has try/catch on most methods

## TASK 4: MISSING FEATURES

### ❌ Genre Filtering on Songs Page
**Status:** NOT IMPLEMENTED
**Required:**
- Filter songs by genre checkboxes
- SQL JOIN with song_genres table
- Genre tags/badges on song cards

### ❌ Public User Profiles
**Status:** NOT IMPLEMENTED
**Required:**
- /profile/{userid} route
- Show only public playlists for other users
- User stats (total songs, total plays)

### ❌ Genre Display on Song Cards
**Status:** NOT IMPLEMENTED
**Required:**
- Show genre tags under song title
- Colored badges/pills for each genre

## TASK 5: CODE CONSISTENCY VIOLATIONS

### 🔴 Lambda Expressions Found
**Location:** DBL/UserDB.cs, lines 13-14
```csharp
protected override string GetTableName() => "users";
protected override string GetPrimaryKeyName() => "userid";
```
**Fix:** Use explicit return statements
```csharp
protected override string GetTableName()
{
    return "users";
}
```

### 🔴 Ternary Operators Found

**Location 1:** DBL/PlaylistSongDB.cs, line 50
```csharp
return exists ? 1 : 0;
```
**Fix:**
```csharp
if (exists)
{
    return 1;
}
else
{
    return 0;
}
```

**Location 2:** DBL/RatingDB.cs, line 27
```csharp
r.daterated = row[4] == null ? null : (System.DateTime?)row[4];
```
**Fix:**
```csharp
if (row[4] == null)
{
    r.daterated = null;
}
else
{
    r.daterated = (System.DateTime?)row[4];
}
```

**Location 3:** DBL/RatingDB.cs, line 93
```csharp
return list.Count > 0 ? list[0] : null;
```
**Fix:**
```csharp
if (list.Count > 0)
{
    return list[0];
}
else
{
    return null;
}
```

**Location 4:** DBL/RatingDB.cs, line 195
```csharp
double average = row[1] == DBNull.Value ? 0 : Convert.ToDouble(row[1]);
```
**Fix:**
```csharp
double average = 0;
if (row[1] == DBNull.Value)
{
    average = 0;
}
else
{
    average = Convert.ToDouble(row[1]);
}
```

## SUMMARY OF FIXES NEEDED

### High Priority (Performance & Memory):
1. ✅ Fix SongService.LoadAllRatingsForSongsAsync() - Use SQL IN clause
2. ✅ Fix PlaylistPageService.FilterAvailableSongs() - Use SQL NOT IN
3. ✅ Fix Songs.razor memory leak - Add Dispose()
4. ✅ Fix lambda expressions in UserDB.cs
5. ✅ Fix all ternary operators in DBL classes

### Medium Priority (Features):
6. ⏳ Implement genre filtering on Songs page
7. ⏳ Implement public user profiles
8. ⏳ Add genre display on song cards

### Low Priority (Error Handling):
9. ⏳ Add try/catch to remaining methods

## IMPLEMENTATION ORDER

1. Fix code consistency violations (lambdas, ternaries)
2. Fix memory leak in Songs.razor
3. Fix SQL-FIRST violations (N+1 queries)
4. Add missing error handling
5. Implement missing features (if time permits)
