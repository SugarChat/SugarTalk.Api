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

# Install OpenSSL 3.0.14
RUN wget https://www.openssl.org/source/openssl-3.0.14.tar.gz && \
    tar -xzf openssl-3.0.14.tar.gz && \
    cd openssl-3.0.14 && \
    ./config --prefix=/usr/local/openssl --openssldir=/usr/local/openssl && \
    make -j$(nproc) && \
    make install_sw && \
    cd .. && \
    rm -rf openssl-3.0.14.tar.gz openssl-3.0.14

ENV PKG_CONFIG_PATH=/usr/local/openssl/lib/pkgconfig
ENV LD_LIBRARY_PATH=/usr/local/openssl/lib

# Install FFmpeg with OpenSSL support
RUN wget https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2 && \
    tar -jxvf ffmpeg-snapshot.tar.bz2 && \
    cd ffmpeg && \
    ./configure --enable-gpl --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-version3 --enable-openssl --extra-cflags="-I/usr/local/openssl/include" --extra-ldflags="-L/usr/local/openssl/lib" && \
    make -j8 && make install && \
    ln -s /usr/local/ffmpeg/bin/ffmpeg /usr/local/bin/ && \
    cd .. && \
    rm -rf ffmpeg-snapshot.tar.bz2 ffmpeg

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]
