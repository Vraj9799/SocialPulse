using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models;
using System.Net;
using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Services;
using SocialMediaApp.Mappings;

namespace SocialMediaApp.Controllers
{
    [Route("api/{postId}/[controller]")]
    public class CommentController(
       ILogger<CommentController> logger,
       IOptions<AppSettings> appSettings,
       ICommentService _commentService,
       UserManager<ApplicationUser> _userManager) : ApiController(logger, appSettings)
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string postId, int pageNo = 1, int limit = 10)
        {
            var comments = _commentService.Get(postId, pageNo, limit);
            var commentsViewModel = new List<CommentViewModel>();
            comments.ToList()
                .ForEach(_ => commentsViewModel.Add(GetCommentViewModel(_)));
            return Ok(new ApiResponse<IEnumerable<CommentViewModel>>(commentsViewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CommentRequest commentRequest, [FromRoute] string postId)
        {
            commentRequest.UserId = GetCurrentUserId();
            commentRequest.PostId = postId;
            var comment = await _commentService.AddAsync(commentRequest);
            var commentViewModel = GetCommentViewModel(comment);
            return Ok(new ApiResponse<CommentViewModel>(HttpStatusCode.Created, commentViewModel));
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> Update([FromBody] CommentRequest commentRequest, [FromRoute] string commentId, [FromRoute] string postId)
        {
            commentRequest.Id = commentId;
            var userId = GetCurrentUserId();
            var comment = await _commentService.GetByIdAsync(commentId);

            if (!comment.UserId.Equals(userId))
            {
                _logger.LogError("User doesn't created the comment {id}", commentId);
                throw new StatusException(HttpStatusCode.BadRequest, "You can't delete the comment.");
            }
            comment = await _commentService.UpdateAsync(commentRequest);
            var commentViewModel = GetCommentViewModel(comment);
            return Ok(new ApiResponse<CommentViewModel>(commentViewModel));
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete([FromRoute] string commentId, string postId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleting comment {commentId} for post {postId} by {userId}", commentId, postId, userId);
            var comment = await _commentService.GetByIdAsync(commentId);
            if (!comment.UserId.Equals(userId))
            {
                _logger.LogError("User doesn't created the comment {id}", commentId);
                throw new StatusException(HttpStatusCode.BadRequest, "You can't delete the comment.");
            }
            await _commentService.DeleteAsync(commentId);
            return Ok(new ApiResponse<string>("Comment deleted successfully."));
        }

        private CommentViewModel GetCommentViewModel(Comment comment)
        {
            var appUser = _userManager.FindByIdAsync(comment.UserId).Result;
            return comment.AsCommentViewModel(appUser);
        }
    }
}
