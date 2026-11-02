using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutTag(int id, Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/tours")]
        public async Task<ActionResult<IEnumerable<Tour>>> GetToursByTag(int id)
        {
            var tours = await _context.TourTags
                .Where(tt => tt.TagId == id)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.City)
                        .ThenInclude(c => c!.Country)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.Category)
                .Include(tt => tt.Tour)
                    .ThenInclude(t => t!.TourOperator)
                .Where(tt => tt.Tour != null && tt.Tour.IsActive)
                .Select(tt => tt.Tour!)
                .ToListAsync();

            return Ok(tours);
        }


        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}