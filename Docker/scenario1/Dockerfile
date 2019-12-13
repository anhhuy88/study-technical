FROM ubuntu:18.04

ENV APP_PATH /venv
WORKDIR $APP_PATH

# Set the timezone.
ENV TZ=Asia/Ho_Chi_Minh
RUN set -x \
    && ln -snf /usr/share/zoneinfo/$TZ /etc/localtime \
    && echo $TZ > /etc/timezone

RUN apt-get update \
    && apt-get install -y wget \
    && rm -rf /var/lib/apt/lists/*
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y software-properties-common
RUN apt-get update && apt-get install -y apt-transport-https

RUN dpkg -i packages-microsoft-prod.deb
RUN add-apt-repository universe
RUN apt-get update && apt-get install -y dotnet-sdk-3.1 aspnetcore-runtime-3.1 dotnet-runtime-3.1
RUN apt-get update && apt-get install -y nginx nano vim

COPY ./demo-mvc.service /etc/systemd/system
COPY ./start.sh /venv

RUN set -x \
    && chmod a+x /venv/*

VOLUME [ "/var/www/demo-mvc", "/var/www/demo-mvc"]
ENTRYPOINT [ "/venv/start.sh" ]
EXPOSE 80

