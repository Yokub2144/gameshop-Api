using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Gameshop_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // PUT: /User/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // อัปเดตข้อมูล
            user.fullname = dto.Fullname ?? user.fullname;
            user.email = dto.Email ?? user.email;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                using var sha256 = SHA256.Create();
                var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
                user.password = Convert.ToBase64String(hashed);
            }

            // อัปโหลดรูปถ้ามี
            if (dto.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{dto.ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ProfileImage.CopyToAsync(stream);

                user.profile_image = fileName;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

    }
}
