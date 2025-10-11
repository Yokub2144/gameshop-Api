using System.Net.NetworkInformation;
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

                // 🌟 แก้ไข: ตรวจสอบเกมที่ผู้ใช้เป็นเจ้าของอยู่แล้ว โดยใช้ตาราง Orders 🌟
                var cartGameIds = cartItems.Select(item => item.game_id).ToList();

                var ownedGames = await (from order in _context.Orders
                                        join orderDetail in _context.OrderDetails on order.oid equals orderDetail.oid
                                        join game in _context.Games on orderDetail.game_id equals game.game_Id
                                        where order.uid == dto.uid && cartGameIds.Contains(Convert.ToInt32(orderDetail.game_id))
                                        select game.title)
                                        .Distinct() // ใช้ Distinct เพื่อให้ได้ชื่อเกมที่ไม่ซ้ำ
                                        .ToListAsync();

                if (ownedGames.Any())
                {
                    var ownedGameTitles = string.Join(", ", ownedGames);
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = $"ไม่สามารถดำเนินการสั่งซื้อได้: มีเกมที่คุณเป็นเจ้าของอยู่แล้วในตะกร้า: {ownedGameTitles}",
                        owned_games = ownedGameTitles
                    });
                }
                // 🌟 สิ้นสุดการตรวจสอบ 🌟

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

                    // ❌ ลบการอ้างอิงถึง _context.UserGames.Add(userGame) ออกแล้ว

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
                    reference_id = newOrder.oid.ToString(),
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
        [HttpGet("getTransactionByUserId/{uid}")]
        public async Task<IActionResult> getTransactionByUserId(int uid)
        {
            try
            {
                // ดึงรายการ Transaction ทั้งหมดของผู้ใช้ตาม uid
                var transactions = await _context.Transactions
                    .Where(t => t.uid == uid)
                    .OrderByDescending(t => t.created_at) // เรียงลำดับจากล่าสุดไปเก่าสุด
                    .ToListAsync();

                if (!transactions.Any())
                {
                    return NotFound(new { message = "ไม่พบประวัติการทำธุรกรรมสำหรับผู้ใช้รายนี้" });
                }

                // เตรียมโครงสร้างเพื่อเก็บผลลัพธ์ที่จัดรูปแบบแล้ว
                var transactionHistory = new List<object>();

                // วนลูปเพื่อจัดรูปแบบและรวมข้อมูล
                foreach (var t in transactions)
                {
                    if (t.transaction_type == "TOPUP")
                    {
                        // 1. รายการเติมเงิน (TOPUP)
                        transactionHistory.Add(new
                        {
                            transaction_id = t.tid,
                            type = "เติมเงิน",
                            amount = t.amount_value,
                            date = t.created_at,
                            detail = t.detail // รายละเอียดที่บันทึกไว้ในตอนเติมเงิน
                        });
                    }
                    else if (t.transaction_type == "PURCHASE")
                    {
                        // 2. รายการซื้อเกม (PURCHASE)

                        // ดึง Order ID จาก referance_id (เราได้กำหนดให้ referance_id เก็บ Order ID ในโค้ดก่อนหน้า)
                        if (int.TryParse(t.reference_id, out int orderId))
                        {
                            // ใช้ Order ID เพื่อดึง Order Details และ Game Information
                            var purchaseDetails = await _context.OrderDetails
                                .Where(od => od.oid == orderId) // ใช้ oid ตาม Schema ของคุณ
                                .Join(
                                    _context.Games,
                                    od => od.game_id,
                                    g => g.game_Id,
                                    (od, g) => new
                                    {
                                        GameName = g.title,
                                        GamePrice = od.price // ใช้ price ตาม Schema ของคุณ
                                    }
                                )
                                .ToListAsync();

                            // คำนวณราคารวม (ควรจะเป็น -t.amount_value)
                            var totalSpent = Math.Abs(t.amount_value);

                            transactionHistory.Add(new
                            {
                                transaction_id = t.tid,
                                type = "ซื้อเกม",
                                total_price = totalSpent,
                                date = t.created_at,
                                // รายละเอียดการซื้อ
                                purchase_items = purchaseDetails,
                                detail = t.detail
                            });
                        }
                    }
                }

                // คืนค่ารายการประวัติการทำธุรกรรมทั้งหมด
                return Ok(transactionHistory);
            }
            catch (Exception ex)
            {
                // ควรมีการ Log ข้อผิดพลาดจริง ๆ ที่นี่
                return StatusCode(500, new { message = $"เกิดข้อผิดพลาดในการดึงประวัติ: {ex.Message}" });
            }
        }
    }
}