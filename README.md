# My Smart Home

## What does it do?

- Emit telegram messages based on some event sent to an http endpoint
- Play music randomly on chromcast compatible devices

## To run the application

Note: Running the application with docker for windows does not work due to the limited compatibility in the windows host network adapter from docker. Chromecast playback has a TTL of 1 and thus packets never arrive the dockerized application
Also the application is not yet properly implemented to even be able to do it :)
```
git clone https://github.com/PereViader/MySmartHome.git
cd MySmartHome
```

Create a `.env` file inside with the proper paths

```
APP_AUDIO_PATH=/home/user/app/mysmarthome/music
APP_CONFIG_PATH=/home/user/app/mysmarthome/appsettings.Configuration.json
```

Start it or recreate it after an update

```
docker compose up -d --build --force-recreate
```

Stop it

```
docker compose down
```