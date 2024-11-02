
Local Dev
docker build -t musiccaster .
docker run --network host -v C:/Users/perev/Music/MusicCaster:/app/audio musiccaster

Push remote
docker tag musiccaster pereviader/musiccaster:latest
docker push musiccaster