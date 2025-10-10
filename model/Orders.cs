namespace Gameshop_Api.Models
{
    public class Orders
    {
        public int oid { get; set; }
        public int uid { get; set; }
        public DateTime order_date { get; set; } = DateTime.Now;
        public decimal total_amount { get; set; }
    }
}