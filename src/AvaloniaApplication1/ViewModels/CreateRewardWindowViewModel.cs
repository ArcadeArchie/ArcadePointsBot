using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Util;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication1.ViewModels
{
    public partial class CreateRewardWindowViewModel : ViewModelBase
    {
        private readonly TwitchPointRewardService _pointRewardService;

        [Reactive]
        public string? Title { get; set; }

        [Reactive]
        public int Cost { get; set; } = 10;

        [Reactive]
        public bool RequireInput { get; set; }

        public ObservableCollection<RewardActionViewModel> Actions { get; } = new ObservableCollection<RewardActionViewModel>();

        public ReactiveCommand<Unit, TwitchReward?> CreateTwitchRewardCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddActionCommand { get; set; }
        public ReactiveCommand<RewardActionViewModel, Unit> DupeActionCommand { get; set; }

        public ReactiveCommand<RewardActionViewModel, Unit> RemoveActionCommand { get; set; }

        [ObservableAsProperty]
        public bool HasActions { get; }

        public CreateRewardWindowViewModel(TwitchPointRewardService pointRewardService)
        {
            _pointRewardService = pointRewardService;
            Actions.ToObservableChangeSet(x => x).ToCollection().Select(x => x.Any()).ToPropertyEx(this, x => x.HasActions);
            CreateTwitchRewardCommand = ReactiveCommand.CreateFromTask(CreateTwitchReward,
                Observable.CombineLatest(
                    IsBusyObservable,
                    this.WhenAnyValue(x => x.Title, x => x.Cost, x => x.HasActions, (title, cost, hasActions) =>
                    !string.IsNullOrEmpty(title) && cost >= 10 && hasActions),
                    (isBusy, a) =>
                    isBusy && a));
            AddActionCommand = ReactiveCommand.Create(() => Actions.Add(new RewardActionViewModel(Actions.Count)));
            DupeActionCommand = ReactiveCommand.Create<RewardActionViewModel>(DuplicateAction);
            RemoveActionCommand = ReactiveCommand.Create<RewardActionViewModel>(action => Actions.Remove(action));
        }


        public CreateRewardWindowViewModel() : this(DesignTimeServices.Services.GetRequiredService<TwitchPointRewardService>())
        {
        }
        void DuplicateAction(RewardActionViewModel action)
        {
            var dupedAction = new RewardActionViewModel(Actions.Count)
            {
                ActionType = action.ActionType,
                ActionKeyType = action.ActionKeyType,
                ActionKey = action.ActionKey,
                Duration = action.Duration,
            };
            Actions.Add(dupedAction);
        }

        Task<TwitchReward?> CreateTwitchReward()
        {
            return _pointRewardService
                .CreateReward(Title!, Cost, RequireInput, Actions);
            return null;
        }
    }
}
