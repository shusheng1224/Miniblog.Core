namespace Miniblog.Core.Services;

using Miniblog.Core.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 博客服务接口，定义博客相关的操作方法。
/// </summary>
public interface IBlogService
{
    /// <summary>
    /// 删除指定文章。
    /// </summary>
    /// <param name="post">要删除的文章</param>
    public Task DeletePost(Post post);

    /// <summary>
    /// 获取所有分类。
    /// </summary>
    /// <returns>分类名称的异步枚举</returns>
    public IAsyncEnumerable<string> GetCategories();

    /// <summary>
    /// 获取所有标签。
    /// </summary>
    /// <returns>标签名称的异步枚举</returns>
    public IAsyncEnumerable<string> GetTags();

    /// <summary>
    /// 通过ID获取文章。
    /// </summary>
    /// <param name="id">文章ID</param>
    /// <returns>文章对象或null</returns>
    public Task<Post?> GetPostById(string id);

    /// <summary>
    /// 通过Slug获取文章。
    /// </summary>
    /// <param name="slug">文章Slug</param>
    /// <returns>文章对象或null</returns>
    public Task<Post?> GetPostBySlug(string slug);

    /// <summary>
    /// 获取所有文章。
    /// </summary>
    /// <returns>文章的异步枚举</returns>
    public IAsyncEnumerable<Post> GetPosts();

    /// <summary>
    /// 获取指定数量的文章，并可跳过部分文章。
    /// </summary>
    /// <param name="count">获取的文章数量</param>
    /// <param name="skip">跳过的文章数量</param>
    /// <returns>文章的异步枚举</returns>
    public IAsyncEnumerable<Post> GetPosts(int count, int skip = 0);

    /// <summary>
    /// 获取指定分类下的文章。
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>文章的异步枚举</returns>
    public IAsyncEnumerable<Post> GetPostsByCategory(string category);

    /// <summary>
    /// 获取指定标签下的文章。
    /// </summary>
    /// <param name="tag">标签名称</param>
    /// <returns>文章的异步枚举</returns>
    public IAsyncEnumerable<Post> GetPostsByTag(string tag);

    /// <summary>
    /// 保存文件并返回文件路径。
    /// </summary>
    /// <param name="bytes">文件字节内容</param>
    /// <param name="fileName">文件名</param>
    /// <param name="suffix">可选后缀</param>
    /// <returns>保存后的文件路径</returns>
    public Task<string> SaveFile(byte[] bytes, string fileName, string? suffix = null);

    /// <summary>
    /// 保存文章。
    /// </summary>
    /// <param name="post">要保存的文章</param>
    public Task SavePost(Post post);
}
