namespace Miniblog.Core.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 博文(Post)类，表示一篇博客文章
/// </summary>
public partial class Post
{
    /// <summary>
    /// 分类列表
    /// </summary>
    public IList<string> Categories { get; } = [];

    /// <summary>
    /// 评论列表
    /// </summary>
    public IList<Comment> Comments { get; } = [];

    /// <summary>
    /// 文章内容，必填
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 文章摘要，必填
    /// </summary>
    [Required]
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// 文章唯一ID，必填，默认为当前UTC时间戳
    /// </summary>
    [Required]
    public string ID { get; set; } = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// 是否已发布
    /// </summary>
    public bool IsPublished { get; set; } = true;
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PubDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 文章短链接Slug，允许为空字符串
    /// </summary>
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 标签列表
    /// </summary>
    public IList<string> Tags { get; } = [];

    /// <summary>
    /// 文章标题，必填
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 创建Slug的静态方法，根据标题生成短链接字符串
    /// </summary>
    /// <param name="title">文章标题</param>
    /// <param name="maxLength">最大长度，默认为50</param>
    /// <returns>生成的slug字符串</returns>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The slug should be lower case.")]
    public static string CreateSlug(string title, int maxLength = 50)
    {
        // 将标题转为小写并替换空格为短横线
        title = title?.ToLowerInvariant().Replace(
            Constants.Space, Constants.Dash, StringComparison.OrdinalIgnoreCase) ?? string.Empty;
        // 移除变音符号
        title = RemoveDiacritics(title);
        // 移除URL保留字符
        title = RemoveReservedUrlCharacters(title);

        // 如果超出最大长度则截断
        if (title.Length > maxLength)
        {
            title = title[..maxLength];
        }

        // 返回小写slug
        return title.ToLowerInvariant();
    }

    /// <summary>
    /// 判断评论是否开放
    /// </summary>
    /// <param name="commentsCloseAfterDays">评论关闭天数</param>
    /// <returns>是否开放评论</returns>
    public bool AreCommentsOpen(int commentsCloseAfterDays) =>
        this.PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;

    /// <summary>
    /// 获取已编码的文章链接
    /// </summary>
    /// <returns>已编码的URL</returns>
    public string GetEncodedLink() => $"/blog/{System.Net.WebUtility.UrlEncode(this.Slug)}/";

    /// <summary>
    /// 获取文章链接
    /// </summary>
    /// <returns>文章URL</returns>
    public string GetLink() => $"/blog/{this.Slug}/";

    /// <summary>
    /// 判断文章是否可见（已发布且发布时间早于当前时间）
    /// </summary>
    /// <returns>是否可见</returns>
    public bool IsVisible() => this.PubDate <= DateTime.UtcNow && this.IsPublished;

    /// <summary>
    /// 渲染文章内容，处理图片懒加载和YouTube嵌入
    /// </summary>
    /// <returns>渲染后的内容HTML</returns>
    public string RenderContent()
    {
        var result = this.Content;

        // 设置图片/iframe懒加载
        if (!string.IsNullOrEmpty(result))
        {
            // 图片懒加载替换
            var replacement = " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"";
            result = ImageLazyLoadRegex().Replace(result, m => m.Groups[1].Value + replacement + m.Groups[4].Value + m.Groups[3].Value);

            // 处理YouTube嵌入语法: [youtube:xyzAbc123]
            var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
            result = YouTubeEmbedRegex().Replace(result, m => string.Format(CultureInfo.InvariantCulture, video, m.Groups[1].Value));
        }

        return result;
    }

    /// <summary>
    /// 匹配图片标签的正则表达式（用于懒加载）
    /// </summary>
    [GeneratedRegex("(<img.*?)(src=[\\\"|'])(?<src>.*?)([\\\"|'].*?[/]?>)")]
    private static partial Regex ImageLazyLoadRegex();

    /// <summary>
    /// 移除字符串中的变音符号
    /// </summary>
    /// <param name="text">输入字符串</param>
    /// <returns>移除变音符号后的字符串</returns>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                _ = stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// 移除URL保留字符
    /// </summary>
    /// <param name="text">输入字符串</param>
    /// <returns>移除保留字符后的字符串</returns>
    private static string RemoveReservedUrlCharacters(string text)
    {
        var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

        foreach (var chr in reservedCharacters)
        {
            text = text.Replace(chr, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    /// <summary>
    /// 匹配YouTube嵌入语法的正则表达式
    /// </summary>
    [GeneratedRegex(@"\[youtube:(.*?)\]")]
    private static partial Regex YouTubeEmbedRegex();
}
