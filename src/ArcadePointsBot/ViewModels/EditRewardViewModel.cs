using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ArcadePointsBot.Domain.Rewards;
using ArcadePointsBot.Services;
using ArcadePointsBot.Util;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ArcadePointsBot.ViewModels;

public partial class EditRewardViewModel : ViewModelBase
{
    private readonly TwitchPointRewardService _pointRewardService;
    private readonly TwitchReward _oldReward;

    [Reactive]
    public string Title { get; set; }

    [Reactive]
    public string? Category { get; set; }

    [Reactive]
    public int Cost { get; set; } = 10;

    [Reactive]
    public bool RequireInput { get; set; }

    public ObservableCollection<RewardActionViewModel> Actions { get; }

    public ReactiveCommand<Unit, TwitchReward?> EditTwitchRewardCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddActionCommand { get; set; }
    public ReactiveCommand<RewardActionViewModel, Unit> DupeActionCommand { get; set; }

    public ReactiveCommand<RewardActionViewModel, Unit> RemoveActionCommand { get; set; }

    [ObservableAsProperty]
    public bool HasActions { get; }

    public EditRewardViewModel(TwitchPointRewardService pointRewardService, TwitchReward reward)
    {
        _pointRewardService = pointRewardService;
        _oldReward = reward;
        Title = _oldReward.Title;
        Category = _oldReward.Category;
        Cost = _oldReward.Cost;
        RequireInput = _oldReward.RequireInput;

        Actions = new(
            new List<RewardActionViewModel>()
                .Concat(reward.KeyboardActions.Select(RewardActionViewModel.FromAction))
                .Concat(reward.MouseActions.Select(RewardActionViewModel.FromAction))
                .OrderBy(x => x.Index)
                .ToList()
        );
        Actions
            .ToObservableChangeSet(x => x)
            .ToCollection()
            .Select(x => x.Any())
            .ToPropertyEx(this, x => x.HasActions);

        EditTwitchRewardCommand = ReactiveCommand.CreateFromTask(
            EditTwitchReward,
            Observable.CombineLatest(
                IsBusyObservable,
                this.WhenAnyValue(
                    x => x.Title,
                    x => x.Cost,
                    x => x.HasActions,
                    (title, cost, hasActions) =>
                        !string.IsNullOrEmpty(title) && cost >= 10 && hasActions
                ),
                (isBusy, a) => isBusy && a
            )
        );
        AddActionCommand = ReactiveCommand.Create(
            () => Actions.Add(new RewardActionViewModel(Actions.Count))
        );
        DupeActionCommand = ReactiveCommand.Create<RewardActionViewModel>(DuplicateAction);
        RemoveActionCommand = ReactiveCommand.Create<RewardActionViewModel>(action =>
            Actions.Remove(action)
        );
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

    async Task<TwitchReward?> EditTwitchReward()
    {
        Errors.Clear();
        _oldReward.Title = Title;
        _oldReward.Category = Category;
        _oldReward.Cost = Cost;
        _oldReward.RequireInput = RequireInput;
        var result = await _pointRewardService.UpdateReward(_oldReward, Actions);
        if (result.IsSuccess)
            return result.Value;
        Errors.Add(result.Error);
        return null;
    }
}
