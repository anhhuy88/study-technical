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

- Liệt kê Image: 
    + Xóa 1: docker image ls
- Xóa image: 
    + docker rmi -f [imageId]
    + Xóa nhiều: docker rmi $(docker images -q) -f
- Liệt kê container: docker container ls
- Xóa nhiều container:
    + docker rm $(docker ps -a -q)

- Run postgres on dockerhub
    + docker run -p 5432:5432 -it postgres:10.11 /bin/bash
    + docker run --name demo_postgresql -e POSTGRES_PASSWORD=123 -d -p 5432:5432 postgres:10.11
- Access container: docker exec -it [ConatinerId] bash