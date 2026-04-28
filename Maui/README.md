# Aruroa Music MAUI App

A simple .NET MAUI mobile app that connects to the Aruroa Music API.

## Features

- 📱 Cross-platform (Android, iOS, Windows)
- 🎵 Display all songs from the API
- ⚡ Simple and clean UI
- 🔄 Real-time data from REST API

## How to Run

### 1. Start the API First

```bash
cd AruroaAPI
dotnet run
```

The API should be running on `http://localhost:5230`

### 2. Run the MAUI App

**For Windows:**
```bash
cd Maui
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

**For Android Emulator:**
```bash
cd Maui
dotnet build -t:Run -f net9.0-android
```

## Usage

1. Open the app
2. Click "Load Songs" button
3. View all songs from your database

## API Connection

- **Windows**: Uses `http://localhost:5230/api`
- **Android Emulator**: Uses `http://10.0.2.2:5230/api` (special address for emulator to access host machine)

## Project Structure

```
Maui/
├── Models/
│   └── Song.cs              # Song data model
├── Services/
│   └── ApiService.cs        # HTTP client for API calls
├── MainPage.xaml            # UI layout
└── MainPage.xaml.cs         # UI logic
```

## Requirements

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code with MAUI workload
- Running AruroaAPI on localhost:5230
