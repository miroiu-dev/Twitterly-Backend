using Twitter.Dtos;

namespace Twitter.Services
{
    public interface ITweetService
    {
        Task PostTweetAsync(TweetDataFromForm data);
        Task<List<TweetDto>> GetProfileTweetsAsync(int offset, int limit);
        Task LikeAsync(int id);
        Task UnLikeAsync(int id);
        Task Vote(int id, string choice);
    }
}