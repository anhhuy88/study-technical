# https://viblo.asia/p/docker-chua-biet-gi-den-biet-dung-phan-2-dockerfile-RQqKLzeOl7z
# ------------------------------- create image and install .net core ----------------------------------------------
# FROM ubuntu:16.04
# RUN apt-get update \
#     && apt-get install -y wget \
#     && rm -rf /var/lib/apt/lists/*
# RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
# RUN apt-get update && apt-get install -y software-properties-common
# RUN apt-get update && apt-get install -y apt-transport-https
# 
# RUN dpkg -i packages-microsoft-prod.deb
# RUN add-apt-repository universe
# RUN apt-get update && apt-get install -y dotnet-sdk-3.1 aspnetcore-runtime-3.1 dotnet-runtime-3.1
# CMD [ "dotnet", "--version" ]

# -----------------------------------------------------------------------------
FROM ubuntu:16.04

RUN apt-get update \
    && apt-get install -y wget \
    && rm -rf /var/lib/apt/lists/*
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y software-properties-common
RUN apt-get update && apt-get install -y apt-transport-https

RUN dpkg -i packages-microsoft-prod.deb
RUN add-apt-repository universe
RUN apt-get update && apt-get install -y dotnet-sdk-3.1 aspnetcore-runtime-3.1 dotnet-runtime-3.1
RUN apt-get update && apt-get install -y nginx
