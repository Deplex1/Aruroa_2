# פרק 4: מימוש הפרויקט - צד שרת

## תיאור כללי
הפרויקט בנוי בארכיטקטורה **שכבתית (Layered Architecture)** המפרידה בין לוגיקת העסקים, גישה לנתונים, ומודלים. השרת מבוסס על **ASP.NET Core** ו-**Blazor Server** עם גישה למסד נתונים **MySQL**.

---

## ארכיטקטורת הפרויקט

```
┌─────────────────────────────────────────┐
│         Blazor Server (UI Layer)        │
│      AruroaBlazor/Components/Pages      │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      REST API Layer (Optional)          │
│        AruroaAPI/Controllers            │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│    Data Access Layer (DBL)              │
│    UserDB, SongDB, PlaylistDB, etc.     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Model Layer (Models)            │
│    User, Song, Playlist, Rating, etc.   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         MySQL Database                  │
│           auroradb                      │
└─────────────────────────────────────────┘
```

---

## שכבת ה-Model (מודלים)

### תרשים UML של המחלקות

**הנחיה:** צרף כאן תרשים UML המציג את כל מחלקות ה-Model וקשרים ביניהן.

### מחלקות עיקריות:

#### 1. מחלקה: `User` (משתמש)

```csharp
public class User
{
    public int userid { get; set; }           // מזהה ייחודי
    public string username { get; set; }      // שם משתמש
    public string password { get; set; }      // סיסמה מוצפנת
    public string email { get; set; }         // דוא"ל
    public byte[] profilepicture { get; set; } // תמונת פרופיל
    public int IsAdmin { get; set; }          // 0=משתמש רגיל, 1=מנהל
    public string ResetCode { get; set; }     // קוד לאיפוס סיסמה
}
```

**הסבר:**
- המחלקה מייצגת משתמש במערכת
- `profilepicture` מסוג `byte[]` - מאחסן תמונה בפורמט בינארי
- `IsAdmin` - שדה מספרי שמשמש כ-Boolean (0/1)
- כל השדות תואמים בדיוק לעמודות בטבלת `users` במסד הנתונים

---

#### 2. מחלקה: `Song` (שיר)

```csharp
public class Song
{
    public int songID { get; set; }           // מזהה ייחודי
    public string title { get; set; }         // שם השיר
    public int duration { get; set; }         // משך בשניות
    public byte[] audioData { get; set; }     // קובץ האודיו
    public int userid { get; set; }           // מזהה המעלה
    public DateTime uploaded { get; set; }    // תאריך העלאה
    public int plays { get; set; }            // מספר השמעות
    public string audioSource { get; set; }   // לא נשמר בDB
    
    // המרת קובץ אודיו ל-Base64 לתצוגה בדפדפן
    public string GetAudioSource(byte[] data)
    {
        if (data == null || data.Length == 0)
            return "";
            
        string mimeType = GetAudioMimeType(data);
        string base64 = Convert.ToBase64String(data);
        return $"data:{mimeType};base64,{base64}";
    }
    
    // זיהוי סוג קובץ האודיו לפי "Magic Numbers"
    private string GetAudioMimeType(byte[] data)
    {
        // בדיקה 1: MP3 עם תג ID3
        if (data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33)
            return "audio/mpeg";
            
        // בדיקה 2: MP3 ללא תג ID3
        if (data[0] == 0xFF && (data[1] & 0xE0) == 0xE0)
            return "audio/mpeg";
            
        // בדיקה 3: WAV (RIFF...WAVE)
        if (data[0] == 0x52 && data[1] == 0x49 && 
            data[2] == 0x46 && data[3] == 0x46 &&
            data[8] == 0x57 && data[9] == 0x41 && 
            data[10] == 0x56 && data[11] == 0x45)
            return "audio/wav";
            
        // בדיקה 4: AAC
        if (data[0] == 0xFF && (data[1] & 0xF0) == 0xF0)
            return "audio/aac";
            
        return "audio/mpeg"; // ברירת מחדל
    }
}
```

