using Microsoft.EntityFrameworkCore;

using Miniblog.Core.Data;
using Miniblog.Core.Models;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miniblog.Core.Services
{
    public class EfBlogService : IBlogService
    {
        private readonly BlogDbContext _db;

        public EfBlogService(BlogDbContext db)
        {
            _db = db;
        }

        #region Post 操作

        public async Task DeletePost(Post post)
        {
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async Task<Post?> GetPostById(string id)
        {
            return await _db.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Post?> GetPostBySlug(string slug)
        {
            return await _db.Posts
                .Include(p => p.Comments)
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

            var query = _db.PostCategories
                .Where(pc => pc.Category.ToLower() == normalized)
                .Select(pc => pc.Post)
                .Include(p => p.Comments)
                .Where(p => p.IsPublished)
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

            var query = _db.PostTags
                .Where(pt => pt.Tag.ToLower() == normalized)
                .Select(pt => pt.Post)
                .Include(p => p.Comments)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PubDate)
                .AsAsyncEnumerable();

            await foreach (var post in query)
            {
                yield return post;
            }
        }

        public async Task SavePost(Post post)
        {
            // EF Core 会跟踪实体，检查是否已存在
            if (await _db.Posts.AnyAsync(p => p.ID == post.ID))
            {
                _db.Posts.Update(post);
            }
            else
            {
                _db.Posts.Add(post);
            }

            await _db.SaveChangesAsync();
        }

        #endregion

        #region Categories & Tags

        public async IAsyncEnumerable<string> GetCategories()
        {
            var categories = await _db.PostCategories
                .Select(pc => pc.Category)
                .Distinct()
                .ToListAsync();

            foreach (var cat in categories)
            {
                yield return cat;
            }
        }

        public async IAsyncEnumerable<string> GetTags()
        {
            var tags = await _db.PostTags
                .Select(pt => pt.Tag)
                .Distinct()
                .ToListAsync();

            foreach (var tag in tags)
            {
                yield return tag;
            }
        }

        #endregion

        #region File 保存 (占位)

        public Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null)
        {
            // 如果你后续要实现文件上传，可在这里加入逻辑
            return Task.FromResult(string.Empty);
        }

        #endregion
    }
}
