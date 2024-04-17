using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.ViewModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.", AllowEmptyStrings = false)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.", AllowEmptyStrings = false)]
        [StringLength(16, ErrorMessage = "Password should be 8-16 characters.", MinimumLength = 8)]
        public string Password { get; set; }
    }
}
