namespace TravelTourCrawler.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? DeparturePoint { get; set; }
        public string? Destination { get; set; }
        public string? Duration { get; set; }
        public string? DepartureTime { get; set; }
        public string? Transportation { get; set; }
        public string? Price { get; set; }
        public string Source { get; set; } = "Dynamic";
        public DateTime CrawledTime { get; set; } = DateTime.UtcNow;
    }
}
