using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaApplication1.Auth;
using AvaloniaApplication1.Data.Abstractions.Repositories;
using AvaloniaApplication1.Interop.Windows;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace AvaloniaApplication1;

public class TwitchWorker : BackgroundService, INotifyPropertyChanged
{
    private readonly ILogger<TwitchWorker> _logger;
    private readonly IAuthenticationService _twitchAuthService;
    private readonly TwitchPubSub _pubSubClient;
    private readonly IRewardRepository _rewardRepository;
    private readonly TwitchAPI _apiClient;
    private readonly IHostEnvironment _env;
    private bool isLive;
    private string? currentCategory;
    private WorkerStatus status = WorkerStatus.Stopped;

    public event PropertyChangedEventHandler? PropertyChanged;

    private string? CurrentCategory
    {
        get => currentCategory;
        set
        {
            currentCategory = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCategory)));
        }
    }

    public WorkerStatus Status
    {
        get => status; 
        set
        {
            status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    public TwitchWorker(IAuthenticationService twitchAuthService, ILoggerFactory loggerFactory,
        ILogger<TwitchWorker> logger, IServiceProvider provider, IHostEnvironment env)
    {
        _env = env;
        _twitchAuthService = twitchAuthService;
        _rewardRepository = provider.CreateScope().ServiceProvider.GetRequiredService<IRewardRepository>();
        _logger = logger;
        _pubSubClient = new();
        _pubSubClient.OnPubSubServiceConnected += (_, _) => _pubSubClient.SendTopics(twitchAuthService.AuthConfig.AccessToken);
        _pubSubClient.OnLog += OnLog;
        _pubSubClient.OnStreamUp += OnStreamUp;
        _pubSubClient.OnStreamDown += OnStreamDown;
        _pubSubClient.OnListenResponse += OnListenResponse;
        _pubSubClient.OnChannelPointsRewardRedeemed += OnChannelPointsRewardRedeemed;
        _apiClient = new TwitchAPI(loggerFactory);
        _apiClient.Settings.ClientId = _twitchAuthService.AuthConfig.ClientId;
        _apiClient.Settings.Secret = _twitchAuthService.AuthConfig.ClientSecret;

        PropertyChanged += ChangeActiveRewards;
    }

    private async void ChangeActiveRewards(object? sender, PropertyChangedEventArgs e)
    {
        if (!isLive) return;
        var toDisableIds = await _rewardRepository
            .GetBy(x => x.IsEnabled && x.Category != null && x.Category != CurrentCategory).Select(x => x.Id).ToListAsync();
        var toEnableIds = await _rewardRepository
            .GetBy(x => !x.IsEnabled && x.Category != null && x.Category == CurrentCategory).Select(x => x.Id).ToListAsync();

        Dispatcher.UIThread.Post(async () =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime &&
                         lifetime.MainWindow is MainWindow mw)
            {
                await mw.ViewModel!.BulkDisable(toDisableIds);
                await mw.ViewModel!.BulkEnable(toEnableIds);
            }
        });
    }

    private void OnStreamDown(object? sender, TwitchLib.PubSub.Events.OnStreamDownArgs e)
    {
        isLive = false;
        _logger.LogInformation("Stream from {Id} went down at {time}", e.ChannelId, e.ServerTime);
    }

    private void OnStreamUp(object? sender, TwitchLib.PubSub.Events.OnStreamUpArgs e)
    {
        isLive = true;
        _logger.LogInformation("Stream from {Id} went up at {time}, delay is {delay}", e.ChannelId, e.ServerTime, e.PlayDelay);
    }

    private async void OnChannelPointsRewardRedeemed(object? sender, TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
    {
        var redemption = e.RewardRedeemed.Redemption;
        _logger.LogInformation("{redeemer} has redeemed {reward} for {cost} points",
            redemption.User.DisplayName,
            redemption.Reward.Title,
            redemption.Reward.Cost
            );

        if (!isLive && !_env.IsDevelopment())
        {
            await CancelReward(redemption);
            return;
        }
        await Task.Delay(1000);
        try
        {
            var reward = await _rewardRepository.FindAsync(redemption.Reward.Id);
            if (reward is null)
                return;
            var actions = new List<RewardAction>()
                .Concat(reward.KeyboardActions)
                .Concat(reward.MouseActions).OrderBy(x => x.Index);
            foreach (var action in actions)
            {
                switch (action)
                {
                    case KeyboardRewardAction keyboardAction:
                        await DoKeyboardAction(keyboardAction);
                        break;
                    case MouseRewardAction mouseAction:
                        await DoMouseAction(mouseAction);
                        break;
                }
                await Task.Delay(100);
            }

            await _twitchAuthService.EnsureValidTokenAsync();
            await _apiClient.Helix.ChannelPoints.UpdateRedemptionStatusAsync(
            _twitchAuthService.AuthConfig.Uid, redemption.Reward.Id,
            new List<string> { redemption.Id },
            new UpdateCustomRewardRedemptionStatusRequest
            {
                Status = CustomRewardRedemptionStatus.FULFILLED
            }, accessToken: _twitchAuthService.AuthConfig.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to execute reward actions for {name}", redemption.User.DisplayName);
            await CancelReward(redemption);
        }
    }

    private async Task DoMouseAction(MouseRewardAction action)
    {
        switch (action.ActionType)
        {
            case MouseActionType.Click:
                Mouse.Click(action.ActionKey);
                break;
            case MouseActionType.DoubleClick:
                Mouse.Click(action.ActionKey);
                break;
            case MouseActionType.Press:
                Mouse.Down(action.ActionKey);
                if (action.Duration.HasValue)
                {
                    await Task.Delay(action.Duration.Value);
                    Mouse.Up(action.ActionKey);
                }
                break;
            case MouseActionType.Release:
                Mouse.Up(action.ActionKey);
                break;
        }
    }

    private async Task DoKeyboardAction(KeyboardRewardAction action)
    {
        switch (action.ActionType)
        {
            case KeyboardActionType.Tap:
                Keyboard.Type(action.ActionKey);
                break;
            case KeyboardActionType.Press:
                Keyboard.Press(action.ActionKey);
                if (action.Duration.HasValue)
                {
                    await Task.Delay(action.Duration.Value);
                    Keyboard.Release(action.ActionKey);
                }
                break;
            case KeyboardActionType.Release:
                Keyboard.Release(action.ActionKey);
                break;
        }
    }

    async Task CancelReward(Redemption redemption)
    {
        await _twitchAuthService.EnsureValidTokenAsync();
        await _apiClient.Helix.ChannelPoints.UpdateRedemptionStatusAsync(
            _twitchAuthService.AuthConfig.Uid, redemption.Reward.Id,
            new List<string> { redemption.Id },
            new UpdateCustomRewardRedemptionStatusRequest
            {
                Status = CustomRewardRedemptionStatus.CANCELED
            }, accessToken: _twitchAuthService.AuthConfig.AccessToken);
    }

    private void OnListenResponse(object? sender, TwitchLib.PubSub.Events.OnListenResponseArgs e)
    {
        if (e.Successful)
            _logger.LogInformation("Now listening for {topic} on {channelId}", e.Topic, e.ChannelId);
        else
        {
            _logger.LogCritical("Failed to listen for {topic}, reason: [{response}]", e.Topic, e.Response.Error);
            StopAsync(default);
        }
    }

    private void OnLog(object? sender, TwitchLib.PubSub.Events.OnLogArgs e) => _logger.LogDebug("{data}", e.Data);

    #region BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting pubsub connection");
        try
        {
            int delaySecond = 1;
            while (!await _twitchAuthService.IsTokenValidAsync() ||
                string.IsNullOrEmpty(_twitchAuthService.AuthConfig.Uid))
            {
                if (stoppingToken.IsCancellationRequested) break;
                _logger.LogWarning("Auth or Id token invalid waiting {delay} second before retrying", delaySecond);
                await Task.Delay(1000 * delaySecond, stoppingToken);
                if (delaySecond < 100)
                    delaySecond++;
                else
                {
                    _logger.LogWarning("Max wait time reached force refreshing credentials");
                    await _twitchAuthService.EnsureValidTokenAsync();
                    break;
                }
            }
            var streams = await _apiClient.Helix.Streams.GetStreamsAsync(
                userIds: new List<string> { _twitchAuthService.AuthConfig.Uid! },
                accessToken: _twitchAuthService.AuthConfig.AccessToken);
            var stream = streams.Streams.FirstOrDefault(x => x.Type == "live");
            if (stream is not null)
            {
                isLive = true;
                CurrentCategory = stream.GameName;
            }
            _pubSubClient.ListenToChannelPoints(_twitchAuthService.AuthConfig.Uid);
            _pubSubClient.ListenToVideoPlayback(_twitchAuthService.AuthConfig.Uid);
            _pubSubClient.Connect();
            Status = WorkerStatus.Running;
            while (!stoppingToken.IsCancellationRequested)
            {
                stream = (await _apiClient.Helix.Streams.GetStreamsAsync(
                    userIds: new List<string> { _twitchAuthService.AuthConfig.Uid! },
                    accessToken: _twitchAuthService.AuthConfig.AccessToken)).Streams.FirstOrDefault(x => x.Type == "live");
                if (stream is not null && !string.Equals(CurrentCategory, stream.GameName))
                    CurrentCategory = stream.GameName;
                await Task.Delay(10_0000, stoppingToken);
            }
        }

        catch (TaskCanceledException) { }
        catch (Exception e)
        {
            _logger.LogCritical(e, "PubSub worker failed unexpectly.");
            _logger.LogInformation("Stopping pubsub");

            _pubSubClient.OnLog -= OnLog;
            _pubSubClient.OnStreamUp -= OnStreamUp;
            _pubSubClient.OnStreamDown -= OnStreamDown;
            _pubSubClient.OnListenResponse -= OnListenResponse;
            _pubSubClient.OnChannelPointsRewardRedeemed -= OnChannelPointsRewardRedeemed;
            _pubSubClient.Disconnect();
            Status = WorkerStatus.Errored;
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Status = WorkerStatus.Starting;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Status = WorkerStatus.Stopping;
        _logger.LogInformation("Stopping pubsub");
        PropertyChanged -= ChangeActiveRewards;
        _pubSubClient.OnLog -= OnLog;
        _pubSubClient.OnStreamUp -= OnStreamUp;
        _pubSubClient.OnStreamDown -= OnStreamDown;
        _pubSubClient.OnListenResponse -= OnListenResponse;
        _pubSubClient.OnChannelPointsRewardRedeemed -= OnChannelPointsRewardRedeemed;
        _pubSubClient.Disconnect();
        Status = WorkerStatus.Stopped;
        return base.StopAsync(cancellationToken);
    }

    #endregion
}


public enum WorkerStatus
{
    Stopped,
    Stopping,
    Starting,
    Running,
    Errored
}