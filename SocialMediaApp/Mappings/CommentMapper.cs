using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;

namespace SocialMediaApp.Mappings
{
    public static class CommentMapper
    {
        public static CommentViewModel AsCommentViewModel(this Comment source, ApplicationUser appUser)
        {
            if (source is null) throw new NullReferenceException();
            var commentViewModel = new CommentViewModel
            {
                Id = source.Id,
                Message = source.Message,
                User = appUser.AsUser([]),
                CreatedOn = source.CreatedOn
            };
            return commentViewModel;
        }
    }
}
