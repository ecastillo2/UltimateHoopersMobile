using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Common
{
    public static class VideoThumbnailGenerator
    {

        /// <summary>
        /// Generate Thumbnail
        /// </summary>
        /// <param name="videoFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static async Task<string> GenerateThumbnail(IFormFile videoFile, TimeSpan? timeStamp = null)
        {
            // Ensure the video file is not null
            if (videoFile == null || videoFile.Length == 0)
            {
                throw new ArgumentException("Video file is required.");
            }
            
            // Save the uploaded video file to a temporary location
            string tempVideoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "uploads");
            if (!Directory.Exists(tempVideoPath))
            {
                Directory.CreateDirectory(tempVideoPath);
            }
            using (var stream = new FileStream(tempVideoPath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            // Define output thumbnail path
            string thumbnailPath = Path.ChangeExtension(tempVideoPath, ".png");

            // Set the timestamp to 00:00:01 if not provided
            string timeStampString = timeStamp.HasValue ? timeStamp.Value.ToString(@"hh\:mm\:ss") : "00:00:01";

            // FFmpeg command to generate a thumbnail
            string ffmpegArgs = $"-ss {timeStampString} -i \"{tempVideoPath}\" -frames:v 1 \"{thumbnailPath}\"";

            // Create a new process to run FFmpeg
            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo.FileName = "ffmpeg"; // Ensure "ffmpeg" is in your PATH
                ffmpegProcess.StartInfo.Arguments = ffmpegArgs;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.CreateNoWindow = true;

                // Start the process and wait for it to finish
                ffmpegProcess.Start();
                ffmpegProcess.WaitForExit();

                // Optionally, capture output and errors
                string output = ffmpegProcess.StandardOutput.ReadToEnd();
                string error = ffmpegProcess.StandardError.ReadToEnd();

                if (ffmpegProcess.ExitCode != 0)
                {
                    // Handle error
                    throw new Exception($"FFmpeg error: {error}");
                }
            }

            // Clean up temporary video file
            File.Delete(tempVideoPath);

            // Return the path to the generated thumbnail
            return thumbnailPath;
        }

	}
}
