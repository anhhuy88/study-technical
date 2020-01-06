- Download pgAdmin 4: https://www.pgadmin.org/download/pgadmin-4-windows/
https://o7planning.org/vi/11325/huong-dan-cai-dat-va-cau-hinh-postgresql-database-tren-ubuntu
- Create database
- Access postgresql: psql -U postgres
- Sql create databse:
CREATE DATABASE "DemoTestXX"
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
    
    
# Remote to VM
1. Install postgresql
2. Change file /etc/postgresql/10/main/pg_hba.conf
    + "host all all 10.0.2.1/24 md5" => "host all all 0.0.0.0/0 trust"
3. Change file: /etc/postgresql/10/main/postgresql.conf
    + remove comment: listen_addresses = '*'
4. run: sudo systemctl restart postgresql
