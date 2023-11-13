using Avalonia.Input;
using ArcadePointsBot.Util;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections;
using ArcadePointsBot.ViewModels;

namespace ArcadePointsBot.Models;

public class MouseRewardAction : RewardAction<MouseActionType, MouseButton>
{
    public MouseRewardAction() { }
    public MouseRewardAction(string id, int index, TwitchReward reward, int? duration, MouseActionType actionType, MouseButton actionKey)
    {
        Id = id;
        Index = index;
        Duration = duration;
        Reward = reward;
        ActionType = actionType;
        ActionKey = actionKey;
    }

    public static MouseRewardAction FromVMType(TwitchReward reward, RewardActionViewModel actionVM)
    {        
        var actionType = (MouseActionType)actionVM.ActionKeyType!;
        var key = (MouseButton)actionVM.ActionKey!;
        return new(Guid.NewGuid().ToString(), actionVM.Index, reward, actionVM.Duration, actionType, key);
    }
}

public class MouseRewardActionVM : ReactiveObject
{
    public IEnumerable ActionValues { get; } = EnumUtils.GetValues<MouseActionType>();
    public IEnumerable KeyValues { get; } = EnumUtils.GetValues<MouseButton>();

    [Reactive] public int? Duration { get; set; }
    [Reactive] public MouseActionType? ActionType { get; set; }
    [Reactive] public MouseButton? ActionKey { get; set; }
    public int Index { get; set; }
    public MouseRewardActionVM()
    {

    }

    public MouseRewardActionVM(int index)
    {
        Index = index;
    }
}
