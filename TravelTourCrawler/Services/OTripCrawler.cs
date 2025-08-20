using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using TravelTourCrawler.Data;
using TravelTourCrawler.DTO;
using TravelTourCrawler.Models;

namespace TravelTourCrawler.Services
{
    public class OTripCrawler : ITourCrawler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OTripCrawler> _logger;
        private readonly ApplicationDbContext _context;
        public string Source => "OTrip";
        public OTripCrawler(HttpClient httpClient, ILogger<OTripCrawler> logger, ApplicationDbContext applicationDbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _context = applicationDbContext;

            // Cấu hình HttpClient
            _httpClient.BaseAddress = new Uri("https://otrip.vn/");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<List<Tour>> CrawlWithCustomClassesAsync(CrawlRequestDto dto)
        {
            var tours = new List<Tour>();

            var response = await _httpClient.GetAsync(dto.Url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            var tourNodes = htmlDoc.DocumentNode.SelectNodes($"//div[contains(@class, '{dto.ListContainerClass}')]");

            if (tourNodes != null)
            {
                foreach (var node in tourNodes)
                {
                    //Console.WriteLine(node.InnerHtml);
                    var tour = new Tour { Source = "OTrip" };
                    

                    // Title & URL
                    var titleNode = node.SelectSingleNode($".//{dto.TitleSelector}");
                    if (titleNode != null)
                    {
                        tour.Title = titleNode.GetAttributeValue("title", "");
                        tour.Url = titleNode.GetAttributeValue("href", "");
                    }

                    // Image
                    var imageNode = node.SelectSingleNode($".//{dto.ImageSelector}");
                    if (imageNode != null)
                    {
                        var outer = imageNode.OuterHtml;
                        _logger.LogInformation("IMG NODE: " + imageNode.OuterHtml);
                        Console.WriteLine("IMG NODE: " + outer);

                        string src = imageNode.GetAttributeValue("src", "");
                        string dataLazy = imageNode.GetAttributeValue("data-lazy-src", "");
                        string dataSrc = imageNode.GetAttributeValue("data-src", "");
                        string srcset = imageNode.GetAttributeValue("srcset", "");

                        Console.WriteLine($"src={src} | data-lazy-src={dataLazy} | data-src={dataSrc} | srcset={srcset}");

                        // Nếu src là placeholder hoặc rỗng => lấy từ data-lazy-src
                        if (string.IsNullOrWhiteSpace(src) || src.StartsWith("data:image"))
                        {
                            if (!string.IsNullOrWhiteSpace(dataLazy))
                                src = dataLazy;
                            else if (!string.IsNullOrWhiteSpace(dataSrc))
                                src = dataSrc;
                            else if (!string.IsNullOrWhiteSpace(srcset))
                                src = srcset.Split(',').FirstOrDefault()?.Trim().Split(' ')[0];

                        }

                        tour.ImageUrl = src;
                    }


                    // Detail fields
                    var detailNodes = node.SelectNodes($".//div[contains(@class, '{dto.DetailContainerClass}')]");
                    if (detailNodes != null)
                    {
                        foreach (var detail in detailNodes)
                        {
                            var label = detail.SelectSingleNode($".//span[contains(@class, '{dto.LabelClass}')]")?.InnerText.Trim();
                            var value = detail.SelectSingleNode($".//span[not(contains(@class, '{dto.LabelClass}'))]")?.InnerText.Trim();

                            if (label != null && value != null)
                            {
                                if (label.Contains("Điểm khởi hành"))
                                    tour.DeparturePoint = value;
                                else if (label.Contains("Điểm đến"))
                                    tour.Destination = value;
                                else if (label.Contains("Lịch trình"))
                                    tour.Duration = value;
                                else if (label.Contains("Khởi hành"))
                                    tour.DepartureTime = value;
                                else if (label.Contains("Phương tiện"))
                                    tour.Transportation = value;
                            }
                        }
                    }

                    // Price
                    var priceNode = node.SelectSingleNode($".//p[contains(@class, '{dto.PriceClass}')]");
                    if (priceNode != null)
                    {
                        tour.Price = priceNode.InnerText.Trim();
                    }
                    if (!_context.Tours.Any(t => t.Url == tour.Url))
                    {
                        tours.Add(tour);
                    }
                }
            }

            await Task.Delay(2000);

            // Lọc trùng theo Url trong danh sách mới
            var distinctTours = tours
                .Where(t => !string.IsNullOrEmpty(t.Url))
                .GroupBy(t => t.Url)
                .Select(g => g.First()) // Giữ lại bản ghi đầu tiên của mỗi URL
                .ToList();

            //// Loại bỏ các tour đã tồn tại trong DB
            var existingUrls = _context.Tours
                .Select(t => t.Url)
                .ToHashSet();

            var newTours = distinctTours
                .Where(t => !existingUrls.Contains(t.Url))
                .ToList();

            ////// Lưu vào DB
            if (newTours.Any())
            {
                await _context.Tours.AddRangeAsync(newTours);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Saved {newTours.Count} new tours to DB");
            }
            else
            {
                _logger.LogInformation("No new tours to save (all already exist in DB or duplicate in source)");
            }
            
            return tours;
        }


        public async Task<List<Tour>> CrawlToursAsync(string url)
        {
            var tours = new List<Tour>();
            try
            {

                _logger.LogInformation($"Starting crawling with {url}");

                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    var tourNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'item-category')]");

                    if (tourNodes != null)
                    {
                        foreach (var tourNode in tourNodes)
                        {
                            var tour = new Tour { Source = "OTrip" };

                            var titleNode = tourNode.SelectSingleNode(".//div/a");
                            if (titleNode != null)
                            {
                                tour.Title = titleNode.GetAttributeValue("title", "");
                                tour.Url = titleNode.GetAttributeValue("href", "");
                            }

                        var imageNode = tourNode.SelectSingleNode(".//img");
                        if (imageNode != null)
                        {
                            tour.ImageUrl = imageNode.GetAttributeValue("src", "") ?? imageNode.GetAttributeValue("data-src", "");
                        }


                        var detailNodes = tourNode.SelectNodes(".//div[contains(@class, 'item-content-detail')]");
                            if (detailNodes != null)
                            {
                                foreach (var detailNode in detailNodes)
                                {
                                    var label = detailNode.SelectSingleNode(".//span[contains(@class, 'item-content-p')]")?.InnerText.Trim();
                                    var value = detailNode.SelectSingleNode(".//span[not(contains(@class, 'item-content-p'))]")?.InnerText.Trim();

                                    if (label != null && value != null)
                                    {
                                        if (label.Contains("Điểm khởi hành:"))
                                            tour.DeparturePoint = value;
                                        else if (label.Contains("Điểm đến:"))
                                            tour.Destination = value;
                                        else if (label.Contains("Lịch trình:"))
                                            tour.Duration = value;
                                        else if (label.Contains("Khởi hành:"))
                                            tour.DepartureTime = value;
                                        else if (label.Contains("Phương tiện:"))
                                            tour.Transportation = value;
                                    }
                                }
                            }

                            var priceNode = tourNode.SelectSingleNode(".//p[contains(@class, 'price-new')]");
                            if (priceNode != null)
                            {
                                tour.Price = priceNode.InnerText.Trim();
                            }

                        // Tránh duplicate do khóa unique Url
                        if (!_context.Tours.Any(t => t.Url == tour.Url))
                        {
                            tours.Add(tour);
                        }
                        //_logger.LogInformation($"Crawl {tours.Count+1} ");

                        //tours.Add(tour);

                        }
                    }

                    await Task.Delay(2000);
                
                // Lọc trùng theo Url trong danh sách mới
                var distinctTours = tours
                    .Where(t => !string.IsNullOrEmpty(t.Url))
                    .GroupBy(t => t.Url)
                    .Select(g => g.First()) // Giữ lại bản ghi đầu tiên của mỗi URL
                    .ToList();

                //// Loại bỏ các tour đã tồn tại trong DB
                var existingUrls = _context.Tours
                    .Select(t => t.Url)
                    .ToHashSet();

                var newTours = distinctTours
                    .Where(t => !existingUrls.Contains(t.Url))
                    .ToList();

                ////// Lưu vào DB
                /*/if (newTours.Any())
                {
                    await _context.Tours.AddRangeAsync(newTours);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Saved {newTours.Count} new tours to DB");
                }
                else
                {
                    _logger.LogInformation("No new tours to save (all already exist in DB or duplicate in source)");
                }/*/


                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crawling OTrip");
                return new List<Tour>();
            }
        }
    }
}