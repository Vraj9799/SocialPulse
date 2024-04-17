using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaApp.Repositories
{
    public interface IUserRepository
    {
        Task FollowUser(UserFollow userFollow);

        Task UnFollowUser(UserFollow userFollow);

        Task<IEnumerable<UserFollow>> GetMyFollowers(string userId);

        Task<IEnumerable<UserFollow>> GetMyFollowings(string userId);

        Task<IEnumerable<ApplicationUser>> GetApplicationUsersAsync(ISet<string> userIds);
    }

    public class UserRepository(ApplicationDbContext _context) : IUserRepository
    {
        public async Task FollowUser(UserFollow userFollow)
        {
            _context.UserFollowers.Add(userFollow);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserFollow>> GetMyFollowings(string userId)
        {
            return await _context.UserFollowers
                .Where(_ => _.FromUserId.Equals(userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserFollow>> GetMyFollowers(string userId)
        {
            return await _context.UserFollowers
                .Where(_ => _.ToUserId.Equals(userId))
                .ToListAsync();
        }

        public async Task UnFollowUser(UserFollow userFollow)
        {
            userFollow = await _context.UserFollowers
                .FirstAsync(_ => _.FromUserId == userFollow.FromUserId && _.ToUserId == userFollow.ToUserId);
            _context.UserFollowers.Remove(userFollow);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetApplicationUsersAsync(ISet<string> userIds)
        {
            return await _context.Users
            .Where(_ => userIds.Contains(_.Id))
            .ToListAsync();
        }
    }
}
