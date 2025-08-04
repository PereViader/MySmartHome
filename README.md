# My Smart Home

## What does it do?

- Emit telegram messages based on some event sent to an http endpoint
- Play music randomly on chromcast compatible devices

## Run locally with docker 
Note: Running the application with docker for windows does not work due to the limited compatibility in the windows host network adapter from docker. Chromecast playback has a TTL of 1 and thus packets never arrive the dockerized application
Also the application is not yet properly implemented to even be able to do it :)
```
docker build -t pereviader/musiccaster:latest .
docker run --name musiccaster --network host -v C:/Users/perev/Music/MusicCaster:/app/audio pereviader/musiccaster:latest
```

## Publish an update of the code to the docker hub
```
docker buildx build --platform linux/arm64 -t pereviader/musiccaster:latest .
docker login
docker push pereviader/musiccaster:latest
```

## Running in a raspberry pereviader
Note that the application relies on a volume `/home/usuari/apps/musiccaster` with mp3 music files in it that must must be bound to `/app/audio` it can be made readonly with `:ro`
To customize the application configuration, you need to also bind `/app/appsettings.Configuration.json`

```
docker run \
  --pull always \
  --detach \
  --network host \
  --name musiccaster \
  --restart unless-stopped \
  -v /home/usuari/apps/musiccaster:/app/audio:ro \
  -v /home/usuari/apps/musiccaster/appsettings.Configuration.json:/app/appsettings.Configuration.json:ro \
  pereviader/musiccaster:latest
```
