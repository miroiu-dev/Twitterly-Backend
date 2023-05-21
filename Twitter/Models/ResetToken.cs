using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter.Models
{
    public class ResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
