using System;
using System.Collections.Generic;

namespace VideoConverter.DM
{
    public class Video
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VideoFolderPath { get; set; }
        public string HdFilePath { get; set; }
        public string HlsFilePath { get; set; }
        public DateTime Created { get; set; }

        public List<Thumbnail> Thumbnails { get; set; }
      }
}
