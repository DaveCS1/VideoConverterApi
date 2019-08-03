using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using VideoConverter.BL;
using VideoConverter.DM;

namespace VideoConverterApi.Controllers
{
    [Route("api/[controller]")]
    public class VideoController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly VideoBL videoBL;

        public VideoController(IConfiguration iConfig)
        {
            configuration = iConfig;
            videoBL = new VideoBL(iConfig);
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                List<Video> list = videoBL.GetAllVideos();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Video video = videoBL.GetVideoById(id);
                return Ok(video);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]Video video)
        {
            try
            {
                int videoId = videoBL.InsertVideo(video);
                return Ok(videoId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }

        }

        //// PUT api/<controller>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                videoBL.Delete(id);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
