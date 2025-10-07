namespace Gameshop_Api.Models
{
    public class Game
    {
        public int game_id { get; set; }
        public string title { get; set; }
        public int rank { get; set; }
        public string category { get; set; }
        public decimal price { get; set; }
        public DateTime release_date { get; set; }
        public string image_url { get; set; }
    }
}