using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.ViewModels
{
    public class CommentRequest
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Comment should be only 50 characters.")]
        public string Message { get; set; }
        public string? UserId { get; set; }
        public string? PostId { get; set; }
    }
}
