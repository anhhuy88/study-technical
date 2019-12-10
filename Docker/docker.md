# install docker on ubuntu:18.04
- install docker: 
    + sudo apt-get update
    + sudo apt-get install \
            apt-transport-https \
            ca-certificates \
            curl \
            gnupg-agent \
            software-properties-common
    + curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
    + sudo apt-key fingerprint 0EBFCD88
    + sudo add-apt-repository \
            "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
            $(lsb_release -cs) \
            stable"
    + sudo apt-get update
    + 2 cách cài đặt
        * cài lastest: sudo apt-get install docker-ce docker-ce-cli containerd.io
        * chọn version: apt-cache madison docker-ce
            > sudo apt-get install docker-ce="18.03.1~ce~3-0~ubuntu" docker-ce-cli="18.03.1~ce~3-0~ubuntu" containerd.io
    + sudo systemctl status docker
    
# Commands

- List Image: docker image ls
- Xóa image: 
    + docker rmi -f [imageId]
    + sudo docker rmi -f $(sudo docker image ls)
- Restart docker: sudo service docker restart
- List container: 
    + docker container ls
    + docker ps -a
- Remove container:
    + docker rm $(docker ps -a -q)
    + docker rm [containerId] -f
- Clear volume
    + docker volume rm $(docker volume ls -q)
- Clear networks
    + docker network rm $(docker network ls | tail -n+2 | awk '{if($2 !~ /bridge|none|host/){ print $1 }}')
    + sudo docker network rm $(sudo docker network ls | tail -n+2 | awk '{if($2 !~ /bridge|none|host/){ print $1 }}')
- Access container: docker exec -it [ConatinerId] bash

----------------------------------------------------------------
Errors:
- Docker & Postgres: Failed to bind tcp 0.0.0.0:5432 address already in use
    + sudo lsof -i :5432
    + sudo kill [PID]