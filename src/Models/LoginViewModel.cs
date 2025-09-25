namespace Miniblog.Core.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// 登录视图模型，包含用户名、密码和记住我选项。
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// 用户密码。
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;


    /// <summary>
    /// 是否记住登录状态。
    /// </summary>
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// 用户名。
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;
}
