using ArcadePointsBot.Data.Abstractions.Repositories;

namespace ArcadePointsBot.Domain.Rewards
{
    public interface IRewardRepository : IEntityRepository<TwitchReward, string> { }
}
