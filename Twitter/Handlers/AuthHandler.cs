using Microsoft.AspNetCore.Authentication.Cookies;
using Twitter.Dtos;
using Twitter.Models;
using Twitter.Services;
using Twitter.ApiErrors;
using Microsoft.AspNetCore.Authentication;

namespace Twitter.Handlers
{
    public static class AuthHandler
    {
        public static async Task<IResult> SignIn(SignInDto login, IAuthService authService, IHttpContextAccessor ctx)
        {
            var user = await authService.GetUserAsync(login.Email);

            if (user is null) return Results.NotFound(new AuthenticationError(SignInError.INVALID_USER_PASSWORD));

            if (!authService.VerifyPassword(login.Password, user.Password))
            {
                return Results.Json(new AuthenticationError(SignInError.INVALID_USER_PASSWORD), statusCode: StatusCodes.Status401Unauthorized);
            }

            var userIdentity = authService.CreateUserIdentity(user.Email, user.Id);

            await ctx.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal: userIdentity);

            return Results.Ok(new UserDto
            {
                Country = user.Country,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FirstName = user.FirstName,
                Handle = user.Handle,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
            });
        }

        public static async Task<IResult> SignUp(SignUpDto register, IAuthService authService)
        {
            bool isExistingUser = await authService.UserExistsAsync(register.Email);

            if (isExistingUser) return Results.Conflict(new AuthenticationError(SignUpError.USER_EXISTS));

            // TODO: Add password salt

            string password = authService.HashPassword(register.Password);

            var country = await authService.GetCountryAsync();

            await authService.CreateUserAsync(new User
            {
                Email = register.Email,
                FirstName = register.FirstName,
                LastName = register.LastName,
                CountryId = country.Id,
                DateOfBirth = register.DateOfBirth,
                Password = password,
                Handle = $"@{register.FirstName}{register.LastName}"
            });

            return Results.Ok();
        }

        public static IResult SignOut()
        {
            return Results.SignOut(authenticationSchemes: new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        public static async Task<IResult> SendResetEmail(string email, IAuthService authService, IEmailService emailService, IConfiguration config)
        {
            UserDto? user = await authService.GetUserAsync(email, user => new UserDto { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Handle = user.Handle });

            if (user is null) return Results.NotFound(new AuthenticationError(ResetError.USER_DOESNT_EXIST));

            if (await authService.IsTokenAlreadySent(user.Id)) return Results.Conflict(new AuthenticationError(ResetError.ALREADY_SENT));

            string token = authService.GenerateResetToken();

            await authService.CreateResetTokenAsync(new ResetToken
            {
                Expires = DateTime.Now.AddDays(1),
                Token = token,
                UserId = user.Id,
            });


            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "Token", token },
                    { "Handle", user.Handle }
                };

                emailService.SendEmailAsync(new EmailDto
                {
                    From = new EmailAddress
                    {
                        Email = config.GetValue<string>("Secrets:Email:From")!,
                        Name = "Twitterly"
                    },
                    To = new EmailAddress
                    {
                        Email = email,
                        Name = user.FullName
                    },
                    Params = parameters
                }, config.GetValue<long>("Secrets:Email:TemplateId"));
            }
            catch (Exception)
            {
                return Results.BadRequest(new AuthenticationError(ResetError.SOMETHIG_WENT_WRONG));
            }

            return Results.Ok();
        }

        public static async Task<IResult> ResetPassword(ResetPasswordDto submission, string token, IAuthService authService)
        {
            var validToken = await authService.GetAndConsumeTokenAsync(token);

            if (validToken is null) return Results.Json(new AuthenticationError(ResetError.INVALID_TOKEN), statusCode: StatusCodes.Status401Unauthorized);

            // TODO: add salt to password
            string hashedPassword = authService.HashPassword(submission.Password);

            await authService.UpdatePasswordAsync(validToken.UserId, hashedPassword);

            return Results.Ok();
        }
    }
}