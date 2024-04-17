using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Repositories;

namespace SocialMediaApp.Services
{
    public interface ICommentService
    {
        IEnumerable<Comment> Get(string postId, int pageNo, int limit);

        Task<Comment> AddAsync(CommentRequest commentRequest);

        Task DeleteAsync(string commentId);

        Task<Comment> GetByIdAsync(string commentId);

        Task<Comment> UpdateAsync(CommentRequest commentRequest);
    }

    public class CommentService(ICommentRepository _commentRepository) : ICommentService
    {
        public async Task<Comment> AddAsync(CommentRequest commentRequest)
        {
            Comment comment = new()
            {
                Message = commentRequest.Message,
                PostId = commentRequest.PostId,
                UserId = commentRequest.UserId
            };
            comment = await _commentRepository.AddAsync(comment);
            return comment;
        }

        public async Task DeleteAsync(string commentId)
        {
            await _commentRepository.DeleteAsync(commentId);
            throw new NotImplementedException();
        }

        public IEnumerable<Comment> Get(string postId, int pageNo, int limit)
        {
            var comments = _commentRepository.GetByPostId(postId, pageNo, limit);
            return comments;
        }

        public async Task<Comment> GetByIdAsync(string commentId)
        {
            var comment = await _commentRepository.GetById(commentId);
            return comment;
        }

        public async Task<Comment> UpdateAsync(CommentRequest commentRequest)
        {
            Comment comment = await GetByIdAsync(commentRequest.Id);
            comment.Message = commentRequest.Message;
            comment = await _commentRepository.UpdateAsync(comment);
            return comment;
        }
    }
}
