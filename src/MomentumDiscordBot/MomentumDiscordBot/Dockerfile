FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY /src/MomentumDiscordBot/MomentumDiscordBot/MomentumDiscordBot.csproj .
RUN dotnet restore MomentumDiscordBot.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

COPY /config /app/config

ENTRYPOINT ["dotnet", "MomentumDiscordBot.dll"]
