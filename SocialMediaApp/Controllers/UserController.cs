using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Services;
using SocialMediaApp.Mappings;
using Microsoft.AspNetCore.WebUtilities;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    public class UserController(
        ILogger<UserController> logger,
        IOptions<AppSettings> appSettings,
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IEmailService emailService,
        IActivityService activityService
    ) : ApiController(logger, appSettings)
    {
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var appUser = await userManager.FindByIdAsync(GetCurrentUserId());
            var roles = await userManager.GetRolesAsync(appUser);
            var user = appUser.AsUser(roles);
            return Ok(new ApiResponse<User>(HttpStatusCode.OK, user));
        }

        [HttpPost("is-email-confirmed")]
        public async Task<IActionResult> IsEmailConfirmed([FromBody] string email)
        {
            var appUser = await GetUserByEmailOrUsername(email);
            bool isEmailConfirmed = await userManager.IsEmailConfirmedAsync(appUser);
            return Ok(new ApiResponse<bool>(isEmailConfirmed));
        }

        [HttpPost("send-email-confirmation")]
        public async Task<IActionResult> SendEmailConfirm([FromBody] string email)
        {
            var appUser = await GetUserByEmailOrUsername(email);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var queryParams = new Dictionary<string, string?>()
            {
                { "userId", appUser.Id },
                { "token", token }
            };
            var url = QueryHelpers.AddQueryString($"{_appSettings.FrontendUrl}/user/verify-email", queryParams);
            await emailService.SendEmailAsync(new EmailModel
            {
                ToAddress = appUser.Email,
                Subject = "Verify Email",
                TemplateId = _appSettings.EmailSettings.ConfirmationTemplate,
                DynamicValues = new Dictionary<string, string>(){
                    { "name", appUser.Name},
                    {"verify_email_url", url}
                }
            });
            return Ok(new ApiResponse<string>("Please check your email. " + url));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmailConfirmation(string userId, string token)
        {
            var appUser = await userManager.FindByIdAsync(userId);
            if (appUser is null)
                throw new StatusException(HttpStatusCode.BadRequest, "Invalid user.");
            var result = await userManager.ConfirmEmailAsync(appUser, token);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse<string>("Email Verified Successfully."));
            }
            return BadRequest(new ApiResponse<List<string>>(result.Errors.Select(_ => _.Description).ToList()));
        }

        [HttpPut("follow")]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequest followRequest)
        {
            await userService.FollowUserAsync(GetCurrentUserId(), followRequest.To);
            var toUser = (await userService.GetApplicationUser(new HashSet<string>() { followRequest.To })).FirstOrDefault();
            await activityService.AddAsync(GetCurrentUserId(), $"You started following ${toUser?.Name}.");
            await activityService.AddAsync(toUser?.Id, $"${GetUserClaim(ClaimTypes.Name)} started following you.");
            return Ok(new ApiResponse<string>($"You started following ${toUser?.Name}."));
        }

        [HttpPut("unfollow")]
        public async Task<IActionResult> UnFollowUser([FromBody] FollowRequest followRequest)
        {
            await userService.UnFollowUserAsync(GetCurrentUserId(), followRequest.To);
            var toUser = (await userService.GetApplicationUser(new HashSet<string>() { followRequest.To })).FirstOrDefault();
            await activityService.AddAsync(GetCurrentUserId(), $"You un-followed ${toUser?.Name}.");
            return Ok(new ApiResponse<string>($"You un-followed ${toUser?.Name}."));
        }

        [HttpGet("get-activities")]
        public IActionResult GetActivities(int pageNo = 1, int limit = 10)
        {
            var activities = activityService.GetUserActivities(GetCurrentUserId(), pageNo, limit);
            return Ok(new ApiResponse<IEnumerable<Activity>>(activities));
        }

        private async Task<ApplicationUser> GetUserByEmailOrUsername(string username)
        {
            var appUser = await userManager.FindByEmailAsync(username) ??
                await userManager.FindByNameAsync(username);
            if (appUser is null)
                throw new StatusException(HttpStatusCode.BadRequest, "Invalid user.");
            return appUser;
        }
    }
}
