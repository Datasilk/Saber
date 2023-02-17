#create an image based on Saber release

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM base AS final
WORKDIR /app
COPY App .
ENTRYPOINT ["dotnet", "Saber.dll"]