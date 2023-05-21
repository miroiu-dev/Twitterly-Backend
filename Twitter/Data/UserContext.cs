using Microsoft.EntityFrameworkCore;
using Twitter.Models;

namespace Twitter.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
       : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
              .HasIndex(user => user.Email)
                .IsUnique();
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<ResetToken> ResetTokens => Set<ResetToken>();
        public DbSet<Tweet> Tweets => Set<Tweet>();
        public DbSet<Like> Likes => Set<Like>();
    }
}
