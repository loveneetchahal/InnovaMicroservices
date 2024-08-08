using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stock.API.Models;

namespace Stock.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        [HttpGet("{productId:int}")]
        public IActionResult StockCheck(int productId)
        {
            return Ok(new StockCheckResponseDto(productId, true));
        }
    }
}