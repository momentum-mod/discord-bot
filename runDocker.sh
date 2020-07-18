echo -e '-= Stopping MMOD Discord Bot Container =-\n'
docker container stop momentum-discord-production

echo -e '-= Pulling MMOD Discord Bot Image from GitHub Packages =-\n'
docker pull docker.pkg.github.com/momentum-mod/discord-bot/mmod-discord-bot:net-core

echo -e '-= Runnning the MMOD Discord Bot Image =-\n'
docker run -v $PWD/config:/app/config --network host --restart on-failure:5 --name "momentum-discord-production" -d docker.pkg.github.com/momentum-mod/discord-bot/mmod-discord-bot:net-core