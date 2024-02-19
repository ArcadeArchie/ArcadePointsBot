using Avalonia.Input;
using ArcadePointsBot.Util;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections;
using ArcadePointsBot.ViewModels;

namespace ArcadePointsBot.Domain.Rewards;

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