echo -e '-= Stopping momentum-discord Production Container =-\n'
docker container stop momentum-discord-production

echo -e '-= Removing Old momentum-discord Production Container =-\n'
docker container rm momentum-discord-production

echo -e '-= Building Docker Image from Dockerfile =-\n'
docker build -t momentum-discord -f ./MomentumDiscordBot/Dockerfile ./MomentumDiscordBot

echo -e '-= Running the Image =-\n'
docker run -v $PWD/config:/app/config --network host --restart on-failure:5 --name "momentum-discord-production" -d momentum-discord