**הסבר:**
- `audioData` - מאחסן את קובץ האודיו המלא כ-BLOB
- `GetAudioSource()` - ממירה את הקובץ הבינארי ל-Data URL שהדפדפן יכול להשמיע
- `GetAudioMimeType()` - מזהה את סוג הקובץ לפי ה-"חתימה" שלו (Bytes הראשונים)
- **Magic Numbers** - כל סוג קובץ מתחיל בסדרת bytes ייחודית:
  - MP3 עם ID3: `49 44 33` (ASCII: "ID3")
  - MP3 ללא ID3: `FF Ex` (x = E-F)
  - WAV: `52 49 46 46 ... 57 41 56 45` (ASCII: "RIFF...WAVE")

---

#### 3. מחלקה: `Genre` (ז'אנר)

```csharp
public class Genre
{
    public int genreid { get; set; }    // מזהה ייחודי
    public string name { get; set; }    // שם הז'אנר
}
```

**הסבר:**
- מחלקה פשוטה המייצגת ז'אנר מוזיקלי
- משמשת לסיווג שירים לפי סגנון מוזיקלי

---

#### 4. מחלקה: `Playlist` (פלייליסט)

```csharp
public class Playlist
{
    public int playlistid { get; set; }     // מזהה ייחודי
    public string name { get; set; }        // שם הפלייליסט
    public int userid { get; set; }         // מזהה היוצר
    public bool ispublic { get; set; }      // ציבורי/פרטי
    public DateTime? created { get; set; }  // תאריך יצירה
}
```

**הסבר:**
- `ispublic` - קובע אם פלייליסט נראה לכולם או רק ליוצר
- `DateTime?` - הסימן `?` מאפשר ערך NULL

---

#### 5. מחלקה: `Rating` (דירוג)

```csharp
public class Rating
{
    public int ratingid { get; set; }       // מזהה ייחודי
    public int userid { get; set; }         // מזהה המדרג
    public int songid { get; set; }         // מזהה השיר
    public int rating { get; set; }         // דירוג 1-5
    public DateTime? daterated { get; set; } // תאריך הדירוג
}
```

**הסבר:**
- מייצג דירוג של משתמש לשיר
- `rating` - ערך בין 1 ל-5 (כוכבים)

---

## שכבת ה-DBL (Data Access Layer)

### תרשים UML - ירושות וקשרים

```
                    ┌──────────┐
                    │    DB    │
                    └──────────┘
                         ↑
                         │ (ירושה)
                         │
                  ┌──────────────┐
                  │   BaseDB<T>  │
                  │  (Generic)   │
                  └──────────────┘
                         ↑
         ┌───────────────┼───────────────┐
         │               │               │
    ┌─────────┐    ┌─────────┐    ┌──────────┐
    │ UserDB  │    │ SongDB  │    │PlaylistDB│
    └─────────┘    └─────────┘    └──────────┘
```

### מחלקת הבסיס: `BaseDB<T>`

**תפקיד:**  
מחלקה גנרית המספקת פעולות CRUD (Create, Read, Update, Delete) בסיסיות לכל הטבלאות.

**שיטות עיקריות:**

```csharp
public abstract class BaseDB<T> : DB
{
    // שיטות מופשטות שכל מחלקה יורשת חייבת לממש
    protected abstract string GetTableName();
    protected abstract string GetPrimaryKeyName();
    protected abstract Task<T> CreateModelAsync(object[] row);
    
    // שליפת כל הרשומות
    protected async Task<List<T>> SelectAllAsync()
    
    // שליפה עם תנאים
    protected async Task<List<T>> SelectAllAsync(
        string query, 
        Dictionary<string, object> parameters)
    
    // הוספת רשומה
    protected async Task<int> InsertAsync(
        Dictionary<string, object> keyAndValue)
    
    // עדכון רשומה
    protected async Task<int> UpdateAsync(
        Dictionary<string, object> fieldsToUpdate,
        Dictionary<string, object> whereConditions)
    
    // מחיקת רשומה
    protected async Task<int> DeleteAsync(
        Dictionary<string, object> whereConditions)
}
```

**יתרונות הגישה הגנרית:**
- ✅ **קוד חוזר פחות** - כל הפעולות הבסיסיות כתובות פעם אחת
- ✅ **עקביות** - כל הטבלאות מטופלות באותה צורה
- ✅ **תחזוקה קלה** - שינוי במחלקת הבסיס משפיע על כולם
- ✅ **Type Safety** - הגנריות מבטיחה בטיחות טיפוסים

---

### דוגמה: מחלקת `UserDB`

