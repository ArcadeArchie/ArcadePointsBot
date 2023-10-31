using AvaloniaApplication1.Data.Abstractions;
using System.Collections.Generic;
using System.ComponentModel;
using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace AvaloniaApplication1.Models
{
    public class TwitchReward : IEntity<string>, INotifyPropertyChanged
    {
        public string Id { get; init; } = null!;
        public string Title { get; init; } = null!;
        public int Cost { get; init; }
        public bool RequireInput { get; init; }
        public bool IsEnabled { get; set; }

        public IList<KeyboardRewardAction> KeyboardActions { get; init; } = new List<KeyboardRewardAction>();
        public IList<MouseRewardAction> MouseActions { get; init; } = new List<MouseRewardAction>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public static TwitchReward FromRemote(CustomReward remoteReward) => new TwitchReward
        {
            Id = remoteReward.Id,
            Title = remoteReward.Title,
            Cost = remoteReward.Cost,
            RequireInput = remoteReward.IsUserInputRequired
        };
    }
}
