using System;

namespace Models
{
    public class Song
    {
        // Basic song information
        public int songID { get; set; }
        public string title { get; set; }
        public int duration { get; set; } // Duration in seconds
        public byte[] audioData { get; set; } // The actual MP3 file stored as binary data
        public int userid { get; set; } // Which user uploaded this song

        // NOTE: We REMOVED genreID from here!
        // Instead, we use the song_genres junction table to support MULTIPLE genres per song
        // Example: A song can be both "Rock" and "Electronic" at the same time

        public DateTime uploaded { get; set; } // When was this song uploaded
        public int plays { get; set; } // How many times has this song been played

        // This property is NOT stored in database - only used for displaying audio in the browser
        public string audioSource { get; set; }

        // This method converts binary audio data into a format the browser can play
        // It creates a "data URL" that the HTML <audio> tag can use
        public string GetAudioSource(byte[] data)
        {
            // First check if we actually have audio data
            if (data == null || data.Length == 0)
            {
                return "";
            }

            try
            {
                // Step 1: Figure out what TYPE of audio file this is (MP3, WAV, AAC, etc.)
                string mimeType = GetAudioMimeType(data);

                // Step 2: Convert the binary data to Base64 string
                // Base64 is a way to represent binary data as text
                string base64 = Convert.ToBase64String(data);

                // Step 3: Create a data URL that looks like:
                // "data:audio/mpeg;base64,ABC123XYZ..."
                // The browser knows how to play this!
                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting audio data: {ex.Message}");
                return "";
            }
        }

        // This method examines the first few bytes of the file to determine its type
        // Different file types have unique "signatures" at the beginning
        // These signatures are called "magic numbers" or "file headers"
        private string GetAudioMimeType(byte[] data)
        {
            // If file is too small, assume it's MP3
            if (data.Length < 12)
            {
                return "audio/mpeg";
            }

            // CHECK #1: Look for MP3 signature with ID3 tag
            // ID3 tags store metadata like artist name, album, etc.
            // The signature is the letters "ID3" at the start of the file
            // In hexadecimal: 0x49='I', 0x44='D', 0x33='3'
            if (data.Length >= 10 &&
                data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33)
            {
                return "audio/mpeg";
            }

            // CHECK #2: Look for MP3 MPEG sync pattern
            // MP3 files without ID3 tags start with 0xFF followed by 0xE0-0xFF
            // This is called the "sync word" - it marks the start of MP3 audio frames
            // The & 0xE0 checks if the top 3 bits are set (binary: 11100000)
            if (data.Length >= 2 &&
                (data[0] == 0xFF && (data[1] & 0xE0) == 0xE0))
            {
                return "audio/mpeg";
            }

            // CHECK #3: Look for WAV file signature
            // WAV files start with "RIFF" followed by size, then "WAVE"
            // In hexadecimal:
            // 0x52='R', 0x49='I', 0x46='F', 0x46='F' (RIFF)
            // 0x57='W', 0x41='A', 0x56='V', 0x45='E' (WAVE)
            if (data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x41 && data[10] == 0x56 && data[11] == 0x45)
            {
                return "audio/wav";
            }

            // CHECK #4: Look for AAC file signature
            // AAC files start with 0xFF followed by 0xF0-0xFF
            // Similar to MP3 but with different bit pattern
            // The & 0xF0 checks if the top 4 bits are set (binary: 11110000)
            if (data.Length >= 4 &&
                data[0] == 0xFF && (data[1] & 0xF0) == 0xF0)
            {
                return "audio/aac";
            }

            // If we couldn't identify the file type, assume it's MP3
            // MP3 is the most common format
            return "audio/mpeg";
        }

        // Empty constructor - creates a new Song object with default values
        public Song()
        {
        }
    }
}