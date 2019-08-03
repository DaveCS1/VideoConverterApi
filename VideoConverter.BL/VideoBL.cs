using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using VideoConverter.DAL;
using VideoConverter.DM;

namespace VideoConverter.BL
{
    public class VideoBL
    {
        private VideoDal dal;
        private readonly IConfiguration configuration;


        public VideoBL(IConfiguration iConfig)
        {
            configuration = iConfig;
            dal = new VideoDal(iConfig);
        }

        public int InsertVideo(Video video)
        {
            return dal.Insert(video);
        }

        public List<Video> GetAllVideos()
        {
            return dal.GetAll();
        }

        public Video GetVideoById(int id)
        {
            return dal.GetById(id);
        }

        public void Delete(int id)
        {
            Video video = dal.GetById(id);
            if (video != null)
            {
                try
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(video.VideoFolderPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {

                        dir.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log error
                    Console.WriteLine(ex.ToString());
                }

                dal.DeleteVideo(id);
            }
        }

    }
}
