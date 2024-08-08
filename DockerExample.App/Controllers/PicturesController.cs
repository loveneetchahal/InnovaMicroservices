using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DockerExample.App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SavePicture(IFormFile picture)
        {
            // save to file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pictures", picture.FileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }

            return Ok();
        }
    }
}