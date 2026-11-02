using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using System.Security.Claims;
using ExcursionsStore.API.DTOs;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ВСЕ методы требуют авторизации
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/cart - моя корзина
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
        {
            var userId = GetUserId();
            return await _context.CartItems
                .Where(c => c.ClientId == userId)
                .Include(c => c.Tour)
                    .ThenInclude(t => t!.City)
                    .ThenInclude(c => c!.Country)
                .Include(c => c.Tour)
                    .ThenInclude(t => t!.Category)
                .Include(c => c.Tour)
                    .ThenInclude(t => t!.TourOperator)
                .Include(c => c.Tour)
                    .ThenInclude(t => t!.TourTags)
                        .ThenInclude(tt=> tt.Tag)
                .ToListAsync();
        }

        // GET: api/cart/summary - сумма корзины
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetCartSummary()
        {
            var userId = GetUserId();
            var items = await _context.CartItems
                .Where(c => c.ClientId == userId)
                .Include(c => c.Tour)
                .ToListAsync();

            var total = items.Sum(item => item.Quantity * (item.Tour?.Price ?? 0));
            var itemCount = items.Sum(item => item.Quantity);

            return Ok(new
            {
                totalItems = items.Count,
                totalQuantity = itemCount,
                totalPrice = total,
                items
            });
        }

        // POST: api/cart - добавить в корзину
       [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.ClientId == userId &&
                    c.TourId == dto.TourId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(existingItem);
            }

            var item = new CartItem
            {
                ClientId = userId,
                TourId = dto.TourId,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        // PUT: api/cart/{id} - изменить количество
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { message = "Количество должно быть больше 0" });
            }

            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            // Проверяем, что элемент принадлежит пользователю
            var userId = GetUserId();
            if (item.ClientId != userId) return Forbid();

            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/cart/{id} - удалить из корзины
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            // Проверяем, что элемент принадлежит пользователю
            var userId = GetUserId();
            if (item.ClientId != userId) return Forbid();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/cart/clear - очистить корзину
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var items = await _context.CartItems
                .Where(c => c.ClientId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/cart/checkout - оформить заказ из корзины
        [HttpPost("checkout")]
        public async Task<ActionResult<Order>> Checkout()
        {
            var userId = GetUserId();
            
            // Получаем элементы корзины
            var cartItems = await _context.CartItems
                .Where(c => c.ClientId == userId)
                .Include(c => c.Tour)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest(new { message = "Корзина пуста" });
            }

            // Создаем заказ для каждого тура в корзине
            var orders = new List<Order>();
            
            foreach (var cartItem in cartItems)
            {
                var order = new Order
                {
                    ClientId = userId,
                    TourId = cartItem.TourId,
                    GuestsCount = cartItem.Quantity,
                    OrderDate = DateTime.UtcNow,
                    Status = "Завершён",
                    TotalPrice = cartItem.Quantity * (cartItem.Tour?.Price ?? 0)
                };
                
                orders.Add(order);
            }

            // Добавляем заказы
            _context.Orders.AddRange(orders);
            
            // Очищаем корзину
            _context.CartItems.RemoveRange(cartItems);
            
            await _context.SaveChangesAsync();
            
            return Ok(new 
            { 
                message = "Заказ оформлен", 
                orderCount = orders.Count,
                orders 
            });
        }

        // ПОЛУЧЕНИЕ ID текущего пользователя
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found");
        }
    }
}