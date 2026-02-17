using System;
using System.Collections.Generic;
using Models;

namespace Services
{
    /// <summary>
    /// Global STATIC service that manages audio player state across the entire application.
    /// 
    /// WHY STATIC?
    /// - A regular class: each page creates its OWN copy (new GlobalAudioPlayerService())
    ///   This means Songs page has one queue, AudioPlayer has another - they never share!
    /// - A static class: there is only ONE copy shared across ALL pages
    ///   Songs page and AudioPlayer BOTH access the same queue automatically!
    /// 
    /// Think of it like a school whiteboard:
    /// - Regular class = each student has their own whiteboard (private, separate)
    /// - Static class = one big whiteboard in the hallway (shared, everyone sees it)
    /// </summary>
    public static class GlobalAudioPlayerService
    {
        // ===== PRIVATE STATE =====
        // These variables store the current state of the audio player
        // They are private so only this class can change them directly
        // Other pages read them using the Get methods below

        // The song that is currently playing (null if nothing is playing)
        private static Song? currentSong = null;

        // Which position in the queue are we at?
        // -1 means nothing is selected
        // 0 means first song, 1 means second song, etc.
        private static int currentQueueIndex = -1;

        // Is the audio currently playing or paused?
        private static bool isPlaying = false;

        // How many seconds into the current song are we?
        private static double currentTime = 0;

        // Volume level from 0.0 (silent) to 1.0 (full volume)
        // Default is 0.7 (70% volume)
        private static double volume = 0.7;

        // The list of songs waiting to be played
        private static List<Song> playQueue = new List<Song>();

        // ===== EVENT =====
        // This event fires whenever something changes (song changes, play/pause, etc.)
        // AudioPlayer.razor and Songs.razor subscribe to this event
        // When it fires, they call StateHasChanged() to update their UI
        // ## 🔐 **`event` is a PROTECTION keyword!** that means people can only un/subscribe to the event and cannot change it by using =
        public static event Action? OnStateChanged;

        // ===== GET METHODS =====
        // These methods let other pages READ the current state
        // Pages cannot change state directly - they must use the action methods below

        /// <summary>
        /// Gets the currently playing song.
        /// Returns null if nothing is playing.
        /// </summary>
        public static Song? GetCurrentSong()
        {
            return currentSong;
        }

        /// <summary>
        /// Gets whether audio is currently playing.
        /// True = playing, False = paused or stopped
        /// </summary>
        public static bool GetIsPlaying()
        {
            return isPlaying;
        }

        /// <summary>
        /// Gets the current playback time in seconds.
        /// Example: 65.5 means 1 minute and 5.5 seconds into the song
        /// </summary>
        public static double GetCurrentTime()
        {
            return currentTime;
        }

        /// <summary>
        /// Gets the current volume level.
        /// Returns a value between 0.0 (silent) and 1.0 (full volume)
        /// </summary>
        public static double GetVolume()
        {
            return volume;
        }

        /// <summary>
        /// Gets a COPY of the play queue.
        /// Returns a copy so pages cannot accidentally modify the real queue
        /// </summary>
        public static List<Song> GetPlayQueue()
        {
            // Create a new list and copy all songs into it
            // This prevents other pages from directly modifying playQueue
            List<Song> copy = new List<Song>();
            for (int i = 0; i < playQueue.Count; i = i + 1)
            {
                copy.Add(playQueue[i]);
            }
            return copy;
        }

        /// <summary>
        /// Gets the index of the currently playing song in the queue.
        /// Returns -1 if nothing is playing.
        /// Example: 2 means the third song in the queue is playing
        /// </summary>
        public static int GetCurrentQueueIndex()
        {
            return currentQueueIndex;
        }

        // ===== ACTION METHODS =====
        // These methods let pages CHANGE the state
        // After changing state, they call NotifyStateChanged()
        // which fires the OnStateChanged event so all pages update their UI

        /// <summary>
        /// Adds a song to the END of the play queue.
        /// Does NOT start playing automatically.
        /// Prevents duplicate songs in the queue.
        /// </summary>
        public static void AddToQueue(Song song)
        {
            // Check if this song is already in the queue
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

                // Tell all pages the queue changed
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Adds a song to the queue and starts playing it IMMEDIATELY.
        /// If the song is already in the queue, just plays it from there.
        /// </summary>
        public static void PlayNow(Song song)
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

            if (existingIndex >= 0)
            {
                // Song is already in queue - just jump to it
                currentQueueIndex = existingIndex;
                currentSong = playQueue[existingIndex];
            }
            else
            {
                // Song is not in queue - add it and play
                playQueue.Add(song);
                currentQueueIndex = playQueue.Count - 1;
                currentSong = song;
            }

            // Start playing from the beginning
            isPlaying = true;
            currentTime = 0;

            // Tell all pages the state changed
            NotifyStateChanged();
        }

