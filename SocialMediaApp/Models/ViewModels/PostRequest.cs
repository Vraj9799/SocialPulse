using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.ViewModels
{
    public class PostRequest
    {
        [Required(ErrorMessage = "Caption is required.")]
        public string Caption { get; set; }

        [Length(1, 10, ErrorMessage = "Only 1 to 10 medias allowed.")]
        public ISet<string> Medias { get; set; }

        [MaxLength(5, ErrorMessage = "You can add maximum 5 tags.")]
        public ISet<string> Tags { get; set; }

        public DateTime? ScheduledOn { get; set; }
    }
}
