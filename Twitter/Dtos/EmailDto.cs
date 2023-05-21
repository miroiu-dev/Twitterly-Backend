using SendGrid.Helpers.Mail;

namespace Twitter.Dtos
{
    public class EmailDto
    {
        public required EmailAddress From { get; set; } = new();
        public required EmailAddress To { get; set; } = new();
        public required object Tags { get; set; } = new { };
    }
}
