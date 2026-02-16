namespace Services
{
    /// <summary>
    /// Service class that provides common UI helper methods.
    /// These are simple formatting and display utilities used across multiple pages.
    /// </summary>
    public class UIHelperService
    {
        /// <summary>
        /// Converts duration in seconds to "minutes:seconds" format.
        /// Example: 125 seconds becomes "2:05"
        /// </summary>
        public string FormatDuration(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            
            string result = minutes.ToString() + ":" + secs.ToString("00");
            return result;
        }

        /// <summary>
        /// Formats file size in human-readable format (B, KB, MB, GB).
        /// Example: 1536 bytes becomes "1.5 KB"
        /// </summary>
        public string FormatFileSize(long bytes)
        {
            string[] sizes = new string[] { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            // Keep dividing by 1024 until we get a reasonable number
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order = order + 1;
                len = len / 1024;
            }

            string result = len.ToString("0.##") + " " + sizes[order];
            return result;
        }

        /// <summary>
        /// Rounds a double value to one decimal place.
        /// Example: 3.456 becomes "3.5"
        /// </summary>
        public string FormatRating(double rating)
        {
            string result = rating.ToString("0.0");
            return result;
        }
    }
}
