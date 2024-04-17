using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models;

namespace SocialMediaApp.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> AddAsync(Comment comment);

        Task DeleteAsync(string commentId);

        Task<Comment> GetById(string commentId);

        IEnumerable<Comment> GetByPostId(string postId, int pageNo, int limit, bool includeDeleted = false);

        Task<long> GetCount(string postId);

        Task<Comment> UpdateAsync(Comment comment);
    }

    public class CommentRepository(ILogger<CommentRepository> _logger, IMongoClient _mongoClient, IOptions<AppSettings> _appSettings) : ICommentRepository
    {
        private readonly IMongoCollection<Comment> _commentCollection = _mongoClient.GetDatabase(_appSettings.Value.MongoDBSettings.Database)
                .GetCollection<Comment>("comments");

        public IEnumerable<Comment> GetByPostId(string postId, int pageNo, int limit, bool includeDeleted = false)
        {
            _logger.LogInformation("Fetching comment with postId {id}", postId);
            var queryable = _commentCollection.AsQueryable()
                .Where(_ => _.PostId == postId);
            if (!includeDeleted)
                queryable = queryable.Where(_ => _.IsDeleted == false);
            var comments = queryable
                .Skip((pageNo - 1) * limit)
                .Take(limit);
            return [.. comments];
        }

        public async Task<Comment> GetById(string commentId)
        {
            _logger.LogInformation("Fetching comment with {id}", commentId);

            var filter = Builders<Comment>.Filter
                .Eq(_ => _.Id, commentId);

            var comment = await _commentCollection.Find(filter).FirstOrDefaultAsync();
            return comment;
        }

        public async Task<long> GetCount(string postId)
        {
            var count = await _commentCollection.CountDocumentsAsync(_ => _.PostId == postId);
            _logger.LogInformation("Comments for post id {id} are {count}", postId, count);
            return count;
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            _logger.LogInformation("Adding new comment for post id {id}", comment.PostId);
            await _commentCollection.InsertOneAsync(comment);
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            _logger.LogInformation("Upating comment with id {id}", comment.Id);
            comment = await _commentCollection.FindOneAndReplaceAsync(_ => _.Id == comment.Id, comment);
            return comment;
        }

        public async Task DeleteAsync(string commentId)
        {
            _logger.LogInformation("Deleted {commentId}", commentId);
            var update = Builders<Comment>.Update
                .Set(_ => _.IsDeleted, true)
                .Set(_ => _.LastModifiedOn, DateTime.UtcNow);

            await _commentCollection.FindOneAndUpdateAsync(_ => _.Id == commentId, update);
        }
    }
}
