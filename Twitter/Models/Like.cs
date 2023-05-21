using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter.Models
{
    public class Like
    {
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [ForeignKey("TweetId")]
        public int TweetId { get; set; }
        public Tweet Tweet { get; set; } = null!;
    }
}
