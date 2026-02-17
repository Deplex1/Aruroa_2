@using Models
@using Services
@implements IDisposable

@* 
    AudioPlayer.razor - Global audio player shown at bottom of ALL pages
    
    This component:
    1. Listens to GlobalAudioPlayerService for state changes
    2. Controls the actual HTML <audio> element
    3. Shows currently playing song info
    4. Has play/pause, next, previous, volume, progress controls
    
    It is added to MainLayout.razor so it appears on every page
*@

@if (currentSong != null)
{
    <div class= "global-audio-player" >
        < div class= "player-container" >

            @* ===== SONG INFO SECTION ===== *@
            < div class= "song-info-section" >
                < div class= "song-title" > @currentSong.title </ div >
                < div class= "song-duration" > @FormatTime(currentTime) / @FormatTime(duration) </ div >
            </ div >

            @* ===== CONTROLS SECTION ===== *@
            < div class= "controls-section" >

                @* Playback buttons - previous, play/pause, next *@
                < div class= "playback-controls" >
                    < button class= "control-btn" @onclick = "Previous" title = "Previous" >
                        ⏮
                    </ button >

                    < button class= "control-btn play-btn" @onclick = "TogglePlayPause" title = "Play/Pause" >
                        @if(isPlaying)
                        {
                            < span >⏸</ span >
                        }
                        else
{
                            < span >▶</ span >
                        }
                    </ button >

                    < button class= "control-btn" @onclick = "Next" title = "Next" >
                        ⏭
                    </ button >
                </ div >

                @* Progress bar - shows how far through the song we are *@
                < div class= "progress-section" >
                    < input type = "range"
                           class= "progress-bar"
                           min = "0"
                           max = "@duration"
                           value = "@currentTime"
                           @oninput = "OnProgressChanged" />
                </ div >
            </ div >

            @* ===== VOLUME SECTION ===== *@
            < div class= "volume-section" >
                < span class= "volume-icon" >🔊</ span >
                < input type = "range"
                       class= "volume-slider"
                       min = "0"
                       max = "100"
                       value = "@volumePercent"
                       @oninput = "OnVolumeChanged" />
                < span class= "volume-label" > @volumePercent %</ span >
            </ div >

            @* ===== QUEUE SECTION ===== *@
            < div class= "queue-info" >
                @{
    int queueSize = GlobalAudioPlayerService.GetPlayQueue().Count;
    int queueIndex = GlobalAudioPlayerService.GetCurrentQueueIndex();
}
                < span class= "queue-label" >@(queueIndex + 1) / @queueSize in queue </ span >
                < button class= "clear-queue-btn" @onclick = "ClearQueue" title = "Clear Queue" >
                    ✕ Clear
                </ button >
            </ div >
        </ div >

        @* 
            The ACTUAL audio element that plays the sound
            controls="false" hides the default browser controls
            We use our own custom controls above instead
            
            The @on events let Blazor know what's happening with the audio:
            - ontimeupdate: fires every second to update progress bar
            - onended: fires when song finishes so we can play next
            - onloadedmetadata: fires when song loads so we know duration
        *@
        < audio id = "global-audio-player"
               src = "@audioSource"
               @ontimeupdate = "OnTimeUpdate"
               @onended = "OnSongEnded"
               @onloadedmetadata = "OnMetadataLoaded"
               style = "display:none" >
        </ audio >
    </ div >
}

