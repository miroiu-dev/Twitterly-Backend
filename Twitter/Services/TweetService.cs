using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using MySqlX.Serialization;
using Newtonsoft.Json;
using Twitter.Data;
using Twitter.Dtos;
using Twitter.Models;

namespace Twitter.Services
{
    class Statistics
    {
        [JsonProperty("choice1")]
        public int Choice1 { get; set; }
        [JsonProperty("choice2")]

        public int Choice2 { get; set; }
        [JsonProperty("choice3")]

        public int Choice3 { get; set; }
        [JsonProperty("choice4")]

        public int Choice4 { get; set; }
    }
    class PollAttachment
    {
        [JsonProperty("choice1")]
        public string Choice1 { get; set; } = string.Empty;
        [JsonProperty("choice2")]

        public string Choice2 { get; set; } = string.Empty;
        [JsonProperty("choice3")]

        public string? Choice3 { get; set; }
        [JsonProperty("choice4")]

        public string? Choice4 { get; set; }
        [JsonProperty("statistics")]

        public Statistics Statistics { get; set; } = new Statistics();
        [JsonProperty("voted")]

        public List<int> Voted { get; set; } = new();
    }
    public class TweetDataFromForm
    {
        public string Text { get; set; } = string.Empty;
        public DateTime? Scheduled { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string Attachment { get; set; } = "{}";
        public IFormFileCollection? Files { get; set; }
    }
    public class TweetService : ITweetService
    {
        private readonly UserContext _ctx;
        private readonly IHttpContextAccessor _httpCtx;
        private readonly Cloudinary _cloudinary;
        public TweetService(UserContext ctx, IHttpContextAccessor httpCtx)
        {
            _ctx = ctx;
            _httpCtx = httpCtx;

            Account account = new()
            {
                ApiKey = "352534948963868",
                ApiSecret = "curTqnfMs7ttT3kua9WLD5HW0_M",
                Cloud = "twitter-clone-react"
            };

            _cloudinary = new(account);
            _cloudinary.Api.Secure = true;
        }

        private async Task<string> UploadImageAsync(IFormFile file)
        {
            var fileStream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, fileStream),
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.Url.ToString();
        }

        private async Task<string> UploadVideoAsync(IFormFile file)
        {
            var fileStream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(file.FileName, fileStream),
            };
            var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
            return uploadResult.Url.ToString();
        }

        private int GetUserId()
        {
            return int.Parse(_httpCtx.HttpContext!.User.Claims.Where(x => x.Type == "Id").FirstOrDefault().Value);
        }

        public async Task PostTweetAsync(TweetDataFromForm data)
        {
            var userId = GetUserId();

            string attachmentValue = data.Attachment;
            var json = JsonParser.Parse(attachmentValue);

            if (data.AttachmentType == "IMAGE")
            {
                var images = new List<string>();

                foreach (var file in data.Files!)
                {
                    var url = await UploadImageAsync(file);
                    images.Add(url);
                }

                json.Add("images", images);

                attachmentValue = JsonConvert.SerializeObject(json);
            }
            else if (data.AttachmentType == "VIDEO")
            {
                var video = data.Files![0];

                var url = await UploadVideoAsync(video);

                json.Add("video", url);

                attachmentValue = JsonConvert.SerializeObject(json);
            }

            await _ctx.AddAsync(new Tweet
            {
                Scheduled = data.Scheduled,
                AttachmentType = data.AttachmentType,
                Text = data.Text,
                UserId = userId,
                Attachment = attachmentValue
            });

            await _ctx.SaveChangesAsync();
        }



        public async Task<List<TweetDto>> GetProfileTweetsAsync(int offset, int limit)
        {

            var userId = GetUserId();

            var tweets = await _ctx.Tweets.OrderByDescending(x => x.CreatedAt).Skip(offset).Take(limit).Select(x => new TweetDto
            {
                Id = x.Id,
                AttachmentType = x.AttachmentType,
                Attachment = x.Attachment,
                CreatedAt = x.CreatedAt,
                Text = x.Text,
                Likes = x.Likes,
                IsAuthor = x.UserId == userId,
                User = new UserDto
                {
                    ProfilePictureUrl = x.User.ProfilePictureUrl,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Handle = x.User.Handle,
                    Id = x.UserId,
                    Email = x.User.Email,
                    Country = x.User.Country
                }
            }).ToListAsync();

            foreach (var tweet in tweets)
            {
                if (tweet.AttachmentType == "POLL")
                {
                    var poll = JsonConvert.DeserializeObject<PollAttachment>(tweet.Attachment);
                    tweet.HasVoted = poll.Voted.Contains(userId);
                }
                tweet.IsLiked = await GetIsLikedAsync(tweet.Id, userId);
            }


            return tweets;
        }

        private async Task<bool> GetIsLikedAsync(int postId, int userId)
        {
            var liked = await _ctx.Likes.Where(x => x.TweetId == postId && x.UserId == userId).FirstOrDefaultAsync();
            return liked is not null;
        }

        public async Task LikeAsync(int id)
        {
            var userId = GetUserId();

            var like = await _ctx.Likes.Where(x => x.TweetId == id && x.UserId == userId).FirstOrDefaultAsync();
            if (like is not null) return;

            await _ctx.Likes.AddAsync(new Like { TweetId = id, UserId = userId });

            var tweet = await _ctx.Tweets.Where(x => x.Id == id).FirstAsync();
            tweet.Likes++;
            await _ctx.SaveChangesAsync();
        }
        public async Task UnLikeAsync(int id)
        {
            var userId = GetUserId();

            var like = await _ctx.Likes.Where(x => x.TweetId == id && x.UserId == userId).FirstOrDefaultAsync();
            if (like is null) return;

            _ctx.Likes.Remove(like);

            var tweet = await _ctx.Tweets.Where(x => x.Id == id).FirstAsync();
            tweet.Likes--;
            await _ctx.SaveChangesAsync();
        }

        public async Task Vote(int id, string choice)
        {
            var userId = GetUserId();

            var tweet = await _ctx.Tweets.Where(x => x.Id == id).FirstAsync();

            var poll = JsonConvert.DeserializeObject<PollAttachment>(tweet.Attachment);
            if (poll.Voted.Contains(userId)) return;
                
            if (choice == "choice1") poll.Statistics.Choice1 += 1;
            if (choice == "choice2") poll.Statistics.Choice2 += 1;
            if (choice == "choice2") poll.Statistics.Choice3 += 1;
            if (choice == "choice2") poll.Statistics.Choice4 += 1;


            if (!poll.Voted.Contains(userId)) poll.Voted.Add(userId);

            var attachment = JsonConvert.SerializeObject(poll);

            tweet.Attachment = attachment;
            await _ctx.SaveChangesAsync();
        }
    }
}
