# נספח א': הסברים טכנולוגיים

## טכנולוגיות וספריות חיצוניות בפרויקט

מסמך זה מפרט את כל הטכנולוגיות והספריות החיצוניות ששימשו בפרויקט Aruroa.

---

## 1. ASP.NET Core 9.0

### מהי הטכנולוגיה?
**ASP.NET Core** היא פלטפורמה חוצת-פלטפורמות (Cross-platform) לבניית אפליקציות ווב מודרניות. זוהי הגרסה המודרנית של ASP.NET המקורי, שנבנתה מחדש להיות קלה, מהירה ומודולרית.

### למה בחרתי בה?
- **ביצועים גבוהים** - אחת המהירות ביותר בשוק
- **חוצת פלטפורמות** - רצה על Windows, Linux, macOS
- **תמיכה מובנית ב-REST API** - קל לבנות Web Services
- **קהילה גדולה** - הרבה משאבים ותמיכה

### איך השתמשתי בה?
- בניית **Blazor Server** לממשק המשתמש
- בניית **REST API** עם Controllers
- ניהול **Dependency Injection**
- **Middleware** ל-CORS ו-Swagger

### גרסה:
```xml
<TargetFramework>net9.0</TargetFramework>
```

---

## 2. Blazor Server

### מהי הטכנולוגיה?
**Blazor Server** היא טכנולוגיה לבניית ממשקי משתמש אינטראקטיביים באמצעות C# במקום JavaScript. הקוד רץ על השרת, והממשק מתעדכן בזמן אמת דרך SignalR.

### למה בחרתי בה?
- **C# בלבד** - לא צריך ללמוד JavaScript
- **שיתוף קוד** - אותן מחלקות בשרת ובלקוח
- **Real-time updates** - SignalR מובנה
- **פחות קוד** - Data Binding אוטומטי

### איך השתמשתי בה?
- **Components** - כל דף הוא Component (`.razor`)
- **Data Binding** - `@bind` לקישור דו-כיווני
- **Event Handling** - `@onclick` לטיפול באירועים
- **Lifecycle Methods** - `OnAfterRenderAsync` לטעינת נתונים

### דוגמה:
```razor
<input @bind="username" />
<button @onclick="LoginMethod">Login</button>

@code {
    string username;
    
    async Task LoginMethod()
    {
        // קוד C# רץ על השרת
        var user = await userDB.LoginAsync(username);
    }
}
```

---

## 3. MySQL 8.0

### מהי הטכנולוגיה?
**MySQL** היא מערכת ניהול בסיסי נתונים יחסית (RDBMS) בקוד פתוח. זוהי אחת ממסדי הנתונים הפופולריים ביותר בעולם.

### למה בחרתי בה?
- **חינמית** - קוד פתוח
- **מהירה** - ביצועים מעולים
- **פופולרית** - הרבה תמיכה ומדריכים
- **קלה לשימוש** - תחביר SQL סטנדרטי

### איך השתמשתי בה?
- **8 טבלאות** - users, songs, playlists, ratings, genres, וכו'
- **Foreign Keys** - קשרים בין טבלאות
- **Indexes** - לשיפור ביצועים
- **Transactions** - לשמירת שלמות נתונים

### חיבור:
```csharp
string connectionString = "server=localhost;database=auroradb;user=root;password=1234";
```

---

## 4. MySql.Data (ADO.NET)

### מהי הספרייה?
**MySql.Data** היא ספריית .NET רשמית להתחברות למסד נתונים MySQL. היא מספקת את המחלקות `MySqlConnection`, `MySqlCommand`, `MySqlDataReader`.

### למה בחרתי בה?
- **רשמית** - מתוחזקת על ידי Oracle
- **מהירה** - גישה ישירה למסד נתונים
- **גמישה** - שליטה מלאה על השאילתות
- **Async Support** - תמיכה ב-`async/await`

### איך השתמשתי בה?
```csharp
using MySqlConnection conn = new MySqlConnection(connectionString);
await conn.OpenAsync();

MySqlCommand cmd = new MySqlCommand("SELECT * FROM users", conn);
MySqlDataReader reader = await cmd.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    // קריאת נתונים
}
```

### גרסה:
```xml
<PackageReference Include="MySql.Data" Version="8.0.33" />
```

---

## 5. Swashbuckle.AspNetCore

### מהי הספרייה?
**Swashbuckle** היא ספרייה ליצירת תיעוד אוטומטי של REST API בפורמט Swagger/OpenAPI. היא יוצרת ממשק אינטראקטיבי לבדיקת ה-API.

### למה בחרתי בה?
- **תיעוד אוטומטי** - לא צריך לכתוב ידנית
- **ממשק אינטראקטיבי** - אפשר לבדוק Endpoints בדפדפן
- **תקן OpenAPI** - תואם לתקן בינלאומי
- **קל להתקנה** - 3 שורות קוד

### איך השתמשתי בה?
```csharp
// הוספה ב-Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Aruroa Music API",
        Version = "v1",
        Description = "REST API for Aruroa Music Management System"
    });
});

app.UseSwagger();
app.UseSwaggerUI();
```

### גישה:
```
http://localhost:5230/
```

### גרסה:
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

