using Twitter.Models;

namespace Twitter.Dtos
{
    public partial class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Country Country { get; set; } = null!;
    }
    public partial class UserDto
    {
        public string FullName => $"{FirstName} {LastName}";
    }
}