```csharp
public class UserDB : BaseDB<User>
{
    // מימוש השיטות המופשטות
    protected override string GetTableName() => "users";
    protected override string GetPrimaryKeyName() => "userid";
    
    protected async override Task<User> CreateModelAsync(object[] row)
    {
        User u = new User();
        u.userid = int.Parse(row[0].ToString());
        u.username = row[1].ToString();
        u.password = row[2].ToString();
        u.email = row[3].ToString();
        u.profilepicture = row[4] as byte[];
        u.IsAdmin = int.Parse(row[5].ToString());
        u.ResetCode = row[6]?.ToString();
        return u;
    }
    
    // פעולות ייעודיות למשתמשים
    public async Task<(bool Success, string Message)> RegisterAsync(
        string username, string password, string email)
    {
        // בדיקת תקינות
        if (string.IsNullOrWhiteSpace(username) || 
            string.IsNullOrWhiteSpace(password) || 
            string.IsNullOrWhiteSpace(email))
            return (false, "All fields are required.");
        
        // בדיקת פורמט דוא"ל
        if (!IsValidEmail(email))
            return (false, "Invalid email format.");
        
        // בדיקת כפילויות
        if (await UsernameExistsAsync(username))
            return (false, "Username already taken.");
        
        if (await EmailExistsAsync(email))
            return (false, "Email already registered.");
        
        // הצפנת סיסמה
        string hashedPassword = HashPassword(password);
        
        // הוספה למסד נתונים
        Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "username", username },
            { "password", hashedPassword },
            { "email", email },
            { "IsAdmin", 0 }
        };
        
        int rows = await InsertAsync(values);
        
        if (rows == 1)
            return (true, "Registration successful.");
        
        return (false, "Registration failed.");
    }
    
    // התחברות למערכת
    public async Task<User> LoginAsync(string username, string password)
    {
        string hashedPassword = HashPassword(password);
        
        string sql = "SELECT * FROM users WHERE username=@username AND password=@password";
        var parameters = new Dictionary<string, object>
        {
            { "username", username },
            { "password", hashedPassword }
        };
        
        List<User> list = await SelectAllAsync(sql, parameters);
        if (list.Count == 1)
            return list[0];
        
        return null;
    }
    
    // הצפנת סיסמה ב-SHA256
    private string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```

**הסבר:**
- `RegisterAsync()` - מבצעת בדיקות תקינות מקיפות לפני הרשמה
- `LoginAsync()` - משווה סיסמה מוצפנת עם המסד נתונים
- `HashPassword()` - משתמשת ב-SHA256 להצפנה חד-כיוונית
- **Tuple Return** - `(bool Success, string Message)` מחזיר 2 ערכים בו-זמנית

---

## תהליכים מייצגים - קוד מורכב

### 1. סינון שירים לפי מספר ז'אנרים (AND Logic)

**בעיה:**  
משתמש רוצה למצוא שירים ששייכים ל**כל** הז'אנרים שבחר (לא רק לאחד מהם).

**פתרון:**

```csharp
public async Task<List<Song>> FilterByGenresAsync(List<int> genreIds)
{
    // בניית שאילתה דינמית עם JOIN ו-GROUP BY
    string sql = @"SELECT s.* 
                  FROM songs s 
                  INNER JOIN song_genres sg ON s.songid = sg.songid 
                  WHERE sg.genreid IN (";
    
    // הוספת פרמטרים דינמית
    Dictionary<string, object> parameters = new Dictionary<string, object>();
    for (int i = 0; i < genreIds.Count; i++)
    {
        string paramName = "g" + i.ToString();
        sql += "@" + paramName;
        
        if (i < genreIds.Count - 1)
            sql += ", ";
        
        parameters.Add(paramName, genreIds[i]);
    }
    
    // HAVING COUNT מבטיח שהשיר שייך לכל הז'אנרים
    sql += @") GROUP BY s.songid, s.title, s.duration, 
             s.audioData, s.userid, s.uploaded, s.plays 
             HAVING COUNT(DISTINCT sg.genreid) = @genreCount 
             ORDER BY s.title";
    
    parameters.Add("genreCount", genreIds.Count);
    
    return await SelectAllAsync(sql, parameters);
}
```

**הסבר:**
1. **INNER JOIN** - מחבר בין `songs` ל-`song_genres`
2. **IN (...)** - בודק אם הז'אנר נמצא ברשימה
3. **GROUP BY** - מקבץ שירים לפי ID
4. **HAVING COUNT** - מבטיח שהשיר שייך ל**כל** הז'אנרים (לא רק לחלק)
5. **פרמטרים דינמיים** - מונע SQL Injection

