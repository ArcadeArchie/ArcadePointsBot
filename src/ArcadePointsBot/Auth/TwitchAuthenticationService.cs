using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcadePointsBot.Auth
{
    internal class TwitchAuthenticationService : IAuthenticationService
    {
        private const string SCOPES = "openid channel:read:redemptions user:read:chat user:bot channel:moderate channel:manage:redemptions";
        private readonly ILogger<TwitchAuthenticationService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly OidcClient _oidcClient;
        public TwitchAuthConfig AuthConfig { get; }

        public TwitchAuthenticationService(IOptions<TwitchAuthConfig> config, ILoggerFactory loggerFactory)
        {
            AuthConfig = config.Value;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<TwitchAuthenticationService>();
            _oidcClient = new OidcClient(new OidcClientOptions
            {
                Authority = "https://id.twitch.tv/oauth2",
                ClientId = AuthConfig.ClientId,
                ClientSecret = AuthConfig.ClientSecret,
                RedirectUri = "http://localhost:5000",
                LoggerFactory = _loggerFactory,
                Scope = SCOPES,
                FilterClaims = false,
                Browser = new SystemBrowser(5000),
            });
        }

        public async Task<bool> RequestCredentials()
        {
            if (string.IsNullOrEmpty(AuthConfig.AccessToken) ||
                AuthConfig.AccessTokenExpiration < DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("Requesting Twitch credentials");

                var result = await _oidcClient.LoginAsync(new LoginRequest());

                if (result.IsError)
                {
                    _logger.LogWarning("Requesting Twitch credentials failed: {err}", result.Error);
                    return false;
                }
                AuthConfig.AccessToken = result.AccessToken;
                AuthConfig.RefreshToken = result.RefreshToken;
                if (string.IsNullOrWhiteSpace(result.IdentityToken))
                {
                    var userInfo = await _oidcClient.GetUserInfoAsync(result.AccessToken);
                }
                else
                    AuthConfig.IdentityToken = result.IdentityToken;
                AuthConfig.AccessTokenExpiration = result.AccessTokenExpiration;
                return true;
            }
            return true;
        }

        public async Task<bool> IsTokenValidAsync()
        {
            if (string.IsNullOrEmpty(AuthConfig.AccessToken) ||
                AuthConfig.AccessTokenExpiration < DateTimeOffset.UtcNow)
                return false;
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", AuthConfig.AccessToken);
            var res = await http.GetAsync("https://id.twitch.tv/oauth2/validate");
            return res.IsSuccessStatusCode;
        }

        public async Task EnsureValidTokenAsync()
        {
            if (await IsTokenValidAsync()) return;

            if (!string.IsNullOrEmpty(AuthConfig.RefreshToken))
                await RefreshCredentials();
            else
                await RequestCredentials();
        }

        public async Task<bool> RefreshCredentials()
        {
            if (string.IsNullOrEmpty(AuthConfig.RefreshToken))
            {
                _logger.LogCritical("Refresh token is null, cannot refresh credentials");
                return false;
            }

            _logger.LogInformation("Refreshing Twitch credentials");

            var result = await _oidcClient.RefreshTokenAsync(AuthConfig.RefreshToken);

            if (result.IsError)
            {
                _logger.LogWarning("Refreshing Twitch credentials failed: {err}", result.Error);
                return false;
            }
            AuthConfig.AccessToken = result.AccessToken;
            AuthConfig.RefreshToken = result.RefreshToken;
            AuthConfig.AccessTokenExpiration = result.AccessTokenExpiration;
            return true;
        }
    }
}