< style >
    /* Fixed player at bottom of screen */
    .global - audio - player {
position: fixed;
    bottom: 0;
left: 0;
right: 0;
background: linear - gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
padding: 12px 20px;
    z - index: 9999;
    box - shadow: 0 - 4px 20px rgba(0,0,0,0.3);
}

    .player - container {
display: flex;
    align - items: center;
gap: 20px;
    max - width: 1400px;
margin: 0 auto;
}

    /* Song info */
    .song - info - section {
    min - width: 200px;
    max - width: 250px;
}

    .song - title {
    font - weight: 600;
    font - size: 14px;
    white - space: nowrap;
overflow: hidden;
    text - overflow: ellipsis;
color: white;
}

    .song - duration {
    font - size: 12px;
opacity: 0.8;
    margin - top: 2px;
}

    /* Controls */
    .controls - section {
flex: 1;
display: flex;
    flex - direction: column;
    align - items: center;
gap: 6px;
}

    .playback - controls {
display: flex;
    align - items: center;
gap: 12px;
}

    .control - btn {
background: rgba(255, 255, 255, 0.2);
border: none;
color: white;
width: 36px;
height: 36px;
    border - radius: 50 %;
    font - size: 14px;
cursor: pointer;
transition: all 0.2s;
display: flex;
    align - items: center;
    justify - content: center;
}

    .control - btn:hover {
        background: rgba(255, 255, 255, 0.4);
transform: scale(1.1);
    }

    .play - btn {
width: 44px;
height: 44px;
    font - size: 18px;
background: rgba(255, 255, 255, 0.3);
}

    /* Progress bar */
    .progress - section {
width: 100 %;
    max - width: 400px;
}

    .progress - bar {
width: 100 %;
height: 4px;
    -webkit - appearance: none;
appearance: none;
background: rgba(255, 255, 255, 0.3);
    border - radius: 2px;
outline: none;
cursor: pointer;
}

    .progress - bar::- webkit - slider - thumb {
    -webkit - appearance: none;
appearance: none;
width: 12px;
height: 12px;
    border - radius: 50 %;
background: white;
cursor: pointer;
}

    /* Volume */
    .volume - section {
display: flex;
    align - items: center;
gap: 8px;
    min - width: 140px;
}

    .volume - icon {
    font - size: 16px;
}

    .volume - slider {
width: 80px;
height: 4px;
    -webkit - appearance: none;
appearance: none;
background: rgba(255, 255, 255, 0.3);
    border - radius: 2px;
outline: none;
cursor: pointer;
}

    .volume - slider::- webkit - slider - thumb {
    -webkit - appearance: none;
appearance: none;
width: 12px;
height: 12px;
    border - radius: 50 %;
background: white;
cursor: pointer;
}

    .volume - label {
    font - size: 12px;
    min - width: 35px;
opacity: 0.9;
}

    /* Queue info */
    .queue - info {
display: flex;
    flex - direction: column;
    align - items: center;
gap: 4px;
    min - width: 100px;
}

    .queue - label {
    font - size: 11px;
opacity: 0.8;
}

    .clear - queue - btn {
background: rgba(255, 255, 255, 0.15);
border: 1px solid rgba(255, 255, 255, 0.3);
color: white;
padding: 4px 8px;
    border - radius: 4px;
    font - size: 11px;
cursor: pointer;
transition: all 0.2s;
}

    .clear - queue - btn:hover {
        background: rgba(255, 255, 255, 0.3);
    }

    /* Push page content up so player doesn't cover it */
    :global(body) {
    padding - bottom: 80px;
}

/* Responsive */
@@media(max - width: 768px) {
        .player - container {
        flex - wrap: wrap;
    gap: 10px;
    }

        .song - info - section {
        min - width: 150px;
    }

        .volume - section {
        min - width: 100px;
    }
}
</ style >

