using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MomentumDiscordBot.Constants;
using TwitchLib.Api;
using Stream = TwitchLib.Api.Helix.Models.Streams.Stream;

namespace MomentumDiscordBot.Services
{
    public class TwitchApiService
    {
        private readonly TwitchAPI _apiService;
        private string _momentumModGameId = null;

        public TwitchApiService()
        {
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
                    $"No Twitch API token file exists, expected it at: '{PathConstants.TwitchAPIClientIdFilePath}'");
            }
        }

        public async Task<string> GetMomentumModIdAsync()
        {
            var games = await _apiService.Helix.Games.GetGamesAsync(gameNames: new List<string> {"Momentum Mod"});
            return games.Games.First().Id;
        }

        public async Task<List<Stream>> GetLiveMomentumModStreamersAsync()
        {
            // Get the game ID once, then reuse it
            var streams = await _apiService.Helix.Streams.GetStreamsAsync(gameIds: new List<string>
                {_momentumModGameId ?? await GetMomentumModIdAsync()});
            return streams.Streams.ToList();
        }

        public async Task<string> GetStreamerIconUrlAsync(string id)
        {
            var users = await _apiService.Helix.Users.GetUsersAsync(new List<string> {id});

            // Selected through ID, should only return one
            var user = users.Users.First();
            return user.ProfileImageUrl;
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
            var response = await _apiService.Helix.Users.GetUsersAsync(ids: new List<string> { id });
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
    }
}