using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models;
using System.Net;
using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;
using SocialMediaApp.Services;
using SocialMediaApp.Mappings;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : ApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthenticationController(ILogger<AuthenticationController> logger, IOptions<AppSettings> appSettings, UserManager<ApplicationUser> userManager, ITokenService tokenService) : base(logger, appSettings)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var user = await _userManager.FindByNameAsync(registerRequest.Username);
            if (user is not null)
            {
                throw new StatusException(HttpStatusCode.BadRequest, "Username already exist.");
            }
            user = await _userManager.FindByEmailAsync(registerRequest.Email);
            if (user is not null)
            {
                throw new StatusException(HttpStatusCode.BadRequest, "Email already exist.");
            }
            ApplicationUser newUser = new ApplicationUser()
            {
                Name = registerRequest.Name,
                UserName = registerRequest.Username,
                Email = registerRequest.Email,
                EmailConfirmed = false,
            };
            var result = await _userManager.CreateAsync(newUser, registerRequest.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Error while creating user --> {errors}", result.Errors);
                throw new StatusException("Some error occurred. please try again later.");
            }
            try
            {
                await _userManager.AddToRoleAsync(newUser, _appSettings.DefaultUserRole);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while adding role to the user --> {userId}", user?.Id);
                _logger.LogError(ex.ToString());
            }
            return Ok(new ApiResponse<string>("Account created successfully."));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var appUser = await _userManager.FindByEmailAsync(loginRequest.Username) ??
                await _userManager.FindByNameAsync(loginRequest.Username);

            if (appUser is null)
                throw new StatusException(HttpStatusCode.BadRequest, "Invalid user.");

            if (await _userManager.IsLockedOutAsync(appUser))
                throw new StatusException($"Your account has been locked out. Please try again after 5 minutes.");
            if (!await _userManager.CheckPasswordAsync(appUser, loginRequest.Password))
            {
                await _userManager.AccessFailedAsync(appUser);
                throw new StatusException("Invalid login credentials.");
            }
            await _userManager.ResetAccessFailedCountAsync(appUser);
            var roles = await _userManager.GetRolesAsync(appUser);

            var (accessToken, refreshToken) = _tokenService.GenerateAuthToken(appUser, roles);
            SetCookie(Constants.ACCESS_TOKEN, accessToken, null, true);
            SetCookie(Constants.REFRESH_TOKEN, refreshToken, null, true);
            var user = appUser.AsUser(roles);

            return Ok(new ApiResponse<User>(user));
        }
    }
}
