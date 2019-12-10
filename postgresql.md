- Download pgAdmin 4: https://www.pgadmin.org/download/pgadmin-4-windows/
- Create database
- Access postgresql: psql -U postgres
- Sql create databse:
CREATE DATABASE "DemoTest"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;


- Run postgres on dockerhub
    + docker run -p 5432:5432 -it postgres:10.11 /bin/bash
    + docker run --name demo_postgresql -e POSTGRES_PASSWORD=123 -d -p 5432:5432 postgres:10.11