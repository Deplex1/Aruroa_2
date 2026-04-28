# פרק 6: צד לקוח (Client Side)

## תיאור כללי
צד הלקוח בנוי על **Blazor Server** - טכנולוגיה המאפשרת בניית ממשק משתמש אינטראקטיבי באמצעות C# במקום JavaScript. הממשק מספק חוויה מודרנית ומהירה עם עדכונים בזמן אמת.

---

## סוגי משתמשים והרשאות

### 1. אורח (Guest) - לא מחובר

**הרשאות:**
- ✅ צפייה בדף הבית
- ✅ עיון ברשימת שירים
- ✅ האזנה לשירים
- ✅ חיפוש שירים
- ❌ העלאת שירים
- ❌ יצירת פלייליסטים
- ❌ דירוג שירים

**ממשק:**
```razor
@if (user == null)
{
    <div class="hero-buttons">
        <a href="/login" class="btn btn-primary">Get Started</a>
        <a href="/songs" class="btn btn-secondary">Browse Songs</a>
    </div>
}
```

---

### 2. משתמש רגיל (Regular User) - מחובר

**הרשאות:**
- ✅ כל הרשאות האורח
- ✅ העלאת שירים
- ✅ יצירת ועריכת פלייליסטים
- ✅ דירוג שירים
- ✅ עריכת פרופיל אישי
- ✅ בקשת ז'אנרים חדשים
- ❌ גישה לפאנל ניהול

**ממשק:**
```razor
@if (user != null && user.IsAdmin == 0)
{
    <div class="hero-buttons">
        <a href="/upload" class="btn btn-primary">Upload Song</a>
        <a href="/playlists" class="btn btn-secondary">My Playlists</a>
    </div>
}
```

**בדיקת הרשאה:**
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!firstRender) return;
    
    var result = await Storage.GetAsync<User>("user");
    if (result.Success)
    {
        user = result.Value;
    }
    
    StateHasChanged();
}
```

---

### 3. מנהל מערכת (Admin)

**הרשאות:**
- ✅ כל הרשאות המשתמש הרגיל
- ✅ ניהול משתמשים (מחיקה, שינוי הרשאות)
- ✅ ניהול ז'אנרים (הוספה, מחיקה)
- ✅ אישור/דחיית בקשות ז'אנרים
- ✅ מחיקת שירים של משתמשים אחרים
- ✅ צפייה בסטטיסטיקות מערכת

**ממשק Admin Dashboard:**
```razor
@if (user != null && user.IsAdmin == 1)
{
    <div class="admin-links">
        <button @onclick="GoToManageGenres">Manage Genres</button>
        <button @onclick="GoToManageUsers">Manage Users</button>
        <button @onclick="GoToManageSongs">Manage Songs</button>
        <button @onclick="GoToManageGenresRequest">Genre Requests</button>
    </div>
}
else
{
    <p>You are not an admin.</p>
}
```

**בדיקת הרשאת Admin:**
```csharp
if (user == null || user.IsAdmin == 0)
{
    message = "Access denied. Admin privileges required.";
    return;
}
```

---

## תרשים זרימה בין מסכים

```
                    ┌─────────────┐
                    │  Home Page  │
                    │      /      │
                    └──────┬──────┘
                           │
            ┌──────────────┼──────────────┐
            │              │              │
            ▼              ▼              ▼
      ┌─────────┐    ┌─────────┐   ┌──────────┐
      │  Login  │    │  Songs  │   │ Register │
      │ /login  │    │ /songs  │   │/register │
      └────┬────┘    └────┬────┘   └────┬─────┘
           │              │              │
           └──────────────┼──────────────┘
                          │
                    [User Logged In]
                          │
            ┌─────────────┼─────────────┐
            │             │             │
            ▼             ▼             ▼
      ┌─────────┐   ┌──────────┐  ┌─────────┐
      │ Upload  │   │Playlists │  │ Profile │
      │/upload  │   │/playlists│  │/profile │
      └─────────┘   └──────────┘  └─────────┘
                          │
                    [If IsAdmin = 1]
                          │
                          ▼
                  ┌───────────────┐
                  │Admin Dashboard│
                  │/adminDashboard│
                  └───────┬───────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
  ┌──────────┐     ┌──────────┐     ┌──────────┐
  │  Manage  │     │  Manage  │     │  Manage  │
  │  Users   │     │  Genres  │     │  Songs   │
  └──────────┘     └──────────┘     └──────────┘
