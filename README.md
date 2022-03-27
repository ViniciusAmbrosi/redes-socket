# redes-socket

Both projects contain a Dockerfile for creation and deployment if so desired.  
To create image for socket server. Go to Redes/SocketServer and run docker build -t socket-server -f Dockerfile .  
To create image for socket client. Go to Redes/SocketClient and run docker build -t socket-client -f Dockerfile .  

This will create two images in docker for deployment, sourcing from either projects.    

In order to get the program running you can:
* To create image for socket server - docker build -t socket-server -f Dockerfile .  
* Start socket server with: docker run --network=host -it --rm socket-server
* Run docker inspect --format '{{ .NetworkSettings.IPAddress }}' <cotainer-name> (you can find with docker ps -a)
* Setup docker ip on socket client code if needed
* Build socket client application
* You can choose to run from host or another container:  
** another container  
*** To create image for socket client - docker build -t socket-client -f Dockerfile .  
*** Start socket client with: docker run -it --network=host --rm socket-client  
** host machine - just run the dotnet program
