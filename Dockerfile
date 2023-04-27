FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

COPY . .
ENTRYPOINT ["dotnet", "App.dll"]