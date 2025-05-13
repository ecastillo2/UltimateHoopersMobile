using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace Common
{
    public static class ConvertImage
    {

        /// <summary>
        /// Convert Png To WebP
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ConvertPngToWebP(string type, IFormFile file)
        {
            using (var inputStream = file.OpenReadStream())
            {
                using (var originalImage = SKBitmap.Decode(inputStream))
                {
                    int x = 0;
                    int y = 0;
                    if (type == "image")
                    {
                        x = 640;
                        y = 426;
                    }
                    if (type == "video")
                    {
                        x = 800;
                        y = 535;
                    }

                    // Resize the image to 504x278
                    using (var resizedImage = originalImage.Resize(new SKImageInfo(x, y), SKFilterQuality.High))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            // Encode the resized image as WebP
                            resizedImage.Encode(outputStream, SKEncodedImageFormat.Webp, 75); // 75 is the quality (0-100)

                            // Return the WebP file as a byte array
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }
    }
}
