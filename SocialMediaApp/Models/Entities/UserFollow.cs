using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.Entities
{
    public class UserFollow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string FromUserId { get; set; }
        public string ToUserId { get; set; }

        [ForeignKey("FromUserId")]
        public virtual ApplicationUser From { get; set; }

        [ForeignKey("ToUserId")]
        public virtual ApplicationUser To { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
