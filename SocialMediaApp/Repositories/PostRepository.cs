using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models;
using System.Linq.Expressions;
using MongoDB.Driver.Linq;

namespace SocialMediaApp.Repositories
{
    public interface IPostRepository
    {
        Task<Post> AddAsync(Post post);

        Task DeleteAsync(string id);

        Task<IQueryable<Post>> Get(Expression<Func<Post, bool>> expression, int pageNo, int limit);

        Task<Post?> GetByIdAsync(string id, bool includeDeleted = false);

        Task<long> GetCountAsync(Expression<Func<Post, bool>> expression);

        Task<Post> UpdateAsync(Post post);

        Task LikePostAsync(string postId, string userId);

        Task UnLikePostAsync(string postId, string userId);

        Task SavePostAsync(string postId, string userId);

        Task UnSavePostAsync(string postId, string userId);
    }

    public class PostRepository(IOptions<AppSettings> appSettings, IMongoClient mongoClient) : IPostRepository
    {
        private readonly IMongoCollection<Post> _postCollection = mongoClient.GetDatabase(appSettings.Value.MongoDBSettings.Database)
                .GetCollection<Post>("posts");

        public async Task<IQueryable<Post>> Get(Expression<Func<Post, bool>> expression, int pageNo, int limit)
        {
            expression ??= _ => true;

            var posts = _postCollection
                .AsQueryable()
                .Where(expression)
                .OrderBy(_ => _.LastModifiedOn)
                .Skip(pageNo - 1 * limit)
                .Take(limit);

            return posts;
        }

        public async Task<Post?> GetByIdAsync(string id, bool includeDeleted = false)
        {
            var posts = _postCollection
                .AsQueryable()
                .Where(p => p.Id == id);

            if (!includeDeleted)
                posts = posts.Where(p => !p.IsDeleted);

            var post = posts.FirstOrDefault();

            return post;
        }

        public async Task<long> GetCountAsync(Expression<Func<Post, bool>> expression)
        {
            expression ??= _ => true;
            var count = await _postCollection.CountDocumentsAsync<Post>(expression);
            return count;
        }

        public async Task<Post> AddAsync(Post post)
        {
            await _postCollection.InsertOneAsync(post, new InsertOneOptions
            {
                BypassDocumentValidation = false,
            });
            return post;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            post = await _postCollection.FindOneAndReplaceAsync(_ => _.Id == post.Id, post);
            return post;
        }

        public async Task DeleteAsync(string id)
        {
            var update = Builders<Post>.Update
                .Set<bool>(_ => _.IsDeleted, true);

            await _postCollection.FindOneAndUpdateAsync(_ => _.Id == id, update);
        }

        public async Task LikePostAsync(string postId, string userId)
        {
            var update = Builders<Post>.Update
            .AddToSet<string>(_ => _.LikedBy, userId);

            await _postCollection.FindOneAndUpdateAsync(_ => _.Id == postId, update);
        }

        public async Task UnLikePostAsync(string postId, string userId)
        {
            var post = await _postCollection.AsQueryable().FirstOrDefaultAsync(_ => _.Id == postId);
            if (post is null)
                return;
            post.LikedBy.Remove(userId);
            var update = Builders<Post>.Update
            .Set(_ => _.LikedBy, post.LikedBy);
            await _postCollection.FindOneAndUpdateAsync(_ => _.Id == postId, update);
        }

        public async Task SavePostAsync(string postId, string userId)
        {
            var update = Builders<Post>.Update
            .AddToSet<string>(_ => _.SavedBy, userId);

            await _postCollection.FindOneAndUpdateAsync(_ => _.Id == postId, update);
        }

        public async Task UnSavePostAsync(string postId, string userId)
        {
            var post = await _postCollection.AsQueryable().FirstOrDefaultAsync(_ => _.Id == postId);
            if (post is null)
                return;
            post.SavedBy.Remove(userId);
            var update = Builders<Post>.Update
            .Set(_ => _.SavedBy, post.SavedBy);
            await _postCollection.FindOneAndUpdateAsync(_ => _.Id == postId, update);
        }
    }
}
