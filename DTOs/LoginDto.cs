using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}