using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using ExcursionsStore.API.DTOs.Tours;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

// GET: api/tours 
[HttpGet]
public async Task<IActionResult> GetTours(
    [FromQuery] int[]? countryId,
    [FromQuery] int[]? cityId,
    [FromQuery] int[]? categoryId,
    [FromQuery] int[]? tourOperatorId,
    [FromQuery] int[]? tagId,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] decimal? minRating,
    [FromQuery] DateTime? startDate,
    [FromQuery] string? search
)

{
    var query = _context.Tours
        .Include(t => t.City)
            .ThenInclude(c => c!.Country)
        .Include(t => t.Category)
        .Include(t => t.TourOperator)
        .Include(t => t.TourTags)
            .ThenInclude(tt => tt.Tag)
        .Where(t => t.IsActive)
        .AsQueryable();

   if (countryId?.Length > 0)
    query = query.Where(t => countryId.Contains(t.City!.CountryId));

    if (cityId?.Length > 0)
        query = query.Where(t => cityId.Contains(t.CityId));

    if (categoryId?.Length > 0)
        query = query.Where(t => categoryId.Contains(t.CategoryId));

    if (tourOperatorId?.Length > 0)
        query = query.Where(t => tourOperatorId.Contains(t.TourOperatorId));

    if (tagId?.Length > 0)
        query = query.Where(t =>
            t.TourTags.Any(tt => tagId.Contains(tt.TagId)));

    // Фильтрация по цене
    if (minPrice.HasValue)
        query = query.Where(t => t.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(t => t.Price <= maxPrice.Value);

    if (minRating.HasValue)
    query = query.Where(t => t.AverageRating >= minRating.Value);

    // Фильтрация по дате начала
    if (startDate.HasValue)
        query = query.Where(t => t.StartDate >= startDate.Value);

    // Поиск
    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchLower = search.ToLower();
        query = query.Where(t =>
            t.Title.ToLower().Contains(searchLower) ||
            (t.Description != null &&
             t.Description.ToLower().Contains(searchLower)));
    }

    var result = await query
        .Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.Price,
            t.StartDate,
            t.EndDate,
            t.ImageFileName,
            t.AverageRating,

            City = new
            {
                t.City!.Id,
                t.City.Name,
                Country = new
                {
                    t.City.Country!.Id,
                    t.City.Country.Name
                }
            },

            Category = new
            {
                t.Category!.Id,
                t.Category.Name
            },

            TourOperator = new
            {
                t.TourOperator!.Id,
                t.TourOperator.Name
            },

            Tags = t.TourTags
                .Select(tt => new
                {
                    tt.Tag!.Id,
                    tt.Tag.Name
                })
                .ToList()
        })
        .ToListAsync();

    return Ok(result);
}


        // GET: api/tours/{id} 
        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetTour(int id)
        {
            var tour = await _context.Tours
                .Include(t => t.City)
                    .ThenInclude(c => c!.Country)
                .Include(t => t.Category)
                .Include(t => t.TourOperator)
                .Include(t => t.TourTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tour == null)
                return NotFound();

            return tour;
        }

        // POST: api/tours 
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> PostTour([FromForm] CreateTourDto dto)
        {
            // 1) создаём тур
            var tour = new Tour
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
                CityId = dto.CityId,
                CategoryId = dto.CategoryId,
                TourOperatorId = dto.TourOperatorId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                AverageRating = 0
            };

            // 2) сохраняем фото
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");
                Directory.CreateDirectory(uploadsDir);

                var ext = Path.GetExtension(dto.Image.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                tour.ImageFileName = fileName;
            }

            // 3) добавляем тур
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            // 4) теги (через join-таблицу)
            if (dto.TagIds != null && dto.TagIds.Count > 0)
            {
                var uniqueTagIds = dto.TagIds.Distinct().ToList();

                var existingTags = await _context.Tags
                    .Where(t => uniqueTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tagId in existingTags)
                {
                    _context.TourTags.Add(new TourTag { TourId = tour.Id, TagId = tagId });
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { tour.Id });
        }



        // PUT: api/tours/{id} 
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> PutTour(int id, [FromForm] UpdateTourDto dto)
        {
            var tour = await _context.Tours
                .Include(t => t.TourTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();

            tour.Title = dto.Title;
            tour.Description = dto.Description;
            tour.Price = dto.Price;
            tour.StartDate = dto.StartDate;
            tour.EndDate = dto.EndDate;
            tour.CityId = dto.CityId;
            tour.CategoryId = dto.CategoryId;
            tour.TourOperatorId = dto.TourOperatorId;

            if (dto.IsActive.HasValue)
                tour.IsActive = dto.IsActive.Value;

            // фото (если прислали новое)
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");
                Directory.CreateDirectory(uploadsDir);

                var ext = Path.GetExtension(dto.Image.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                tour.ImageFileName = fileName;
            }

            // синхронизация тегов
            var newTagIds = (dto.TagIds ?? new List<int>()).Distinct().ToHashSet();
            var currentTagIds = tour.TourTags.Select(x => x.TagId).ToHashSet();

            var toAdd = newTagIds.Except(currentTagIds).ToList();
            var toRemove = currentTagIds.Except(newTagIds).ToList();

            if (toRemove.Count > 0)
            {
                var removeEntities = tour.TourTags.Where(tt => toRemove.Contains(tt.TagId)).ToList();
                _context.TourTags.RemoveRange(removeEntities);
            }

            foreach (var tagId in toAdd)
                _context.TourTags.Add(new TourTag { TourId = tour.Id, TagId = tagId });

            await _context.SaveChangesAsync();
            return NoContent();
        }
        // DELETE: api/tours/{id} 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                return NotFound();

            tour.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/tours/{id}/tags/{tagId} 
        [HttpPost("{id}/tags/{tagId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTagToTour(int id, int tagId)
        {
            var tour = await _context.Tours
                .Include(t => t.TourTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            if (!_context.Tags.Any(t => t.Id == tagId))
                return NotFound();

            if (tour.TourTags.Any(tt => tt.TagId == tagId))
                return BadRequest(new { message = "Тег уже добавлен к туру" });

            tour.TourTags.Add(new TourTag
            {
                TourId = id,
                TagId = tagId
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tours/{id}/tags/{tagId} 
        [HttpDelete("{id}/tags/{tagId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveTagFromTour(int id, int tagId)
        {
            var tour = await _context.Tours
                .Include(t => t.TourTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            var tourTag = tour.TourTags
                .FirstOrDefault(tt => tt.TagId == tagId);

            if (tourTag == null)
                return NotFound();

            tour.TourTags.Remove(tourTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
