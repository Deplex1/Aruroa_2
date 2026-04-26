# 🎵 Aurora Music Platform

A full-stack music streaming and management platform built with **Blazor Server** and **MySQL**. Upload, discover, rate, and organize your favorite music with a modern, responsive interface.

---

## 📋 Table of Contents

- [Features](#features)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Setup Instructions](#setup-instructions)
- [Database Schema](#database-schema)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Screenshots](#screenshots)
- [Future Enhancements](#future-enhancements)

---

## ✨ Features

### **Core Functionality**
- 🎧 **Audio Streaming** - Upload and stream MP3, WAV, and AAC files
- 📝 **Playlist Management** - Create, edit, and organize custom playlists
- ⭐ **Rating System** - 5-star rating with average calculations
- 🏷️ **Multi-Genre Tagging** - Songs can have multiple genres
- 🔍 **Advanced Search & Filtering** - Search by title, filter by genres (AND logic)
- 🎵 **Global Audio Player** - Persistent player with queue management across all pages

### **User Features**
- 👤 **User Authentication** - Secure login and registration
- 📊 **User Profiles** - View statistics and manage uploads
- 🎨 **Profile Customization** - Upload profile pictures
- 📱 **Responsive Design** - Works on desktop and mobile

### **Admin Panel**
- 👥 **User Management** - View, edit, and manage users
- 🎵 **Song Moderation** - Manage uploaded songs
- 🏷️ **Genre Management** - Add, edit, and delete genres
- 📋 **Genre Requests** - Review user-requested genres
- 📊 **Dashboard** - Site-wide statistics

### **Advanced Features**
- 🎯 **Context Menu** - Right-click songs to add to playlists
- 🔔 **Toast Notifications** - User feedback for all actions
- ⏳ **Loading States** - Spinners and progress indicators
- 🎨 **Modern UI** - Dark themes, gradients, smooth animations
- ♿ **Accessibility** - Keyboard navigation and ARIA labels

---

## 🛠️ Technologies

### **Backend**
- **ASP.NET Core 9.0** - Blazor Server framework
- **C# 12** - Programming language
- **MySQL 8.0** - Relational database
- **MySql.Data** - Database connector

### **Frontend**
- **Blazor Server** - Component-based UI framework
- **CSS3** - Styling with gradients and animations
- **JavaScript** - Audio player controls only

### **Architecture**
- **3-Layer Architecture**:
  - **DBL** (Database Layer) - Direct database access
  - **Services** (Business Logic Layer) - Business rules and validation
  - **Razor Components** (Presentation Layer) - UI components

---

## 🏗️ Architecture

### **Design Principles**

1. **SQL-First Approach**
   - All filtering, sorting, and aggregations done in SQL
   - Uses `JOIN`, `GROUP BY`, `HAVING`, `IN` clauses
   - Avoids N+1 query problems

2. **No LINQ Policy**
   - Explicit `for` loops instead of LINQ methods
   - Beginner-friendly, verbose code
   - Better understanding of algorithms

3. **Explicit Code Style**
   - No lambda expressions (`=>`)
   - No ternary operators in C# code
   - Full method bodies with braces
   - Detailed comments explaining logic

### **Layer Responsibilities**

**DBL (Database Layer)**
- Inherits from `BaseDB<T>`
- Contains only SQL queries
- Returns data models
- No business logic

**Services (Business Logic Layer)**
- Validates input
- Implements business rules
- Calls DBL methods
- Returns processed data

**Razor Components (Presentation Layer)**
- UI rendering only
- Calls Service methods
- Simple formatting helpers
- No database or business logic

---

## 🚀 Setup Instructions

### **Prerequisites**
- .NET 9.0 SDK
- MySQL 8.0 or higher
- Visual Studio 2022 or VS Code

### **Step 1: Clone Repository**
```bash
git clone https://github.com/yourusername/aurora-music.git
cd aurora-music
```

### **Step 2: Setup Database**

1. Create MySQL database:
```sql
CREATE DATABASE auroradb;
```

2. Run the SQL setup script (create tables):
```sql
-- See Database/setup.sql for full schema
```

### **Step 3: Configure Connection String**

Update `appsettings.json` in `AruroaBlazor` project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;user id=root;password=YOUR_PASSWORD;database=auroradb"
  }
}
```

### **Step 4: Build and Run**
```bash
dotnet restore
dotnet build
dotnet run --project AruroaBlazor/AruroaBlazor.csproj
```

### **Step 5: Access Application**
Open browser and navigate to:
```
https://localhost:5001
```

### **Default Admin Account**
- Username: `admin`
- Password: `admin123`

---

## 🗄️ Database Schema

### **Main Tables**

**users**
- `userid` (PK)
- `username`
- `password` (SHA256 hashed)
- `email`
- `profilepicture` (BLOB)
- `IsAdmin` (0 or 1)

**songs**
- `songid` (PK)
- `title`
- `duration` (seconds)
- `audioData` (BLOB - MP3/WAV/AAC)
- `userid` (FK → users)
- `uploaded` (DATETIME)
- `plays` (INT)

**genres**
- `genreid` (PK)
- `name`

**song_genres** (Junction Table)
- `songid` (FK → songs)
- `genreid` (FK → genres)

**playlists**
- `playlistid` (PK)
- `name`
- `userid` (FK → users)
- `ispublic` (BOOLEAN)
- `created` (DATETIME)

**playlist_songs** (Junction Table)
- `playlistid` (FK → playlists)
- `songid` (FK → songs)
- `position` (INT)
- `dateadded` (DATETIME)

**ratings**
- `ratingid` (PK)
- `userid` (FK → users)
- `songid` (FK → songs)
- `rating` (1-5)
- `daterated` (DATETIME)

**genre_requests**
- `requestid` (PK)
- `userid` (FK → users)
- `genre_name`
- `requested_date` (DATETIME)
- `status` (pending/approved/rejected)
- `reviewed_by` (FK → users)
- `reviewed_date` (DATETIME)

---

## 📁 Project Structure

```
Aruroa/
├── AruroaBlazor/              # Main Blazor application
│   ├── Components/
│   │   ├── Layout/            # Layout components (NavMenu, AudioPlayer, Toast)
│   │   └── Pages/             # Page components
│   │       ├── Admin/         # Admin panel pages
│   │       ├── Home.razor
│   │       ├── Songs.razor
│   │       ├── Playlists.razor
│   │       ├── Upload.razor
│   │       └── Profile.razor
│   ├── wwwroot/               # Static files (CSS, JS)
│   └── Program.cs
├── DBL/                       # Database Layer
│   ├── BaseDB.cs              # Base database class
│   ├── SongsDB.cs
│   ├── UserDB.cs
│   ├── PlaylistDB.cs
│   └── ...
├── Services/                  # Business Logic Layer
│   ├── SongService.cs
│   ├── PlaylistService.cs
│   ├── ToastService.cs
│   └── ...
├── Models/                    # Data models
│   ├── Song.cs
│   ├── User.cs
│   ├── Playlist.cs
│   └── ...
└── README.md
```

---

## 📝 Coding Standards

### **C# Style Rules**

❌ **Never Use:**
- Lambda expressions (`=>`)
- Ternary operators (`? :`) in C# code
- LINQ methods (`.Where()`, `.Select()`, etc.)
- `foreach` loops in `@code` sections
- Expression-bodied members

✅ **Always Use:**
- Explicit `if/else` blocks
- `for` loops with explicit index: `for (int i = 0; i < count; i = i + 1)`
- Explicit return statements
- Full method bodies with braces `{ }`
- Detailed comments

### **SQL-First Approach**

✅ **Do:**
- Use SQL `JOIN` instead of loading tables separately
- Use SQL `ORDER BY` instead of sorting in C#
- Use SQL `COUNT()`, `AVG()`, `SUM()` instead of calculating in C#
- Use SQL `WHERE` with `LOWER()` for case-insensitive comparisons
- Use SQL `IN` clause for batch operations
- Use SQL `GROUP BY` for grouping data

❌ **Don't:**
- Load all data then filter in C# with loops
- Run queries inside loops (N+1 query problem)

---

## 📸 Screenshots

### Home Page
![Home Page](screenshots/home.png)

### Songs Page with Filters
![Songs Page](screenshots/songs.png)

### Playlist Management
![Playlists](screenshots/playlists.png)

### Admin Dashboard
![Admin Dashboard](screenshots/admin.png)

---

## 🔮 Future Enhancements

### **Planned Features**
- [ ] Social features (follow users, see their uploads)
- [ ] Lyrics display
- [ ] Album support
- [ ] Artist profiles
- [ ] Collaborative playlists
- [ ] Song recommendations based on listening history
- [ ] Export playlists
- [ ] Keyboard shortcuts (Space = play/pause)
- [ ] Dark mode toggle
- [ ] Mobile app (Blazor Hybrid)

### **Technical Improvements**
- [ ] Caching layer (Redis)
- [ ] CDN for audio files
- [ ] Real-time notifications (SignalR)
- [ ] Unit tests
- [ ] Integration tests
- [ ] CI/CD pipeline
- [ ] Docker containerization

---

## 👨‍💻 Author

**Your Name**
- School: [Your School Name]
- Course: [Course Name]
- Year: 2026

---

## 📄 License

This project is created for educational purposes as part of a school project.

---

## 🙏 Acknowledgments

- ASP.NET Core Team for Blazor framework
- MySQL for the database
- All open-source contributors

---

## 📞 Support

For questions or issues, please contact:
- Email: your.email@example.com
- GitHub Issues: [Project Issues](https://github.com/yourusername/aurora-music/issues)

---

**Made with ❤️ using Blazor Server and MySQL**
