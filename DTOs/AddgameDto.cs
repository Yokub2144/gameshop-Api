using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.DTOs
{
    public class AddgameDto
    {
        [Required]
        public string title { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

        public IFormFile? profile_image { get; set; }
    }
}