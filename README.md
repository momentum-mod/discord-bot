# Momentum Mod Discord Bot

![Momentum Mod](https://i.imgur.com/80pzbzZ.png)

> A Discord bot for Momentum Mod's Official Discord, running in a [dockerized](https://www.docker.com/) [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) container, using [Discord.NET](https://discord.foxbot.me/).

![.NET Core](https://github.com/momentum-mod/discord-bot/workflows/.NET%20Core/badge.svg?branch=net-core)

## Purpose

The bot is used to manage and accompany the Discord server:

 * Monitor Twitch livestreams playing Momentum Mod
 * Get custom notification roles
 * Force users to read the FAQ
 * [Planned] Integrate with the website's API, providing relative discord commands.

## Installing the Bot

A shell script is made to automate the process of building and running the Docker image.

1. Clone the repo using Git CLI
2. Navigate to the root folder: `cd discord-bot`
3. Make the script executable: `chmod +x runDocker.sh`
4. Make a folder called config in the root directory called config: `mkdir config`
5. Navigate to the folder: `cd config`
6. Create and fill out the following files:

   `config.json`, containing data found in the [model file](https://github.com/momentum-mod/discord-bot/blob/net-core/src/MomentumDiscordBot/MomentumDiscordBot/Models/Config.cs) in valid JSON
   
   `discordToken.txt`, enter your Discord bot token - ensure there are no newlines/whitespace
   
   `twitchApiClientId.txt`, enter your Twitch API Client ID
   
7. Navigate up to the root directory: `..`
8. Run the bot: `./runDocker.sh`

## Contributing

Pull Requests are highly appreciated! Just branch off of net-core, make your edits, and open a Pull Request into net-core describing what you changed. Pages could be edited directly through GitHub, or you can download your fork and edit through a program like [Visual Studio Code](https://code.visualstudio.com/).

If you need a guide for how to contribute, try looking at past pull-requests, or asking around in the [Momentum Mod Discord server](https://discord.gg/V4gS7Qg).
