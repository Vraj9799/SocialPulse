namespace SocialMediaApp.Models.ViewModels
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public bool EmailVerified { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
