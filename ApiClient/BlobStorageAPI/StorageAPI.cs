using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace ApiClient.BlobStorageAPI
{
    public partial class StorageAPI
    {
        // Other methods would be here...

        /// <summary>
        /// Removes Exif data from an image to prevent privacy issues
        /// </summary>
        /// <param name="imageStream">Source image stream</param>
        /// <returns>Clean image stream without Exif data</returns>
        private MemoryStream RemoveExifData(Stream imageStream)
        {
            try
            {
                // Load the image from the stream
                using (var image = System.Drawing.Image.FromStream(imageStream))
                {
                    // Create a new memory stream for the cleaned image
                    var outputStream = new MemoryStream();

                    // Create a copy of the image without the metadata
                    // by explicitly removing property items that contain Exif data
                    foreach (var propItem in image.PropertyItems)
                    {
                        // Try to remove Exif data
                        try
                        {
                            image.RemovePropertyItem(propItem.Id);
                        }
                        catch
                        {
                            // Ignore errors if a property can't be removed
                        }
                    }

                    // Save the image to the output stream without Exif data
                    // Detect the format based on the original image
                    ImageFormat format = image.RawFormat;
                    image.Save(outputStream, format);

                    // Reset the position to the beginning of the stream
                    outputStream.Position = 0;

                    return outputStream;
                }
            }
            catch (Exception ex)
            {
                // Log the error (in a real application)
                Console.WriteLine($"Error removing Exif data: {ex.Message}");

                // In case of error, reset the original stream and return it
                imageStream.Position = 0;
                var fallbackStream = new MemoryStream();
                imageStream.CopyTo(fallbackStream);
                fallbackStream.Position = 0;
                return fallbackStream;
            }
        }

        /// <summary>
        /// Process image before upload (resize, compress, remove Exif data)
        /// </summary>
        /// <param name="file">Image file from form upload</param>
        /// <returns>Processed image as a memory stream</returns>
        public MemoryStream ProcessImageForUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided", nameof(file));
            }

            using (var stream = file.OpenReadStream())
            {
                // First remove Exif data for privacy
                using (var cleanStream = RemoveExifData(stream))
                {
                    // Load the clean image
                    using (var image = System.Drawing.Image.FromStream(cleanStream))
                    {
                        // Define max dimensions
                        const int maxWidth = 1200;
                        const int maxHeight = 1200;

                        // Calculate new dimensions while maintaining aspect ratio
                        int newWidth, newHeight;
                        if (image.Width > image.Height)
                        {
                            newWidth = maxWidth;
                            newHeight = (int)(image.Height * ((float)maxWidth / image.Width));
                        }
                        else
                        {
                            newHeight = maxHeight;
                            newWidth = (int)(image.Width * ((float)maxHeight / image.Height));
                        }

                        // Resize the image
                        using (var resized = new Bitmap(newWidth, newHeight))
                        {
                            using (var graphics = Graphics.FromImage(resized))
                            {
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                            }

                            // Save to memory stream with compression
                            var outputStream = new MemoryStream();

                            // Use JPEG encoder with quality setting
                            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            var encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L); // 85% quality

                            resized.Save(outputStream, encoder, encoderParams);
                            outputStream.Position = 0;
                            return outputStream;
                        }
                    }
                }
            }
        }
    }
}