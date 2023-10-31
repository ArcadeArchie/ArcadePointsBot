using AvaloniaApplication1.Data.Abstractions.Repositories;
using AvaloniaApplication1.Data.Contexts;
using AvaloniaApplication1.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Data.Repositories
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
