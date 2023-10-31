using ReactiveUI;
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
    }
}
