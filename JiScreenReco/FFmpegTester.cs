using System;
using System.Diagnostics;
using System.Windows;

namespace JiScreenReco
{
    public static class FFmpegTester
    {
        public static bool TestFFmpeg()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = "-version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(3000);

                    return output.Contains("ffmpeg version");
                }
            }
            catch
            {
                return false;
            }
        }
    }
}