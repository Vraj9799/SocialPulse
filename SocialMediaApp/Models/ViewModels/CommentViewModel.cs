namespace SocialMediaApp.Models.ViewModels
{
    public class CommentViewModel
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
