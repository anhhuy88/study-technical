# https://viblo.asia/p/docker-chua-biet-gi-den-biet-dung-phan-2-dockerfile-RQqKLzeOl7z

# FROM ubuntu:16.04
# RUN echo 'deb http://apt.postgresql.org/pub/repos/apt/ xenial-pgdg main' >> /etc/apt/sources.list.d/pgdg.list
# RUN # wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -
# RUN apt-get update && apt-get install -y postgresql-10
# RUN /etc/init.d/postgresql start &&\
#     psql --command "CREATE USER docker WITH SUPERUSER PASSWORD 'docker';" &&\
#     createdb -O docker docker

# RUN echo "host all  all    0.0.0.0/0  md5" >> /etc/postgresql/10/main/pg_hba.conf
# RUN echo "listen_addresses='*'" >> /etc/postgresql/10/main/postgresql.conf
# EXPOSE 5432
# VOLUME  ["D:\\Program Files\\Postgres", "/var/log/postgresql", "/var/lib/postgresql"]
# CMD ["/usr/lib/postgresql/10/bin/postgres", "-D", "/var/lib/postgresql/10/main", "-c", "config_file=/etc/postgresql/10/main/postgresql.conf"]

FROM ubuntu:16.04
RUN echo "apt-get install -y gpg"
RUN echo "wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o microsoft.asc.gpg"
RUN echo "mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/"
RUN echo "wget -q https://packages.microsoft.com/config/ubuntu/16.04/prod.list"
RUN echo "mv prod.list /etc/apt/sources.list.d/microsoft-prod.list"
RUN echo "chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg"
RUN echo "chown root:root /etc/apt/sources.list.d/microsoft-prod.list"
RUN apt-get update && apt-get install -y apt-transport-https dotnet-sdk-2.1 aspnetcore-runtime-2.1 dotnet-runtime-2.1
CMD [ "dotnet", "--version" ]