namespace SocialMediaApp.Models.ViewModels
{
    public class PostViewModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public ISet<string> Medias { get; set; } = new HashSet<string>();
        public ISet<string> Tags { get; set; } = new HashSet<string>();
        public User User { get; set; }
        public long Likes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
    }
}
