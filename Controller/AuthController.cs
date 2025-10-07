using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
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
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.profile_image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.profile_image.CopyToAsync(stream);
                }

                user.profile_image = $"Uploads/{fileName}";
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "สมัครสมาชิกสำเร็จ", user.uid, user.fullname, user.email, user.profile_image });
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
                    user.profile_image
                },

            });
        }

    }
}
