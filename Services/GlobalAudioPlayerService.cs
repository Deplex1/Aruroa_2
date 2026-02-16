using System;
using System.Collections.Generic;
using Models;

namespace Services
{
    /// <summary>
    /// Global service that manages the audio player state across the entire application.
    /// Handles play queue, current song, playback controls, and volume.
    /// This is a singleton service that maintains state without JavaScript.
    /// </summary>
    public class GlobalAudioPlayerService
    {
        // Current playback state
        private Song? currentSong = null;
        private int currentQueueIndex = -1;
        private bool isPlaying = false;
        private double currentTime = 0;
        private double volume = 0.7; // Default volume 70%

        // Play queue
        private List<Song> playQueue = new List<Song>();

        // Event to notify UI components when state changes
        public event Action? OnStateChanged;

        /// <summary>
        /// Gets the currently playing song.
        /// Returns null if no song is playing.
        /// </summary>
        public Song? GetCurrentSong()
        {
            return currentSong;
        }

        /// <summary>
        /// Gets whether audio is currently playing.
        /// Returns true if playing, false if paused or stopped.
        /// </summary>
        public bool GetIsPlaying()
        {
            return isPlaying;
        }

        /// <summary>
        /// Gets the current playback time in seconds.
        /// </summary>
        public double GetCurrentTime()
        {
            return currentTime;
        }

        /// <summary>
        /// Gets the current volume level (0.0 to 1.0).
        /// </summary>
        public double GetVolume()
        {
            return volume;
        }

        /// <summary>
        /// Gets the entire play queue.
        /// Returns a copy to prevent external modification.
        /// </summary>
        public List<Song> GetPlayQueue()
        {
            List<Song> copy = new List<Song>();
            for (int i = 0; i < playQueue.Count; i = i + 1)
            {
                copy.Add(playQueue[i]);
            }
            return copy;
        }

        /// <summary>
        /// Gets the current queue index.
        /// Returns -1 if no song is selected.
        /// </summary>
        public int GetCurrentQueueIndex()
        {
            return currentQueueIndex;
        }

        /// <summary>
        /// Adds a song to the end of the play queue.
        /// Does not start playing automatically.
        /// </summary>
        public void AddToQueue(Song song)
        {
            // Check if song is already in queue
            bool alreadyExists = false;
            for (int i = 0; i < playQueue.Count; i = i + 1)
            {
                if (playQueue[i].songID == song.songID)
                {
                    alreadyExists = true;
                    break;
                }
            }

            // Only add if not already in queue
            if (alreadyExists == false)
            {
                playQueue.Add(song);
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Adds a song to the queue and starts playing it immediately.
        /// Sets the song as current and begins playback.
        /// </summary>
        public void PlayNow(Song song)
        {
            // Check if song is already in queue
            int existingIndex = -1;
            for (int i = 0; i < playQueue.Count; i = i + 1)
            {
                if (playQueue[i].songID == song.songID)
                {
                    existingIndex = i;
                    break;
                }
            }

            // If song is in queue, play it from there
            if (existingIndex >= 0)
            {
                currentQueueIndex = existingIndex;
                currentSong = playQueue[existingIndex];
            }
            else
            {
                // Add to queue and play
                playQueue.Add(song);
                currentQueueIndex = playQueue.Count - 1;
                currentSong = song;
            }

            // Start playing
            isPlaying = true;
            currentTime = 0;
            NotifyStateChanged();
        }

        /// <summary>
        /// Toggles between play and pause states.
        /// </summary>
        public void TogglePlayPause()
        {
            if (currentSong == null)
            {
                return;
            }

            if (isPlaying)
            {
                isPlaying = false;
            }
            else
            {
                isPlaying = true;
            }

            NotifyStateChanged();
        }

        /// <summary>
        /// Plays the next song in the queue.
        /// If at end of queue, stops playback.
        /// </summary>
        public void PlayNext()
        {
            if (playQueue.Count == 0)
            {
                return;
            }

            // Calculate next index
            int nextIndex = currentQueueIndex + 1;

            // Check if we have a next song
            if (nextIndex < playQueue.Count)
            {
                currentQueueIndex = nextIndex;
                currentSong = playQueue[nextIndex];
                isPlaying = true;
                currentTime = 0;
                NotifyStateChanged();
            }
            else
            {
                // End of queue - stop playing
                isPlaying = false;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Plays the previous song in the queue.
        /// If at start of queue, restarts current song.
        /// </summary>
        public void PlayPrevious()
        {
            if (playQueue.Count == 0)
            {
                return;
            }

            // If more than 3 seconds into song, restart it
            if (currentTime > 3)
            {
                currentTime = 0;
                NotifyStateChanged();
                return;
            }

            // Calculate previous index
            int previousIndex = currentQueueIndex - 1;

            // Check if we have a previous song
            if (previousIndex >= 0)
            {
                currentQueueIndex = previousIndex;
                currentSong = playQueue[previousIndex];
                isPlaying = true;
                currentTime = 0;
                NotifyStateChanged();
            }
            else
            {
                // At start of queue - restart current song
                currentTime = 0;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Sets the current playback time.
        /// Used when user seeks to a position.
        /// </summary>
        public void SetCurrentTime(double time)
        {
            currentTime = time;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the volume level.
        /// Value should be between 0.0 and 1.0.
        /// </summary>
        public void SetVolume(double newVolume)
        {
            // Clamp volume between 0 and 1
            if (newVolume < 0)
            {
                volume = 0;
            }
            else if (newVolume > 1)
            {
                volume = 1;
            }
            else
            {
                volume = newVolume;
            }

            NotifyStateChanged();
        }

        /// <summary>
        /// Called when the current song ends.
        /// Automatically plays the next song in queue.
        /// </summary>
        public void OnSongEnded()
        {
            PlayNext();
        }

        /// <summary>
        /// Removes a song from the queue by index.
        /// If removing current song, stops playback.
        /// </summary>
        public void RemoveFromQueue(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (index >= playQueue.Count)
            {
                return;
            }

            // If removing current song, stop playback
            if (index == currentQueueIndex)
            {
                currentSong = null;
                isPlaying = false;
                currentQueueIndex = -1;
            }
            else if (index < currentQueueIndex)
            {
                // Adjust current index if removing song before it
                currentQueueIndex = currentQueueIndex - 1;
            }

            // Remove the song
            playQueue.RemoveAt(index);
            NotifyStateChanged();
        }

        /// <summary>
        /// Clears the entire play queue and stops playback.
        /// </summary>
        public void ClearQueue()
        {
            playQueue.Clear();
            currentSong = null;
            currentQueueIndex = -1;
            isPlaying = false;
            currentTime = 0;
            NotifyStateChanged();
        }

        /// <summary>
        /// Plays a specific song from the queue by index.
        /// </summary>
        public void PlayFromQueue(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (index >= playQueue.Count)
            {
                return;
            }

            currentQueueIndex = index;
            currentSong = playQueue[index];
            isPlaying = true;
            currentTime = 0;
            NotifyStateChanged();
        }

        /// <summary>
        /// Notifies all subscribers that the state has changed.
        /// This triggers UI updates in components.
        /// </summary>
        private void NotifyStateChanged()
        {
            if (OnStateChanged != null)
            {
                OnStateChanged.Invoke();
            }
        }
    }
}
