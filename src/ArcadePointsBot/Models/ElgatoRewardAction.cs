using ArcadePointsBot.Util;
using ArcadePointsBot.ViewModels;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Models;
public class ElgatoRewardAction : RewardAction<ElgatoActionType>
{
    /// <summary>
    /// New value to be applied.
    /// </summary>
    /// <remarks>
    /// Should only be set if <see cref="ElgatoActionType.ChangeBrightness"/> or <see cref="ElgatoActionType.ChangeTemperature"/> is used
    /// </remarks>
    public int? ChangeValue { get; set; }


    public ElgatoRewardAction() { }
    public ElgatoRewardAction(string id, int index, TwitchReward reward, ElgatoActionType actionType, int? duration = null, int? changeValue = null)
    {
        Id = id;
        Index = index;
        Duration = duration;
        Reward = reward;
        ActionType = actionType;
        ChangeValue = changeValue;
    }

    public static ElgatoRewardAction FromVMType(TwitchReward reward, RewardActionViewModel actionVM)
    {
        if (actionVM is ElgatoRewardActionVM vm)
        {
            return new(Guid.NewGuid().ToString(), vm.Index, reward, (ElgatoActionType)actionVM.ActionKeyType!, vm.Duration, vm.ChangeValue);
        }
        var actionType = (ElgatoActionType)actionVM.ActionKeyType!;
        return new(Guid.NewGuid().ToString(), actionVM.Index, reward, actionType, actionVM.Duration);
    }
}

public class ElgatoRewardActionVM : RewardActionViewModel
{
    [Reactive] public int? ChangeValue { get; set; }
    [Reactive] public new ElgatoActionType ActionKeyType { get; set; }
    public ElgatoRewardActionVM()
    {

    }

    public ElgatoRewardActionVM(int index)
    {
        Index = index;
    }
}
