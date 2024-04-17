using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.Shared;
using SocialMediaApp.Models.ViewModels;

namespace SocialMediaApp.Mappings
{
    public static class UserMapper
    {
        public static User AsUser(this ApplicationUser source, IList<string>? roles)
        {
            if (source is null) throw new StatusException("Invalid details.");
            User user = new()
            {
                Id = source.Id,
                Name = source.Name,
                Email = source.Email,
                Username = source.UserName,
                EmailVerified = source.EmailConfirmed,
                Roles = roles is null ? new List<string>() : roles.ToList()
            };
            return user;
        }
    }
}
