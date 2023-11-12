using Avalonia.Input;
using AvaloniaApplication1.Util;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Models;

public class KeyboardRewardAction : RewardAction<KeyboardActionType, Key>
{

    public KeyboardRewardAction() { }
    public KeyboardRewardAction(string id, int index, TwitchReward reward, int? duration, KeyboardActionType actionType, Key actionKey)
    {
        Id = id;
        Index = index;
        Duration = duration;
        Reward = reward;
        ActionType = actionType;
        ActionKey = actionKey;
    }

    public static KeyboardRewardAction FromVMType(TwitchReward reward, RewardActionViewModel actionVM)
    {
        var actionType = (KeyboardActionType)actionVM.ActionKeyType!;
        var key = (Key)actionVM.ActionKey!;
        return new(Guid.NewGuid().ToString(), actionVM.Index, reward, actionVM.Duration, actionType, key);
    }
}

public class KeyboardRewardActionVM : ReactiveObject
{
    public IEnumerable ActionValues { get; } = EnumUtils.GetValues<KeyboardActionType>();
    public IEnumerable KeyValues { get; } = EnumUtils.GetValues<Key>();

    [Reactive] public int? Duration { get; set; }
    [Reactive] public KeyboardActionType? ActionType { get; set; }
    [Reactive] public Key? ActionKey { get; set; }
    public int Index { get; set; }
    public KeyboardRewardActionVM()
    {

    }

    public KeyboardRewardActionVM(int index)
    {
        Index = index;
    }
}