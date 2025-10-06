using Microsoft.AspNetCore.Http;

namespace Gameshop_Api.DTOs
{
    public class UpdateUserDto
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}