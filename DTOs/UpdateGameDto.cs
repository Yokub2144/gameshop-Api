using Microsoft.AspNetCore.Http;

namespace Gameshop_Api.DTOs
{
    public class UpdateGameDto
    {

        public string title { get; set; }

        public string category { get; set; }

        public decimal price { get; set; } = 0;

        public string detail { get; set; }
        public IFormFile? image_url { get; set; }
        public DateTime? release_date { get; set; }
    }
}