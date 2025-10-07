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
        [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _context.Games
                .Select(g => new
                {
                    g.game_id,
                    g.title,
                    g.rank,
                    g.category,
                    g.price,
                    release_date = g.release_date.ToString("dd/MM/yyyy"),
                    g.image_url
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Get game list successful",
                total = games.Count,
                data = games
            });
        }


    }
}