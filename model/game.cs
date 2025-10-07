using System.ComponentModel.DataAnnotations;

namespace Gameshop_Api.Models
{
    public class Game
    {
        [Key]
        public int game_Id { get; set; }
        public string title { get; set; }
        public int rank { get; set; }
        public string category { get; set; }
        public decimal price { get; set; } = 0;
        public DateTime? release_date { get; set; }
        public string image_url { get; set; }

    }
}