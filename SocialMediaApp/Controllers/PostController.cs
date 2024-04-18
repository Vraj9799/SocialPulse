using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models;
using System.Net;
using System.Security.Claims;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Services;
using SocialMediaApp.Mappings;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    public class PostController(
           ILogger<PostController> logger,
           IOptions<AppSettings> appSettings,
           IPostService _postService,
           IUserService _userService,
           IActivityService activityService
           ) : ApiController(logger, appSettings)
    {
        [HttpGet("feed")]
        public async Task<IActionResult> GetMyFeed(int pageNo = 1, int limit = 10)
        {
            var userId = GetCurrentUserId();
            var followers = (await _userService.GetMyFollowers(userId)).Select(_ => _.Id).ToHashSet<string>();
            var postsTask = await _postService.GetMyFeedAsync(userId, followers, pageNo, limit);
            var userIds = new HashSet<string>(postsTask.Select(_ => _.UserId).ToHashSet<string>()){
                userId
            };
            var usersTask = await _userService.GetApplicationUser(userIds);

            var postViewModels = new List<PostViewModel>();
            postsTask
            .ToList()
            .ForEach(_ =>
                postViewModels.Add(_.AsPostViewModel(usersTask.FirstOrDefault(u => u.Id == _.UserId)))
            );
            return Ok(new ApiResponse<IEnumerable<PostViewModel>>(postViewModels));
        }

        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMy(int pageNo = 1, int limit = 10)
        {
            var userId = GetCurrentUserId();
            var posts = await _postService.GetMyPosts(userId, pageNo, limit);
            var user = await _userService.GetApplicationUser(new HashSet<string>() { userId });

            var postViewModels = posts.ToList()
                .Select(_ => _.AsPostViewModel(user.FirstOrDefault()));

            return Ok(new ApiResponse<IEnumerable<PostViewModel>>(postViewModels));
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetById(string postId)
        {
            var userId = GetCurrentUserId();
            var post = await _postService.GetById(postId, userId);
            if (post is null)
            {
                return NotFound(new ApiResponse<string>("Invalid post."));
            }
            var users = await _userService.GetApplicationUser(new HashSet<string>() { userId });
            var postViewModel = post.AsPostViewModel(users.FirstOrDefault());
            return Ok(new ApiResponse<PostViewModel>(postViewModel));
        }

        [HttpPost()]
        public async Task<IActionResult> Add([FromBody] PostRequest postRequest)
        {
            var userId = GetCurrentUserId();
            var postId = await _postService.AddAsync(postRequest, userId);
            return Ok(new ApiResponse<string>(HttpStatusCode.Created, postId));
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> Update(string postId, PostRequest postRequest)
        {
            var userId = GetCurrentUserId();
            await _postService.UpdateAsync(postRequest, postId, userId);
            return Ok(new ApiResponse<string>("Post updated succesfully."));
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> Delete(string postId)
        {
            var userId = GetCurrentUserId();
            await _postService.DeleteAsync(postId, userId);
            return Ok(new ApiResponse<string>("Post deleted succesfully."));
        }

        [HttpPut("{postId}/like")]
        public async Task<IActionResult> LikePost(string postId)
        {
            var userId = GetCurrentUserId();
            await _postService.LikeAsync(postId, userId);
            var post = await _postService.GetById(postId, userId);
            await activityService.AddAsync(userId, $"{GetUserClaim(ClaimTypes.Name)} liked your post.");
            return Ok(new ApiResponse<string>("You liked the post."));
        }

        [HttpPut("{postId}/un-like")]
        public async Task<IActionResult> UnLikePost(string postId)
        {
            var userId = GetCurrentUserId();
            await _postService.UnLikeAsync(postId, userId);
            return Ok(new ApiResponse<string>("You un-liked the post."));
        }

        [HttpPut("{postId}/save")]
        public async Task<IActionResult> Save(string postId)
        {
            var userId = GetCurrentUserId();
            await _postService.SaveAsync(postId, userId);
            return Ok(new ApiResponse<string>("You saved the post."));
        }

        [HttpPut("{postId}/un-save")]
        public async Task<IActionResult> Unsave(string postId)
        {
            var userId = GetCurrentUserId();
            await _postService.UnSaveAsync(postId, userId);
            return Ok(new ApiResponse<string>("You removed the post."));
        }
    }
}
