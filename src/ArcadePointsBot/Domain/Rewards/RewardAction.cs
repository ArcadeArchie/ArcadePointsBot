using System;
using System.ComponentModel;

namespace ArcadePointsBot.Domain.Rewards;

public abstract class RewardAction : IEntity<string>
{
    public string Id { get; set; } = null!;
    public int Index { get; set; }
    public TwitchReward Reward { get; set; } = null!;
    public int? Duration { get; set; }
}

public abstract class RewardAction<TType, TKey> : RewardAction
    where TType : struct, Enum
    where TKey : struct, Enum
{
    public TType ActionType { get; set; }
    public TKey ActionKey { get; set; }
}

public enum ActionType
{
    [Description("ActionType_Keyboard")]
    Keyboard,

    [Description("ActionType_Mouse")]
    Mouse
}

public enum KeyboardActionType
{
    [Description("KeyboardActionType_Tap")]
    Tap,

    [Description("KeyboardActionType_Press")]
    Press,

    [Description("KeyboardActionType_Release")]
    Release
}

public enum MouseActionType
{
    [Description("MouseActionType_Click")]
    Click,

    [Description("MouseActionType_DoubleClick")]
    DoubleClick,

    [Description("KeyboardActionType_Press")]
    Press,

    [Description("KeyboardActionType_Release")]
    Release
}
