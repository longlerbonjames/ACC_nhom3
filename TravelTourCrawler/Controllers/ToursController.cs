using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelTourCrawler.Data;
using TravelTourCrawler.DTO;
using TravelTourCrawler.Models;
using TravelTourCrawler.Services;

namespace TravelTourCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        private readonly IEnumerable<ITourCrawler> _tourCrawlers;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ToursController> _logger;

        public ToursController(
            IEnumerable<ITourCrawler> tourCrawlers,
            ApplicationDbContext context,
            ILogger<ToursController> logger)
        {
            _tourCrawlers = tourCrawlers;
            _context = context;
            _logger = logger;
        }

        // ✅ GET: api/tours
        [HttpGet]
        public async Task<IActionResult> GetAllTours()
        {
            var tours = await _context.Tours
                .OrderByDescending(t => t.CrawledTime)
                .ToListAsync();
            return Ok(tours);
        }

        // ✅ GET: api/tours/crawl?url=https://otrip.vn/...
        [HttpGet("crawl")]
        public async Task<IActionResult> CrawlFromUrl([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { error = "URL is required" });

            _logger.LogInformation($"Start crawling from URL: {url}");

            var crawler = _tourCrawlers.FirstOrDefault(c => c.Source == "OTrip");
            if (crawler is not OTripCrawler otrip)
                return NotFound(new { error = "OTrip crawler not found" });

            var tours = await otrip.CrawlToursAsync(url);
            return Ok(tours);
        }

        // ✅ POST: api/tours/crawl (với JSON selector)
        [HttpPost("crawl")]
        public async Task<IActionResult> CrawlCustom([FromBody] CrawlRequestDto request)
        {
            var crawler = _tourCrawlers.FirstOrDefault(c => c.Source == "OTrip");
            if (crawler is OTripCrawler otrip)
            {
                var tours = await otrip.CrawlWithCustomClassesAsync(request);
                return Ok(tours);
            }

            return NotFound("OTrip crawler not available");
        }

        // ✅ GET: api/tours/sources (cho frontend chọn nguồn sau này)
        [HttpGet("sources")]
        public IActionResult GetAvailableSources()
        {
            var sources = _tourCrawlers.Select(c => c.Source).Distinct().ToList();
            return Ok(sources);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] Tour updatedTour)
        {
            var existing = await _context.Tours.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = updatedTour.Title;
            existing.Destination = updatedTour.Destination;
            existing.Price = updatedTour.Price;
            existing.Duration = updatedTour.Duration;
            existing.DepartureTime = updatedTour.DepartureTime;
            existing.ImageUrl = updatedTour.ImageUrl;
            existing.Url = updatedTour.Url;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
