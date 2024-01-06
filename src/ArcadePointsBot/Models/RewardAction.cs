using ArcadePointsBot.Data.Abstractions;
using System;
using System.ComponentModel;

namespace ArcadePointsBot.Models;
public abstract class RewardAction : IEntity<string>
{
    public string Id { get; set; } = null!;
    public int Index { get; set; }
    public TwitchReward Reward { get; set; } = null!;
    public int? Duration { get; set; }
}
public abstract class RewardAction<TType> : RewardAction
    where TType : struct, Enum
{
    public TType ActionType { get; set; }
}
public abstract class RewardAction<TType, TKey> : RewardAction<TType>
    where TType : struct, Enum
    where TKey : struct, Enum
{
    public TKey ActionKey { get; set; }
}

public enum ActionType
{
    [Description("ActionType_Elgato")]
    Elgato,
    [Description("ActionType_Keyboard")]
    Keyboard,
    [Description("ActionType_Mouse")]
    Mouse
}

public enum ElgatoActionType
{
    [Description("ElgatoActionType_TurnOn")]
    TurnOn,
    [Description("ElgatoActionType_TurnOff")]
    TurnOff,
    [Description("ElgatoActionType_ChangeBrightness")]
    ChangeBrightness,
    [Description("ElgatoActionType_ChangeTemperature")]
    ChangeTemperature
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