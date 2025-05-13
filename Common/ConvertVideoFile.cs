using Domain;
using Xabe.FFmpeg;

namespace Common
{
    public static class ConvertVideoFile
    {
        private static readonly ErrorException _errorException = new ErrorException();

        /// <summary>
        /// Convert Mov To Mp4 Async
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        public static async Task<string> ConvertMovToMp4Async(string inputFilePath, string outputFolder)
        {
            try
            {
                // Set FFmpeg executables path
                var ffmpegDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin");
                FFmpeg.SetExecutablesPath(ffmpegDirectory);

                // Ensure output folder exists
                Directory.CreateDirectory(outputFolder);

                // If input file is already MP4, simply move it to the output folder
                if (inputFilePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    var outputFileName = Path.GetFileName(inputFilePath);
                    var outputFilePath = Path.Combine(outputFolder, outputFileName);
                    File.Copy(inputFilePath, outputFilePath, true);
                    return outputFilePath;
                }

                // Convert MOV to MP4
                var outputFileNameMp4 = Path.GetFileNameWithoutExtension(inputFilePath) + ".mp4";
                var outputFilePathMp4 = Path.Combine(outputFolder, outputFileNameMp4);

                await FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{inputFilePath}\"")
                    .SetOutput(outputFilePathMp4)
                    .Start();

                return outputFilePathMp4;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during video conversion: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Copy File With Retry Async
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        /// <param name="maxRetries"></param>
        /// <param name="delayMilliseconds"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private static async Task CopyFileWithRetryAsync(string sourceFilePath, string destinationFilePath, int maxRetries = 3, int delayMilliseconds = 1000)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Ensure no other process is locking the file by opening a FileStream
                    using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                    using (var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                    return; // Success, exit the method
                }
                catch (IOException ex) when (attempt < maxRetries - 1)
                {
                    Console.WriteLine($"File is being used by another process, retrying... ({attempt + 1}/{maxRetries})");
                    await Task.Delay(delayMilliseconds); // Wait before retrying
                }
            }

            throw new IOException($"Unable to copy file after {maxRetries} attempts.");
        }

    }
}
