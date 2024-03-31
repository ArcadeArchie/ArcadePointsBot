using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcadePointsBot.Data.Abstractions;
using ArcadePointsBot.Domain.Rewards;
using ArcadePointsBot.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArcadePointsBot.Data.Contexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<TwitchReward> Rewards { get; set; }
    public DbSet<KeyboardRewardAction> Actions { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TwitchReward>().HasMany(x => x.KeyboardActions).WithOne(x => x.Reward);
        base.OnModelCreating(modelBuilder);
    }

    public IDbContextTransaction BeginTransaction() => Database.BeginTransaction();

    public Task<IDbContextTransaction> BeginTransactionAsync() => Database.BeginTransactionAsync();

    public void CommitTransaction() => Database.CommitTransaction();

    public Task CommitTransactionAsync() => Database.CommitTransactionAsync();
}
