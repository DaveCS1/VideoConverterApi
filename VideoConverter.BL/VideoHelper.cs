using Microsoft.AspNetCore.Http;
using System;

namespace VideoConverter.BL
{
    public static class VideoHelper
    {
        private static readonly string[] mediaExtensions = {
            "AVI", "MP4", "DIVX", "WMV", "MPEG", "AVI", "OGG", "WEBM", "FLV",
             "M4A", "M4V", "F4V", "F4A", "M4B", "M4R", "F4B", "MOV", "MKV"
    };

        public static bool IsMediaFileExtension(IFormFile file)
        {
            string fileName = file.FileName;
            string fileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper();
            return -1 != Array.IndexOf(mediaExtensions, fileExtension);
        }
    }
}
