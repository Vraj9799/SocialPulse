using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Name is required.", AllowEmptyStrings = false)]
        public string Name { get; set; }

        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string username) : base(username)
        {
        }
    }
}