```

---

## תיאור ממשקים עיקריים

### 1. דף הבית (Home Page)

**נתיב:** `/`

**תכונות:**
- 🎨 Hero Section עם גרדיאנט צבעוני
- 📊 סטטיסטיקות מערכת (שירים, משתמשים, פלייליסטים, השמעות)
- 🔥 Most Played - 5 השירים הכי פופולריים
- ⭐ Top Rated - 5 השירים עם הדירוג הגבוה ביותר
- 🆕 Recently Uploaded - 5 השירים האחרונים שהועלו
- 💀 Skeleton Loading - אנימציית טעינה

**קוד Hero Section:**
```razor
<div class="home-hero">
    <div class="hero-content">
        <h1 class="hero-title">Aurora Music</h1>
        <p class="hero-subtitle">Discover, Upload, and Share Your Favorite Music</p>
        
        @if (user == null)
        {
            <div class="hero-buttons">
                <a href="/login" class="btn btn-primary">Get Started</a>
                <a href="/songs" class="btn btn-secondary">Browse Songs</a>
            </div>
        }
        else
        {
            <div class="hero-buttons">
                <a href="/upload" class="btn btn-primary">Upload Song</a>
                <a href="/songs" class="btn btn-secondary">Browse Songs</a>
            </div>
        }
    </div>
</div>
```

**קוד Skeleton Loading:**
```razor
@if (isLoading)
{
    <div class="skeleton-grid">
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
    </div>
}
else
{
    @* תוכן אמיתי *@
}
```

**CSS Animation:**
```css
.skeleton-card {
    height: 80px;
    border-radius: 12px;
    background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
    background-size: 200% 100%;
    animation: shimmer 1.5s ease-in-out infinite;
}

@keyframes shimmer {
    0% { background-position: 200% 0; }
    100% { background-position: -200% 0; }
}
```

**הסבר:**
- `isLoading` - משתנה Boolean שמציג Skeleton בזמן טעינה
- `shimmer` - אנימציה שמזיזה גרדיאנט מימין לשמאל
- `StateHasChanged()` - מעדכן את הממשק אחרי טעינת נתונים

---

### 2. דף התחברות (Login)

**נתיב:** `/login`

**שדות:**
- 📝 Username
- 🔒 Password

**קוד:**
```razor
<h3>Login</h3>

<input placeholder="Username" @bind="username" />
<br />
<input placeholder="Password" type="password" @bind="password" />
<br />
<button @onclick="LoginMethod">Login</button>

<p>@message</p>

@code {
    string username;
    string password;
    string message;
    UserDB userDB = new UserDB();

    async Task LoginMethod()
    {
        // בדיקת תקינות
        if (string.IsNullOrWhiteSpace(username) || 
            string.IsNullOrWhiteSpace(password))
        {
            message = "Username and password are required";
            return;
        }

        // ניסיון התחברות
        var user = await userDB.LoginAsync(username, password);
        
        if (user != null)
        {
            // שמירה ב-Session Storage
            await Storage.SetAsync("user", user);
            
            // ניווט לדף הבית
            Nav.NavigateTo("/");
        }
        else
        {
            message = "Invalid username or password";
        }
    }
}
```

**הסבר:**
- `@bind` - קישור דו-כיווני בין Input למשתנה
- `ProtectedSessionStorage` - אחסון מוצפן של פרטי המשתמש
- `NavigationManager` - ניווט בין דפים
- הסיסמה מוצפנת ב-SHA256 לפני השוואה

---

### 3. דף הרשמה (Register)

**נתיב:** `/register`

**שדות:**
- 📝 Username
- 📧 Email
- 🔒 Password

**קוד עם Validation:**
```razor
<h3>Register</h3>

<input placeholder="Username" @bind="username" />
<br />
<input placeholder="Email" @bind="email" />
<br />
<input placeholder="Password" type="password" @bind="password" />
<br />
<button @onclick="RegisterMethod">Register</button>

<p>@message</p>

