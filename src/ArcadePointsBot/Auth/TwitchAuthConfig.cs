using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AvaloniaApplication1.Auth;
public class TwitchAuthConfig : INotifyPropertyChanged
{
    private DateTimeOffset? accessTokenExpiration;
    private string? refreshToken;
    private string? identityToken;
    private string? accessToken;
        
    [Required]
    public string ClientId { get; init; } = string.Empty;
    
    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    public string? AccessToken
    {
        get => accessToken;
        set
        {
            accessToken = value;
            OnPropertyChanged(nameof(AccessToken), value);
        }
    }

    public string? IdentityToken
    {
        get => identityToken;
        set
        {
            identityToken = value;
            ParseIdToken();
            OnPropertyChanged(nameof(IdentityToken), value);
        }
    }

    public string? RefreshToken
    {
        get => refreshToken;
        set
        {
            refreshToken = value;
            OnPropertyChanged(nameof(RefreshToken), value);
        }
    }

    public DateTimeOffset? AccessTokenExpiration
    {
        get => accessTokenExpiration;
        set
        {
            accessTokenExpiration = value;
            OnPropertyChanged(nameof(AccessTokenExpiration), value?.ToString("u"));
        }
    }

    [JsonIgnore]
    public string? Uid { get; private set; }

    [JsonIgnore]
    public string? Username { get; private set; }

    private void ParseIdToken()
    {
        if (string.IsNullOrEmpty(IdentityToken)) return;
        var parts = IdentityToken.Split('.');
        byte[]? json = null;
        try
        {
            json = Convert.FromBase64String(parts[1] + "=");
        }
        catch (Exception)
        {
            json = Convert.FromBase64String(parts[1]);
        }
        var jsonObj = JsonNode.Parse(json);

        Uid = jsonObj?["sub"]?.GetValue<string>();
        Username = jsonObj?["preferred_username"]?.GetValue<string>();
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null, object? value = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgsEx(propertyName, value));
    }
    #endregion 

}
public class PropertyChangedEventArgsEx : PropertyChangedEventArgs
{
    private readonly object? _value;

    public virtual object? Value => _value;

    public PropertyChangedEventArgsEx(string? propertyName, object? value) : base(propertyName)
    {
        _value = value;
    }
}
