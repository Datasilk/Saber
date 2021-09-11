#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["App/Saber.csproj", "App/"]
COPY ["Query/Query.csproj", "Query/"]
RUN dotnet restore "App/Saber.csproj"
COPY . .
WORKDIR "/src/App"
RUN dotnet build "Saber.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Saber.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Saber.dll"]