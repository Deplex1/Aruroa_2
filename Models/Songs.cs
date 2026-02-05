namespace Models
{
    public class Song
    {
        public int songID { get; set; }
        public string title { get; set; }
        public int duration { get; set; } // seconds
        public byte[] audioData { get; set; } // MP3 stored as BLOB
        public int userid { get; set; }
        public int genreID { get; set; }

        public DateTime uploaded { get; set; }
        public int plays { get; set; }

        // Used only in UI to play audio (not saved in DB)
        public string audioSource { get; set; }

        public string GetAudioSource(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "";
            }

            try
            {
                // Check file signature to determine audio format
                string mimeType = GetAudioMimeType(data);
                string base64 = Convert.ToBase64String(data);
                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting audio data: {ex.Message}");
                return "";
            }
        }

        private string GetAudioMimeType(byte[] data)
        {
            if (data.Length < 12) return "audio/mpeg";

            // Check for MP3 signature (ID3v2 tag)
            if (data.Length >= 10 && 
                data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33) // "ID3"
            {
                return "audio/mpeg";
            }

            // Check for MP3 signature (MPEG sync)
            if (data.Length >= 2 && 
                (data[0] == 0xFF && (data[1] & 0xE0) == 0xE0))
            {
                return "audio/mpeg";
            }

            // Check for WAV signature
            if (data.Length >= 12 && 
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 && // "RIFF"
                data[8] == 0x57 && data[9] == 0x41 && data[10] == 0x56 && data[11] == 0x45) // "WAVE"
            {
                return "audio/wav";
            }

            // Check for AAC signature
            if (data.Length >= 4 && 
                data[0] == 0xFF && (data[1] & 0xF0) == 0xF0)
            {
                return "audio/aac";
            }

            // Default to MP3 if we can't determine the format
            return "audio/mpeg";
        }
        public Song()
        {
        }
    }
}
