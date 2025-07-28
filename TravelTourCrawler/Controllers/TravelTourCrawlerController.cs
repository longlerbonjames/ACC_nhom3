using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using TravelTourCrawler.Models;

namespace TravelTourCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicTourCrawlerController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DynamicTourCrawlerController> _logger;

        public DynamicTourCrawlerController(IHttpClientFactory httpClientFactory, ILogger<DynamicTourCrawlerController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("crawl")]
        public async Task<IActionResult> Crawl([FromBody] DynamicCrawlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.ListSelector))
                return BadRequest("Url and ListSelector are required.");

            var client = _httpClientFactory.CreateClient();
            var html = await client.GetStringAsync(request.Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tourList = new List<Tour>();

            var listNodes = doc.DocumentNode.SelectNodes($"//{request.ListSelector}");
            if (listNodes == null)
                return Ok(tourList); // empty

            foreach (var node in listNodes)
            {
                var tour = new Tour();
                foreach (var field in request.Fields)
                {
                    var name = field.Key;
                    var xpath = field.Value;

                    try
                    {
                        string? value;
                        if (xpath.Contains("@")) // lấy thuộc tính
                        {
                            var attrParts = xpath.Split('@');
                            var subXPath = attrParts[0].TrimEnd('/');
                            var attrName = attrParts[1];
                            value = node.SelectSingleNode(subXPath)?.GetAttributeValue(attrName, null);
                        }
                        else
                        {
                            value = node.SelectSingleNode(xpath)?.InnerText?.Trim();
                        }

                        if (value != null)
                        {
                            typeof(Tour).GetProperty(name)?.SetValue(tour, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to extract field {name}");
                    }
                }

                tour.Source = request.Url;
                tourList.Add(tour);
            }

            return Ok(tourList);
        }
    }
}
