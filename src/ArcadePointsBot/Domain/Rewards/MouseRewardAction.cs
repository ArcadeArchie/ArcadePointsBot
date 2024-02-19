using Avalonia.Input;
using System;
using ArcadePointsBot.ViewModels;

namespace ArcadePointsBot.Domain.Rewards;

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
