namespace Twitter.Dtos
{
    public class TweetDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string Attachment { get; set; } = string.Empty;
        public int Likes { get; set; }
        public bool? IsAuthor { get; set; }
        public bool IsLiked { get; set; }
        public bool? HasVoted { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }
}