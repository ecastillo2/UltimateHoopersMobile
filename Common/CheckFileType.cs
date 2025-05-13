using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class CheckFileType
    {
        // Use readonly collections that are initialized once
        private static readonly Dictionary<string, string> _mediaContentTypes = new Dictionary<string, string>
        {
            // Images
            { "image/jpeg", "image" },
            { "image/png", "image" },
            { "image/gif", "image" },
            { "image/webp", "image" },
            
            // Videos
            { "video/mp4", "video" },
            { "video/x-msvideo", "video" },
            { "video/x-flv", "video" },
            { "video/x-matroska", "video" },
            { "video/quicktime", "video" }
        };

        /// <summary>
        /// Determines the type of file based on its MIME content type
        /// </summary>
        /// <param name="file">The uploaded file to check</param>
        /// <returns>The file type category or an error message</returns>
        public static string CheckFileTypeReturn(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "No file uploaded.";
            }

            return _mediaContentTypes.TryGetValue(file.ContentType, out string fileType)
                ? fileType
                : "Invalid file type. Only images and videos are allowed.";
        }
    }
}