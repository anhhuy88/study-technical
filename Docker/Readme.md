- Liệt kê Image: 
    + Xóa 1: docker image ls
- Xóa image: 
    + docker rmi -f [imageId]
    + Xóa nhiều: docker rmi $(docker images -q)
- Liệt kê container: docker container ls
- Xóa nhiều container:
    + docker rm $(docker ps -a -q)
