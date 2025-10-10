using Gameshop_Api.Data;
using Gameshop_Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gameshop_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] DTOs.CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cartItems = await _context.Cart
                    .Where(c => c.uid == dto.uid)
                    .Join(
                        _context.Games,
                        cart => cart.game_id,
                        game => game.game_Id,
                        (cart, game) => new
                        {
                            cart.game_id,
                            game.price,
                            game.title
                        }
                    )
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "ตะกร้าสินค้าว่างเปล่า" });
                }

                var totalPrice = cartItems.Sum(item => item.price);
                var purchasedGameTitles = string.Join(", ", cartItems.Select(item => item.title));

                var existingWallet = await _context.Wallets.FindAsync(dto.uid);

                if (existingWallet == null || existingWallet.balance < totalPrice)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "ยอดเงินใน Wallet ไม่เพียงพอ" });
                }
                var newOrder = new Gameshop_Api.Models.Orders
                {
                    uid = dto.uid,
                    total_amount = totalPrice,
                    order_date = DateTime.Now,
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();


                foreach (var item in cartItems)
                {
                    var orderDetail = new Gameshop_Api.Models.OrderDetail
                    {
                        oid = newOrder.oid,
                        game_id = item.game_id,
                        price = item.price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    var cartToRemove = await _context.Cart.FirstOrDefaultAsync(c => c.uid == dto.uid && c.game_id == item.game_id);
                    if (cartToRemove != null)
                    {
                        _context.Cart.Remove(cartToRemove);
                    }
                }

                existingWallet.balance -= totalPrice;
                _context.Wallets.Update(existingWallet);

                var transactionRecord = new Gameshop_Api.Models.Transaction
                {
                    uid = dto.uid,
                    transaction_type = "PURCHASE",
                    reference_id = newOrder.oid.ToString(), // ใช้ oid เป็น Reference
                    amount_value = -totalPrice,
                    detail = $"Purchase order ID: {newOrder.oid}. Games: {purchasedGameTitles}",
                    status = "COMPLETED",
                    created_at = DateTime.Now
                };
                _context.Transactions.Add(transactionRecord);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "สร้างรายการสั่งซื้อและชำระเงินสำเร็จ",
                    order_id = newOrder.oid,
                    total_price = newOrder.total_amount,
                    purchased_games = purchasedGameTitles
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการสร้างคำสั่งซื้อ: {ex.Message}" });
            }
        }
    }
}