namespace Miniblog.Core.Services;

/// <summary>
/// 用户服务接口，定义用户验证方法。
/// </summary>
public interface IUserServices
{
    /// <summary>
    /// 验证用户名和密码是否正确。
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>验证通过返回 true，否则返回 false</returns>
    public bool ValidateUser(string username, string password);
}
