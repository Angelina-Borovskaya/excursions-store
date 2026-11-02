// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Все методы только для админов
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === СТАТИСТИКА ===

        // GET: api/admin/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            var totalTours = await _context.Tours.CountAsync();
            var activeTours = await _context.Tours.CountAsync(t => t.IsActive);
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalUsers = await _context.Clients.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);

            // Популярные туры (по количеству заказов)
            var popularTours = await _context.Tours
                .Include(t => t.Orders)
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.Orders.Count)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    OrderCount = t.Orders.Count,
                    AverageRating = t.Reviews.Any() ? t.Reviews.Average(r => r.Rating) : 0
                })
                .ToListAsync();

            return new
            {
                summary = new
                {
                    totalTours,
                    activeTours,
                    totalOrders,
                    pendingOrders,
                    totalUsers,
                    totalReviews,
                    totalRevenue = Math.Round(totalRevenue, 2)
                },
                popularTours,
                lastUpdated = DateTime.UtcNow
            };
        }

        // === ОТЧЕТЫ ===

        // GET: api/admin/reports/sales?startDate=2025-01-01&endDate=2025-12-31
        [HttpGet("reports/sales")]
        public async Task<ActionResult<object>> GetSalesReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate);
            
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate);

            var orders = await query
                .Include(o => o.Tour)
                .Include(o => o.Client)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalPrice);
            var orderCount = orders.Count;

            // Группировка по турам
            var toursRevenue = orders
                .GroupBy(o => o.Tour)
                .Select(g => new
                {
                    TourId = g.Key?.Id,
                    TourTitle = g.Key?.Title,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(t => t.Revenue)
                .ToList();

            // Группировка по месяцам
            var monthlyRevenue = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            return new
            {
                period = new
                {
                    startDate = startDate?.ToString("yyyy-MM-dd") ?? "Все время",
                    endDate = endDate?.ToString("yyyy-MM-dd") ?? "Все время"
                },
                summary = new
                {
                    totalOrders = orderCount,
                    totalRevenue = Math.Round(totalRevenue, 2),
                    averageOrderValue = orderCount > 0 ? Math.Round(totalRevenue / orderCount, 2) : 0
                },
                toursRevenue,
                monthlyRevenue,
                orders = orders.Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalPrice,
                    o.Status,
                    TourTitle = o.Tour?.Title,
                    ClientName = o.Client != null ? $"{o.Client.FirstName} {o.Client.LastName}" : "Неизвестно"
                })
            };
        }

        // === УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ===

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Clients
                .Select(c => new
                {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.Role,
                    c.RegistrationDate,
                    OrderCount = c.Orders.Count,
                    ReviewCount = c.Reviews.Count
                })
                .OrderByDescending(c => c.RegistrationDate)
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{id}/role
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string role)
        {
            var user = await _context.Clients.FindAsync(id);
            if (user == null) return NotFound();

            // Проверяем допустимые роли
            if (role != "Admin" && role != "User")
                return BadRequest(new { message = "Недопустимая роль. Используйте 'Admin' или 'User'" });

            user.Role = role;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Роль пользователя обновлена", userId = id, newRole = role });
        }

        // === БЫСТРЫЕ ДЕЙСТВИЯ ===

        // POST: api/admin/tours/{id}/activate
        [HttpPost("tours/{id}/activate")]
        public async Task<IActionResult> ActivateTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Тур активирован", tourId = id });
        }

        // POST: api/admin/tours/{id}/deactivate
        [HttpPost("tours/{id}/deactivate")]
        public async Task<IActionResult> DeactivateTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Тур деактивирован", tourId = id });
        }

        // PUT: api/admin/orders/{id}/status
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // Допустимые статусы
            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };
            if (!validStatuses.Contains(status))
                return BadRequest(new { message = $"Недопустимый статус. Используйте: {string.Join(", ", validStatuses)}" });

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус заказа обновлен", orderId = id, newStatus = status });
        }

        // === СИСТЕМНАЯ ИНФОРМАЦИЯ ===

        // GET: api/admin/system-info
        [HttpGet("system-info")]
        public ActionResult<object> GetSystemInfo()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            return new
            {
                application = new
                {
                    name = "Excursions Store API",
                    version = assembly.GetName().Version?.ToString() ?? "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                },
                server = new
                {
                    time = DateTime.UtcNow,
                    timezone = TimeZoneInfo.Local.DisplayName,
                    machineName = Environment.MachineName
                },
                database = new
                {
                    provider = "PostgreSQL",
                    connected = _context.Database.CanConnect()
                }
            };
        }
    }
}