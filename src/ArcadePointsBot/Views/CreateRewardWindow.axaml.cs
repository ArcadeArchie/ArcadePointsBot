using System;
using ArcadePointsBot.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace ArcadePointsBot;

public partial class CreateRewardWindow : ReactiveWindow<CreateRewardWindowViewModel>
{
    public CreateRewardWindow()
    {
        InitializeComponent();

        this.WhenActivated(d =>
            d(
                ViewModel!.CreateTwitchRewardCommand.Subscribe(x =>
                {
                    if (x != null)
                        Close(x);
                })
            )
        );
    }
}
