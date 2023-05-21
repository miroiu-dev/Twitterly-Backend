using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime? Scheduled { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        public string AttachmentType { get; set; } = string.Empty;

        [Column(TypeName = "json")]
        public string Attachment { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
