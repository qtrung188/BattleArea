using Asp.Versioning;
using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IShopService _shopService;

        public InventoryController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<InventoryItemDto>>> GetInventory([FromQuery] PagedRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var items = await _shopService.GetInventoryAsync(userId.Value, request);
            return Ok(items);
        }
    }
}
