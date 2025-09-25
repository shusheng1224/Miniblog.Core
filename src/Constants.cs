namespace Miniblog.Core;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// 提供全局常量定义，包含视图、配置、页面等相关字符串常量。
/// </summary>
public static class Constants
{
    /// <summary>
    /// 所有分类的键名。
    /// </summary>
    public static readonly string AllCats = "AllCats";
    /// <summary>
    /// 所有标签的键名。
    /// </summary>
    public static readonly string AllTags = "AllTags";
    /// <summary>
    /// 分类集合的键名。
    /// </summary>
    public static readonly string categories = "categories";
    /// <summary>
    /// 短横线字符串常量。
    /// </summary>
    public static readonly string Dash = "-";
    /// <summary>
    /// 描述信息的键名。
    /// </summary>
    public static readonly string Description = "Description";
    /// <summary>
    /// 页头的键名。
    /// </summary>
    public static readonly string Head = "Head";
    /// <summary>
    /// 下一页的键名。
    /// </summary>
    public static readonly string next = "next";
    /// <summary>
    /// 页码的键名。
    /// </summary>
    public static readonly string page = "page";
    /// <summary>
    /// 每页文章数的键名。
    /// </summary>
    public static readonly string PostsPerPage = "PostsPerPage";
    /// <summary>
    /// 预加载的键名。
    /// </summary>
    public static readonly string Preload = "Preload";
    /// <summary>
    /// 上一页的键名。
    /// </summary>
    public static readonly string prev = "prev";
    /// <summary>
    /// 返回地址的键名。
    /// </summary>
    public static readonly string ReturnUrl = "ReturnUrl";
    /// <summary>
    /// 脚本的键名。
    /// </summary>
    public static readonly string Scripts = "Scripts";
    /// <summary>
    /// Slug（短链接）的键名。
    /// </summary>
    public static readonly string slug = "slug";
    /// <summary>
    /// 空格字符串常量。
    /// </summary>
    public static readonly string Space = " ";
    /// <summary>
    /// 标签集合的键名。
    /// </summary>
    public static readonly string tags = "tags";
    /// <summary>
    /// 标题的键名。
    /// </summary>
    public static readonly string Title = "Title";
    /// <summary>
    /// 总页数的键名。
    /// </summary>
    public static readonly string TotalPages = "TotalPages";
    /// <summary>
    /// 文章总数的键名。
    /// </summary>
    public static readonly string TotalPostCount = "TotalPostCount";
    /// <summary>
    /// 视图选项的键名。
    /// </summary>
    public static readonly string ViewOption = "ViewOption";

    /// <summary>
    /// 配置相关常量。
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1034:Nested types should not be visible",
        Justification = "Constant classes are nested for easy intellisense.")]
    public static class Config
    {
        /// <summary>
        /// 博客相关配置常量。
        /// </summary>
        public static class Blog
        {
            /// <summary>
            /// 博客名称配置键。
            /// </summary>
            public static readonly string Name = "blog:name";
        }

        /// <summary>
        /// 用户相关配置常量。
        /// </summary>
        public static class User
        {
            /// <summary>
            /// 用户密码配置键。
            /// </summary>
            public static readonly string Password = "user:password";
            /// <summary>
            /// 用户密码盐配置键。
            /// </summary>
            public static readonly string Salt = "user:salt";
            /// <summary>
            /// 用户名配置键。
            /// </summary>
            public static readonly string UserName = "user:username";
        }
    }
}
