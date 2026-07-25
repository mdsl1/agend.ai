FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

WORKDIR /app

EXPOSE 5000
EXPOSE 5001

ENV DOTNET_USE_POLLING_FILE_WATCHER=1

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]