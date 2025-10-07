using Microsoft.EntityFrameworkCore;

using Miniblog.Core.Models;

using System.Collections.Generic;
using System.Reflection.Emit;

namespace Miniblog.Core.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 关系：一个 Post 对应多个 Comment
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
