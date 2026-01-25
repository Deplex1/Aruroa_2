using System;
using System.IO;

public static class TestAudioHelper
{
    // Test using a real mp3 file from disk
    public static void TestFromFile()
    {
        string path = @"C:\Users\Twelve.DESKTOP-MG3L518\Desktop\songs\Her Last Sight - _The Fall_ _ Official Music Video.mp3";

        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            return;
        }

        byte[] data = File.ReadAllBytes(path);

        int duration = AudioHelper.GetMp3DurationInSeconds(data);

        Console.WriteLine("Duration in seconds: " + duration);
    }
}