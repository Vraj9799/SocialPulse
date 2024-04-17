using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models.Entities;

namespace SocialMediaApp.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(_ =>
            {
                _.ToTable(name: "Users");
                _.HasIndex(_ => _.NormalizedEmail).IsUnique();
                _.HasIndex(_ => _.NormalizedUserName).IsUnique();
            });

            builder.Entity<ApplicationRole>(_ =>
            {
                _.ToTable(name: "Roles");
                _.HasIndex(_ => _.NormalizedName).IsUnique();
            });

            builder.Entity<UserFollow>(_ =>
            {
                _.ToTable(name: "UserFollowers");
                _.HasIndex(_ => new { _.FromUserId, _.ToUserId }).IsUnique();
                _.HasOne<ApplicationUser>(uf => uf.From).WithOne().OnDelete(DeleteBehavior.ClientSetNull);
                _.HasOne<ApplicationUser>(uf => uf.To).WithOne().OnDelete(DeleteBehavior.ClientSetNull);
            });
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserFollow> UserFollowers { get; set; }
    }

}
