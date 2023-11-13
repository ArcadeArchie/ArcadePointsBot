using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ArcadePointsBot.ViewModels;
using ReactiveUI;
using System;

namespace ArcadePointsBot.Views
{
    public partial class EditRewardWindow : ReactiveWindow<EditRewardViewModel>
    {
        public EditRewardWindow()
        {
            InitializeComponent();
            if (Design.IsDesignMode) return;
            this.WhenActivated(d => d(ViewModel!.EditTwitchRewardCommand.Subscribe(x =>
            {
                if (x != null)
                    Close(x);
            })));
        }
    }
}
