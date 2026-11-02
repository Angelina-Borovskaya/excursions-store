using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourTagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TourTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === ПУБЛИЧНЫЕ МЕТОДЫ ===

        // GET: api/tourtags/tour/{tourId} 
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsByTour(int tourId)
        {
            var tags = await _context.TourTags
                .Where(tt => tt.TourId == tourId)
                .Include(tt => tt.Tag)
                .Select(tt => tt.Tag)
                .ToListAsync();

            return Ok(tags);
        }

        // GET: api/tourtags/tag/{tagId} 
        [HttpGet("tag/{tagId}")]
        public async Task<ActionResult<IEnumerable<Tour>>> GetToursByTag(int tagId)
        {
            var tours = await _context.TourTags
                .Where(tt => tt.TagId == tagId)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.City)
                    .ThenInclude(c => c!.Country)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.Category)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.TourOperator)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.TourTags)
                        .ThenInclude(tt=> tt.Tag)
                .Where(tt => tt.Tour != null && tt.Tour.IsActive)
                .Select(tt => tt.Tour!)
                .ToListAsync();

            return Ok(tours);
        }

        // GET: api/tourtags/popular-tags 
        [HttpGet("popular-tags")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularTags([FromQuery] int limit = 10)
        {
            var popularTags = await _context.TourTags
                .GroupBy(tt => tt.Tag)
                .Where(g => g.Key != null)
                .Select(g => new
                {
                    Tag = g.Key!,
                    TourCount = g.Count()
                })
                .OrderByDescending(x => x.TourCount)
                .Take(limit)
                .ToListAsync();

            return Ok(popularTags);
        }

        // === МЕТОДЫ ДЛЯ АДМИНОВ ===

        // POST: api/tourtags 
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TourTag>> PostTourTag(TourTag tourTag)
        {
            // Проверяем, существует ли уже такая связь
            var exists = await _context.TourTags
                .AnyAsync(tt => tt.TourId == tourTag.TourId && tt.TagId == tourTag.TagId);

            if (exists)
            {
                return BadRequest(new { message = "Связь уже существует" });
            }

            _context.TourTags.Add(tourTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTagsByTour), new { tourId = tourTag.TourId }, tourTag);
        }

        // DELETE: api/tourtags/{id} 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTourTag(int id)
        {
            var tourTag = await _context.TourTags.FindAsync(id);
            if (tourTag == null) return NotFound();

            _context.TourTags.Remove(tourTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/tourtags/tour/{tourId}/tag/{tagId} 
        [HttpDelete("tour/{tourId}/tag/{tagId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTourTagByTourAndTag(int tourId, int tagId)
        {
            var tourTag = await _context.TourTags
                .FirstOrDefaultAsync(tt => tt.TourId == tourId && tt.TagId == tagId);

            if (tourTag == null) return NotFound();

            _context.TourTags.Remove(tourTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/tourtags
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TourTag>>> GetTourTags()
        {
            return await _context.TourTags
                .Include(tt => tt.Tour)
                .Include(tt => tt.Tag)
                .ToListAsync();
        }
    }
}