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

# Install necessary packages for ffmpeg including libssl-dev for HTTPS support
RUN apt-get update && apt-get install -y \
    bzip2 \
    make \
    gcc \
    yasm \
    libopencore-amrnb-dev \
    libopencore-amrwb-dev \
    libssl-dev \
    wget \
    pkg-config \
    libavcodec-dev \
    libavformat-dev \
    libavfilter-dev \
    libavdevice-dev

# Download ffmpeg source code
RUN wget https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2

# Extract ffmpeg source code
RUN tar -jxvf ffmpeg-snapshot.tar.bz2

# Build and install ffmpeg
RUN cd ffmpeg && \
    ./configure --enable-gpl --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-version3 --enable-openssl && \
    make -j8 && \
    make install

# Create symbolic link
RUN ln -s /usr/local/ffmpeg/bin/ffmpeg /usr/local/bin/

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]