using Twitter.Services;

namespace Twitter.Handlers
{
    public class TweetHandler
    {
        public static async Task<IResult> PostTweet(IHttpContextAccessor ctx, ITweetService tweetService)
        {
            var request = ctx.HttpContext.Request;
            string? text = request.Form["text"];
            string? attachmentType = request.Form["attachmentType"];
            string? date = request.Form["scheduled"];

            DateTime? scheduled = null;
            if (date is not null && date != "undefined") scheduled = DateTimeOffset.FromUnixTimeMilliseconds(int.Parse(date)).DateTime;

            string? attachmentValue = request.Form["attachment"];
            var files = request.Form.Files;
            try
            {
                await tweetService.PostTweetAsync(new() { Text = text, Attachment = attachmentValue, AttachmentType = attachmentType, Files = files, Scheduled = scheduled });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.BadRequest("Something went wrong, please try again!");
            }

            return Results.Ok();
        }

        public static async Task<IResult> GetProfileTweets(int offset, int limit, ITweetService tweetService)
        {
            return Results.Ok(await tweetService.GetProfileTweetsAsync(offset, limit));
        }

        public static async Task<IResult> Like(int id, ITweetService tweetService)
        {
            await tweetService.LikeAsync(id);
            return Results.Ok();
        }
        public static async Task<IResult> UnLike(int id, ITweetService tweetService)
        {
            await tweetService.UnLikeAsync(id);
            return Results.Ok();
        }
        public static async Task<IResult> Vote(int id, string choice, ITweetService tweetService)
        {
            await tweetService.Vote(id, choice);
            return Results.Ok();
        }
    }
}
