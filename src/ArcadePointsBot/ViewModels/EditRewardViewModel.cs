using ArcadePointsBot.Models;
using ArcadePointsBot.Services;
using ArcadePointsBot.Util;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

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

        Actions = new(new List<RewardActionViewModel>()
                .Concat(reward.KeyboardActions.Select(RewardActionViewModel.FromAction))
                .Concat(reward.ElgatoActions.Select(RewardActionViewModel.FromAction))
                .Concat(reward.MouseActions.Select(RewardActionViewModel.FromAction)).OrderBy(x => x.Index).ToList());
        Actions.ToObservableChangeSet(x => x).ToCollection().Select(x => x.Any()).ToPropertyEx(this, x => x.HasActions);

        EditTwitchRewardCommand = ReactiveCommand.CreateFromTask(EditTwitchReward,
            Observable.CombineLatest(
                IsBusyObservable,
                this.WhenAnyValue(x => x.Title, x => x.Cost, x => x.HasActions, (title, cost, hasActions) =>
                !string.IsNullOrEmpty(title) && cost >= 10 && hasActions),
                (isBusy, a) => isBusy && a));
        AddActionCommand = ReactiveCommand.Create(() => Actions.Add(new RewardActionViewModel(Actions.Count)));
        DupeActionCommand = ReactiveCommand.Create<RewardActionViewModel>(DuplicateAction);
        RemoveActionCommand = ReactiveCommand.Create<RewardActionViewModel>(action => Actions.Remove(action));

        Actions.CollectionChanged += Actions_CollectionChanged;
    }

    private void Actions_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems != null)
            foreach (RewardActionViewModel oldItem in args.OldItems)
                oldItem.PropertyChanged -= ActionItemChanged;
        if (args.NewItems != null)
            foreach (RewardActionViewModel newItem in args.NewItems)
                newItem.PropertyChanged += ActionItemChanged;
    }

    private void ActionItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RewardActionViewModel.ActionType))
            return;
        if (sender is RewardActionViewModel vm)
        {
            Actions.Remove(vm);
            Actions.Add(vm.ActionType switch
            {
                ActionType.Elgato => new ElgatoRewardActionVM(vm.Index)
                {
                    Id = vm.Id,
                    ActionType = ActionType.Elgato,
                    ActionKeyType = vm.ActionKeyType is null ? ElgatoActionType.TurnOn : (ElgatoActionType)vm.ActionKeyType,
                    Duration = vm.Duration,
                },
                _ => vm
            });
        }
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