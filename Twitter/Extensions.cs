using Twitter.Handlers;
using Twitter.Filters;
using Twitter.Dtos;
using Twitter.Validators;

namespace Twitter
{
    public static class Extensions
    {
        public static void UseTwitterApi(this WebApplication app)
        {
            var auth = app.MapGroup("api/v1/auth");
            var tweet = app.MapGroup("api/v1/tweet");

            auth.MapPost("/sign-in", AuthHandler.SignIn).AllowAnonymous();
            auth.MapPost("/sign-up", AuthHandler.SignUp).AllowAnonymous().AddEndpointFilter<ValidateEndpointFilter<SignUpValidator, SignUpDto>>();
            auth.MapDelete("/sign-out", AuthHandler.SignOut).AllowAnonymous();
            auth.MapPost("/reset", AuthHandler.SendResetEmail).AllowAnonymous();
            auth.MapPost("/reset/{token}", AuthHandler.ResetPassword).RequireAuthorization().AddEndpointFilter<ValidateEndpointFilter<ResetPasswordValidator, ResetPasswordDto>>();
            tweet.MapPost("/", TweetHandler.PostTweet).RequireAuthorization();
            tweet.MapGet("/profile", TweetHandler.GetProfileTweets).RequireAuthorization();
            tweet.MapPost("/like/{id}", TweetHandler.Like).RequireAuthorization();
            tweet.MapPost("/unlike/{id}", TweetHandler.UnLike).RequireAuthorization();
            tweet.MapPost("/vote", TweetHandler.Vote).RequireAuthorization();
        }   
    }
}
