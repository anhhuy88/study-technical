# Problems
- install docker
- learn Dockerfile
- Learn Docker-compose
- Create redis image
- create postgresql image
    + + docker run --name demo_postgresql -e POSTGRES_PASSWORD=123 -d -p 5432:5432 postgres:10.11
    + mount folder host vs container
- Create image for app
    + Install: netcore, ngix
- How to connect between container...

------------------------------------------------------------------------------------------------
- scenario 1: Run app in docker
    1. Create mvc app .net core
    2. Build image ubuntu
        + Install: .net core, nginx
    3. Run app in image ubuntu
    4. Access http://localhost:5000 via browser on localhost
- scenario 2: remove container database
    1. run container database
    2. Remove container database
    3. run container database
    4. check database old?
- scenario 3: backup and restore database
    1. Backup database to folder destination
    2. Restore database to folder source
- scenario 4: install FTP on server
    1. install FTP
    2. connect FTP
    3. Donwload file from FTP




