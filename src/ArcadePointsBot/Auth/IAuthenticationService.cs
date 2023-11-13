using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Auth;

public interface IAuthenticationService
{
    TwitchAuthConfig AuthConfig { get; }
    Task<bool> RequestCredentials();
    Task<bool> RefreshCredentials();
    Task<bool> IsTokenValidAsync();
    Task EnsureValidTokenAsync();
}
