using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using ExcursionsStore.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Tour)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Commenting = r.Commenting,
                    CreatedAt = r.CreatedAt,
                    ClientId = r.ClientId,
                    TourId = r.TourId,
                    ClientName = r.Client != null ? $"{r.Client.FirstName} {r.Client.LastName}" : null,
                    TourTitle = r.Tour != null ? r.Tour.Title : null
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/{id} 
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Tour)
                .Where(r => r.Id == id)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Commenting = r.Commenting,
                    CreatedAt = r.CreatedAt,
                    ClientId = r.ClientId,
                    TourId = r.TourId,
                    ClientName = r.Client != null ? $"{r.Client.FirstName} {r.Client.LastName}" : null,
                    TourTitle = r.Tour != null ? r.Tour.Title : null
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // GET: api/reviews/tour/{tourId} 
        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<object>> GetTourReviews(int tourId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Client)
                .Where(r => r.TourId == tourId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Commenting = r.Commenting,
                    CreatedAt = r.CreatedAt,
                    ClientId = r.ClientId,
                    TourId = r.TourId,
                    ClientName = r.Client != null ? $"{r.Client.FirstName} {r.Client.LastName}" : null
                })
                .ToListAsync();

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return Ok(new
            {
                reviews,
                averageRating = Math.Round(averageRating, 1),
                totalReviews = reviews.Count
            });
        }

        // GET: api/reviews/client/{clientId} 
        [HttpGet("client/{clientId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetClientReviews(int clientId)
        {
            // Проверка: пользователь запрашивает свои отзывы ИЛИ админ
            var userId = GetUserId();
            if (clientId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            var reviews = await _context.Reviews
                .Include(r => r.Tour)
                .Where(r => r.ClientId == clientId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Commenting = r.Commenting,
                    CreatedAt = r.CreatedAt,
                    ClientId = r.ClientId,
                    TourId = r.TourId,
                    TourTitle = r.Tour != null ? r.Tour.Title : null
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews 
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> PostReview(CreateReviewDto dto)
        {
            // текущий пользователь
            var userId = GetUserId();

            // Проверяем, не оставлял ли пользователь уже отзыв на этот заказ
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.OrderId == dto.OrderId &&
                    r.ClientId == userId);

            if (existingReview != null)
            {
                return BadRequest(new { message = "Вы уже оставляли отзыв на этот тур" });
            }

            // Проверяем, существует ли тур
            var tour = await _context.Tours.FindAsync(dto.TourId);
            if (tour == null)
            {
                return BadRequest(new { message = "Тур не найден" });
            }

            // Проверяем, был ли у пользователя завершённый заказ
            var hasOrder = await _context.Orders.AnyAsync(o =>
                o.Id == dto.OrderId &&
                o.ClientId == userId &&
                o.TourId == dto.TourId &&
                o.Status == "Завершён"
            );

            if (!hasOrder && !User.IsInRole("Admin"))
            {
                return BadRequest(new { message = "Вы не можете оставить отзыв на тур, который не заказывали" });
            }

            // Проверяем рейтинг (1–5)
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return BadRequest(new { message = "Рейтинг должен быть от 1 до 5" });
            }

            // 🔹 создаём сущность Review
            var review = new Review
            {
                ClientId = userId,
                TourId = dto.TourId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Commenting = dto.Commenting,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Возвращаем DTO
            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Commenting = review.Commenting,
                CreatedAt = review.CreatedAt,
                ClientId = review.ClientId,
                TourId = review.TourId
            };

            return CreatedAtAction("GetReview", new { id = review.Id }, reviewDto);
        }

        // PUT: api/reviews/{id} 
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutReview(int id, ReviewDto reviewDto)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            
            // Проверка: только владелец может редактировать
            var userId = GetUserId();
            if (review.ClientId != userId)
            {
                return Forbid();
            }
            
            // Проверяем рейтинг (1-5)
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            {
                return BadRequest(new { message = "Рейтинг должен быть от 1 до 5" });
            }

            // Обновляем только разрешенные поля
            review.Rating = reviewDto.Rating;
            review.Commenting = reviewDto.Commenting;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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

        // DELETE: api/reviews/{id} 
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            
            // Проверка: владелец ИЛИ админ
            var userId = GetUserId();
            if (review.ClientId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
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

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}