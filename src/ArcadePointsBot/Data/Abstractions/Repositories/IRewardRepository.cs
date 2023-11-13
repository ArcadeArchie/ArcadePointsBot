using ArcadePointsBot.Models;

namespace ArcadePointsBot.Data.Abstractions.Repositories
{
    public interface IRewardRepository : IEntityRepository<TwitchReward, string>
    {
    }
}
