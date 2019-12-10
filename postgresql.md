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


