using Avalonia.ReactiveUI;
using AvaloniaApplication1.ViewModels;
using ReactiveUI;
using System;

namespace AvaloniaApplication1;

public partial class CreateRewardWindow : ReactiveWindow<CreateRewardWindowViewModel>
{
    public CreateRewardWindow()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.CreateTwitchRewardCommand.Subscribe(Close)));
    }
}