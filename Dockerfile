# build the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build

COPY . .

RUN dotnet build --configuration Release --output /app/publish ./src

# create the runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0 as runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordPingPongBot.dll"]