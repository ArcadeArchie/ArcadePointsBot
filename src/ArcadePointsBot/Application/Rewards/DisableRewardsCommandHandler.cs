using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArcadePointsBot.Common.Abstractions.Messaging;
using ArcadePointsBot.Common.Primitives;
using ArcadePointsBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArcadePointsBot.Application.Rewards;

public record DisableRewardsCommand() : ICommand<Result>;

public class DisableRewardsCommandHandler : ICommandHandler<DisableRewardsCommand, Result>
{
    private ILogger<DisableRewardsCommandHandler> _logger;
    private TwitchPointRewardService _rewardService;

    public DisableRewardsCommandHandler(
        ILogger<DisableRewardsCommandHandler> logger,
        TwitchPointRewardService rewardService
    )
    {
        _logger = logger;
        _rewardService = rewardService;
    }

    public async ValueTask<Result> Handle(
        DisableRewardsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var rewards = await _rewardService.GetAll().ToListAsync();
            var toDisable = rewards
                .Where(x => x.IsEnabled)
                .Select(x =>
                {
                    x.IsEnabled = false;
                    return x;
                })
                .ToList();
            await _rewardService.BulkUpdateRewards(
                toDisable,
                u => u.SetProperty(p => p.IsEnabled, false)
            );
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to disable rewards");
            return Result.Failure(new Error("", ex.Message));
        }
    }
}
