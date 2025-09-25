namespace Miniblog.Core.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

using Miniblog.Core.Models;
using Miniblog.Core.Services;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using WebEssentials.AspNetCore.Pwa;

/// <summary>
/// 博客主控制器，处理文章、评论、标签、分类等相关操作。
/// </summary>
public partial class BlogController(IBlogService blog, IOptionsSnapshot<BlogSettings> settings, WebManifest manifest) : Controller
{
    /// <summary>
    /// 添加评论到指定文章。
    /// </summary>
    /// <param name="postId">文章ID</param>
    /// <param name="comment">评论对象</param>
    /// <returns>重定向到评论锚点</returns>
    [Route("/blog/comment/{postId}")]
    [HttpPost]
    public async Task<IActionResult> AddComment(string postId, Comment comment)
    {
        // 获取目标文章
        var post = await blog.GetPostById(postId).ConfigureAwait(true);

        // 校验模型有效性
        if (!this.ModelState.IsValid)
        {
            return this.View(nameof(Post), post);
        }

        // 检查评论是否开放
        if (post is null || !post.AreCommentsOpen(settings.Value.CommentsCloseAfterDays))
        {
            return this.NotFound();
        }

        ArgumentNullException.ThrowIfNull(comment);

        // 设置评论属性
        comment.IsAdmin = this.User.Identity!.IsAuthenticated;
        comment.Content = comment.Content.Trim();
        comment.Author = comment.Author.Trim();
        comment.Email = comment.Email.Trim();

        // 防止机器人提交
        if (!this.Request.Form.ContainsKey("website"))
        {
            post.Comments.Add(comment);
            await blog.SavePost(post).ConfigureAwait(false);
        }

        return this.Redirect($"{post.GetEncodedLink()}#{comment.ID}");
    }

    /// <summary>
    /// 显示指定分类下的文章列表。
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <param name="page">页码</param>
    /// <returns>分类文章视图</returns>
    [Route("/blog/category/{category}/{page:int?}")]
    [OutputCache(PolicyName = "default")]
    public async Task<IActionResult> Category(string category, int page = 0)
    {
        // 获取分类下所有文章
        var posts = blog.GetPostsByCategory(category);

        var totalPostCount = await posts.CountAsync().ConfigureAwait(true);

        // 分页处理
        var postsPerPage = settings.Value.PostsPerPage;
        if (postsPerPage <= 0)
        {
            postsPerPage = 4; // 默认值
        }

        var totalPages = (totalPostCount / postsPerPage) - (totalPostCount % postsPerPage == 0 ? 1 : 0);
        var pagedPosts = posts.Skip(postsPerPage * page).Take(postsPerPage);

        // 设置视图参数
        this.ViewData[Constants.ViewOption] = settings.Value.ListView;
        this.ViewData[Constants.PostsPerPage] = postsPerPage;
        this.ViewData[Constants.TotalPages] = totalPages;
        this.ViewData[Constants.TotalPostCount] = totalPostCount;
        this.ViewData[Constants.Title] = $"{manifest.Name} {category}";
        this.ViewData[Constants.Description] = $"Articles posted in the {category} category";
        this.ViewData[Constants.prev] = $"/blog/category/{category}/{page + 1}/";
        this.ViewData[Constants.next] = $"/blog/category/{category}/{(page <= 1 ? null : page - 1 + "/")}";

        return this.View("~/Views/Blog/Index.cshtml", pagedPosts);
    }

