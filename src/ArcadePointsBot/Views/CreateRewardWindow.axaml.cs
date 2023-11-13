using Avalonia.ReactiveUI;
using ArcadePointsBot.ViewModels;
using ReactiveUI;
using System;

namespace ArcadePointsBot;

public partial class CreateRewardWindow : ReactiveWindow<CreateRewardWindowViewModel>
{
    public CreateRewardWindow()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.CreateTwitchRewardCommand.Subscribe(x =>
        {
            if (x != null)
                Close(x);
        })));
    }
}