namespace Miniblog.Core;

/// <summary>
/// 文章列表显示方式的枚举。
/// </summary>
public enum PostListView
{
    /// <summary>
    /// 仅显示标题。
    /// </summary>
    TitlesOnly,

    /// <summary>
    /// 显示标题和摘要。
    /// </summary>
    TitlesAndExcerpts,

    /// <summary>
    /// 显示完整文章。
    /// </summary>
    FullPosts
}
