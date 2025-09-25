namespace Miniblog.Core.Services;

using Microsoft.AspNetCore.Http;

using Miniblog.Core.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 博客服务的内存实现基类，提供缓存和通用操作。
/// </summary>
public abstract class InMemoryBlogServiceBase(IHttpContextAccessor contextAccessor) : IBlogService
{
    /// <summary>
    /// 文章缓存列表。
    /// </summary>
    protected List<Post> Cache { get; } = [];

    /// <summary>
    /// HTTP上下文访问器。
    /// </summary>
    protected IHttpContextAccessor ContextAccessor { get; } = contextAccessor;

    /// <inheritdoc/>
    public abstract Task DeletePost(Post post);

    /// <inheritdoc/>
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Consumer preference.")]
    public virtual IAsyncEnumerable<string> GetCategories()
    {
        var isAdmin = this.IsAdmin();

        var categories = this.Cache
            .Where(p => p.IsPublished || isAdmin)
            .SelectMany(post => post.Categories)
            .Select(cat => cat.ToLowerInvariant())
            .Distinct()
            .ToAsyncEnumerable();

        return categories;
    }

    /// <inheritdoc/>
    public virtual Task<Post?> GetPostById(string id)
    {
        var isAdmin = this.IsAdmin();
        var post = this.Cache.FirstOrDefault(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(
            post is null || !post.IsVisible() || !isAdmin
            ? null
            : post);
    }

    /// <inheritdoc/>
    public virtual Task<Post?> GetPostBySlug(string slug)
    {
        var isAdmin = this.IsAdmin();
        var post = this.Cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(
            post is null || !post.IsVisible() || !isAdmin
            ? null
            : post);
    }

    /// <summary>
    /// 获取所有文章。
    /// </summary>
    /// <returns>文章的异步枚举</returns>
    /// <remarks>重载方法，获取所有文章。</remarks>
    public virtual IAsyncEnumerable<Post> GetPosts()
    {
        var isAdmin = this.IsAdmin();
        return this.Cache.Where(p => p.IsVisible() || isAdmin).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<Post> GetPosts(int count, int skip = 0)
    {
        var isAdmin = this.IsAdmin();

        var posts = this.Cache
            .Where(p => p.IsVisible() || isAdmin)
            .Skip(skip)
            .Take(count)
            .ToAsyncEnumerable();

        return posts;
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<Post> GetPostsByCategory(string category)
    {
        var isAdmin = this.IsAdmin();

        var posts = from p in this.Cache
                    where p.IsVisible() || isAdmin
                    where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
                    select p;

        return posts.ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<Post> GetPostsByTag(string tag)
    {
        var isAdmin = this.IsAdmin();

        var posts = from p in this.Cache
                    where p.IsVisible() || isAdmin
                    where p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)
                    select p;

        return posts.ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Consumer preference.")]
    public virtual IAsyncEnumerable<string> GetTags()
    {
        var isAdmin = this.IsAdmin();

        var tags = this.Cache
            .Where(p => p.IsPublished || isAdmin)
            .SelectMany(post => post.Tags)
            .Select(tag => tag.ToLowerInvariant())
            .Distinct()
            .ToAsyncEnumerable();

        return tags;
    }

    /// <inheritdoc/>
    public abstract Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null);

    /// <inheritdoc/>
    public abstract Task SavePost(Post post);

    /// <summary>
    /// 判断当前用户是否为管理员。
    /// </summary>
    /// <returns>是管理员返回 true，否则返回 false</returns>
    protected bool IsAdmin() => this.ContextAccessor.HttpContext?.User?.Identity!.IsAuthenticated ?? false;

    /// <summary>
    /// 按发布时间对缓存排序。
    /// </summary>
    protected void SortCache() => this.Cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
}
