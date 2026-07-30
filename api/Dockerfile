FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

WORKDIR /app

EXPOSE 5000

ENV DOTNET_USE_POLLING_FILE_WATCHER=1

CMD ["dotnet", "watch", "run", "--project", "AgendAi.API/AgendAi.API.csproj", "--urls", "http://0.0.0.0:5000"]