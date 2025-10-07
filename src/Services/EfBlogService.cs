using Miniblog.Core.Data;
namespace Miniblog.Core.Services
{
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

        public IAsyncEnumerable<string> GetCategories() => throw new NotImplementedException();
        public Task<Post?> GetPostById(string id) => throw new NotImplementedException();
        public Task<Post?> GetPostBySlug(string slug) => throw new NotImplementedException();
        public IAsyncEnumerable<Post> GetPosts() => throw new NotImplementedException();
        public IAsyncEnumerable<Post> GetPosts(int count, int skip = 0) => throw new NotImplementedException();
        public IAsyncEnumerable<Post> GetPostsByCategory(string category) => throw new NotImplementedException();
        public IAsyncEnumerable<Post> GetPostsByTag(string tag) => throw new NotImplementedException();
        public IAsyncEnumerable<string> GetTags() => throw new NotImplementedException();
        public Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null) => throw new NotImplementedException();
        public Task SavePost(Post post) => throw new NotImplementedException();
    }
}
