
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models;
using SocialMediaApp.Models.Entities;
namespace SocialMediaApp.Repositories
{
    public interface ITokenRepository
    {
        Task Save(RefreshToken refreshToken);

        Task<RefreshToken> GetByToken(string token, string userId);

        Task DeleteByToken(string token, string userId);
    }

    public class TokenRepository(ApplicationDbContext _dbContext) : ITokenRepository
    {
        public async Task Save(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetByToken(string token, string userId)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .Where(_ => _.Token.Equals(token) && _.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            return refreshToken;
        }

        public async Task DeleteByToken(string token, string userId)
        {
            var refreshToken = await GetByToken(token, userId);
            if (refreshToken is null) return;
            _dbContext.RefreshTokens.Remove(refreshToken);
            await _dbContext.SaveChangesAsync();
        }
    }
}