**הערה:** הורדתי מגרסה 10.x ל-7.2.0 בגלל בעיות תאימות עם `OpenApiInfo`.

---

## 6. System.Security.Cryptography (SHA256)

### מהי הספרייה?
**System.Security.Cryptography** היא ספרייה מובנית ב-.NET לפעולות קריפטוגרפיה. השתמשתי ב-**SHA256** להצפנת סיסמאות.

### למה בחרתי בה?
- **מובנית** - לא צריך להתקין
- **בטוחה** - תקן מאושר (NIST)
- **חד-כיוונית** - אי אפשר לפענח
- **מהירה** - ביצועים טובים

### איך השתמשתי בה?
```csharp
using System.Security.Cryptography;
using System.Text;

private string HashPassword(string password)
{
    using SHA256 sha256 = SHA256.Create();
    byte[] bytes = Encoding.UTF8.GetBytes(password);
    byte[] hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

### דוגמה:
```
Input:  "mypassword123"
Output: "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI="
```

---

## 7. Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage

### מהי הספרייה?
ספרייה לאחסון מוצפן של נתונים בדפדפן (Session Storage / Local Storage) ב-Blazor Server.

### למה בחרתי בה?
- **הצפנה אוטומטית** - הנתונים מוצפנים
- **קלה לשימוש** - API פשוט
- **בטוחה** - מגנה על פרטי משתמשים
- **מובנית** - חלק מ-Blazor

### איך השתמשתי בה?
```csharp
@inject ProtectedSessionStorage Storage

// שמירה
await Storage.SetAsync("user", user);

// קריאה
var result = await Storage.GetAsync<User>("user");
if (result.Success)
{
    user = result.Value;
}
```

### שימוש בפרויקט:
- שמירת פרטי משתמש מחובר
- נמחק אוטומטית בסגירת הדפדפן

---

## 8. Microsoft.AspNetCore.Components.Forms (InputFile)

### מהי הספרייה?
רכיב Blazor להעלאת קבצים מהדפדפן לשרת.

### למה בחרתי בה?
- **מובנית** - חלק מ-Blazor
- **קלה לשימוש** - רכיב אחד
- **תמיכה בגדלים גדולים** - עד 50MB
- **Async** - לא חוסמת את הממשק

### איך השתמשתי בה?
```razor
<InputFile OnChange="OnFileSelected" accept="audio/*" />

@code {
    private IBrowserFile selectedFile;
    
    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
    }
    
    private async Task UploadSong()
    {
        using var stream = selectedFile.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] audioData = memoryStream.ToArray();
    }
}
```

---

## 9. System.Text.RegularExpressions (Regex)

### מהי הספרייה?
ספרייה מובנית לעבודה עם ביטויים רגולריים (Regular Expressions).

### למה בחרתי בה?
- **מובנית** - לא צריך להתקין
- **חזקה** - תומכת בכל תחביר Regex
- **מהירה** - מותאמת לביצועים
- **גמישה** - אפשר לבדוק כל דפוס

### איך השתמשתי בה?
```csharp
using System.Text.RegularExpressions;

// בדיקת תקינות דוא"ל
private bool IsValidEmail(string email)
{
    return Regex.IsMatch(
        email,
        @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b",
        RegexOptions.IgnoreCase
    );
}
```

### שימושים בפרויקט:
- בדיקת פורמט דוא"ל
- Validation של קלט משתמש

---

## 10. SignalR (מובנה ב-Blazor Server)

### מהי הטכנולוגיה?
**SignalR** היא ספרייה לתקשורת בזמן אמת בין שרת ללקוח דרך WebSockets.

### למה היא חשובה?
- **Real-time updates** - שינויים מתעדכנים מיד
- **Automatic reconnection** - מתחבר מחדש אוטומטית
- **Fallback** - עובר ל-Long Polling אם WebSocket לא זמין

### איך Blazor משתמש בה?
Blazor Server משתמש ב-SignalR **אוטומטית** מאחורי הקלעים:
- כל אירוע (click, input) נשלח לשרת
- השרת מעבד ומחזיר עדכון
- הממשק מתעדכן בזמן אמת

### לא צריך לכתוב קוד!
SignalR עובד אוטומטית - פשוט כותבים Blazor רגיל.

---

## סיכום טכנולוגיות

| טכנולוגיה | תפקיד | גרסה |
|-----------|-------|------|
| ASP.NET Core | פלטפורמת השרת | 9.0 |
| Blazor Server | ממשק משתמש | 9.0 |
| MySQL | מסד נתונים | 8.0 |
| MySql.Data | חיבור למסד נתונים | 8.0.33 |
| Swashbuckle | תיעוד API | 7.2.0 |
| SHA256 | הצפנת סיסמאות | מובנה |
| ProtectedBrowserStorage | אחסון מוצפן | מובנה |
| InputFile | העלאת קבצים | מובנה |
| Regex | Validation | מובנה |
| SignalR | Real-time | מובנה |

---

## קבצי הגדרות (Configuration Files)

### AruroaAPI.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\DBL\DBL.csproj" />
    <ProjectReference Include="..\Models\Models.csproj" />
  </ItemGroup>
</Project>
```

### appsettings.json
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

---

**סיום נספח א'**
