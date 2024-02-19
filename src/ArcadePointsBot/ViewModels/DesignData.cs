using Avalonia;
using ArcadePointsBot;
using ArcadePointsBot.Services;
using ArcadePointsBot.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;

namespace ArcadePointsBot;

/// <summary>
/// Used by for the Designer as a source of generated view models
/// </summary>
public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } = 
        ((App)Application.Current!).GlobalHost!.Services.GetRequiredService<MainWindowViewModel>();


    public static CreateRewardWindowViewModel CreateRewardWindowViewModel { get; } =
        ((App)Application.Current!).GlobalHost!.Services.GetRequiredService<CreateRewardWindowViewModel>();

    public static EditRewardViewModel EditRewardViewModel { get; } = new EditRewardViewModel(null, 
        new ArcadePointsBot.Domain.Rewards.TwitchReward());
}
