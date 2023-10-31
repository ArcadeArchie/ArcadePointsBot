using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaApplication1.Auth;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Util;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly TwitchPointRewardService _rewardService;
        private bool isAuthed;

        public bool IsAuthed
        {
            get => isAuthed;
            set => Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref isAuthed, value));
        }

        public ObservableCollection<TwitchReward> Rewards { get; set; } = new();

        public ReactiveCommand<Unit, Unit> CreateRewardCommand { get; set; }
        public ReactiveCommand<TwitchReward, Unit> DeleteRewardCommand { get; set; }

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
            IAuthenticationService authenticationService, TwitchPointRewardService rewardService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            StatusText = "Checking Auth status";
            CreateRewardCommand = ReactiveCommand.CreateFromTask(CreateReward,
                Observable.CombineLatest(IsBusyObservable, this.WhenAny(x => x.IsAuthed, x => x.Value), (isBusy, isAuthed) => isBusy && isAuthed));
            DeleteRewardCommand = ReactiveCommand.CreateFromTask<TwitchReward>(DeleteReward, IsBusyObservable);

            _rewardService = rewardService;
        }

        public MainWindowViewModel() : this(
            DesignTimeServices.Services.GetRequiredService<ILogger<MainWindowViewModel>>(),
            DesignTimeServices.Services.GetRequiredService<IAuthenticationService>(),
            DesignTimeServices.Services.GetRequiredService<TwitchPointRewardService>())
        { }

        private async Task CreateReward()
        {
            IsBusy = true;
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var reward = await new CreateRewardWindow()
                {
                    DataContext = Avalonia.Application.Current.CreateInstance<CreateRewardWindowViewModel>(),
                }.ShowDialog<TwitchReward>(desktop.MainWindow!);
                if (reward != null)
                {
                    Rewards.Add(reward);
                }
            }
            IsBusy = false;
        }


        private async Task DeleteReward(TwitchReward reward)
        {
            IsBusy = true;
            Rewards.Remove(reward);
            await _rewardService.DeleteReward(reward);
            IsBusy = false;
        }

        public async Task LoadRewards()
        {
            IsBusy = true;
            Rewards.AddRange(await _rewardService.GetAll().ToListAsync());
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
    }
}