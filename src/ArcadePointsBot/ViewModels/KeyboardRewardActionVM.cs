using System.Collections;
using ArcadePointsBot.Domain.Rewards;
using ArcadePointsBot.Util;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ArcadePointsBot.ViewModels;

public class KeyboardRewardActionVM : ReactiveObject
{
    public IEnumerable ActionValues { get; } = EnumUtils.GetValues<KeyboardActionType>();
    public IEnumerable KeyValues { get; } = EnumUtils.GetValues<Key>();

    [Reactive]
    public int? Duration { get; set; }

    [Reactive]
    public KeyboardActionType? ActionType { get; set; }

    [Reactive]
    public Key? ActionKey { get; set; }
    public int Index { get; set; }

    public KeyboardRewardActionVM() { }

    public KeyboardRewardActionVM(int index)
    {
        Index = index;
    }
}
