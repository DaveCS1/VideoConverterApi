using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VideoConverter.BL;
using VideoConverter.DM;

namespace VideoConverterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoConverterController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private VideoConvertBL videoConvertBL;

        public VideoConverterController(IConfiguration iConfig)
        {
            configuration = iConfig;
            videoConvertBL = new VideoConvertBL(iConfig);
        }

        // POST api/converter
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                IFormFile file = Request.Form.Files[0];

                if (file.Length > 0 && VideoHelper.IsMediaFileExtension(file))
                {
                    Video video = videoConvertBL.UploadFile(file);

                    return Ok(video);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }

}