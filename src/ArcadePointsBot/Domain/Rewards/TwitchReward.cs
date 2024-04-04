using System.Collections.Generic;
using System.ComponentModel;
using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace ArcadePointsBot.Domain.Rewards
{
    public class TwitchReward : IEntity<string>, INotifyPropertyChanged
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Category { get; set; }
        public int Cost { get; set; }
        public bool RequireInput { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        public IList<KeyboardRewardAction> KeyboardActions { get; init; } =
            new List<KeyboardRewardAction>();
        public IList<MouseRewardAction> MouseActions { get; init; } = new List<MouseRewardAction>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public static TwitchReward FromRemote(CustomReward remoteReward) =>
            new TwitchReward
            {
                Id = remoteReward.Id,
                Title = remoteReward.Title,
                Cost = remoteReward.Cost,
                RequireInput = remoteReward.IsUserInputRequired
            };
    }
}
