namespace Miniblog.Core.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 提供通用视图控制器，如错误页和离线页。
/// </summary>
public class SharedController : Controller
{
    /// <summary>
    /// 显示错误页面。
    /// </summary>
    /// <returns>错误视图</returns>
    public IActionResult Error() => this.View(this.Response.StatusCode);

    /// <summary>
    /// 用于支持离线场景的视图。
    /// </summary>
    /// <returns>离线视图</returns>
    public IActionResult Offline() => this.View();
}
