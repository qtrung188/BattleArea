using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetItems()
        {
            var items = await _shopService.GetItemsAsync();
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

            var (outcome, response) = await _shopService.BuyAsync(userId.Value, request.ItemId, request.Quantity);

            return outcome switch
            {
                BuyOutcome.Success => Ok(response),
                BuyOutcome.ItemNotFound => NotFound(new { message = "Item not found." }),
                BuyOutcome.UserNotFound => Unauthorized(new { message = "User not found." }),
                BuyOutcome.InsufficientGold => BadRequest(new { message = "Not enough gold." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
