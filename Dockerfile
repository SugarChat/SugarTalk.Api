FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy everything else and build
COPY ./src/SugarTalk.Core ./build/SugarTalk.Core
COPY ./src/SugarTalk.Api ./build/SugarTalk.Api
COPY ./src/SugarTalk.Messages ./build/SugarTalk.Messages

RUN dotnet publish build/SugarTalk.Api -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SugarTalk.Api.dll"]