    /// <summary>
    /// 删除指定文章下的评论。
    /// </summary>
    /// <param name="postId">文章ID</param>
    /// <param name="commentId">评论ID</param>
    /// <returns>重定向到评论区</returns>
    [Route("/blog/comment/{postId}/{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(string postId, string commentId)
    {
        // 获取目标文章
        var post = await blog.GetPostById(postId).ConfigureAwait(false);
        if (post is null)
        {
            return this.NotFound();
        }

        // 查找评论
        var comment = post.Comments.FirstOrDefault(c => c.ID.Equals(commentId, StringComparison.OrdinalIgnoreCase));
        if (comment is null)
        {
            return this.NotFound();
        }

        _ = post.Comments.Remove(comment);
        await blog.SavePost(post).ConfigureAwait(false);

        return this.Redirect($"{post.GetEncodedLink()}#comments");
    }

    /// <summary>
    /// 删除指定文章。
    /// </summary>
    /// <param name="id">文章ID</param>
    /// <returns>重定向到首页</returns>
    [Route("/blog/deletepost/{id}")]
    [HttpPost, Authorize, AutoValidateAntiforgeryToken]
    public async Task<IActionResult> DeletePost(string id)
    {
        // 获取目标文章
        var existing = await blog.GetPostById(id).ConfigureAwait(false);
        if (existing is null)
        {
            return this.NotFound();
        }

        await blog.DeletePost(existing).ConfigureAwait(false);
        return this.Redirect("/");
    }

    /// <summary>
    /// 编辑或新建文章。
    /// </summary>
    /// <param name="id">文章ID，可为空</param>
    /// <returns>编辑视图</returns>
    [Route("/blog/edit/{id?}")]
    [HttpGet, Authorize]
    public async Task<IActionResult> Edit(string? id)
    {
        // 获取所有分类和标签
        var categories = await blog.GetCategories().ToListAsync();
        categories.Sort();
        this.ViewData[Constants.AllCats] = categories;

        var tags = await blog.GetTags().ToListAsync();
        tags.Sort();
        this.ViewData[Constants.AllTags] = tags;

        if (string.IsNullOrEmpty(id))
        {
            return this.View(new Post());
        }

        var post = await blog.GetPostById(id).ConfigureAwait(false);

        return post is null ? this.NotFound() : this.View(post);
    }

    /// <summary>
    /// 显示文章列表首页。
    /// </summary>
    /// <param name="page">页码</param>
    /// <returns>文章列表视图</returns>
    [Route("/{page:int?}")]
    [OutputCache(PolicyName = "default")]
    public async Task<IActionResult> Index([FromRoute] int page = 0)
    {
        // 获取所有已发布文章
        var posts = blog.GetPosts();

        var totalPostCount = await posts.CountAsync().ConfigureAwait(true);

        // 分页处理
        var postsPerPage = settings.Value.PostsPerPage;
        if (postsPerPage <= 0)
        {
            postsPerPage = 4; // 默认值
        }

        var pagedPosts = posts.Skip(postsPerPage * page).Take(postsPerPage);
        var totalPages = (totalPostCount / postsPerPage) - (totalPostCount % postsPerPage == 0 ? 1 : 0);

        // 设置视图参数
        this.ViewData[Constants.ViewOption] = settings.Value.ListView;
        this.ViewData[Constants.PostsPerPage] = postsPerPage;
        this.ViewData[Constants.TotalPages] = totalPages;
        this.ViewData[Constants.TotalPostCount] = totalPostCount;
        this.ViewData[Constants.Title] = manifest.Name;
        this.ViewData[Constants.Description] = manifest.Description;
        this.ViewData[Constants.prev] = $"/{page + 1}/";
        this.ViewData[Constants.next] = $"/{(page <= 1 ? null : $"{page - 1}/")}";

        return this.View("~/Views/Blog/Index.cshtml", pagedPosts);
    }

    /// <summary>
    /// 显示指定slug的文章。
    /// </summary>
    /// <param name="slug">文章slug</param>
    /// <returns>文章视图</returns>
    [Route("/blog/{slug?}")]
    [OutputCache(PolicyName = "default")]
    public async Task<IActionResult> Post(string slug)
    {
        // 获取目标文章
        var post = await blog.GetPostBySlug(slug).ConfigureAwait(true);

        return post is null ? this.NotFound() : this.View(post);
    }

    /// <summary>
    /// 将旧格式URL重定向到新格式。
    /// </summary>
    /// <param name="slug">文章slug</param>
    /// <returns>重定向结果</returns>
    [Route("/post/{slug}")]
    [HttpGet]
    public IActionResult Redirects(string slug) => this.LocalRedirectPermanent($"/blog/{slug}");

    /// <summary>
    /// 显示指定标签下的文章列表。
    /// </summary>
    /// <param name="tag">标签名称</param>
    /// <param name="page">页码</param>
    /// <returns>标签文章视图</returns>
    [Route("/blog/tag/{tag}/{page:int?}")]
    [OutputCache(PolicyName = "default")]
    public async Task<IActionResult> Tag(string tag, int page = 0)
    {
        // 获取标签下所有文章
        var posts = blog.GetPostsByTag(tag);

        var totalPostCount = await posts.CountAsync().ConfigureAwait(true);

        // 分页处理
        var postsPerPage = settings.Value.PostsPerPage;
        if (postsPerPage <= 0)
        {
            postsPerPage = 4; // 默认值
        }

        var totalPages = (totalPostCount / postsPerPage) - (totalPostCount % postsPerPage == 0 ? 1 : 0);
        var pagedPosts = posts.Skip(postsPerPage * page).Take(postsPerPage);

        // 设置视图参数
        this.ViewData[Constants.ViewOption] = settings.Value.ListView;
        this.ViewData[Constants.PostsPerPage] = postsPerPage;
        this.ViewData[Constants.TotalPages] = totalPages;
        this.ViewData[Constants.TotalPostCount] = totalPostCount;
        this.ViewData[Constants.Title] = $"{manifest.Name} {tag}";
        this.ViewData[Constants.Description] = $"Articles posted in the {tag} tag";
        this.ViewData[Constants.prev] = $"/blog/tag/{tag}/{page + 1}/";
        this.ViewData[Constants.next] = $"/blog/tag/{tag}/{(page <= 1 ? null : $"{page - 1}/")}";

        return this.View("~/Views/Blog/Index.cshtml", pagedPosts);
    }

    /// <summary>
    /// 更新文章内容。
    /// </summary>
    /// <param name="slug">文章slug</param>
    /// <param name="post">文章对象</param>
    /// <returns>重定向到文章</returns>
    [Route("/blog/{slug?}")]
    [HttpPost, Authorize, AutoValidateAntiforgeryToken]
    public async Task<IActionResult> UpdatePost(string? slug, Post post)
    {
        // 校验模型有效性
        if (!this.ModelState.IsValid)
        {
            return this.View(nameof(Edit), post);
        }

        ArgumentNullException.ThrowIfNull(post);

        // 检查slug唯一性
        var existing = await blog.GetPostById(post.ID).ConfigureAwait(false) ?? post;
        var existingPostWithSameSlug = await blog.GetPostBySlug(existing.Slug).ConfigureAwait(true);
        if (existingPostWithSameSlug is not null && existingPostWithSameSlug.ID != post.ID)
        {
            existing.Slug = Models.Post.CreateSlug(post.Title + DateTime.UtcNow.ToString("yyyyMMddHHmm"), 50);
        }

        // 处理分类和标签
        string categories = this.Request.Form[Constants.categories]!;
        string tags = this.Request.Form[Constants.tags]!;

        existing.Categories.Clear();
        foreach (var category in categories
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToLowerInvariant()))
        {
            existing.Categories.Add(category);
        }

        existing.Tags.Clear();
        foreach (var tag in tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant()))
        {
            existing.Tags.Add(tag);
        }

        // 更新文章属性
        existing.Title = post.Title.Trim();
        existing.Slug = string.IsNullOrWhiteSpace(post.Slug) ? Models.Post.CreateSlug(post.Title) : post.Slug.Trim();
        existing.IsPublished = post.IsPublished;
        existing.Content = post.Content.Trim();
        existing.Excerpt = post.Excerpt.Trim();

        await this.SaveFilesToDisk(existing).ConfigureAwait(false);
        await blog.SavePost(existing).ConfigureAwait(false);

        return this.Redirect(post.GetEncodedLink());
    }

