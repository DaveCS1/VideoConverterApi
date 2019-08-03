using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VideoConverter.DAL;
using VideoConverter.DM;

namespace VideoConverter.BL
{
    public class VideoConvertBL
    {
        private VideoDal dal;
        private IConfiguration configuration;

        private static int lineCount = 0; // used for log ffmpeg error
        private readonly string videoFolderName;
        private readonly string hdResolution;

        public VideoConvertBL(IConfiguration iConfig)
        {
            configuration = iConfig;
            dal = new VideoDal(iConfig);
            videoFolderName = configuration.GetValue<string>("AppConfiguration:VideoFolderName");
            hdResolution = configuration.GetValue<string>("AppConfiguration:HdResolution");
        }

        public Video UploadFile(IFormFile file)
        {
            try
            {
                string fileName = file.FileName.Replace(" ", "");
                string videosFolder = Path.Combine(Directory.GetCurrentDirectory(), videoFolderName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                // Create video folder
                string videoFolderPath = Path.Combine(videosFolder, fileNameWithoutExtension);
                int ind = 0;
                while (Directory.Exists(videoFolderPath))
                {
                    ind++;
                    videoFolderPath = videoFolderPath + "_" + ind;
                }

                Directory.CreateDirectory(videoFolderPath);
                string originalFilePath = Path.Combine(videoFolderPath, fileName);

                using (FileStream stream = new FileStream(originalFilePath, FileMode.Create))
                {
                    // Save file in videos folder
                    file.CopyTo(stream);
                }

                // Create video metadate to store in DB
                Video video = new Video()
                {
                    Name = fileName,
                    VideoFolderPath = videoFolderPath,
                    Created = DateTime.Now
                };


                // Create thumbnails
                HandleThumbnails(originalFilePath, video);

                // Convert to HD:
                string hdFilePath = ScaleVideo(originalFilePath, hdResolution);
                if (!string.IsNullOrEmpty(hdFilePath))
                {
                    video.HdFilePath = hdFilePath;
                }

                //Create hls files
                string hlsFilePath = CreateHlsFiles(originalFilePath);
                if (!string.IsNullOrEmpty(hlsFilePath))
                {
                    video.HlsFilePath = hlsFilePath;

                }

                video.Id = dal.Insert(video);
                return video;
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private void HandleThumbnails(string originalFilePath, Video videoMetadata)
        {
            Thumbnail thumbnail1 = CreateThumbnail(originalFilePath, 1);
            Thumbnail thumbnail2 = CreateThumbnail(originalFilePath, 3);
            videoMetadata.Thumbnails = new List<Thumbnail>();

            if (thumbnail1 != null)
            {
                videoMetadata.Thumbnails.Add(thumbnail1);
            }

            if (thumbnail2 != null)
            {
                videoMetadata.Thumbnails.Add(thumbnail2);
            }
        }

        private string ScaleVideo(string fileFullPath, string resolution)
        {
            string res = string.Empty;
            try
            {
                string fileName = Path.GetFileName(fileFullPath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileFullPath);
                string hdFileName = fileNameWithoutExtension + "_hd.mp4";
                string hdFilePath = fileFullPath.Replace(fileName, hdFileName);
                string hdRelativePath = Path.Combine(videoFolderName, fileNameWithoutExtension, hdFileName);

                bool ffmpegSuccess = FFmpegProccess(string.Format("-i {0} -vf scale={2} -c:v libx264 -crf 35 {1}",
                    fileFullPath, hdFilePath, resolution));

                if (ffmpegSuccess)
                {
                    res = hdRelativePath;
                }

            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
            }
            return res;
        }

        private Thumbnail CreateThumbnail(string fileFullPath, decimal fromSecond)
        {
            Thumbnail res = null;
            try
            {
                string fileName = Path.GetFileName(fileFullPath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileFullPath);
                string thumbnailFileName = fileNameWithoutExtension + "_thumbnail_" + fromSecond + ".jpg";
                string thumbnailPath = fileFullPath.Replace(fileName, thumbnailFileName);

                string thumbnailRelativePath = Path.Combine(videoFolderName, fileNameWithoutExtension, thumbnailFileName);

                bool ffmpegSuccess = FFmpegProccess(string.Format("-i {0} -ss {2} -vframes 1 {1}", fileFullPath, thumbnailPath, fromSecond));
                if (ffmpegSuccess)
                {
                    byte[] content = File.ReadAllBytes(thumbnailPath);
                    res = new Thumbnail
                    {
                        Path = thumbnailRelativePath,
                        Content = content,
                        TimeInVideo = fromSecond,
                        ContentLength = content.Length
                    };

                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
            return res;
        }

        private string CreateHlsFiles(string fileFullPath)
        {
            string res = string.Empty;

            try
            {
                string fileName = Path.GetFileName(fileFullPath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileFullPath);

                string hlsFileName = fileNameWithoutExtension + ".m3u8";
                string hdFilePath = fileFullPath.Replace(fileName, hlsFileName);
                string hlsRelativePath = Path.Combine(videoFolderName, fileNameWithoutExtension, hlsFileName);
                string hlsFilePath = fileFullPath.Replace(fileName, hlsFileName);

                bool ffmpegSuccess = FFmpegProccess(string.Format("-i {0} -codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls {1}",
                    fileFullPath, hlsFilePath));

                if (ffmpegSuccess)
                {
                    res = hlsRelativePath;
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
            }
            return res;

        }

        private bool FFmpegProccess(string args)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe"),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = args,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false
                };

                Process ffmpeg = new Process
                {
                    StartInfo = startInfo
                };

                ffmpeg.ErrorDataReceived += new DataReceivedEventHandler(ffmpeg_ErrorDataReceived);

                ffmpeg.Start();
                ffmpeg.WaitForExit();

                // Check if exit code is success (0)
                if (ffmpeg.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private static void ffmpeg_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Input line: {0} ({1:m:s:fff})", lineCount++, DateTime.Now);
            Console.WriteLine(e.Data);
            Console.WriteLine();
        }

    }
}
