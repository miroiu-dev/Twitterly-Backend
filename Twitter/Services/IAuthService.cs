using System.Linq.Expressions;
using System.Security.Claims;
using Twitter.Dtos;
using Twitter.Models;

namespace Twitter.Services
{
    public interface IAuthService
    {
        Task<User?> GetUserAsync(string email);
        Task<UserDto?> GetUserAsync(string email, Expression<Func<User, UserDto>> selector);
        Task CreateUserAsync(User user);
        Task<Country> GetCountryAsync();
        Task<bool> UserExistsAsync(string email);
        string HashPassword(string password);
        string GenerateResetToken();
        Task CreateResetTokenAsync(ResetToken token);
        Task<ResetToken?> GetAndConsumeTokenAsync(string token);
        Task UpdatePasswordAsync(int id, string password);
        bool VerifyPassword(string password, string hash);
        Task<bool> IsTokenAlreadySent(int userId);
        ClaimsPrincipal CreateUserIdentity(string email, int Id);
        Task SendResetPasswordEmailAsync(EmailDto email);
    }
}
