using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.ViewModels;
using ReactiveUI;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {

                Dispatcher.UIThread.Post(async () =>
                {
                    if (Design.IsDesignMode) return;
                    await ViewModel!.Login();
                    await ViewModel!.LoadRewards();
                });
            });
        }

        void OnEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (e.Source is CheckBox parent && parent.DataContext is TwitchReward reward)
            {
                reward.IsEnabled = parent.IsChecked ?? false;
                ViewModel!.ChangeRewardEnabled(reward);
            }
        }
        void ChangeWorkerStatus(object sender, RoutedEventArgs e)
        {
            if (ViewModel!.WorkerStatus == AvaloniaApplication1.WorkerStatus.Running)
                ViewModel!.StopWorker();
            if (ViewModel!.WorkerStatus == AvaloniaApplication1.WorkerStatus.Stopped ||
                ViewModel!.WorkerStatus == AvaloniaApplication1.WorkerStatus.Errored )
                ViewModel!.StartWorker();
        }
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            ViewModel?.StopWorker();
            base.OnClosing(e);
        }
    }
}