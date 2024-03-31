using System.Diagnostics;
using System.Threading;
using ArcadePointsBot;
using ArcadePointsBot.Services;
using ArcadePointsBot.ViewModels;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadePointsBot;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
/// <summary>
/// Used by for the Designer as a source of generated view models
/// </summary>
public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } =
        (
            (App)Avalonia.Application.Current!
        ).GlobalHost!.Services.GetRequiredService<MainWindowViewModel>();

    public static CreateRewardWindowViewModel CreateRewardWindowViewModel { get; } =
        (
            (App)Avalonia.Application.Current!
        ).GlobalHost!.Services.GetRequiredService<CreateRewardWindowViewModel>();

    public static EditRewardViewModel EditRewardViewModel { get; } =
        new EditRewardViewModel(null, new ArcadePointsBot.Domain.Rewards.TwitchReward());
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
