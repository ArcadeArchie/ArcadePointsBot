using ArcadePointsBot.Data.Abstractions.Repositories;
using ArcadePointsBot.Data.Contexts;
using ArcadePointsBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ArcadePointsBot.Data.Repositories
{
    internal class RewardRepository : DataEntityRepository<TwitchReward, string>, IRewardRepository
    {
        public RewardRepository(ApplicationDbContext context) : base(context)
        {
        }


        public new TwitchReward? Find(string id)
        {
            return _entities.Find(id);
        }
        public new async Task<TwitchReward?> FindAsync(string id, bool track = false)
        {
            if (track)
                return await _entities
                .Include(x => x.KeyboardActions)
                .Include(x => x.ElgatoActions)
                .Include(x => x.MouseActions)
                .FirstOrDefaultAsync(x => x.Id == id);
            var entity = await _entities.FindAsync(id);
            if (entity != null)
                _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }
    }
}
