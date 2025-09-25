namespace Miniblog.Core.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 评论实体，包含作者、内容、邮箱等信息。
/// </summary>
public class Comment
{
    /// <summary>
    /// 评论作者。
    /// </summary>
    [Required]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 评论内容。
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 作者邮箱。
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 评论唯一ID。
    /// </summary>
    [Required]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 是否为管理员评论。
    /// </summary>
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// 评论发布时间。
    /// </summary>
    [Required]
    public DateTime PubDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 获取Gravatar头像URL。
    /// </summary>
    /// <returns>头像图片URL</returns>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "It is an email address.")]
    [SuppressMessage(
        "Performance",
        "CA1850:Prefer static 'HashData' method over 'ComputeHash'",
        Justification = "We aren't using it for encryption so we don't care.")]
    [SuppressMessage(
        "Security",
        "CA5351:Do Not Use Broken Cryptographic Algorithms",
        Justification = "We aren't using it for encryption so we don't care.")]
    public string GetGravatar()
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(this.Email.Trim().ToLowerInvariant());
        var hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        var sb = new StringBuilder();
        for (var i = 0; i < hashBytes.Length; i++)
        {
            _ = sb.Append(hashBytes[i].ToString("X2", CultureInfo.InvariantCulture));
        }

        return $"https://www.gravatar.com/avatar/{sb.ToString().ToLowerInvariant()}?s=60&d=blank";
    }

    /// <summary>
    /// 渲染评论内容。
    /// </summary>
    /// <returns>评论内容字符串</returns>
    public string RenderContent() => this.Content;
}
