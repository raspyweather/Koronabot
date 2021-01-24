FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app/src
COPY src/ .
RUN dotnet restore KoronaBot.TelegramBot/KoronaBot.TelegramBot.csproj
RUN dotnet publish -c Release KoronaBot.TelegramBot/KoronaBot.TelegramBot.csproj -o /app/build

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app/build/ ./
ENTRYPOINT ["dotnet", "KoronaBot.TelegramBot.dll"]