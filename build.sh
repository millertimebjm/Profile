docker build -t profile:latest .
docker stop profile
docker rm profile
docker run -d \
  -p 10200:8080 \
  --name profile \
  --restart always \
  -e "Values__Profile__AppConfigConnection"="$1" \
  profile:latest
