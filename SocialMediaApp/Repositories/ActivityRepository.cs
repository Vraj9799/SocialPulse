using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SocialMediaApp.Models;
using SocialMediaApp.Models.Entities;

namespace SocialMediaApp.Repositories;

public interface IActivityRepository
{
    IEnumerable<Activity> GetUserActivity(string userId, int pageNo, int limit, bool onlyUnRead = false);
    Task<Activity> AddAsync(Activity activity);
    Task MarkAsReadAsync(string activityId);
    Task<Activity> UpdateAsync(Activity activity);
}

public class ActivityRepository(
    ILogger<CommentRepository> _logger,
    IMongoClient mongoClient,
    IOptions<AppSettings> appSettings) : IActivityRepository
{
    private readonly IMongoCollection<Activity> _activityCollection = mongoClient
        .GetDatabase(appSettings.Value.MongoDBSettings.Database)
        .GetCollection<Activity>("activities");

    public IEnumerable<Activity> GetUserActivity(string userId, int pageNo, int limit, bool onlyUnRead = false)
    {
        var queryable = _activityCollection.AsQueryable()
            .Where(q => q.UserId == userId);

        if (onlyUnRead)
            queryable = queryable.Where(q => q.IsRead == false);

        var activities = queryable
            .OrderByDescending(q => q.CreatedOn)
            .Skip((pageNo - 1) * limit)
            .Take(limit);

        return [.. activities];
    }
    
    public async Task<Activity> AddAsync(Activity activity)
    {
        await _activityCollection.InsertOneAsync(activity);
        return activity;
    }

    public async Task MarkAsReadAsync(string activityId)
    {
        var update = Builders<Activity>.Update
            .Set(u => u.IsRead, true);
        var filter = Builders<Activity>.Filter
            .Eq(u => u.Id, activityId);
        await _activityCollection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<Activity> UpdateAsync(Activity activity)
    {
        activity = await _activityCollection.FindOneAndReplaceAsync(_ => _.Id == activity.Id, activity);
        return activity;
    }
}