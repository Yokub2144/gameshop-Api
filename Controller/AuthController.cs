using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Gameshop_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AuthController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST /Auth/Register
        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto, [FromServices] Cloudinary cloudinary)
        {
            if (await _context.Users.AnyAsync(u => u.email == dto.email))
                return BadRequest("Email นี้มีผู้ใช้งานแล้ว");

            var user = new User
            {
                fullname = dto.fullname,
                email = dto.email,
                password = dto.password
            };

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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "สมัครสมาชิกสำเร็จ", user.uid, user.fullname, user.email, user.profile_image, });

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == dto.email && u.password == dto.password);
            if (user == null)
                return Unauthorized("Invalid email or password");

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    user.uid,
                    user.fullname,
                    user.email,
                    user.role,
                    user.profile_image,

                },

            });
        }

    }
}
