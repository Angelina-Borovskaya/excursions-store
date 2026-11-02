using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // GET: api/Clients/{id} 
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            // Проверяем права: пользователь получает свои данные или админ
            var userId = GetUserId();
            if (client.Id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return client;
        }

        // POST: api/Clients 
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        // PUT: api/Clients/{id} 
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            if (id != client.Id)
            {
                return BadRequest();
            }

            // Проверяем права
            var userId = GetUserId();
            if (client.Id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Clients/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Clients/{id}/orders 
        [HttpGet("{id}/orders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetClientOrders(int id)
        {
            // Проверяем права
            var userId = GetUserId();
            if (id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var client = await _context.Clients
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client.Orders);
        }

        // GET: api/Clients/{id}/reviews 
        [HttpGet("{id}/reviews")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Review>>> GetClientReviews(int id)
        {
            // Проверяем права
            var userId = GetUserId();
            if (id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var client = await _context.Clients
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client.Reviews);
        }

        // GET: api/Clients/{id}/favorites 
        [HttpGet("{id}/favorites")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<FavoriteTour>>> GetClientFavorites(int id)
        {
            // Проверяем права
            var userId = GetUserId();
            if (id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var client = await _context.Clients
                .Include(c => c.FavoriteTours)
                .ThenInclude(ft => ft.Tour)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client.FavoriteTours);
        }

        // Вспомогательный метод для получения ID текущего пользователя
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found");
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}