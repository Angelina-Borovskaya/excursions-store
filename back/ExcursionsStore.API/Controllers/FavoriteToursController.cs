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
    public class FavoriteToursController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoriteToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/favoriteTours 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FavoriteTour>>> GetFavoriteTours()
        {
            return await _context.FavoriteTours
                .Include(ft => ft.Client)
                .Include(ft => ft.Tour)
                .ToListAsync();
        }

        // GET: api/favoriteTours/{id} 
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FavoriteTour>> GetFavoriteTour(int id)
        {
            var favoriteTour = await _context.FavoriteTours
                .Include(ft => ft.Client)
                .Include(ft => ft.Tour)
                .FirstOrDefaultAsync(ft => ft.Id == id);

            if (favoriteTour == null) return NotFound();
            return favoriteTour;
        }

        // POST: api/favoriteTours 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteDto dto)
        {
            var userId = GetUserId();

            // Проверка на дубликат
            var exists = await _context.FavoriteTours
                .AnyAsync(f => f.ClientId == userId && f.TourId == dto.TourId);

            if (exists)
                return BadRequest(new { message = "Тур уже в избранном" });

            // Проверка существования тура
            var tourExists = await _context.Tours
                .AnyAsync(t => t.Id == dto.TourId);

            if (!tourExists)
                return BadRequest(new { message = "Тур не найден" });

            var favorite = new FavoriteTour
            {
                ClientId = userId,
                TourId = dto.TourId,
                CreatedAt = DateTime.UtcNow
            };

            _context.FavoriteTours.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/favoriteTours/{id} 
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFavoriteTour(int id)
        {
            var favoriteTour = await _context.FavoriteTours.FindAsync(id);
            if (favoriteTour == null) return NotFound();
            
            // Проверка: владелец ИЛИ админ
            var userId = GetUserId();
            if (favoriteTour.ClientId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            _context.FavoriteTours.Remove(favoriteTour);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/favoriteTours/tour/{tourId} 
        [HttpDelete("tour/{tourId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromFavoritesByTour(int tourId)
        {
            var userId = GetUserId();
            var favoriteTour = await _context.FavoriteTours
                .FirstOrDefaultAsync(ft => ft.ClientId == userId && ft.TourId == tourId);

            if (favoriteTour == null) return NotFound();
            
            _context.FavoriteTours.Remove(favoriteTour);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/favoriteTours/my 
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = GetUserId();

            var favorites = await _context.FavoriteTours
                .Where(ft => ft.ClientId == userId)
                .Select(ft => new
                {
                    ft.Id,
                    Tour = new
                    {
                        ft.Tour!.Id,
                        ft.Tour.Title,
                        ft.Tour.Description,
                        ft.Tour.Price,
                        ft.Tour.ImageFileName,
                        ft.Tour.AverageRating,
                        City = new
                        {
                            ft.Tour.City!.Id,
                            ft.Tour.City.Name,
                            Country = new
                            {
                                ft.Tour.City.Country!.Id,
                                ft.Tour.City.Country.Name
                            }
                        }
                    }
                })
                .ToListAsync();

            return Ok(favorites);
        }


        // GET: api/favoriteTours/check/{tourId} 
        [HttpGet("check/{tourId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckIfFavorite(int tourId)
        {
            var userId = GetUserId();
            var isFavorite = await _context.FavoriteTours
                .AnyAsync(ft => ft.ClientId == userId && ft.TourId == tourId);

            return Ok(new { tourId, isFavorite });
        }

        // GET: api/favoriteTours/tour/{tourId}/count 
        [HttpGet("tour/{tourId}/count")]
        public async Task<ActionResult<int>> GetFavoriteCount(int tourId)
        {
            var count = await _context.FavoriteTours
                .CountAsync(ft => ft.TourId == tourId);
            return Ok(new { tourId, favoriteCount = count });
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

        private bool FavoriteTourExists(int id)
        {
            return _context.FavoriteTours.Any(e => e.Id == id);
        }
    }
}