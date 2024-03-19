using ArcadePointsBot.Common.Primitives;

namespace ArcadePointsBot.Errors
{
    public static class TwitchAPI
    {
        public static Error BadCredentials => new(
            "TTV.API.BadCreds",
            "The Credentials are invalid, resetting them and restarting the bot is recommended");
        public static Error BadRewardTitle => new(
            "TTV.API.BadTitle",
            "The Reward Title was disallowed by twitch, try a differend one");
        public static Error DupeRewardTitle => new(
            "TTV.API.DupeTitle",
            "The Reward with this title already exists, try a differend one or delete the duplicate");
    }
    public static class Database
    {
        public static Error NotFound => new(
            "DB.404",
            "Entry not found in the database");
    }
}
