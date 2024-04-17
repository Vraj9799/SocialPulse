using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models.ViewModels;

namespace SocialMediaApp.Mappings
{
    public static class PostMapper
    {
        public static PostViewModel AsPostViewModel(this Post post, ApplicationUser user)
        {
            return new PostViewModel
            {
                Id = post.Id,
                Caption = post.Caption,
                Medias = post.Medias,
                Tags = post.Tags,
                CreatedOn = post.CreatedOn,
                LastModifiedOn = post.LastModifiedOn,
                IsDeleted = post.IsDeleted,
                Likes = post.LikedBy is null ? 0 : post.LikedBy.Count,
                User = user.AsUser([])
            };
        }
    }
}
