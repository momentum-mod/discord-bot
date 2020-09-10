using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MomentumDiscordBot.Constants;
using Serilog;
using TwitchLib.Api;
using Stream = TwitchLib.Api.Helix.Models.Streams.Stream;

namespace MomentumDiscordBot.Services
{
    public class TwitchApiService
    {
        private readonly TwitchAPI _apiService;
        private readonly ILogger _logger;
        private readonly string _momentumModGameId = null;
        private ConcurrentDictionary<string, string> _categoryNames = new ConcurrentDictionary<string, string>();

        public TwitchApiService(ILogger logger)
        {
            _logger = logger;

            if (File.Exists(PathConstants.TwitchAPIClientIdFilePath))
            {
                // File exists, get the text
                var twitchClientId = File.ReadAllText(PathConstants.TwitchAPIClientIdFilePath);
                _apiService = new TwitchAPI();
                _apiService.Settings.ClientId = twitchClientId;
            }
            else
            {
                throw new Exception(
                    $"No Twitch API token file exists, expected it at: '{PathConstants.TwitchAPIClientIdFilePath}'");
            }

            if (File.Exists(PathConstants.TwitchAPIClientSecretFilePath))
            {
                // File exists, get the text
                var secret = File.ReadAllText(PathConstants.TwitchAPIClientSecretFilePath);
                _apiService.Settings.Secret = secret;
            }
            else
            {
                throw new Exception(
                    $"No Twitch API secret file exists, expected it at: '{PathConstants.TwitchAPIClientSecretFilePath}'");
            }
        }

        public List<Stream> PreviousLivestreams { get; set; }

        public async Task<string> GetMomentumModIdAsync()
        {
            var games = await _apiService.Helix.Games.GetGamesAsync(gameNames: new List<string> {"Momentum Mod"});
            return games.Games.First().Id;
        }

        public async Task<string> GetGameNameAsync(string id)
        {
            if (_categoryNames.TryGetValue(id, out var result))
            {
                return result;
            }


            var game = await _apiService.Helix.Games.GetGamesAsync(gameIds: new List<string> { id });

            _categoryNames.TryAdd(id, game.Games.First().Name);
            return game.Games.First().Name;
        }

        public async Task<List<Stream>> GetLiveMomentumModStreamersAsync()
        {
            try
            {
                // Get the game ID once, then reuse it
                var streams = await _apiService.Helix.Streams.GetStreamsAsync(gameIds: new List<string>
                    {_momentumModGameId ?? await GetMomentumModIdAsync()});
                return streams.Streams.ToList();
            }
            catch (Exception e)
            {
                _logger.Error(e, "TwitchApiService");
                return null;
            }
        }

        public async Task<string> GetStreamerIconUrlAsync(string id)
        {
            try
            {
                var users = await _apiService.Helix.Users.GetUsersAsync(new List<string> {id});

                // Selected through ID, should only return one
                var user = users.Users.First();
                return user.ProfileImageUrl;
            }
            catch (Exception e)
            {
                _logger.Error(e, "TwitchApiService");
                return string.Empty;
            }
        }

        public async Task<string> GetStreamerIDAsync(string name)
        {
            var response = await _apiService.Helix.Users.GetUsersAsync(logins: new List<string> {name});
            var users = response.Users;

            if (users.Length == 0)
            {
                throw new Exception("No user was found for that input");
            }

            if (users.Length > 1)
            {
                throw new Exception("More than one user was found for that input");
            }

            return users.First().Id;
        }

        public async Task<string> GetStreamerNameAsync(string id)
        {
            var response = await _apiService.Helix.Users.GetUsersAsync(new List<string> {id});
            var users = response.Users;

            if (users.Length == 0)
            {
                throw new Exception("No user was found for that input");
            }

            if (users.Length > 1)
            {
                throw new Exception("More than one user was found for that input");
            }

            return users.First().DisplayName;
        }

        public async Task<string> GetOrDownloadTwitchIDAsync(string username)
        {
            if (ulong.TryParse(username, out _))
            {
                // Input is a explicit Twitch ID
                return username;
            }

            // Input is the Twitch username
            var cachedUser = PreviousLivestreams.FirstOrDefault(x =>
                string.Equals(username, x.UserName, StringComparison.InvariantCultureIgnoreCase));

            if (cachedUser != null)
            {
                // User is in the cache
                return cachedUser.UserId;
            }

            try
            {
                // Search the API, throws exception if not found
                return await GetStreamerIDAsync(username);
            }
            catch (Exception e)
            {
                _logger.Error(e, "TwitchApiService");
                return null;
            }
        }
    }
}