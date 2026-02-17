// Aurora Music - Audio Player Controls
// Minimal JavaScript to control the HTML audio element
// Only used because Blazor cannot control audio playback without JS

// Play the audio element
function playAudio(elementId) {
    var audio = document.getElementById(elementId);
    if (audio) {
        audio.play();
    }
}

// Pause the audio element
function pauseAudio(elementId) {
    var audio = document.getElementById(elementId);
    if (audio) {
        audio.pause();
    }
}

// Set the current time (seeking)
function setAudioTime(elementId, time) {
    var audio = document.getElementById(elementId);
    if (audio) {
        audio.currentTime = time;
    }
}

// Set the volume (0.0 to 1.0)
function setAudioVolume(elementId, volume) {
    var audio = document.getElementById(elementId);
    if (audio) {
        audio.volume = volume;
    }
}

// Get the current time of the audio element
function getAudioTime(elementId) {
    var audio = document.getElementById(elementId);
    if (audio) {
        return audio.currentTime;
    }
    return 0;
}