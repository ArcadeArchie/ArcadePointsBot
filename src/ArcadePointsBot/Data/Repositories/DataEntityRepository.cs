using ArcadePointsBot.Data.Abstractions;
using ArcadePointsBot.Data.Abstractions.Repositories;
using ArcadePointsBot.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Data.Repositories
{
    internal class DataEntityRepository<T, TId> : IEntityRepository<T, TId> where T : class, IEntity<TId>
    {
        protected readonly DbSet<T> _entities;
        protected readonly ApplicationDbContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public DataEntityRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = context.Set<T>();
        }

        public void Add(T entity)
        {
            _entities.Add(entity);
        }

        public async ValueTask AddAsync(T entity)
        {
            await _entities.AddAsync(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _entities.AddRange(entities);
        }

        public Task AddRangeAsync(IEnumerable<T> entities)
        {
            return _entities.AddRangeAsync(entities);
        }

        public void Delete(T entity)
        {
            _entities.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _entities.RemoveRange(entities);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return _entities.Any(predicate);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return _entities.AnyAsync(predicate);
        }

        public IQueryable<T> GetAll()
        {
            return GetEntities();
        }

        public IQueryable<T> GetBy(Expression<Func<T, bool>> predicate)
        {
            return GetEntities().Where(predicate);
        }

        public T? GetFirst(Expression<Func<T, bool>> predicate)
        {
            return GetEntities().FirstOrDefault(predicate);
        }

        public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate)
        {
            return await GetEntities().FirstOrDefaultAsync(predicate);
        }

        public T? GetSingle(Expression<Func<T, bool>> predicate)
        {
            return GetEntities().SingleOrDefault(predicate);
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate)
        {
            return await GetEntities().SingleOrDefaultAsync(predicate);
        }

        public void Update(T entity)
        {
            var dbEntry = _context.Entry(entity);
            dbEntry.CurrentValues.SetValues(entity);
            dbEntry.State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public T? Find(TId id)
        {
            return _entities.Find(id);
        }

        public async Task<T?> FindAsync(TId id)
        {
            return await _entities.FindAsync(id);
        }

        private IQueryable<T> GetEntities(bool asNoTracking = true)
        {
            if (asNoTracking)
                return _entities.AsNoTracking();
            return _entities;
        }
    }
}
