using AruroaMusicPlayer.Models;
using AruroaMusicPlayer.Services;

namespace AruroaMusicPlayer;

public partial class Form1 : Form
{
    private readonly ApiService _apiService;
    private readonly AudioPlayerService _audioPlayer;
    private List<Song> _songs = new();
    private Song? _currentSong;

    public Form1()
    {
        InitializeComponent();
        _apiService = new ApiService();
        _audioPlayer = new AudioPlayerService();
        _audioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;
        
        // Disable player controls initially
        SetPlayerControlsEnabled(false);
    }

    private async void btnLoadSongs_Click(object sender, EventArgs e)
    {
        await LoadSongsAsync(() => _apiService.GetAllSongsAsync(), "all songs");
    }

    private async void btnPopular_Click(object sender, EventArgs e)
    {
        await LoadSongsAsync(() => _apiService.GetPopularSongsAsync(), "popular songs");
    }

    private async void btnNew_Click(object sender, EventArgs e)
    {
        await LoadSongsAsync(() => _apiService.GetNewSongsAsync(), "new songs");
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            MessageBox.Show("Please enter a search term", "Search", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await LoadSongsAsync(() => _apiService.SearchSongsAsync(txtSearch.Text), 
            $"search results for '{txtSearch.Text}'");
    }

    private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            btnSearch_Click(sender, e);
            e.Handled = true;
        }
    }

    private async Task LoadSongsAsync(Func<Task<List<Song>>> loadFunction, string description)
    {
        try
        {
            DisableAllButtons(true);
            lblStatus.Text = $"Loading {description}...";
            lblStatus.ForeColor = Color.Blue;

            _songs = await loadFunction();

            if (_songs.Count > 0)
            {
                // Configure DataGridView
                dataGridViewSongs.DataSource = null;
                dataGridViewSongs.DataSource = _songs;
                
                // Hide unwanted columns
                dataGridViewSongs.Columns["AudioData"]!.Visible = false;
                dataGridViewSongs.Columns["UserId"]!.Visible = false;
                dataGridViewSongs.Columns["SongId"]!.Visible = false;
                dataGridViewSongs.Columns["Uploaded"]!.Visible = false;
                dataGridViewSongs.Columns["Duration"]!.Visible = false;
                
                // Rename columns
                dataGridViewSongs.Columns["Title"]!.HeaderText = "Song Title";
                dataGridViewSongs.Columns["DurationFormatted"]!.HeaderText = "Duration";
                dataGridViewSongs.Columns["UploadedFormatted"]!.HeaderText = "Upload Date";
                dataGridViewSongs.Columns["Plays"]!.HeaderText = "Plays";
                
                // Auto-size columns
                dataGridViewSongs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                lblStatus.Text = $"✅ Loaded {_songs.Count} {description}";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = $"No {description} found";
                lblStatus.ForeColor = Color.Orange;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ Error: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"Failed to load {description}:\n{ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            DisableAllButtons(false);
        }
    }

    private void dataGridViewSongs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            var song = _songs[e.RowIndex];
            PlaySong(song);
        }
    }

    private async void dataGridViewSongs_SelectionChanged(object sender, EventArgs e)
    {
        if (dataGridViewSongs.SelectedRows.Count > 0)
        {
            var selectedIndex = dataGridViewSongs.SelectedRows[0].Index;
            if (selectedIndex >= 0 && selectedIndex < _songs.Count)
            {
                var song = _songs[selectedIndex];
                await LoadGenresForSong(song.SongId);
            }
        }
    }

    private async Task LoadGenresForSong(int songId)
    {
        try
        {
            var genres = await _apiService.GetSongGenresAsync(songId);
            if (genres.Count > 0)
            {
                lblGenres.Text = $"Genres: {string.Join(", ", genres.Select(g => g.Name))}";
            }
            else
            {
                lblGenres.Text = "Genres: None";
            }
        }
        catch
        {
            lblGenres.Text = "Genres: -";
        }
    }

    private void PlaySong(Song song)
    {
        try
        {
            _currentSong = song;
            _audioPlayer.PlaySong(song);
            
            lblNowPlaying.Text = $"🎵 Now Playing: {song.Title}";
            lblStatus.Text = $"▶️ Playing: {song.Title}";
            lblStatus.ForeColor = Color.Green;
            
            SetPlayerControlsEnabled(true);
            timerUpdate.Start();
            
            // Increment play count in background
            _ = _apiService.IncrementPlayCountAsync(song.SongId);
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ Playback error: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"Failed to play song:\n{ex.Message}", "Playback Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnPlay_Click(object sender, EventArgs e)
    {
        if (_audioPlayer.IsPaused)
        {
            _audioPlayer.Resume();
            timerUpdate.Start();
            lblStatus.Text = "▶️ Playing";
            lblStatus.ForeColor = Color.Green;
        }
        else if (_currentSong != null)
        {
            PlaySong(_currentSong);
        }
    }

    private void btnPause_Click(object sender, EventArgs e)
    {
        if (_audioPlayer.IsPlaying)
        {
            _audioPlayer.Pause();
            timerUpdate.Stop();
            lblStatus.Text = "⏸️ Paused";
            lblStatus.ForeColor = Color.Orange;
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _audioPlayer.Stop();
        timerUpdate.Stop();
        trackBarPosition.Value = 0;
        lblCurrentTime.Text = "00:00";
        lblStatus.Text = "⏹️ Stopped";
        lblStatus.ForeColor = Color.Gray;
    }

    private void timerUpdate_Tick(object sender, EventArgs e)
    {
        if (_audioPlayer.IsPlaying)
        {
            var current = _audioPlayer.GetCurrentTime();
            var total = _audioPlayer.GetTotalTime();

            if (total.TotalSeconds > 0)
            {
                trackBarPosition.Value = (int)((current.TotalSeconds / total.TotalSeconds) * 100);
            }

            lblCurrentTime.Text = current.ToString(@"mm\:ss");
            lblTotalTime.Text = total.ToString(@"mm\:ss");
        }
    }

    private void trackBarPosition_Scroll(object sender, EventArgs e)
    {
        if (_audioPlayer.IsPlaying || _audioPlayer.IsPaused)
        {
            var total = _audioPlayer.GetTotalTime();
            var newPosition = TimeSpan.FromSeconds((trackBarPosition.Value / 100.0) * total.TotalSeconds);
            _audioPlayer.SetPosition(newPosition);
        }
    }

    private void AudioPlayer_PlaybackStopped(object? sender, EventArgs e)
    {
        // Invoke on UI thread
        if (InvokeRequired)
        {
            Invoke(new Action(() => AudioPlayer_PlaybackStopped(sender, e)));
            return;
        }

        timerUpdate.Stop();
        trackBarPosition.Value = 0;
        lblCurrentTime.Text = "00:00";
        lblStatus.Text = "⏹️ Playback finished";
        lblStatus.ForeColor = Color.Gray;
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        btnPlay.Enabled = enabled;
        btnPause.Enabled = enabled;
        btnStop.Enabled = enabled;
        trackBarPosition.Enabled = enabled;
    }

    private void DisableAllButtons(bool disable)
    {
        btnLoadSongs.Enabled = !disable;
        btnPopular.Enabled = !disable;
        btnNew.Enabled = !disable;
        btnSearch.Enabled = !disable;
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        _audioPlayer.Dispose();
    }
}
