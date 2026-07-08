using Asp.Versioning;
using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/shop")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet("items")]
        public async Task<ActionResult<PagedResult<ItemDto>>> GetItems([FromQuery] PagedRequest request)
        {
            var items = await _shopService.GetItemsAsync(request);
            return Ok(items);
        }

        [Authorize]
        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BuyRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var response = await _shopService.BuyAsync(userId.Value, request.ItemId, request.Quantity);
            return Ok(response);
        }
    }
}
