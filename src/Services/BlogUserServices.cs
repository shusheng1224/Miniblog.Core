namespace Miniblog.Core.Services;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

using System;
using System.Text;

/// <summary>
/// 用户服务实现，提供用户验证功能。
/// </summary>
public class BlogUserServices(IConfiguration config) : IUserServices
{
    /// <summary>
    /// 验证用户名和密码是否正确。
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>验证通过返回 true，否则返回 false</returns>
    public bool ValidateUser(string username, string password) =>
        username == config[Constants.Config.User.UserName] && VerifyHashedPassword(password, config);

    /// <summary>
    /// 验证密码哈希值是否匹配。
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="config">配置对象</param>
    /// <returns>密码匹配返回 true，否则返回 false</returns>
    private static bool VerifyHashedPassword(string password, IConfiguration config)
    {
        var saltBytes = Encoding.UTF8.GetBytes(config[Constants.Config.User.Salt]!);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8);

        var hashText = BitConverter.ToString(hashBytes).Replace(Constants.Dash, string.Empty, StringComparison.OrdinalIgnoreCase);
        return hashText == config[Constants.Config.User.Password];
    }
}
