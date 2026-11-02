using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourOperatorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TourOperatorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourOperator>>> GetTourOperators()
        {
            return await _context.TourOperators.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TourOperator>> GetTourOperator(int id)
        {
            var tourOperator = await _context.TourOperators.FindAsync(id);

            if (tourOperator == null)
            {
                return NotFound();
            }

            return tourOperator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TourOperator>> PostTourOperator(TourOperator tourOperator)
        {
            _context.TourOperators.Add(tourOperator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTourOperator", new { id = tourOperator.Id }, tourOperator);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutTourOperator(int id, TourOperator tourOperator)
        {
            if (id != tourOperator.Id)
            {
                return BadRequest();
            }

            _context.Entry(tourOperator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourOperatorExists(id))
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
        public async Task<IActionResult> DeleteTourOperator(int id)
        {
            var tourOperator = await _context.TourOperators.FindAsync(id);
            if (tourOperator == null)
            {
                return NotFound();
            }

            _context.TourOperators.Remove(tourOperator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/tours")]
        public async Task<ActionResult<IEnumerable<Tour>>> GetToursByOperator(int id)
        {
            var tourOperator = await _context.TourOperators
                .Include(to => to.Tours)
                .FirstOrDefaultAsync(to => to.Id == id);

            if (tourOperator == null)
            {
                return NotFound();
            }

            return Ok(tourOperator.Tours);
        }

        private bool TourOperatorExists(int id)
        {
            return _context.TourOperators.Any(e => e.Id == id);
        }
    }
}