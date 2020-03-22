echo '-= Stopping momentum-discord Production Container =-'
docker container stop momentum-discord-production

echo '-= Removing Old momentum-discord Production Container =-'
docker container rm momentum-discord-production

echo '-= Building Docker Image from Dockerfile =-\n'
docker build -t momentum-discord -f ./src/MomentumDiscordBot/MomentumDiscordBot/Dockerfile .

echo '-= Runnning the Image =-\n'
docker run -v $PWD/config:/app/config --network host --restart on-failure:5 --name "momentum-discord-production" -d momentum-discord