**דוגמה:**  
אם המשתמש בחר Rock (ID=1) ו-Electronic (ID=5):
- השאילתה תחזיר רק שירים ששייכים **גם** ל-Rock **וגם** ל-Electronic
- שיר ששייך רק ל-Rock לא יוחזר

---

### 2. חישוב סטטיסטיקות משתמש

```csharp
public async Task<(int totalSongs, int totalPlays)> GetUserStatsAsync(int userId)
{
    // ספירת שירים
    string countSql = "SELECT COUNT(*) FROM songs WHERE userid = @userid";
    Dictionary<string, object> countParams = new Dictionary<string, object>();
    countParams.Add("userid", userId);
    List<object[]> countRows = await ExecuteQueryAsync(countSql, countParams);
    
    int totalSongs = 0;
    if (countRows.Count > 0 && countRows[0][0] != null)
    {
        totalSongs = Convert.ToInt32(countRows[0][0]);
    }
    
    // סכימת השמעות
    string playsSql = "SELECT COALESCE(SUM(plays), 0) FROM songs WHERE userid = @userid";
    Dictionary<string, object> playsParams = new Dictionary<string, object>();
    playsParams.Add("userid", userId);
    List<object[]> playsRows = await ExecuteQueryAsync(playsSql, playsParams);
    
    int totalPlays = 0;
    if (playsRows.Count > 0 && playsRows[0][0] != null)
    {
        totalPlays = Convert.ToInt32(playsRows[0][0]);
    }
    
    return (totalSongs, totalPlays);
}
```

**הסבר:**
- `COUNT(*)` - סופר את מספר השירים
- `SUM(plays)` - מסכם את כל ההשמעות
- `COALESCE(..., 0)` - מחזיר 0 אם אין שירים (במקום NULL)
- **Tuple Return** - מחזיר 2 ערכים בפעולה אחת

---

## הרחבות צד שרת

### 1. תכנות א-סינכרוני (Async/Await) ⭐

**מדוע חשוב?**
- ✅ **ביצועים** - השרת לא נחסם בזמן המתנה למסד נתונים
- ✅ **סקלביליות** - יכול לטפל ביותר בקשות במקביל
- ✅ **חוויית משתמש** - הממשק לא קופא בזמן טעינה

**דוגמה 1: שליפת כל השירים**

```csharp
// ❌ גרסה סינכרונית (חוסמת)
public List<Song> SelectAllSongs()
{
    // השרת חסום עד שהמסד נתונים מחזיר תוצאות
    return base.SelectAll();
}

// ✅ גרסה א-סינכרונית (לא חוסמת)
public async Task<List<Song>> SelectAllSongsAsync()
{
    // השרת יכול לטפל בבקשות אחרות בזמן ההמתנה
    return await base.SelectAllAsync();
}
```

**דוגמה 2: הרשמה עם מספר בדיקות**

```csharp
public async Task<(bool Success, string Message)> RegisterAsync(
    string username, string password, string email)
{
    // כל בדיקה היא פעולה א-סינכרונית
    if (await UsernameExistsAsync(username))
        return (false, "Username already taken.");
    
    if (await EmailExistsAsync(email))
        return (false, "Email already registered.");
    
    // הוספה למסד נתונים - גם א-סינכרונית
    int rows = await InsertAsync(values);
    
    return (rows == 1, "Registration successful.");
}
```

**הסבר:**
- `async` - מסמן שהפעולה א-סינכרונית
- `await` - ממתין לתוצאה מבלי לחסום את השרת
- `Task<T>` - מייצג פעולה שתחזיר ערך מסוג T בעתיד

**דוגמה 3: עדכון מונה השמעות**

```csharp
public async Task AddPlayAsync(int songId)
{
    string sql = "UPDATE songs SET plays = plays + 1 WHERE songid = @songid";
    
    Dictionary<string, object> parameters = new Dictionary<string, object>();
    parameters.Add("songid", songId);
    
    // פרמטר ידני
    cmd.Parameters.Clear();
    var param = cmd.CreateParameter();
    param.ParameterName = "@songid";
    param.Value = songId;
    cmd.Parameters.Add(param);
    
    // ביצוע א-סינכרוני
    await ExecNonQueryAsync(sql);
}
```

