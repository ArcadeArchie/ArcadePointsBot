using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ArcadePointsBot.Auth;
using ArcadePointsBot.Services;
using ArcadePointsBot.Views;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ArcadePointsBot.Domain.Rewards;

namespace ArcadePointsBot.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IServiceScope _serviceScope;
        private readonly IAuthenticationService _authenticationService;
        private readonly TwitchPointRewardService _rewardService;
        private readonly TwitchWorker _worker;
        private bool isAuthed;

        [Reactive]
        public WorkerStatus WorkerStatus { get; set; }

        public bool IsAuthed
        {
            get => isAuthed;
            set => Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref isAuthed, value));
        }
        private ObservableCollection<TwitchReward> _rewardList = new();
        public DataGridCollectionView? Rewards { get; set; }

        public ReactiveCommand<Unit, Unit> CreateRewardCommand { get; set; }
        public ReactiveCommand<TwitchReward, Unit> EditRewardCommand { get; set; }
        public ReactiveCommand<TwitchReward, Unit> DeleteRewardCommand { get; set; }

        public MainWindowViewModel(IServiceProvider serviceProvider, TwitchWorker worker)
        {
            _serviceScope = serviceProvider.CreateScope();
            _authenticationService = _serviceScope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            _rewardService = _serviceScope.ServiceProvider.GetRequiredService<TwitchPointRewardService>();
            StatusText = "Checking Auth status";

            _worker = worker;
            _worker.PropertyChanged += WorkerPropertyChanged;


            
            CreateRewardCommand = ReactiveCommand.CreateFromTask(CreateReward,
                Observable.CombineLatest(IsBusyObservable, this.WhenAny(x => x.IsAuthed, x => x.Value), (isBusy, isAuthed) => isBusy && isAuthed));
            EditRewardCommand = ReactiveCommand.CreateFromTask<TwitchReward>(EditReward,
                Observable.CombineLatest(IsBusyObservable, this.WhenAny(x => x.IsAuthed, x => x.Value), (isBusy, isAuthed) => isBusy && isAuthed));
            DeleteRewardCommand = ReactiveCommand.CreateFromTask<TwitchReward>(DeleteReward, IsBusyObservable);
        }

        private void WorkerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_worker.Status)) return;
            WorkerStatus = _worker.Status;
        }

        private async Task CreateReward()
        {
            IsBusy = true;
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var reward = await new CreateRewardWindow()
                {
                    DataContext = _serviceScope.ServiceProvider.GetRequiredService<CreateRewardWindowViewModel>(),
                }.ShowDialog<TwitchReward>(desktop.MainWindow!);
                if (reward != null)
                {
                    _rewardList.Add(reward);
                }
            }
            IsBusy = false;
        }

        private async Task EditReward(TwitchReward reward)
        {
            IsBusy = true;
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var res = await new EditRewardWindow()
                {
                    DataContext = new EditRewardViewModel(_rewardService, await _rewardService.GetReward(reward.Id)),
                }.ShowDialog<TwitchReward>(desktop.MainWindow!);
                if (res != null)
                    _rewardList.Replace(reward, res);
            }
            IsBusy = false;
        }


        private async Task DeleteReward(TwitchReward reward)
        {
            IsBusy = true;
            _rewardList.Remove(reward);
            await _rewardService.DeleteReward(reward);
            IsBusy = false;
        }

        public async Task LoadRewards()
        {
            IsBusy = true;
            _rewardList = new(await _rewardService.GetAll().ToListAsync());
            Rewards = new(_rewardList);
            Rewards.GroupDescriptions.Add(new DataGridPathGroupDescription("Category"));
            this.RaisePropertyChanged(nameof(Rewards));
            StartWorker();
            IsBusy = false;
        }

        public async Task Login()
        {
            IsAuthed = false;
            IsBusy = true;
            await Task.Delay(1000);
            if (string.IsNullOrEmpty(_authenticationService.AuthConfig.AccessToken))
            {
                StatusText = "Accestoken not found, redirecting to twitch";
                var result = await _authenticationService.RequestCredentials();

                StatusText = result ? "Auth OK" : "Authenticating with twitch failed, check logs";
                IsBusy = false;
                IsAuthed = result;

                return;
            }

            if (_authenticationService.AuthConfig.AccessTokenExpiration < System.DateTimeOffset.UtcNow)
            {
                StatusText = "Accestoken expired, refreshing token";
                var result = await _authenticationService.RefreshCredentials();

                StatusText = result ? "Auth OK" : "Authenticating with twitch failed, check logs";
                IsBusy = false;
                IsAuthed = result;
                return;
            }

            StatusText = "Auth OK";
            IsBusy = false;
            IsAuthed = true;
        }

        public async void ChangeRewardEnabled(TwitchReward reward)
        {
            await _rewardService.UpdateReward(reward);
        }

        internal async Task BulkDisable(List<string> toDisableIds)
        {
            if(toDisableIds.Count == 0) return;
            await _rewardService.BulkUpdateRewards(
                _rewardList
                .Where(x => toDisableIds.Contains(x.Id))
                .Select(x => { x.IsEnabled = false; return x; }), u => u.SetProperty(p => p.IsEnabled, false));
            //foreach (var id in toDisableIds)
            //{
            //    var reward = _rewardList.FirstOrDefault(x => x.Id == id);
            //    if (reward is null) return;
            //    reward.IsEnabled = false;
            //    await _rewardService.UpdateReward(reward);
            //}
        }

        internal async Task BulkEnable(List<string> toEnableIds)
        {
            if(toEnableIds.Count == 0) return;
            await _rewardService.BulkUpdateRewards(
                _rewardList
                .Where(x => toEnableIds.Contains(x.Id))
                .Select(x => { x.IsEnabled = true; return x; }), u => u.SetProperty(p => p.IsEnabled, true));
            //foreach (var id in toEnableIds)
            //{
            //    var reward = _rewardList.FirstOrDefault(x => x.Id == id);
            //    if (reward is null) return;
            //    reward.IsEnabled = true;
            //    await _rewardService.UpdateReward(reward);
            //}
        }

        internal async void StopWorker()
        {
            await _worker.StopAsync(default);
        }
        internal void StartWorker()
        {
            Task.Factory.StartNew(async () => await _worker.StartAsync(default), TaskCreationOptions.LongRunning);
        }
    }
}