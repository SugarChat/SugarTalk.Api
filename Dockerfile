FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
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

# open lls
RUN apt-get update && \
    apt-get install -y perl

RUN wget -O - https://www.openssl.org/source/openssl-3.0.9.tar.gz | tar zxf - && \
    cd openssl-3.0.9 && \
    ./config --prefix=/usr/local && \
    make -j$(nproc) && \
    make install_sw install_ssldirs

RUN ldconfig -v

ENV SSL_CERT_DIR=/etc/ssl/certs

# ffmpeg
RUN apt-get update && apt-get install -y bzip2 make gcc yasm libopencore-amrnb-dev libopencore-amrwb-dev wget

RUN wget https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2 && \
 tar -jxvf ffmpeg-snapshot.tar.bz2 && \
cd ffmpeg && \
 ./configure --enable-gpl --enable-libopencore-amrnb --enable-libopencore-amrwb --prefix=/usr/local/ffmpeg --enable-version3 && \
make -j8 && make install && \
 ln -s /usr/local/ffmpeg/bin/ffmpeg /usr/local/bin/

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]