    /// <summary>
    /// 匹配Base64图片的正则表达式。
    /// </summary>
    [GeneratedRegex("data:[^/]+/(?<ext>[a-z]+);base64,(?<base64>.+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Base64Regex();

    /// <summary>
    /// 匹配img标签的正则表达式。
    /// </summary>
    [GeneratedRegex("<img[^>]+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex ImgRegex();

    /// <summary>
    /// 将Base64图片保存为文件。
    /// </summary>
    /// <param name="post">文章对象</param>
    private async Task SaveFilesToDisk(Post post)
    {
        // 匹配所有图片标签
        var imgRegex = ImgRegex();
        var base64Regex = Base64Regex();
        var allowedExtensions = new[] {
          ".jpg",
          ".jpeg",
          ".gif",
          ".png",
          ".webp"
        };

        foreach (Match? match in imgRegex.Matches(post.Content))
        {
            if (match is null)
            {
                continue;
            }

            // 解析img标签
            var doc = new XmlDocument();
            doc.LoadXml($"<root>{match.Value}</root>");

            var img = doc.FirstChild!.FirstChild;
            var srcNode = img!.Attributes!["src"];
            var fileNameNode = img.Attributes["data-filename"];

            // 仅处理带有base64的图片
            if (srcNode is null || fileNameNode is null)
            {
                continue;
            }

            var extension = Path.GetExtension(fileNameNode.Value);

            // 只接受图片文件
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var base64Match = base64Regex.Match(srcNode.Value);
            if (base64Match.Success)
            {
                var bytes = Convert.FromBase64String(base64Match.Groups["base64"].Value);
                srcNode.Value = await blog.SaveFile(bytes, fileNameNode.Value).ConfigureAwait(false);

                _ = img.Attributes.Remove(fileNameNode);
                post.Content = post.Content.Replace(match.Value, img.OuterXml, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
