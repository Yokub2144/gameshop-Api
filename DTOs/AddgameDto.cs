using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.DTOs
{
    public class AddgameDto
    {
        [Required]
        public string title { get; set; }
        [Required]
        public string category { get; set; }
        [Required]
        public decimal price { get; set; } = 0;
        [Required]
        public string detail { get; set; }
        public IFormFile? image_url { get; set; }
        public DateTime? release_date { get; set; }

    }
}
