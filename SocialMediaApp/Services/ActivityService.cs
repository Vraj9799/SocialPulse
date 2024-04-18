using SocialMediaApp.Models.Entities;
using SocialMediaApp.Repositories;

namespace SocialMediaApp.Services;

public interface IActivityService
{
    IEnumerable<Activity> GetUserActivities(string userId, int pageNo, int limit);
    Task AddAsync(string userId, string message);
    Task MarkAsReadAsync(string activityId);
}

public class ActivityService(IActivityRepository activityRepository): IActivityService
{
    public IEnumerable<Activity> GetUserActivities(string userId, int pageNo, int limit)
    {
        return activityRepository.GetUserActivity(userId, pageNo, limit);
    }

    public async Task AddAsync(string userId, string message)
    {
        Activity activity = new()
        {
            Message = message,
            UserId = userId
        };
        await activityRepository.AddAsync(activity);
    }

    public async Task MarkAsReadAsync(string activityId)
    {
        await activityRepository.MarkAsReadAsync(activityId);
    }
}