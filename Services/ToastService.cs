using System;

namespace Services
{
    /// <summary>
    /// Global toast notification service for showing user feedback messages.
    /// Uses static event pattern like GlobalAudioPlayerService.
    /// </summary>
    public static class ToastService
    {
        // Event that fires when a new toast should be shown
        public static event Action? OnShow;

        // Current toast message
        private static string currentMessage = "";

        // Toast type: success, error, info, warning
        private static string currentType = "info";

        // How long to show toast (milliseconds)
        private static int currentDuration = 3000;

        /// <summary>
        /// Show a success toast (green)
        /// </summary>
        public static void ShowSuccess(string message, int durationMs = 3000)
        {
            currentMessage = message;
            currentType = "success";
            currentDuration = durationMs;
            OnShow?.Invoke();
        }

        /// <summary>
        /// Show an error toast (red)
        /// </summary>
        public static void ShowError(string message, int durationMs = 4000)
        {
            currentMessage = message;
            currentType = "error";
            currentDuration = durationMs;
            OnShow?.Invoke();
        }

        /// <summary>
        /// Show an info toast (blue)
        /// </summary>
        public static void ShowInfo(string message, int durationMs = 3000)
        {
            currentMessage = message;
            currentType = "info";
            currentDuration = durationMs;
            OnShow?.Invoke();
        }

        /// <summary>
        /// Show a warning toast (orange)
        /// </summary>
        public static void ShowWarning(string message, int durationMs = 3000)
        {
            currentMessage = message;
            currentType = "warning";
            currentDuration = durationMs;
            OnShow?.Invoke();
        }

        /// <summary>
        /// Get current toast message
        /// </summary>
        public static string GetMessage()
        {
            return currentMessage;
        }

        /// <summary>
        /// Get current toast type
        /// </summary>
        public static string GetToastType()
        {
            return currentType;
        }

        /// <summary>
        /// Get current toast duration
        /// </summary>
        public static int GetDuration()
        {
            return currentDuration;
        }
    }
}
