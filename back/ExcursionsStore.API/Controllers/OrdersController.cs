using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // ← ДОБАВИТЬ

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Tour)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Tour)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            
            // Проверка прав: админ или владелец
            var userId = GetUserId();
            if (User.IsInRole("Admin") || order.ClientId == userId)
            {
                return order;
            }
            
            return Forbid();
        }

        // СОЗДАНИЕ ЗАКАЗА 
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // Автоматически устанавливаем текущего пользователя
            var userId = GetUserId();
            order.ClientId = userId;
            order.OrderDate = DateTime.UtcNow;
            
            // Вычисляем общую стоимость
            var tour = await _context.Tours.FindAsync(order.TourId);
            if (tour != null)
            {
                order.TotalPrice = tour.Price * order.GuestsCount;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // ОБНОВЛЕНИЕ 
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id) return BadRequest();
            _context.Entry(order).State = EntityState.Modified;
            
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // ИЗМЕНЕНИЕ СТАТУСА 
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            
            order.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // УДАЛЕНИЕ 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // МОИ ЗАКАЗЫ 
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();

            var orders = await _context.Orders
                .Where(o => o.ClientId == userId)
                .Select(o => new OrderProfileDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TourId = o.TourId,
                    TourTitle = o.Tour!.Title,

                    HasReview = _context.Reviews.Any(r =>
                        r.OrderId == o.Id)
                })
                .ToListAsync();

            return Ok(orders);
        }



        // Заказы клиента 
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetClientOrders(int clientId)
        {
            var orders = await _context.Orders
                .Include(o => o.Tour)
                .Where(o => o.ClientId == clientId)
                .ToListAsync();
            return orders;
        }

        // Заказы по туру 
        [HttpGet("tour/{tourId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetTourOrders(int tourId)
        {
            var orders = await _context.Orders
                .Include(o => o.Client)
                .Where(o => o.TourId == tourId)
                .ToListAsync();
            return orders;
        }

        // Получение ID текущего пользователя
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found");
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}