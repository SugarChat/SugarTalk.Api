FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

USER root

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy everything else and build
COPY ./src/SugarTalk.Core ./build/SugarTalk.Core
COPY ./src/SugarTalk.Api ./build/SugarTalk.Api
COPY ./src/SugarTalk.Messages ./build/SugarTalk.Messages
COPY ./NuGet.Config ./build

RUN dotnet publish build/SugarTalk.Api -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

USER root

RUN apt-get update && apt-get install -y \
    bzip2 \
    make \
    gcc \
    yasm \
    libopencore-amrnb-dev \
    libopencore-amrwb-dev \
    wget \
    perl

# open lls
RUN apt-get update && \
    apt-get install -y perl

RUN wget -O - https://www.openssl.org/source/openssl-3.0.14.tar.gz | tar zxf - && \
    cd openssl-3.0.14 && \
    ./config --prefix=/usr/local && \
    make -j$(nproc) && \
    make install_sw install_ssldirs

RUN ldconfig -v

# ffmpeg
RUN apt-get update && apt-get install -y bzip2 make gcc yasm libopencore-amrnb-dev libopencore-amrwb-dev wget

# Install FFmpeg with OpenSSL support
RUN wget https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2 && \
    tar -jxvf ffmpeg-snapshot.tar.bz2 && \
    cd ffmpeg && \
    ./configure --enable-gpl --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-version3 --enable-openssl && \
    make -j8 && make install && \
    ln -s /usr/local/ffmpeg/bin/ffmpeg /usr/local/bin/

ENV SSL_CERT_DIR=/etc/ssl/certs
ENV LD_LIBRARY_PATH=/usr/local/lib

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]