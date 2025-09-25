namespace Miniblog.Core;

/// <summary>
/// 博客设置类，包含评论、显示、分页等相关配置。
/// </summary>
public class BlogSettings
{
    /// <summary>
    /// 评论关闭天数，超过该天数后文章评论自动关闭。
    /// </summary>
    public int CommentsCloseAfterDays { get; set; } = 10;

    /// <summary>
    /// 是否显示评论。
    /// </summary>
    public bool DisplayComments { get; set; } = true;

    /// <summary>
    /// 文章列表显示方式。
    /// </summary>
    public PostListView ListView { get; set; } = PostListView.TitlesAndExcerpts;

    /// <summary>
    /// 博主名称。
    /// </summary>
    public string Owner { get; set; } = "The Owner";

    /// <summary>
    /// 每页显示的文章数量。
    /// </summary>
    public int PostsPerPage { get; set; } = 4;
}