        /// <summary>
        /// Toggles between playing and paused.
        /// If playing → pause. If paused → play.
        /// Does nothing if no song is selected.
        /// </summary>
        public static void TogglePlayPause()
        {
            // Can't play/pause if no song is selected
            if (currentSong == null)
            {
                return;
            }

            // Flip the playing state
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
        /// Plays the NEXT song in the queue.
        /// If we are at the last song, stops playback.
        /// </summary>
        public static void PlayNext()
        {
            // Can't go next if queue is empty
            if (playQueue.Count == 0)
            {
                return;
            }

            // Calculate what the next index would be
            int nextIndex = currentQueueIndex + 1;

            if (nextIndex < playQueue.Count)
            {
                // There is a next song - play it
                currentQueueIndex = nextIndex;
                currentSong = playQueue[nextIndex];
                isPlaying = true;
                currentTime = 0;
            }
            else
            {
                // We reached the end of the queue - stop playing
                isPlaying = false;
            }

            NotifyStateChanged();
        }

        /// <summary>
        /// Plays the PREVIOUS song in the queue.
        /// If more than 3 seconds into current song, restarts it instead.
        /// If at first song, restarts it.
        /// </summary>
        public static void PlayPrevious()
        {
            // Can't go previous if queue is empty
            if (playQueue.Count == 0)
            {
                return;
            }

            // If more than 3 seconds in, restart current song instead
            if (currentTime > 3)
            {
                currentTime = 0;
                NotifyStateChanged();
                return;
            }

            // Calculate what the previous index would be
            int previousIndex = currentQueueIndex - 1;

            if (previousIndex >= 0)
            {
                // There is a previous song - play it
                currentQueueIndex = previousIndex;
                currentSong = playQueue[previousIndex];
                isPlaying = true;
                currentTime = 0;
            }
            else
            {
                // Already at first song - just restart it
                currentTime = 0;
            }

            NotifyStateChanged();
        }

        /// <summary>
        /// Seeks to a specific time in the current song.
        /// Called when user drags the progress bar.
        /// Time is in seconds.
        /// </summary>
        public static void SetCurrentTime(double time)
        {
            currentTime = time;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the volume level.
        /// Automatically clamps value between 0.0 and 1.0.
        /// </summary>
        public static void SetVolume(double newVolume)
        {
            // Make sure volume stays between 0 and 1
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
        /// Called when the current song finishes playing.
        /// Automatically advances to the next song in the queue.
        /// </summary>
        public static void OnSongEnded()
        {
            // Simply play the next song
            PlayNext();
        }

        /// <summary>
        /// Removes a specific song from the queue by its index position.
        /// If removing the currently playing song, stops playback.
        /// </summary>
        public static void RemoveFromQueue(int index)
        {
            // Validate index
            if (index < 0)
            {
                return;
            }
            if (index >= playQueue.Count)
            {
                return;
            }

            if (index == currentQueueIndex)
            {
                // We are removing the currently playing song
                // Stop playback and clear current song
                currentSong = null;
                isPlaying = false;
                currentQueueIndex = -1;
            }
            else if (index < currentQueueIndex)
            {
                // We are removing a song BEFORE the current one
                // Adjust the current index so it still points to the same song
                currentQueueIndex = currentQueueIndex - 1;
            }

            // Remove the song from the queue
            playQueue.RemoveAt(index);
            NotifyStateChanged();
        }

        /// <summary>
        /// Clears the entire queue and stops playback completely.
        /// </summary>
        public static void ClearQueue()
        {
            playQueue.Clear();
            currentSong = null;
            currentQueueIndex = -1;
            isPlaying = false;
            currentTime = 0;
            NotifyStateChanged();
        }

        /// <summary>
        /// Plays a specific song from the queue by its index position.
        /// Example: PlayFromQueue(2) plays the third song in the queue
        /// </summary>
        public static void PlayFromQueue(int index)
        {
            // Validate index
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
        /// Fires the OnStateChanged event to notify all subscribers.
        /// Called after EVERY state change so UI updates automatically.
        /// 
        /// HOW IT WORKS:
        /// 1. Songs.razor subscribes: GlobalAudioPlayerService.OnStateChanged += MyMethod
        /// 2. AudioPlayer.razor subscribes: GlobalAudioPlayerService.OnStateChanged += MyMethod
        /// 3. When state changes, this fires the event
        /// 4. Both pages receive the notification and call StateHasChanged()
        /// 5. Both pages update their UI with the new state
        /// </summary>
        private static void NotifyStateChanged()
        {
            if (OnStateChanged != null)
            {
                OnStateChanged.Invoke();
            }
        }
    }
}