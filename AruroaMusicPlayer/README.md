# 🎵 Aruroa Music Player - WinForms Application

A complete Windows Forms desktop music player that connects to your Aruroa REST API.

## 🎯 Features

- ✅ Load songs from REST API
- ✅ Display songs in a DataGridView
- ✅ Play MP3 audio from base64 data
- ✅ Playback controls (Play, Pause, Stop)
- ✅ Progress bar with seek functionality
- ✅ Time display (current/total)
- ✅ Double-click to play songs
- ✅ Automatic play count tracking

## 🚀 How to Run

### 1. Start the API
```bash
cd AruroaAPI
dotnet run
```
API will run on: `http://localhost:5230`

### 2. Run the WinForms App
```bash
cd AruroaMusicPlayer
dotnet run
```

## 📖 How to Use

1. **Load Songs**: Click the "🔄 Load Songs" button to fetch all songs from the API
2. **Play a Song**: Double-click any song in the list
3. **Controls**:
   - **▶️ Play**: Resume playback if paused
   - **⏸️ Pause**: Pause current playback
   - **⏹️ Stop**: Stop playback completely
4. **Seek**: Drag the progress bar to jump to any position
5. **Status**: Watch the status label for feedback

## 🏗️ Project Structure

```
AruroaMusicPlayer/
├── Models/
│   └── Song.cs                 # Song data model with JSON mapping
├── Services/
│   ├── ApiService.cs           # HTTP client for API calls
│   └── AudioPlayerService.cs   # NAudio wrapper for audio playback
├── Form1.cs                    # Main form logic
├── Form1.Designer.cs           # UI design (auto-generated)
└── Program.cs                  # Application entry point
```

## 📚 What You'll Learn

### 1. **WinForms Basics**
- Creating forms and controls
- Event handling (button clicks, double-clicks)
- DataGridView for displaying data
- Timers for periodic updates

### 2. **API Integration**
- Using HttpClient to call REST APIs
- Async/await for non-blocking operations
- JSON deserialization with Newtonsoft.Json
- Error handling and user feedback

### 3. **Audio Playback**
- NAudio library for MP3 playback
- Converting base64 strings to audio files
- Managing audio state (play, pause, stop)
- Progress tracking and seeking

### 4. **Best Practices**
- Separation of concerns (Models, Services, UI)
- Proper resource disposal (IDisposable)
- Thread-safe UI updates (InvokeRequired)
- User-friendly error messages

## 🔧 Technologies Used

- **.NET 9.0** - Latest .NET framework
- **Windows Forms** - Desktop UI framework
- **NAudio 2.2.1** - Audio playback library
- **Newtonsoft.Json 13.0.3** - JSON serialization
- **HttpClient** - REST API communication

## 🎓 Learning Resources

### WinForms
- [Microsoft WinForms Docs](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
- DataGridView: Display tabular data
- Event Handlers: Respond to user actions
- Timers: Periodic UI updates

### NAudio
- [NAudio Documentation](https://github.com/naudio/NAudio)
- WaveOutEvent: Audio output
- Mp3FileReader: Read MP3 files
- PlaybackState: Track playback status

### HttpClient
- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- Async operations
- Error handling
- Timeout configuration

## 🐛 Troubleshooting

### "Failed to fetch songs"
- Make sure the API is running on `http://localhost:5230`
- Check the API connection string in `ApiService.cs`

### "Failed to play song"
- Ensure the song has valid audio data
- Check that NAudio package is installed
- Verify temp folder permissions

### DataGridView not showing data
- Make sure songs were loaded successfully
- Check the status label for error messages

## 🎨 Customization Ideas

1. **Add Search**: Filter songs by title
2. **Playlists**: Create and manage playlists
3. **Volume Control**: Add a volume slider
4. **Shuffle/Repeat**: Add playback modes
5. **Favorites**: Mark songs as favorites
6. **Dark Theme**: Customize colors and fonts

## 📝 Code Walkthrough

### Loading Songs
```csharp
// ApiService.cs - Fetches songs from API
var songs = await _apiService.GetAllSongsAsync();
dataGridViewSongs.DataSource = songs;
```

### Playing Audio
```csharp
// AudioPlayerService.cs - Plays MP3 from base64
var audioBytes = Convert.FromBase64String(song.AudioData);
File.WriteAllBytes(tempFile, audioBytes);
_mp3Reader = new Mp3FileReader(tempFile);
_waveOut.Init(_mp3Reader);
_waveOut.Play();
```

### Updating UI
```csharp
// Form1.cs - Timer updates progress bar
var current = _audioPlayer.GetCurrentTime();
var total = _audioPlayer.GetTotalTime();
trackBarPosition.Value = (int)((current.TotalSeconds / total.TotalSeconds) * 100);
```

## 🎉 Next Steps

Now that you have a working music player, try:
1. Run the app and explore the code
2. Add breakpoints to understand the flow
3. Modify the UI to your liking
4. Add new features from the customization ideas
5. Learn more about WinForms and NAudio

Happy coding! 🚀
