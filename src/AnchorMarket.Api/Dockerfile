FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo
COPY ["src/AnchorMarket.Api/AnchorMarket.Api.csproj", "src/AnchorMarket.Api/"]
COPY ["src/AnchorMarket.Application/AnchorMarket.Application.csproj", "src/AnchorMarket.Application/"]
COPY ["src/AnchorMarket.Infrastructure/AnchorMarket.Infrastructure.csproj", "src/AnchorMarket.Infrastructure/"]
COPY ["src/AnchorMarket.Domain/AnchorMarket.Domain.csproj", "src/AnchorMarket.Domain/"]
RUN dotnet restore "src/AnchorMarket.Api/AnchorMarket.Api.csproj"
COPY . .
WORKDIR "/repo/src/AnchorMarket.Api"
RUN dotnet build "./AnchorMarket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnchorMarket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnchorMarket.Api.dll"]
