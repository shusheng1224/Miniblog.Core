using Miniblog.Core.Data;
namespace Miniblog.Core.Services
{
    using Microsoft.EntityFrameworkCore;

    using Miniblog.Core.Models;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class EfBlogService : IBlogService
    {
        private readonly BlogDbContext _db;

        public EfBlogService(BlogDbContext db)
        {
            _db = db;
        }

        public async Task DeletePost(Post post)
        {
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async IAsyncEnumerable<string> GetCategories()
        {
            var categories = await _db.Posts
                .SelectMany(p => p.Categories)
                .Distinct()
                .ToListAsync();

            foreach (var cat in categories)
            {
                yield return cat;
            }
        }

        public async Task<Post?> GetPostById(string id)
        {
            return await _db.Posts.Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Post?> GetPostBySlug(string slug)
        {
            return await _db.Posts.Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async IAsyncEnumerable<Post> GetPosts()
        {
            var posts = await _db.Posts
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PubDate)
                .ToListAsync();

            foreach (var post in posts)
            {
                yield return post;
            }
                
        }

        public async IAsyncEnumerable<Post> GetPosts(int count, int skip = 0)
        {
            var posts = await _db.Posts
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PubDate)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            foreach (var post in posts)
            {
                yield return post;
            }
        }

        public async IAsyncEnumerable<Post> GetPostsByCategory(string category)
        {
            var normalized = category.ToLowerInvariant();

            var query = _db.Posts
                .Where(p => p.Categories.Any(c => c.ToLower() == normalized) && p.IsPublished)
                .OrderByDescending(p => p.PubDate)
                .AsAsyncEnumerable();

            await foreach (var post in query)
            {
                yield return post;
            }
        }

        public async IAsyncEnumerable<Post> GetPostsByTag(string tag)
        {
            var normalized = tag.ToLowerInvariant();

            var query = _db.Posts
                .Where(p => p.Tags.Any(t => t.ToLower() == normalized) && p.IsPublished)
                .OrderByDescending(p => p.PubDate)
                .AsAsyncEnumerable();

            await foreach (var post in query)
            {
                yield return post;
            }
        }
        public async IAsyncEnumerable<string> GetTags()
        {
            var tags = await _db.Posts
                .SelectMany(p => p.Tags)
                .Distinct()
                .ToListAsync();

            foreach (var tag in tags)
            {
                yield return tag;
            }
        }

        public Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null)
        {
            return Task.FromResult(string.Empty);
        }

        public async Task SavePost(Post post)
        {
            if (_db.Posts.Any(p => p.ID == post.ID))
            {
                _db.Posts.Update(post);
            }
            else
            {
                _db.Posts.Add(post);
            }

            await _db.SaveChangesAsync();
        }
    }
}
