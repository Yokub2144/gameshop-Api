using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Gameshop_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gameshop_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Cart/AddToCart
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                // ตรวจสอบว่ามีในตะกร้าอยู่แล้วหรือไม่
                var exists = await _context.Cart
                    .AnyAsync(c => c.uid == dto.uid && c.game_id == dto.game_id);

                if (exists)
                    return BadRequest(new { message = "เกมนี้มีในตะกร้าอยู่แล้ว" });

                var cart = new Cart
                {
                    uid = dto.uid,
                    game_id = dto.game_id,

                };

                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "เพิ่มเกมลงตะกร้าสำเร็จ",
                    cart_id = cart.cart_id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: Cart/GetCart/{uid}
        [HttpGet("GetCart/{uid}")]
        public async Task<IActionResult> GetCart(int uid)
        {
            try
            {
                var cartItems = await _context.Cart
                    .Where(c => c.uid == uid)
                    .Include(c => c.Game)
                    .Select(c => new
                    {
                        cart_id = c.cart_id,
                        game_id = c.game_id,
                        title = c.Game.title,
                        price = c.Game.price,
                        image_url = c.Game.image_url,
                        category = c.Game.category,
                        rank = c.Game.rank,

                    })

                    .ToListAsync();

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: Cart/RemoveFromCart/{cartId}
        [HttpDelete("RemoveFromCart/{cartId}")]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            try
            {
                var cart = await _context.Cart.FindAsync(cartId);

                if (cart == null)
                    return NotFound(new { message = "ไม่พบรายการในตะกร้า" });

                _context.Cart.Remove(cart);
                await _context.SaveChangesAsync();

                return Ok(new { message = "ลบออกจากตะกร้าแล้ว" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: Cart/ClearCart/{uid}
        [HttpDelete("ClearCart/{uid}")]
        public async Task<IActionResult> ClearCart(int uid)
        {
            try
            {
                var cartItems = await _context.Cart.Where(c => c.uid == uid).ToListAsync();

                if (cartItems.Count == 0)
                    return NotFound(new { message = "ไม่มีสินค้าในตะกร้า" });

                _context.Cart.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "ล้างตะกร้าเรียบร้อย",
                    items_removed = cartItems.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}