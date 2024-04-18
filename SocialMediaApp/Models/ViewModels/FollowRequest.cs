using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.ViewModels
{
    public class FollowRequest
    {
        public string? From { get; set; }

        [Required]
        public string To { get; set; }
    }
}
