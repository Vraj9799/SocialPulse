using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.ViewModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Name is required.", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required.", AllowEmptyStrings = false)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(16, ErrorMessage = "Password should be 8-16 characters.", MinimumLength = 8)]
        public string Password { get; set; }
    }
}
