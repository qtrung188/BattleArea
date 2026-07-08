using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IShopService _shopService;

        public InventoryController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<ActionResult<List<InventoryItemDto>>> GetInventory()
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var items = await _shopService.GetInventoryAsync(userId.Value);
            return Ok(items);
        }
    }
}
