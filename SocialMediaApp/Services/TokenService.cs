using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models;
using SocialMediaApp.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialMediaApp.Services
{
    public interface ITokenService
    {
        (string, string) GenerateAuthToken(ApplicationUser appUser, IList<string> roles);

        Task<bool> IsValidRefreshToken(string refreshToken, string userId);

        Task DeleteRefreshToken(string token, string userId);

        ClaimsPrincipal GetClaimsFromToken(string refreshToken);
    }
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly AppSettings _appSettings;

        public TokenService(IOptions<AppSettings> appSettings, ITokenRepository tokenRepository)
        {
            _appSettings = appSettings.Value;
            _tokenRepository = tokenRepository;
        }

        public (string, string) GenerateAuthToken(ApplicationUser appUser, IList<string> roles)
        {
            string jwtId = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            string accessToken = GenerateJwtToken(jwtId, now, appUser, roles);
            var refreshToken = new RefreshToken()
            {
                User = appUser,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresOn = now.AddDays(_appSettings.Jwt.RefreshTokenExpiryInDays),
                JwtId = jwtId,
            };
            _tokenRepository.Save(refreshToken);
            return (accessToken, refreshToken.Token);
        }

        public async Task<bool> IsValidRefreshToken(string token, string userId)
        {
            var refreshToken = await _tokenRepository.GetByToken(token, userId);
            if (refreshToken is null || refreshToken.ExpiresOn > DateTime.UtcNow)
                return false;
            return true;
        }

        public async Task DeleteRefreshToken(string token, string userId) => await _tokenRepository.DeleteByToken(token, userId);

        private string GenerateJwtToken(string jwtId, DateTime now, ApplicationUser appUser, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, appUser.Id),
                new(JwtRegisteredClaimNames.Sub, appUser.Id),
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(JwtRegisteredClaimNames.Name,appUser.Name),
                new(JwtRegisteredClaimNames.Email,appUser.Email),
            };

            foreach (var role in roles)
                claims.Add(new(ClaimTypes.Role, role));

            var tokenDescriptor = new JwtSecurityToken(
                    issuer: _appSettings.Jwt.Issuer,
                    claims: claims,
                    audience: _appSettings.Jwt.Audience,
                    expires: now.AddMinutes(_appSettings.Jwt.ExpiryInMinutes),
                    signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.SecretKey)),
                            SecurityAlgorithms.HmacSha256
                        )
                );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            return token;
        }

        public ClaimsPrincipal GetClaimsFromToken(string refreshToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
