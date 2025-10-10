using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.Models
{
    public class Transaction
    {
        [Key]
        public int tid { get; set; }
        public int uid { get; set; }
        public string? transaction_type { get; set; }
        public string reference_id { get; set; }
        public decimal amount_value { get; set; }
        public string? detail { get; set; }
        public string? status { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
    }
}