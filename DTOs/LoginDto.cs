using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.DTOs
{
    public class LoginDto
    {
        [Required]           // บังคับต้องกรอก
        [EmailAddress]       // ต้องเป็นรูปแบบอีเมล
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}