using Avalonia.Input;
using ArcadePointsBot.Data.Abstractions;
using ArcadePointsBot.Util;
using ArcadePointsBot.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;

namespace ArcadePointsBot.Models;
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
    Keyboard,
    Mouse
}

public enum KeyboardActionType
{
    Tap,
    Press,
    Release
}
public enum MouseActionType
{
    Click,
    DoubleClick,
    Press,
    Release
}