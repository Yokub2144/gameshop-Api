using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

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
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto dto, [FromServices] Cloudinary cloudinary)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // อัปเดตข้อมูล
            user.fullname = dto.Fullname ?? user.fullname;
            user.email = dto.Email ?? user.email;
            user.password = dto.Password ?? user.password;

            // อัปโหลดรูปถ้ามี

            if (dto.profile_image != null && dto.profile_image.Length > 0)
            {
                // 1. เตรียมข้อมูลเพื่ออัปโหลด
                using var stream = dto.profile_image.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.profile_image.FileName, stream),
                    // ตั้งชื่อไฟล์ให้ไม่ซ้ำกัน (optional)
                    PublicId = Guid.NewGuid().ToString()
                };

                // 2. อัปโหลดไฟล์
                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return BadRequest($"อัปโหลดรูปภาพไม่สำเร็จ: {uploadResult.Error.Message}");
                }
                user.profile_image = uploadResult.SecureUrl.ToString();
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

    }
}
