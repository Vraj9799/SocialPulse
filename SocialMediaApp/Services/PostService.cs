using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Repositories;
using System.Net;

namespace SocialMediaApp.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetMyFeedAsync(string currUserId, ISet<string> followings, int pageNo = 1, int limit = 10);

        Task<IEnumerable<Post>> GetMyPosts(string userId, int pageNo = 1, int limit = 10);

        Task<Post?> GetById(string postId, string userId);

        Task<string> AddAsync(PostRequest postRequest, string userId);

        Task<bool> UpdateAsync(PostRequest postRequest, string postId, string userId);

        Task DeleteAsync(string postId, string userId);

        Task LikeAsync(string postId, string userId);

        Task UnLikeAsync(string postId, string userId);

        Task SaveAsync(string postId, string userId);

        Task UnSaveAsync(string postId, string userId);
    }

    public class PostService(ILogger<PostService> _logger, IPostRepository _postRepository) : IPostService
    {
        public async Task<IEnumerable<Post>> GetMyFeedAsync(string currUserId, ISet<string> followings, int pageNo = 1, int limit = 10)
        {
            _logger.LogInformation("Fetching Feed for User {}", currUserId);
            var posts = await _postRepository.Get(_ =>
                followings.Contains(_.UserId) && (!_.IsDeleted || _.UserId.Equals(currUserId)),
                pageNo, limit);
            return posts;
        }

        public async Task<IEnumerable<Post>> GetMyPosts(string userId, int pageNo = 1, int limit = 10)
        {
            var posts = await _postRepository.Get(_ => _.UserId.Equals(userId), pageNo, limit);
            return posts;
        }

        public async Task<Post?> GetById(string postId, string userId)
        {
            Post post = await _postRepository.GetByIdAsync(postId, true);
            return !post.IsDeleted || post.UserId.Equals(userId) ? post : null;
        }

        public async Task<string> AddAsync(PostRequest postRequest, string userId)
        {
            Post post = new()
            {
                Caption = postRequest.Caption,
                Medias = postRequest.Medias,
                Tags = postRequest.Tags,
                UserId = userId,
                ScheduledOn = postRequest.ScheduledOn,
                Status = postRequest.ScheduledOn.HasValue ? PostStatus.SCHEDULED : PostStatus.ACTIVE
            };
            await _postRepository.AddAsync(post);
            return post.Id;
        }

        public async Task<bool> UpdateAsync(PostRequest postRequest, string postId, string userId)
        {
            Post? post = await GetById(postId, userId);
            if (post is null)
            {
                return false;
            }
            post.Caption = postRequest.Caption;
            post.Medias = postRequest.Medias;
            post.Tags = postRequest.Tags;
            post.LastModifiedOn = DateTime.UtcNow;
            await _postRepository.UpdateAsync(post);
            return true;
        }

        public async Task DeleteAsync(string postId, string userId)
        {
            Post? post = await GetById(postId, userId);
            if (post is null)
            {
                _logger.LogError("Invalid post request for id {}", postId);
                throw new StatusException(HttpStatusCode.BadRequest, $"Invalid post.");
            }
            await _postRepository.DeleteAsync(postId);
        }

        public async Task LikeAsync(string postId, string userId)
        {
            _logger.LogInformation("User {} liked Post {}.", userId, postId);
            await _postRepository.LikePostAsync(postId, userId);
        }

        public async Task UnLikeAsync(string postId, string userId)
        {
            await _postRepository.UnLikePostAsync(postId, userId);
        }

        public async Task SaveAsync(string postId, string userId)
        {
            await _postRepository.UnSavePostAsync(postId, userId);
        }

        public Task UnSaveAsync(string postId, string userId)
        {
            throw new NotImplementedException();
        }
    }

}
