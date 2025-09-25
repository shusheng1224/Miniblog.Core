namespace Miniblog.Core.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Miniblog.Core.Models;
using Miniblog.Core.Services;

using System.Security.Claims;
using System.Threading.Tasks;

/// <summary>
/// 账户相关控制器，处理登录与注销。
/// </summary>
[Authorize]
public class AccountController(IUserServices userServices) : Controller
{
    /// <summary>
    /// 显示登录页面。
    /// </summary>
    /// <param name="returnUrl">登录后返回的地址</param>
    /// <returns>登录视图</returns>
    [Route("/login")]
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        this.ViewData[Constants.ReturnUrl] = returnUrl;
        return this.View();
    }

    /// <summary>
    /// 处理登录请求。
    /// </summary>
    /// <param name="returnUrl">登录后返回的地址</param>
    /// <param name="model">登录视图模型</param>
    /// <returns>重定向或登录视图</returns>
    [Route("/login")]
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(string? returnUrl, LoginViewModel? model)
    {
        this.ViewData[Constants.ReturnUrl] = returnUrl;

        if (model is null || model.UserName is null || model.Password is null)
        {
            this.ModelState.AddModelError(string.Empty, Properties.Resources.UsernameOrPasswordIsInvalid);
            return this.View(nameof(Login), model);
        }

        if (!this.ModelState.IsValid || !userServices.ValidateUser(model.UserName, model.Password))
        {
            this.ModelState.AddModelError(string.Empty, Properties.Resources.UsernameOrPasswordIsInvalid);
            return this.View(nameof(Login), model);
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

        var principle = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
        await this.HttpContext.SignInAsync(principle, properties).ConfigureAwait(false);

        return this.LocalRedirect(returnUrl ?? "/");
    }

    /// <summary>
    /// 注销当前用户。
    /// </summary>
    /// <returns>重定向到首页</returns>
    [Route("/logout")]
    public async Task<IActionResult> LogOutAsync()
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return this.LocalRedirect("/");
    }
}
