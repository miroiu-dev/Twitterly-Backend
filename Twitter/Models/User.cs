using System.ComponentModel.DataAnnotations.Schema;
namespace Twitter.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = "https://abs.twimg.com/sticky/default_profile_images/default_profile_400x400.png";
        public DateTime DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;
        public virtual List<Tweet> Tweets { get; set; } = null!;
    }
}