---

### 2. הגנה מפני SQL Injection ⭐

**מהי SQL Injection?**  
התקפה שבה תוקף מזריק קוד SQL זדוני דרך שדות קלט.

**דוגמה להתקפה:**

```csharp
// ❌ קוד פגיע - שרשור ישיר
string username = "admin' OR '1'='1";
string sql = "SELECT * FROM users WHERE username='" + username + "'";
// התוצאה: SELECT * FROM users WHERE username='admin' OR '1'='1'
// זה יחזיר את כל המשתמשים!
```

**הפתרון: שימוש בפרמטרים**

```csharp
// ✅ קוד מוגן - שימוש בפרמטרים
public async Task<User> LoginAsync(string username, string password)
{
    string hashedPassword = HashPassword(password);
    
    // השאילתה עם placeholders (@username, @password)
    string sql = "SELECT * FROM users WHERE username=@username AND password=@password";
    
    // הפרמטרים מועברים בנפרד
    var parameters = new Dictionary<string, object>
    {
        { "username", username },
        { "password", hashedPassword }
    };
    
    List<User> list = await SelectAllAsync(sql, parameters);
    return list.Count == 1 ? list[0] : null;
}
```

**איך זה עובד?**
1. השאילתה מכילה **placeholders** (`@username`, `@password`)
2. הערכים מועברים ב-**Dictionary נפרד**
3. המסד נתונים **מתייחס לערכים כטקסט בלבד** - לא כקוד SQL
4. גם אם התוקף מזין `admin' OR '1'='1`, זה יטופל כשם משתמש רגיל

**דוגמה נוספת: חיפוש שירים**

```csharp
public async Task<List<Song>> SearchSongsAsync(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return await SelectAllSongsAsync();
    
    // ✅ שימוש בפרמטר @t
    string sql = "SELECT * FROM songs WHERE title LIKE @t ORDER BY title";
    
    Dictionary<string, object> p = new Dictionary<string, object>();
    p.Add("t", $"%{text}%");  // % = wildcard ב-LIKE
    
    return await SelectAllAsync(sql, p);
}
```

**דוגמה מורכבת: סינון דינמי**

```csharp
public async Task<List<Song>> FilterByGenresAsync(List<int> genreIds)
{
    string sql = @"SELECT s.* FROM songs s 
                  INNER JOIN song_genres sg ON s.songid = sg.songid 
                  WHERE sg.genreid IN (";
    
    // יצירת פרמטרים דינמית
    Dictionary<string, object> parameters = new Dictionary<string, object>();
    for (int i = 0; i < genreIds.Count; i++)
    {
        string paramName = "g" + i.ToString();
        sql += "@" + paramName;
        
        if (i < genreIds.Count - 1)
            sql += ", ";
        
        // ✅ כל ID מועבר כפרמטר נפרד
        parameters.Add(paramName, genreIds[i]);
    }
    
    sql += ") GROUP BY s.songid ... HAVING COUNT(DISTINCT sg.genreid) = @genreCount";
    parameters.Add("genreCount", genreIds.Count);
    
    return await SelectAllAsync(sql, parameters);
}
```

**יתרונות השיטה:**
- ✅ **אבטחה מלאה** - אין אפשרות להזרקת קוד
- ✅ **ביצועים** - המסד נתונים יכול לשמור query plans במטמון
- ✅ **קריאות** - הקוד ברור ומובן

---

## סיכום טכנולוגיות

### שפות ותשתיות
- **C# 11** - שפת התכנות העיקרית
- **ASP.NET Core 9.0** - פריימוורק השרת
- **Blazor Server** - ממשק משתמש אינטראקטיבי
- **MySQL 8.0** - מסד נתונים

### דפוסי עיצוב (Design Patterns)
- **Repository Pattern** - הפרדה בין לוגיקה לגישה לנתונים
- **Generic Programming** - `BaseDB<T>` לקוד חוזר
- **Async/Await Pattern** - תכנות א-סינכרוני
- **Layered Architecture** - הפרדה לשכבות

### אבטחה
- ✅ **SHA256 Hashing** - הצפנת סיסמאות
- ✅ **Parameterized Queries** - הגנה מפני SQL Injection
- ✅ **Input Validation** - בדיקת תקינות קלט
- ✅ **Role-Based Access** - הרשאות לפי תפקיד (User/Admin)

---

**סיום פרק 4**
