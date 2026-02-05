using System;
using System.IO;
using TagLib;

namespace SongManagment
{
    public static class AudioHelper
    {
        public static async Task<int> GetMp3DurationInSeconds(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);

            StreamFileAbstraction file = new StreamFileAbstraction("audio.mp3", ms, ms);

            TagLib.File tagFile = TagLib.File.Create(file);

            TimeSpan duration = tagFile.Properties.Duration;

            tagFile.Dispose();
            ms.Dispose();

            return (int)duration.TotalSeconds;
        }
    }
}