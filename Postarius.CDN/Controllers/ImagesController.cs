using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using Web.StaticFiles.Models.Images;

namespace Web.StaticFiles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private ILogger<ImagesController> Logger { get; }
        private IImageService ImageService { get; }

        public ImagesController(ILogger<ImagesController> logger, IImageService imageService)
        {
            Logger = logger;
            ImageService = imageService;
        }
        
        [HttpPost("Upload")]
        public async Task<ActionResult> Upload([FromForm(Name = "images")] IFormFileCollection files)
        {
            var model = new UploadModel();

            foreach (var file in files)
            {
                var meta = ImageService.Save(file.OpenReadStream());
                
                model.Images.Add(new UploadModel.ImageInfo
                {
                    RawUrl = meta.RawUrl,
                    Cropped150x150Url = meta.Cropped150x150Url,
                    Cropped300x400Url = meta.Cropped300x400Url
                });
            }
            
            model.Date = DateTime.UtcNow;

            return Ok(model);
        }

        [HttpPost("Crop")]
        public async Task<ActionResult> Crop([FromForm(Name = "images")] IFormFile file)
        {
            var inputStream = file.OpenReadStream();
            var resultStream = ImageService.CropImage(inputStream, 300, 400);
            resultStream.Position = 0;
            return File(resultStream, "image/png");
        }
    }
}