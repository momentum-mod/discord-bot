# Momentum Mod Discord Bot

![Momentum Mod](https://momentum-mod.org/assets/images/logo.svg)

> A Discord bot for Momentum Mod's Official Discord, running in a [dockerized](https://www.docker.com/) [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) container, using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

![.NET Core](https://github.com/momentum-mod/discord-bot/workflows/.NET%20Core/badge.svg?branch=net-core)

## Purpose

The bot is used to manage and accompany the Discord server:

* Monitor Twitch livestreams playing Momentum Mod
* Get custom notification roles
* Force users to read the FAQ

## Dependencies

* [Docker Compose V3.8+](https://docs.docker.com/compose/install/)

## Dev Setup

Firstly, you will need to make a test Discord server with the various roles and channels used by the bot.

Then, clone the repo using a CLI.

1. Navigate to the root folder: `cd discord-bot`
2. Copy env.TEMPLATE to .env.dev: `cp env.TEMPLATE .env.dev`
3. In config/, copy config.template.json.TEMPLATE to config.json: `cd config/ && cp config.json.TEMPLATE config.json`
4. Fill out the config.json file with your test server's settings. Your `mysql_connection_string` should correspond with
   the MySQL env vars in .env.dev
5. Build and run the Docker containers using Docker Compose with `docker-compose up -d`. For testing changes, you'll
   need to rebuild with `docker-compose build`.

## Contributing

Contributions are welcome, though we encourage you either tackle existing issues or ask about ideas in
the [Momentum Mod Discord server](https://discord.gg/momentummod) first.

Whilst we originally planned to include integrations with Momentum Mod's official API for stat tracking, WR
announcements, etc., we've since decided to move that work to
a [separate repository](https://github.com/momentum-mod/discord-bot-public). Therefore we expect to make relatively few
additions in the future, mostly just small housekeeping features.

