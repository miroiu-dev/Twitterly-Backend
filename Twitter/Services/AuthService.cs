using IPGeolocation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Linq.Expressions;
using System.Security.Claims;
using Twitter.Data;
using Twitter.Dtos;
using Twitter.Models;
using BC = BCrypt.Net.BCrypt;

namespace Twitter.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpCtx;
        private readonly UserContext _ctx;
        private readonly ISendGridClient _client;
        private readonly IConfiguration _config;
        public AuthService(IHttpContextAccessor httpCtx, ISendGridClient client, IConfiguration config, UserContext ctx)
        {
            _httpCtx = httpCtx;
            _ctx = ctx;
            _client = client;
            _config = config;
        }
        public async Task<User?> GetUserAsync(string email) =>
            await _ctx.Users.Where(user => user.Email == email).Include(user => user.Country).FirstOrDefaultAsync();
        public async Task<UserDto?> GetUserAsync(string email, Expression<Func<User, UserDto>> selector) => await _ctx.Users.Where(user => user.Email == email).Select(selector).FirstOrDefaultAsync();

        public async Task<bool> UserExistsAsync(string email) =>
            await _ctx.Users.AnyAsync(user => user.Email == email);

        public async Task<Country> GetCountryAsync()
        {
            string countryName = GetCountryNameFromIp();

            return await _ctx.Countries.Where(country => country.Name == countryName).FirstAsync();
        }
        private string GetCountryNameFromIp()
        {

            var ipAddress = _httpCtx.HttpContext!.Connection.LocalIpAddress;
            var defaultIp = _config.GetValue<string>("DefaultIp")!;

            string ip = defaultIp;

            if (_config.GetValue<string>("Environment") != "Development")
                ip = ipAddress?.ToString() ?? defaultIp;

            IPGeolocationAPI api = new(_config.GetValue<string>("Secrets:CountryApiKey"));

            GeolocationParams geoParams = new();
            geoParams.SetIPAddress(ip);
            geoParams.SetFields("geo");

            Geolocation? geolocation = (Geolocation?)api.GetGeolocation(geoParams).GetValueOrDefault("response");

            return geolocation is null ? "Romania" : geolocation.GetCountryName();
        }
        public string HashPassword(string password) => BC.HashPassword(password);
        public bool VerifyPassword(string password, string hash) => BC.Verify(password, hash);
        public ClaimsPrincipal CreateUserIdentity(string email, int id)
        {
            var claims = new List<Claim>
            {
                new Claim("Id",id.ToString()),
                new Claim("Email", email),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
        public async Task CreateUserAsync(User user)
        {
            _ctx.Add(user);
            await _ctx.SaveChangesAsync();
        }
        public string GenerateResetToken() => Guid.NewGuid().ToString()[..8];

        public async Task CreateResetTokenAsync(ResetToken token)
        {
            _ctx.Add(token);
            await _ctx.SaveChangesAsync();
        }

        public async Task SendResetPasswordEmailAsync(EmailDto email)
        {
            var message = new SendGridMessage()
            {
                From = email.From,
                TemplateId = _config.GetValue<string>("Secrets:Email:TemplateId")
            };

            message.SetTemplateData(email.Tags);
            message.AddTo(email.To);

            await _client.SendEmailAsync(message);
        }

        public async Task UpdatePasswordAsync(int id, string password)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(user => user.Id == id);

            user!.Password = password;

            await _ctx.SaveChangesAsync();
        }

        public async Task<bool> IsTokenAlreadySent(int userId)
        {
            return await _ctx.ResetTokens.Where(token => token.UserId == userId).AnyAsync();
        }

        public async Task<ResetToken?> GetAndConsumeTokenAsync(string token)
        {
            var resetToken = await _ctx.ResetTokens.Where(resetToken => resetToken.Token == token && resetToken.Expires > DateTime.Now).FirstOrDefaultAsync();

            if (resetToken is not null)
            {
                _ctx.Remove(resetToken);
            }

            return resetToken;
        }
    }
}
