using ArcadePointsBot.Data.Contexts;
using ArcadePointsBot.Domain.Rewards;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ArcadePointsBot.Data.Repositories
{
    internal class RewardRepository : DataEntityRepository<TwitchReward, string>, IRewardRepository
    {
        public RewardRepository(ApplicationDbContext context) : base(context)
        {
        }

        
        public new TwitchReward? Find(string id) {
            return _entities.Find(id);
        }
        public new Task<TwitchReward?> FindAsync(string id) {
            return _entities.Include(x => x.KeyboardActions).Include(x => x.MouseActions).FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
