using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gameshop_Api.Models
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        [Column("cart_id")]
        public int cart_id { get; set; }

        [Required]
        [Column("uid")]
        public int uid { get; set; }

        [Required]
        [Column("game_id")]
        public int game_id { get; set; }

        // Navigation Properties
        [ForeignKey("uid")]
        public virtual User? User { get; set; }

        [ForeignKey("game_id")]
        public virtual Game? Game { get; set; }
    }
}