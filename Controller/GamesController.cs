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
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GamesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetAllGames()
        {
            var games = await _context.Games.ToListAsync();
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGameById(int id)
        {
            var games = await _context.Games.FindAsync(id);
            if (games == null)
                return NotFound(new { message = "User not found" });

            return Ok(games);
        }

        // POST /Game/AddGame
        [HttpPost("AddGame")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Addgame([FromForm] AddgameDto dto, [FromServices] Cloudinary cloudinary)
        {

            if (string.IsNullOrWhiteSpace(dto.title))
                return BadRequest("กรุณากรอกชื่อเกม");

            if (string.IsNullOrWhiteSpace(dto.detail))
                return BadRequest("กรุณากรอกรายละเอียดเกม");

            if (string.IsNullOrWhiteSpace(dto.category))
                return BadRequest("กรุณาเลือกประเภทเกม");

            if (dto.price <= 0)
                return BadRequest("กรุณากรอกราคาที่ถูกต้อง");


            if (await _context.Games.AnyAsync(g => g.title == dto.title))
                return BadRequest("ชื่อเกมนี้มีอยู่ในระบบแล้ว");
            var game = new Game
            {
                title = dto.title,
                detail = dto.detail,
                category = dto.category,
                price = dto.price,
                release_date = dto.release_date ?? DateTime.Now,
                image_url = "",

            };
            if (dto.image_url != null && dto.image_url.Length > 0)
            {
                // 1. เตรียมข้อมูลเพื่ออัปโหลด
                using var stream = dto.image_url.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(dto.image_url.FileName, stream),
                    // ตั้งชื่อไฟล์ให้ไม่ซ้ำกัน (optional)
                    PublicId = Guid.NewGuid().ToString()
                };

                // 2. อัปโหลดไฟล์
                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return BadRequest($"อัปโหลดรูปภาพไม่สำเร็จ: {uploadResult.Error.Message}");
                }
                game.image_url = uploadResult.SecureUrl.ToString();
            }

            try
            {
                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "เพิ่มเกมสำเร็จ",
                    game_id = game.game_Id,
                    title = game.title,
                    category = game.category,
                    price = game.price,
                    release_date = game.release_date,
                    detail = game.detail,
                    image_url = game.image_url,
                    rank = game.rank
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"เกิดข้อผิดพลาดในการบันทึกข้อมูล: {ex.Message}");
            }


        }

        // PUT /Game/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(
        int id,
        [FromForm] UpdateGameDto dto,
        [FromServices] Cloudinary cloudinary)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound(new { message = "Game not found" });

            //  อัปเดตข้อมูลทั่วไป
            game.title = dto.title ?? game.title;
            game.category = dto.category ?? game.category;
            game.price = dto.price > 0 ? dto.price : game.price;
            game.detail = dto.detail ?? game.detail;
            game.release_date = dto.release_date ?? game.release_date;

            //  อัปโหลดรูปถ้ามีไฟล์ส่งมา
            if (dto.image_url != null && dto.image_url.Length > 0)
            {
                using var stream = dto.image_url.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(dto.image_url.FileName, stream),
                    PublicId = Guid.NewGuid().ToString() // ตั้งชื่อไม่ซ้ำ
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return BadRequest($"อัปโหลดรูปภาพไม่สำเร็จ: {uploadResult.Error.Message}");

                //  บันทึกรูปใหม่
                game.image_url = uploadResult.SecureUrl.ToString();
            }

            //  บันทึกลงฐานข้อมูล
            _context.Games.Update(game);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "อัปเดตข้อมูลเกมสำเร็จ",
                data = game
            });
        }

        // DELETE /Game/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound(new { message = "เกมไม่พบ" });
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ลบเกมเรียบร้อยแล้ว" });
        }
    }



}
