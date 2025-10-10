namespace Gameshop_Api.Models
{
    public class OrderDetail
    {
        public int did { get; set; }
        public int oid { get; set; }
        public decimal game_id { get; set; }
        public decimal price { get; set; }
    }
}