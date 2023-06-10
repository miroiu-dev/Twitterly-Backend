
namespace Twitter.Dtos
{
    public class EmailAddress
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
    public class EmailDto
    {
        public required EmailAddress From { get; set; } = new();
        public required EmailAddress To { get; set; } = new();
        public required Dictionary<string, object> Params { get; set; } = new();
    }
}
