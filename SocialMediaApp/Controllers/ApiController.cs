using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models;
using System.Net;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected readonly AppSettings _appSettings;

        public ApiController(ILogger logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        protected string GetCurrentUserId()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Current User -> {userId}", userId);
            return userId ?? throw new StatusException(HttpStatusCode.Unauthorized, "Login Required");
        }

        protected string? GetUserClaim(string claim)
        {
            return HttpContext.User.FindFirstValue(claim);
        }

        protected void SetCookie(string key, string value, int? expiresInMinutes, bool isEssential = false)
        {
            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                IsEssential = isEssential,
                Expires = expiresInMinutes.HasValue ? DateTime.UtcNow.AddMinutes(expiresInMinutes.Value) : null,
            });
        }
    }
}
