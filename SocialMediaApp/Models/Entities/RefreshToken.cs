using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models.Entities
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Token { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public string UserId { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string JwtId { get; set; }
    }
}
