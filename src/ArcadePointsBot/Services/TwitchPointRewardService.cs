using ArcadePointsBot.Auth;
using ArcadePointsBot.Data.Abstractions.Repositories;
using ArcadePointsBot.Models;
using ArcadePointsBot.ViewModels;
using DynamicData;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.Api.Helix.Models.ChannelPoints.CreateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace ArcadePointsBot.Services
{
    public class TwitchPointRewardService
    {
        private readonly ILogger<TwitchPointRewardService> _logger;
        private readonly TwitchAPI _apiClient;
        private readonly IAuthenticationService _twitchAuthService;
        private readonly IRewardRepository _rewardRepository;
        public TwitchPointRewardService(
            IAuthenticationService twitchAuthService, ILoggerFactory loggerFactory, IRewardRepository rewardRepository)
        {
            _twitchAuthService = twitchAuthService;
            _rewardRepository = rewardRepository;
            _logger = loggerFactory.CreateLogger<TwitchPointRewardService>();
            _apiClient = new TwitchAPI(loggerFactory);
            _apiClient.Settings.ClientId = _twitchAuthService.AuthConfig.ClientId;
            _apiClient.Settings.Secret = _twitchAuthService.AuthConfig.ClientSecret;
        }
        public async Task<Result<TwitchReward>> CreateReward(string title, string? category, int cost, bool requireInput, IList<RewardActionViewModel> actions)
        {
            try
            {
                await _twitchAuthService.EnsureValidTokenAsync();
                var res = await _apiClient.Helix.ChannelPoints.CreateCustomRewardsAsync(_twitchAuthService.AuthConfig.Uid, new CreateCustomRewardsRequest
                {
                    Title = title,
                    Cost = cost,
                    IsUserInputRequired = requireInput
                }, accessToken: _twitchAuthService.AuthConfig.AccessToken);
                var entry = res.Data.First();
                var dbEntry = new TwitchReward
                {
                    Id = entry.Id,
                    Title = entry.Title,
                    Category = category,
                    Cost = entry.Cost
                };

                dbEntry.KeyboardActions.AddRange(actions
                    .Where(x => x.ActionType == ActionType.Keyboard)
                    .Select(x => KeyboardRewardAction.FromVMType(dbEntry, x)).ToList());
                
                dbEntry.ElgatoActions.AddRange(actions
                    .Where(x => x.ActionType == ActionType.Elgato)
                    .Select(x => ElgatoRewardAction.FromVMType(dbEntry, x)).ToList());
                dbEntry.MouseActions.AddRange(actions
                    .Where(x => x.ActionType == ActionType.Mouse)
                    .Select(x => MouseRewardAction.FromVMType(dbEntry, x)).ToList());

                var transaction = _rewardRepository.UnitOfWork.BeginTransaction();
                try
                {
                    await _rewardRepository.AddAsync(dbEntry);
                    await _rewardRepository.UnitOfWork.SaveChangesAsync();

                    await _rewardRepository.UnitOfWork.CommitTransactionAsync();

                    return Result.Success(dbEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to create db entry for reward, rolling back changes");
                    await DeleteReward(dbEntry);
                    await transaction.RollbackAsync();
                }
                return Result.Failure<TwitchReward>(Error.None);
            }
            catch (BadRequestException ex)
            {
                var msg = await ex.HttpResponse.Content.ReadAsStringAsync();
                if (msg.Contains("CREATE_CUSTOM_REWARD_DUPLICATE_REWARD"))
                {
                    _logger.LogCritical(ex, "Reward update failed due to Duplicate Title, please choose a differend title");
                    return Result.Failure<TwitchReward>(Errors.TwitchAPI.DupeRewardTitle);
                }
                if (msg.Contains("CREATE_CUSTOM_REWARD_TITLE_AUTOMOD_FAILED"))
                {
                    _logger.LogCritical(ex, "Reward update failed due to the title not being allowed by twitch, please choose a differend title");
                    return Result.Failure<TwitchReward>(Errors.TwitchAPI.BadRewardTitle);
                }
                _logger.LogCritical(ex, "Reward update failed due to bad credentials, resetting the credentials and restarting the bot is recommended");
                return Result.Failure<TwitchReward>(Errors.TwitchAPI.BadCredentials);
            }
        }

        public async Task<TwitchReward> GetReward(string id)
        {
            var dbEntry = await _rewardRepository.FindAsync(id);
            if (dbEntry is null)
            {
                _logger.LogWarning("entry with id {id} not stored in local db, requesting info from twitch", id);

                await _twitchAuthService.EnsureValidTokenAsync();
                var response = await _apiClient.Helix.ChannelPoints.GetCustomRewardAsync(_twitchAuthService.AuthConfig.Uid, new List<string> { id },
                    accessToken: _twitchAuthService.AuthConfig.AccessToken);
                var remoteEntry = response.Data.First();
                dbEntry = TwitchReward.FromRemote(remoteEntry);
                await _rewardRepository.AddAsync(dbEntry);
            }
            return dbEntry;
        }

        public IQueryable<TwitchReward> GetAll() => _rewardRepository.GetAll();

        public async Task<Result<TwitchReward>> UpdateReward(TwitchReward reward, IList<RewardActionViewModel> actions)
        {
            try
            {
                await _twitchAuthService.EnsureValidTokenAsync();
                var res = await _apiClient.Helix.ChannelPoints.UpdateCustomRewardAsync(
                    _twitchAuthService.AuthConfig.Uid, reward.Id, new UpdateCustomRewardRequest
                    {
                        Title = reward.Title,
                        Cost = reward.Cost,
                        IsUserInputRequired = reward.RequireInput
                    }, accessToken: _twitchAuthService.AuthConfig.AccessToken);

                var transaction = _rewardRepository.UnitOfWork.BeginTransaction();
                var dbEntry = await _rewardRepository.FindAsync(reward.Id, true);
                if (dbEntry is null) return Result.Failure<TwitchReward>(Errors.Database.NotFound);
                dbEntry.KeyboardActions.Clear();
                dbEntry.MouseActions.Clear();
                dbEntry.KeyboardActions.AddRange(actions
                    .Where(x => x.ActionType == ActionType.Keyboard)
                    .Select(x => KeyboardRewardAction.FromVMType(dbEntry, x)).ToList());
                dbEntry.MouseActions.AddRange(actions
                    .Where(x => x.ActionType == ActionType.Mouse)
                    .Select(x => MouseRewardAction.FromVMType(dbEntry, x)).ToList());

                try
                {
                    _rewardRepository.Update(dbEntry);
                    await _rewardRepository.UnitOfWork.SaveChangesAsync();

                    await _rewardRepository.UnitOfWork.CommitTransactionAsync();

                    return Result.Success(dbEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to update db entry for reward, rolling back changes");
                    await transaction.RollbackAsync();
                }
                return Result.Failure<TwitchReward>(Error.None);
            }
            catch (BadRequestException ex)
            {
                var msg = await ex.HttpResponse.Content.ReadAsStringAsync();
                if (msg.Contains("UPDATE_CUSTOM_REWARD_DUPLICATE_REWARD"))
                {
                    _logger.LogCritical(ex, "Reward update failed due to Duplicate Title, please choose a differend title");
                    return Result.Failure<TwitchReward>(Errors.TwitchAPI.DupeRewardTitle);
                }
                if (msg.Contains("UPDATE_CUSTOM_REWARD_TITLE_AUTOMOD_FAILED"))
                {
                    _logger.LogCritical(ex, "Reward update failed due to the title not being allowed by twitch, please choose a differend title");
                    return Result.Failure<TwitchReward>(Errors.TwitchAPI.BadRewardTitle);
                }
                _logger.LogCritical(ex, "Reward update failed due to bad credentials, resetting the credentials and restarting the bot is recommended");
                return Result.Failure<TwitchReward>(Errors.TwitchAPI.BadCredentials);
            }
        }

        public async Task UpdateReward(TwitchReward reward)
        {
            _rewardRepository.Update(reward);
            await _rewardRepository.UnitOfWork.SaveChangesAsync();

            await _twitchAuthService.EnsureValidTokenAsync();
            await _apiClient.Helix.ChannelPoints.UpdateCustomRewardAsync(_twitchAuthService.AuthConfig.Uid, reward.Id, new UpdateCustomRewardRequest
            {
                IsEnabled = reward.IsEnabled,
                Title = reward.Title,
                Cost = reward.Cost,
                IsUserInputRequired = reward.RequireInput,
            }, accessToken: _twitchAuthService.AuthConfig.AccessToken);
        }
        public async Task DeleteReward(TwitchReward reward)
        {
            try
            {
                await _twitchAuthService.EnsureValidTokenAsync();
                await _apiClient.Helix.ChannelPoints
                    .DeleteCustomRewardAsync(_twitchAuthService.AuthConfig.Uid, reward.Id, accessToken: _twitchAuthService.AuthConfig.AccessToken);
            }
            catch (Exception ex) { _logger.LogCritical(ex, "Failed to delte reward from twitch"); }
            finally
            {
                _rewardRepository.Delete(reward);
                await _rewardRepository.UnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<RewardRedemption[]> GetRewardRedemptions(string rewardId, params string[] redemptionIds)
        {
            await _twitchAuthService.EnsureValidTokenAsync();
            var res = await _apiClient
                .Helix
                .ChannelPoints
                .GetCustomRewardRedemptionAsync(
                _twitchAuthService.AuthConfig.Uid,
                rewardId, new List<string>(redemptionIds), accessToken: _twitchAuthService.AuthConfig.AccessToken);

            return res.Data;
        }
        public async Task UpdateRewardRedemption(string rewardId, CustomRewardRedemptionStatus newStatus, params string[] redemptionIds)
        {
            await _twitchAuthService.EnsureValidTokenAsync();

            await _apiClient.Helix.ChannelPoints.UpdateRedemptionStatusAsync(
                _twitchAuthService.AuthConfig.Uid, rewardId,
                    new List<string>(redemptionIds),
                    new UpdateCustomRewardRedemptionStatusRequest
                    {
                        Status = newStatus
                    }, accessToken: _twitchAuthService.AuthConfig.AccessToken);
        }

    }
}