@code {
    // Currently playing song (null if nothing playing)
    private Song? currentSong = null;

// Playback state
private bool isPlaying = false;

// Time tracking
private double currentTime = 0;
private double duration = 0;

// Volume (0-100 for the slider display)
private int volumePercent = 70;

// The audio source URL (base64 data URL)
private string audioSource = "";

// Called when component first initializes
// Subscribe to the global player service events
protected override void OnInitialized()
{
    // Subscribe to state changes from the global player service
    // When any page calls PlayNow() or AddToQueue(), this fires
    GlobalAudioPlayerService.OnStateChanged += OnPlayerStateChanged;
}

// Called when global player state changes
// Switches to UI thread using InvokeAsync before updating UI
private void OnPlayerStateChanged()
{
    // InvokeAsync is REQUIRED here because the event fires from
    // a different thread than Blazor's UI thread
    // Without it we get "not associated with Dispatcher" error
    InvokeAsync(() =>
    {
        // Get latest state from the service
        currentSong = GlobalAudioPlayerService.GetCurrentSong();
        isPlaying = GlobalAudioPlayerService.GetIsPlaying();
        currentTime = GlobalAudioPlayerService.GetCurrentTime();

        // Convert volume from 0.0-1.0 to 0-100 for the slider
        volumePercent = (int)(GlobalAudioPlayerService.GetVolume() * 100);

        // If we have a song, build its audio source URL
        if (currentSong != null && currentSong.audioData != null)
        {
            audioSource = currentSong.GetAudioSource(currentSong.audioData);
        }
        else
        {
            audioSource = "";
        }

        // Tell Blazor to redraw this component
        StateHasChanged();
    });
}

// Called every second while audio is playing
// Updates the progress bar position
private void OnTimeUpdate(EventArgs e)
{
    // We can't get exact time without JavaScript
    // So we increment manually each time this fires
    // ontimeupdate fires approximately every 250ms
    if (isPlaying)
    {
        currentTime = currentTime + 0.25;

        // Update the service too so it stays in sync
        GlobalAudioPlayerService.SetCurrentTime(currentTime);

        StateHasChanged();
    }
}

// Called when audio finishes playing
// Tells the service to play next song
private void OnSongEnded(EventArgs e)
{
    // Reset time
    currentTime = 0;
    duration = 0;

    // Tell service song ended - it will play next automatically
    GlobalAudioPlayerService.OnSongEnded();
}

// Called when audio file loads and we know its duration
private void OnMetadataLoaded(EventArgs e)
{
    // We use the song's duration from the database
    // since we can't get it from the audio element without JavaScript
    if (currentSong != null)
    {
        duration = currentSong.duration;
    }
    StateHasChanged();
}

// Called when user drags the progress bar
private void OnProgressChanged(ChangeEventArgs e)
{
    if (e.Value != null)
    {
        double newTime = Convert.ToDouble(e.Value);
        currentTime = newTime;

        // Update service state
        GlobalAudioPlayerService.SetCurrentTime(newTime);

        StateHasChanged();
    }
}

// Called when user drags the volume slider
private void OnVolumeChanged(ChangeEventArgs e)
{
    if (e.Value != null)
    {
        // Slider gives 0-100, service needs 0.0-1.0
        int newVolumePercent = Convert.ToInt32(e.Value);
        volumePercent = newVolumePercent;

        // Convert percentage to decimal for service
        double newVolume = newVolumePercent / 100.0;
        GlobalAudioPlayerService.SetVolume(newVolume);

        StateHasChanged();
    }
}

// Play/pause button clicked
private void TogglePlayPause()
{
    GlobalAudioPlayerService.TogglePlayPause();
    isPlaying = GlobalAudioPlayerService.GetIsPlaying();
    StateHasChanged();
}

// Next button clicked
private void Next()
{
    currentTime = 0;
    duration = 0;
    GlobalAudioPlayerService.PlayNext();
}

// Previous button clicked
private void Previous()
{
    currentTime = 0;
    GlobalAudioPlayerService.PlayPrevious();
}

// Clear queue button clicked
private void ClearQueue()
{
    currentTime = 0;
    duration = 0;
    audioSource = "";
    GlobalAudioPlayerService.ClearQueue();
    StateHasChanged();
}

// Format seconds into MM:SS display
// Example: 125 seconds = "2:05"
private string FormatTime(double seconds)
{
    int totalSeconds = (int)seconds;
    int min = totalSeconds / 60;
    int sec = totalSeconds % 60;
    return min + ":" + sec.ToString("D2");
}

// Clean up event subscription when component is destroyed
// This prevents memory leaks from event handlers
public void Dispose()
{
    GlobalAudioPlayerService.OnStateChanged -= OnPlayerStateChanged;
}
}