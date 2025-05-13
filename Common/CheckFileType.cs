using Microsoft.AspNetCore.Http;

namespace Common
{
    public static class CheckFileType
    {

        private static readonly List<string> _imageContentTypes = new List<string>
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        // Add other image MIME types if necessary
    };

        private static readonly List<string> _videoContentTypes = new List<string>
    {
       "video/mp4",
    "video/x-msvideo",
    "video/x-flv",
    "video/x-matroska",
    "video/quicktime", // Added for .MOV files
    // Add other video MIME types if necessary
        // Add other video MIME types if necessary
    };

        /// <summary>
        /// Get8Digits
        /// </summary>
        /// <returns></returns>
        public static string CheckFileTypeReturn(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "No file uploaded.";
            }

            // Check if the file is an image
            if (_imageContentTypes.Contains(file.ContentType))
            {
                return "image";
            }
            // Check if the file is a video
            else if (_videoContentTypes.Contains(file.ContentType))
            {
                return "video";
            }
            else
            {
                return "Invalid file type. Only images and videos are allowed.";
            }
        }

    }
}