@code {
    string username;
    string email;
    string password;
    string message;
    UserDB userDB = new UserDB();

    async Task RegisterMethod()
    {
        // בדיקה 1: שדות חובה
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(email))
        {
            message = "All fields are required";
            return;
        }

        // בדיקה 2: פורמט דוא"ל
        if (!IsValidEmail(email))
        {
            message = "Invalid email address";
            return;
        }

        // ביצוע הרשמה
        var result = await userDB.RegisterAsync(username, password, email);

        if (result.Success)
        {
            Nav.NavigateTo("/login");
        }
        else
        {
            message = result.Message;
        }
    }

    // בדיקת תקינות דוא"ל באמצעות Regex
    static bool IsValidEmail(string email)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }
}
```

**הסבר Validation:**
1. **שדות חובה** - `IsNullOrWhiteSpace()` בודק אם השדה ריק
2. **פורמט דוא"ל** - Regex מוודא מבנה תקין של כתובת דוא"ל
3. **כפילויות** - נבדק ב-`RegisterAsync()` במסד הנתונים

---

### 4. דף העלאת שיר (Upload)

**נתיב:** `/upload`

**שדות:**
- 📝 Song Title
- 🎵 Genres (Multiple Selection)
- 📁 Audio File
- ➕ Request New Genre

**קוד:**
```razor
<div class="upload-form">
    <div class="form-group">
        <label for="title">Song Title</label>
        <input id="title" placeholder="Enter song title" @bind="title" />
    </div>

    <div class="form-group">
        <label>Genres (Select multiple)</label>
        <div class="genre-checkboxes">
            @for (int i = 0; i < availableGenres.Count; i++)
            {
                int index = i;
                Genre currentGenre = availableGenres[index];
                bool isChecked = IsGenreSelected(currentGenre.genreid);

                <div class="genre-checkbox-item">
                    <input type="checkbox"
                           id="genre-@currentGenre.genreid"
                           checked="@isChecked"
                           @onchange="(e) => ToggleGenre(currentGenre.genreid, e)" />
                    <label for="genre-@currentGenre.genreid">@currentGenre.name</label>
                </div>
            }
        </div>
    </div>

    <div class="form-group">
        <label for="audioFile">Audio File</label>
        <InputFile id="audioFile" OnChange="OnFileSelected" accept="audio/*" />
        @if (selectedFile != null)
        {
            <p class="file-info">
                Selected: @selectedFile.Name (@FormatFileSize(selectedFile.Size))
            </p>
        }
    </div>

    <button @onclick="UploadSong" disabled="@(isUploading)" class="btn">
        @if (isUploading)
        {
            <span>Uploading...</span>
        }
        else
        {
            <span>Upload Song</span>
        }
    </button>
</div>
```

**קוד Validation:**
```csharp
private async Task UploadSong()
{
    // בדיקה 1: משתמש מחובר
    if (user == null)
    {
        message = "You must be logged in to upload";
        return;
    }

    // בדיקה 2: כותרת לא ריקה
    if (string.IsNullOrWhiteSpace(title))
    {
        message = "Song title is required";
        return;
    }

    // בדיקה 3: לפחות ז'אנר אחד
    if (selectedGenreIds.Count == 0)
    {
        message = "Please select at least one genre";
        return;
    }

    // בדיקה 4: קובץ נבחר
    if (selectedFile == null)
    {
        message = "Please select an audio file";
        return;
    }

    // בדיקה 5: גודל קובץ (מקסימום 50MB)
    if (selectedFile.Size > 50 * 1024 * 1024)
    {
        message = "File size must be less than 50MB";
        return;
    }

    // הצגת מצב טעינה
    isUploading = true;
    StateHasChanged();

    // ביצוע העלאה
    bool success = await uploadService.UploadSongAsync(
        user, title, selectedGenreIds, selectedFile
    );

    if (success)
    {
        message = "Song uploaded successfully!";
        
        // ניקוי טופס
        title = "";
        selectedFile = null;
        selectedGenreIds.Clear();
        
        // ניווט לדף שירים
        await Task.Delay(2000);
        Nav.NavigateTo("/songs");
    }
    else
    {
        message = "Upload failed. Please try again.";
    }

    isUploading = false;
    StateHasChanged();
}
```

**טיפול בקובץ:**
```csharp
private void OnFileSelected(InputFileChangeEventArgs e)
{
    selectedFile = e.File;
    StateHasChanged();
}

