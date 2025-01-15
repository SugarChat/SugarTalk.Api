FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy everything else and build
COPY ./src/SugarTalk.Core ./build/SugarTalk.Core
COPY ./src/SugarTalk.Api ./build/SugarTalk.Api
COPY ./src/SugarTalk.Messages ./build/SugarTalk.Messages
COPY ./NuGet.Config ./build
COPY ./src/SugarTalk.Core/Aspose.Total.NET.txt ./build/SugarTalk.Core/Aspose.Total.NET.txt

RUN dotnet publish build/SugarTalk.Api -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# ffmpeg
RUN apt-get update && apt-get install -y nasm
RUN apt-get update && apt-get install -y bzip2 make gcc yasm libopencore-amrnb-dev libopencore-amrwb-dev wget libmp3lame-dev

RUN apt-get update && apt-get install -y \
    fonts-noto fonts-noto-cjk fonts-noto-mono \
    && apt-get clean

RUN wget https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2 && \
 tar -jxvf ffmpeg-snapshot.tar.bz2 && \
cd ffmpeg && \
 ./configure --enable-gpl --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libmp3lame --prefix=/usr/local/ffmpeg --enable-version3 && \
make -j8 && make install && \
 ln -s /usr/local/ffmpeg/bin/ffmpeg /usr/local/bin/

WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/build/SugarTalk.Core/Aspose.Total.NET.txt .
COPY --from=build-env /app/build/SugarTalk.Core/Aspose.Total.NET.txt ./Aspose.Total.NET.txt

ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]