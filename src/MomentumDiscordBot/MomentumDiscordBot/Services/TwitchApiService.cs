using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MomentumDiscordBot.Constants;
using TwitchLib.Api;
using Stream = TwitchLib.Api.Helix.Models.Streams.Stream;

namespace MomentumDiscordBot.Services
{
    public class TwitchApiService
    {
        private readonly TwitchAPI _apiService;
        public TwitchApiService()
        {
            if (File.Exists(PathConstants.DiscordTokenFilePath))
            {
                // File exists, get the text
                var twitchClientId = File.ReadAllText(PathConstants.TwitchAPIClientIdFilePath);
                _apiService = new TwitchAPI();
                _apiService.Settings.ClientId = twitchClientId;
            }
            else
            {
                throw new Exception($"No Twitch API token file exists, expected it at: '{PathConstants.TwitchAPIClientIdFilePath}'");
            }
        }

        public async Task<string> GetMomentumModIdAsync()
        {
            var games = await _apiService.Helix.Games.GetGamesAsync(gameNames: new List<string> {"Momentum Mod"});
            return games.Games.First().Id;
        }
        public async Task<List<Stream>> GetLiveMomentumModStreamersAsync()
        {
            var streams = await _apiService.Helix.Streams.GetStreamsAsync(gameIds: new List<string>
                {await GetMomentumModIdAsync()});
            return streams.Streams.ToList();
        }

        public async Task<string> GetStreamerIconUrlAsync(string id)
        {
            var users = await _apiService.Helix.Users.GetUsersAsync(ids: new List<string> {id});

            // Selected through ID, should only return one
            var user = users.Users.First();
            return user.ProfileImageUrl;
        }
    }
}
