using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using VideoConverter.DM;

namespace VideoConverter.DAL
{
    public class VideoDal
    {
        private readonly IConfiguration configuration;

        public VideoDal(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        // Insert video meta data to DB
        public int Insert(Video video)
        {
            try
            {
                string commandStr = @"INSERT INTO Videos (Name, VideoFolderPath, HdFilePath, HlsFilePath, Created) 
                                                 VALUES (@name, @videoFolderPath, @hdFilePath, @hlsFilePath, @created);
                                                 SELECT LAST_INSERT_ID();";

                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;

                        cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = video.Name ?? null;
                        cmd.Parameters.Add("@videoFolderPath", MySqlDbType.VarChar).Value = video.VideoFolderPath ?? null;
                        cmd.Parameters.Add("@hdFilePath", MySqlDbType.VarChar).Value = video.HdFilePath ?? null;
                        cmd.Parameters.Add("@hlsFilePath", MySqlDbType.VarChar).Value = video.HlsFilePath ?? null;
                        cmd.Parameters.Add("@created", MySqlDbType.DateTime).Value = video.Created;

                        int videoId = Convert.ToInt32(cmd.ExecuteScalar());
                        video?.Thumbnails?.ForEach(thumb =>
                        {
                            thumb.VideoId = videoId;
                            thumb.Id = InsertThumbnail(thumb);
                        });

                        return videoId;
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        public int InsertThumbnail(Thumbnail thumbnail)
        {
            try
            {
                string commandStr = @"INSERT INTO Thumbnails (Path, TimeInVideo, VideoId, Content, ContentLength) 
                                                 VALUES (@path, @timeInVideo, @videoId, @content, @contentLength);
                                                 SELECT LAST_INSERT_ID();";
                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;

                        cmd.Parameters.Add("@path", MySqlDbType.VarChar).Value = thumbnail.Path;
                        cmd.Parameters.Add("@timeInVideo", MySqlDbType.Decimal).Value = thumbnail.TimeInVideo;
                        cmd.Parameters.Add("@videoId", MySqlDbType.Int32).Value = thumbnail.VideoId;
                        cmd.Parameters.Add("@content", MySqlDbType.Blob).Value = thumbnail.Content;
                        cmd.Parameters.Add("@contentLength", MySqlDbType.Int32).Value = thumbnail.ContentLength;


                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        // Get list of all videos in DB
        public List<Video> GetAll()
        {
            List<Video> list = null;
            try
            {
                string commandStr = @"SELECT Id, Name, VideoFolderPath, HdFilePath, HlsFilePath, Created FROM Videos";
                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                list = new List<Video>();
                                while (reader.Read())
                                {
                                    Video item = ExtractVideoItem(reader);
                                    List<Thumbnail> thumbnails = GetThumbnailsByVideoId(item.Id);
                                    item.Thumbnails = thumbnails;
                                    list.Add(item);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
            return list;
        }

        // Get video by id
        public Video GetById(int id)
        {
            Video item = null;
            try
            {
                string commandStr = @"SELECT Id, Name, VideoFolderPath, HdFilePath, HlsFilePath, Created 
                                      FROM Videos v
                                      WHERE v.Id=@id";

                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;
                        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    item = ExtractVideoItem(reader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
            return item;

        }


        // Delete Video
        public void DeleteVideo(int id)
        {
            try
            {
                // Delete video dependencies
                DeleteThumbnailsByVideoId(id);

                string commandStr = @"DELETE FROM Videos WHERE Id=@id";

                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;
                        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public void DeleteThumbnailsByVideoId(int videoId)
        {
            try
            {
                string commandStr = @"DELETE FROM Thumbnails WHERE VideoId=@videoId";

                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;
                        cmd.Parameters.Add("@videoId", MySqlDbType.Int32).Value = videoId;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        // Get thumbnails by video id
        public List<Thumbnail> GetThumbnailsByVideoId(int videoId)
        {
            List<Thumbnail> list = null;
            try
            {
                string commandStr = @"SELECT Id, Path, TimeInVideo, VideoId, Content, ContentLength
                                      FROM Thumbnails t
                                      WHERE t.VideoId=@videoId";

                using (MySqlConnection conn = DBUtils.GetDBConnection(configuration))
                {
                    conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = commandStr;
                        cmd.Parameters.Add("@videoId", MySqlDbType.Int32).Value = videoId;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                list = new List<Thumbnail>();
                                while (reader.Read())
                                {
                                    Thumbnail item = ExtractThumbnailItem(reader);
                                    list.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log error
                Console.WriteLine(ex.ToString());
                throw;
            }
            return list;

        }

        private Video ExtractVideoItem(MySqlDataReader reader)
        {
            Video video = new Video
            {
                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? -1 : reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                VideoFolderPath = reader.IsDBNull(reader.GetOrdinal("VideoFolderPath")) ? string.Empty : reader.GetString(reader.GetOrdinal("VideoFolderPath")),
                HdFilePath = reader.IsDBNull(reader.GetOrdinal("HdFilePath")) ? string.Empty : reader.GetString(reader.GetOrdinal("HdFilePath")),
                HlsFilePath = reader.IsDBNull(reader.GetOrdinal("HlsFilePath")) ? string.Empty : reader.GetString(reader.GetOrdinal("HlsFilePath")),
                Created = reader.IsDBNull(reader.GetOrdinal("Created")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Created"))
            };


            return video;
        }

        private Thumbnail ExtractThumbnailItem(MySqlDataReader reader)
        {
            Thumbnail thumbnail = new Thumbnail
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Path = reader.GetString(reader.GetOrdinal("Path")),
                TimeInVideo = reader.GetDecimal(reader.GetOrdinal("TimeInVideo")),
                VideoId = reader.GetInt32(reader.GetOrdinal("VideoId")),
                ContentLength = reader.GetInt32(reader.GetOrdinal("ContentLength"))
            };

            reader.GetBytes(reader.GetOrdinal("Content"), 0, thumbnail.Content, 0, thumbnail.ContentLength);

            return thumbnail;

        }

    }
}
