using ArcadePointsBot.Domain.Rewards;
using ArcadePointsBot.Util;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections;
using Avalonia.Input;

namespace ArcadePointsBot.ViewModels;

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
