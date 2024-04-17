using SocialMediaApp.Models.Entities;
using SocialMediaApp.Repositories;

namespace SocialMediaApp.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetApplicationUser(ISet<string> userIds);

        Task<IEnumerable<ApplicationUser>> GetMyFollowers(string userId);

        Task<IEnumerable<ApplicationUser>> GetMyFollowings(string userId);

        Task FollowUserAsync(string from, string to);

        Task UnFollowUserAsync(string from, string to);
    }

    public class UserService(IUserRepository _userRepository) : IUserService
    {
        public async Task<IEnumerable<ApplicationUser>> GetMyFollowers(string userId)
        {
            var followers = await _userRepository.GetMyFollowers(userId);
            return followers.Select(_ => _.From);
        }

        public async Task<IEnumerable<ApplicationUser>> GetMyFollowings(string userId)
        {
            var followers = await _userRepository.GetMyFollowings(userId);
            return followers.Select(_ => _.To);
        }

        public async Task FollowUserAsync(string from, string to)
        {
            UserFollow userFollow = new() { FromUserId = from, ToUserId = to, CreatedOn = DateTime.UtcNow };
            await _userRepository.FollowUser(userFollow);
        }

        public async Task UnFollowUserAsync(string from, string to)
        {
            UserFollow userFollow = new() { FromUserId = from, ToUserId = to, CreatedOn = DateTime.UtcNow };
            await _userRepository.UnFollowUser(userFollow);
        }

        public async Task<IEnumerable<ApplicationUser>> GetApplicationUser(ISet<string> userIds)
        {
            var users = await _userRepository.GetApplicationUsersAsync(userIds);
            return users;
        }
    }
}
