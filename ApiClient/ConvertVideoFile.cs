using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ApiClient
{
    /// <summary>
    /// Utility class for converting video files
    /// </summary>
    public static class ConvertVideoFile
    {
        /// <summary>
        /// Converts a MOV file to MP4 format
        /// </summary>
        /// <param name="inputFilePath">Path to the input MOV file</param>
        /// <param name="outputDirectory">Directory to save the converted MP4 file</param>
        /// <returns>Path to the converted MP4 file</returns>
        public static async Task<string> ConvertMovToMp4Async(string inputFilePath, string outputDirectory)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("Input file path cannot be empty", nameof(inputFilePath));

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input file not found", inputFilePath);

            // If the input file is already MP4, just return it
            if (inputFilePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return inputFilePath;
            }

            // Determine output file path
            string outputFileName = Path.GetFileNameWithoutExtension(inputFilePath) + ".mp4";
            string outputFilePath;

            if (string.IsNullOrEmpty(outputDirectory))
            {
                // Use the same directory as the input file
                outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), outputFileName);
            }
            else
            {
                // Ensure output directory exists
                Directory.CreateDirectory(outputDirectory);
                outputFilePath = Path.Combine(outputDirectory, outputFileName);
            }

            // Get FFmpeg executable path
            string ffmpegPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "ffmpeg",
                "bin",
                "ffmpeg.exe");

            // Set up FFmpeg command
            string ffmpegArgs = $"-i \"{inputFilePath}\" -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 128k \"{outputFilePath}\"";

            try
            {
                // Run FFmpeg process
                using (var process = new Process())
                {
                    process.StartInfo.FileName = ffmpegPath;
                    process.StartInfo.Arguments = ffmpegArgs;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        string error = await process.StandardError.ReadToEndAsync();
                        throw new Exception($"FFmpeg conversion failed: {error}");
                    }
                }

                // Ensure the output file exists
                if (!File.Exists(outputFilePath))
                {
                    throw new FileNotFoundException("Converted file not found", outputFilePath);
                }

                return outputFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting video file: {ex.Message}", ex);
            }
        }
    }
}