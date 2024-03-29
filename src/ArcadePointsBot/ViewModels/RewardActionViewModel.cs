﻿using ReactiveUI;
using ArcadePointsBot.Util;
using System.Collections;
using Avalonia.Input;
using ReactiveUI.Fody.Helpers;
using System;
using ArcadePointsBot.Domain.Rewards;

namespace ArcadePointsBot.ViewModels;

public class RewardActionViewModel : ReactiveObject
{
    public string? Id { get; init; }
    [Reactive] public int? Duration { get; set; }
    [Reactive] public ActionType? ActionType { get; set; }
    [Reactive] public Enum? ActionKeyType { get; set; }
    [Reactive] public Enum? ActionKey { get; set; }
    public int Index { get; set; }


    public RewardActionViewModel()
    {

    }

    public RewardActionViewModel(int index)
    {
        Index = index;
    }

    internal static RewardActionViewModel FromAction(RewardAction action)
    {
        switch (action)
        {
            case MouseRewardAction actual:
                return new RewardActionViewModel(action.Index)
                {
                    Id = action.Id,
                    Duration = action.Duration,
                    ActionType = Domain.Rewards.ActionType.Mouse,
                    ActionKeyType = actual.ActionType,
                    ActionKey = actual.ActionKey,
                };
            case KeyboardRewardAction actual:
                return new RewardActionViewModel(action.Index)
                {
                    Id = action.Id,
                    Duration = action.Duration,
                    ActionType = Domain.Rewards.ActionType.Keyboard,
                    ActionKeyType = actual.ActionType,
                    ActionKey = actual.ActionKey,
                };
            default:
                throw new InvalidOperationException("Action type not supported");
        }
    }
}
