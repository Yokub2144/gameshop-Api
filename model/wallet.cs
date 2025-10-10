using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.Models
{
    public class Wallet
    {
        [Key]
        public int uid { get; set; }
        public decimal balance { get; set; } = 0;
    }
}