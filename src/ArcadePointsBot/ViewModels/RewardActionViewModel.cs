﻿using ReactiveUI;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Util;
using System.Collections;
using Avalonia.Input;
using ReactiveUI.Fody.Helpers;
using System;

namespace AvaloniaApplication1.ViewModels
{
    public class RewardActionViewModel : ReactiveObject
    {

        public IEnumerable ActionValues { get; } = EnumUtils.GetValues<ActionType>();
        public IEnumerable MouseActionValues { get; } = EnumUtils.GetValues<MouseActionType>();
        public IEnumerable KeyboardActionValues { get; } = EnumUtils.GetValues<KeyboardActionType>();
        public IEnumerable MouseValues { get; } = EnumUtils.GetValues<MouseButton>();
        public IEnumerable KeyboardValues { get; } = EnumUtils.GetValues<Key>();
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
                        ActionType = Models.ActionType.Mouse,
                        ActionKeyType = actual.ActionType,
                        ActionKey = actual.ActionKey,
                    };
                case KeyboardRewardAction actual:
                    return new RewardActionViewModel(action.Index)
                    {
                        Id = action.Id,
                        Duration = action.Duration,
                        ActionType = Models.ActionType.Keyboard,
                        ActionKeyType = actual.ActionType,
                        ActionKey = actual.ActionKey,
                    };
                default:
                    throw new InvalidOperationException("Action type not supported");
            }
        }
    }
}