private string FormatFileSize(long bytes)
{
    if (bytes < 1024)
        return bytes + " B";
    else if (bytes < 1024 * 1024)
        return (bytes / 1024.0).ToString("0.0") + " KB";
    else
        return (bytes / (1024.0 * 1024.0)).ToString("0.0") + " MB";
}
```

**הסבר:**
- `InputFile` - רכיב Blazor לבחירת קבצים
- `accept="audio/*"` - מגביל לקבצי אודיו בלבד
- `disabled="@(isUploading)"` - מנטרל כפתור בזמן העלאה
- `StateHasChanged()` - מעדכן את הממשק

---

### 5. פאנל ניהול משתמשים (Admin)

**נתיב:** `/adminDashboard/manageUsers`

**תכונות:**
- 📋 טבלת משתמשים
- 🔄 Toggle Admin Status
- 🗑️ Delete User
- 🔒 הגנה מפני מחיקה עצמית

**קוד:**
```razor
@if (user == null || user.IsAdmin == 0)
{
    <p>You do not have permission to view this page.</p>
}
else
{
    <table class="users-table">
        <thead>
            <tr>
                <th>ID</th>
                <th>Username</th>
                <th>Email</th>
                <th>Is Admin</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @for (int i = 0; i < users.Count; i++)
            {
                int currentIndex = i;
                <tr>
                    <td>@users[currentIndex].userid</td>
                    <td>@users[currentIndex].username</td>
                    <td>@users[currentIndex].email</td>
                    <td>
                        @if (users[currentIndex].IsAdmin == 1)
                        {
                            @:Yes
                        }
                        else
                        {
                            @:No
                        }
                    </td>
                    <td>
                        <button @onclick="() => ToggleAdminByIndex(currentIndex)">
                            Toggle Admin
                        </button>
                        <button @onclick="() => DeleteUserByIndex(currentIndex)">
                            Delete
                        </button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}
```

**קוד מחיקה עם Validation:**
```csharp
private async Task DeleteUserByIndex(int index)
{
    User targetUser = users[index];

    // בדיקה 1: לא ניתן למחוק את עצמך
    if (targetUser.userid == user.userid)
    {
        message = "You cannot delete yourself";
        return;
    }

    // בדיקה 2: אישור מחיקה
    bool confirmed = await ConfirmAsync("Are you sure you want to delete this user?");
    if (!confirmed)
    {
        return;
    }

    // ביצוע מחיקה
    bool success = await userService.DeleteUserAsync(targetUser.userid);
    
    if (success)
    {
        message = "User deleted successfully.";
        await LoadUsers(); // רענון רשימה
    }
    else
    {
        message = "Error deleting user.";
    }
}
```

**קוד Toggle Admin:**
```csharp
private async Task ToggleAdminByIndex(int index)
{
    User selectedUser = users[index];

    // בדיקה: לא ניתן לשנות את ההרשאות שלך
    if (selectedUser.userid == user.userid)
    {
        message = "You cannot change your own admin status";
        return;
    }

    // ביצוע שינוי
    bool success = await userService.ToggleAdminStatusAsync(selectedUser);
    
    if (success)
    {
        await LoadUsers();
        message = "User admin status updated successfully.";
    }
    else
    {
        message = "Error updating user.";
    }
}
```

---

## בדיקת תקינות (Validation)

### 1. Validation בצד לקוח (Client-Side)

#### א. בדיקת שדות חובה
```csharp
if (string.IsNullOrWhiteSpace(username))
{
    message = "Username is required";
    return;
}
```

**יתרונות:**
- ✅ תגובה מיידית למשתמש
- ✅ חוסך בקשות שרת מיותרות
- ✅ חוויית משתמש טובה יותר

---

#### ב. בדיקת פורמט דוא"ל
```csharp
static bool IsValidEmail(string email)
{
    return System.Text.RegularExpressions.Regex.IsMatch(
        email,
        @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );
}
```

**הסבר Regex:**
- `[A-Z0-9._%+-]+` - שם משתמש (אותיות, מספרים, תווים מיוחדים)
- `@` - סימן @
- `[A-Z0-9.-]+` - שם דומיין
- `\.[A-Z]{2,}` - סיומת (לפחות 2 אותיות)

**דוגמאות:**
- ✅ `user@example.com`
- ✅ `john.doe@company.co.il`
- ❌ `invalid@`
- ❌ `@example.com`

---

#### ג. בדיקת גודל קובץ
```csharp
if (selectedFile.Size > 50 * 1024 * 1024) // 50MB
{
    message = "File size must be less than 50MB";
    return;
}
```

**המרות:**
- `1 KB = 1024 bytes`
- `1 MB = 1024 KB = 1,048,576 bytes`
- `50 MB = 52,428,800 bytes`

---

#### ד. בדיקת סוג קובץ
```html
<InputFile accept="audio/*" />
```

**הסבר:**
- `accept="audio/*"` - מגביל לקבצי אודיו בלבד
- הדפדפן מציג רק קבצים מתאימים בחלון הבחירה
- **חשוב:** זו בדיקה ברמת UI בלבד, לא אבטחה מלאה

---

### 2. Validation בצד שרת (Server-Side)

#### א. בדיקת כפילויות
```csharp
public async Task<(bool Success, string Message)> RegisterAsync(
    string username, string password, string email)
{
    // בדיקת שם משתמש קיים
    if (await UsernameExistsAsync(username))
        return (false, "Username already taken.");
    
    // בדיקת דוא"ל קיים
    if (await EmailExistsAsync(email))
        return (false, "Email already registered.");
    
    // המשך הרשמה...
}
```

---

#### ב. בדיקת הרשאות
```csharp
if (user == null || user.IsAdmin == 0)
{
    return (false, "Access denied. Admin privileges required.");
}
```

---

#### ג. בדיקת קיום משאב
```csharp
var song = await _songDB.SelectByIdAsync(id);
if (song == null)
{
    return NotFound(new { message = $"Song with ID {id} not found" });
}
```

---

### 3. Validation מורכבת

#### א. מניעת מחיקה עצמית
```csharp
if (targetUser.userid == currentUser.userid)
{
    return (false, "You cannot delete yourself");
}
```

---

#### ב. בדיקת בעלות
```csharp
if (playlist.userid != currentUser.userid && currentUser.IsAdmin == 0)
{
    return (false, "You can only edit your own playlists");
}
```

---

#### ג. בדיקת מינימום בחירות
```csharp
if (selectedGenreIds.Count == 0)
{
    message = "Please select at least one genre";
    return;
}
```

---

## הודעות משתמש (User Feedback)

### 1. הודעות הצלחה
```razor
@if (message.Contains("success"))
{
    <div class="message message-success">
        @message
    </div>
}
```

**CSS:**
```css
.message-success {
    background-color: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
    padding: 12px;
    border-radius: 8px;
    margin-top: 16px;
}
```

---

### 2. הודעות שגיאה
```razor
@if (!string.IsNullOrEmpty(message) && !message.Contains("success"))
{
    <div class="message message-error">
        @message
    </div>
}
```

**CSS:**
```css
.message-error {
    background-color: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
    padding: 12px;
    border-radius: 8px;
    margin-top: 16px;
}
```

---

### 3. מצבי טעינה
```razor
<button @onclick="UploadSong" disabled="@(isUploading)">
    @if (isUploading)
    {
        <span>Uploading...</span>
    }
    else
    {
        <span>Upload Song</span>
    }
</button>
```

**CSS:**
```css
button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}
```

---

## טכנולוגיות ותכונות

### Blazor Server
- ✅ **C# במקום JavaScript** - קוד אחיד בשרת ובלקוח
- ✅ **SignalR** - תקשורת בזמן אמת
- ✅ **Component-Based** - רכיבים לשימוש חוזר
- ✅ **Data Binding** - `@bind` לקישור דו-כיווני

### Protected Session Storage
```csharp
// שמירה
await Storage.SetAsync("user", user);

// קריאה
var result = await Storage.GetAsync<User>("user");
if (result.Success)
{
    user = result.Value;
}
```

**יתרונות:**
- 🔒 הצפנה אוטומטית
- 💾 נשמר בזיכרון הדפדפן
- 🔄 נמחק בסגירת הדפדפן

### Navigation Manager
```csharp
@inject NavigationManager Nav

// ניווט פשוט
Nav.NavigateTo("/songs");

// ניווט עם Force Reload
Nav.NavigateTo("/songs", forceLoad: true);
```

---

## סיכום תכונות UI/UX

### עיצוב מודרני
- 🎨 **Gradient Backgrounds** - גרדיאנטים צבעוניים
- 🌓 **Dark Mode** - ממשק כהה נעים לעין
- 📱 **Responsive Design** - מתאים לכל מסך
- ✨ **Animations** - אנימציות חלקות

### חוויית משתמש
- ⚡ **Fast Loading** - Skeleton Loading
- 💬 **Clear Feedback** - הודעות ברורות
- 🔒 **Secure** - הצפנה והרשאות
- ♿ **Accessible** - נגיש לכולם

---

**סיום פרק 6**
