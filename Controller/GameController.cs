using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gameshop_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GameController(AppDbContext context, IWebHostEnvironment env)
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
        public async Task<IActionResult> AddGame([FromForm] AddgameDto dto)
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
                release_date = dto.release_date,
                image_url = "",
                rank = dto.rank
            };

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
    }
}
