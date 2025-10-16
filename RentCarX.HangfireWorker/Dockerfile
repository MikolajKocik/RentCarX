# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RentCarX.HangfireWorker/RentCarX.HangfireWorker.csproj", "RentCarX.HangfireWorker/"]
COPY ["RentCarX.Application/RentCarX.Application.csproj", "RentCarX.Application/"]
COPY ["RentCarX.Domain/RentCarX.Domain.csproj", "RentCarX.Domain/"]
COPY ["RentCarX.Infrastructure/RentCarX.Infrastructure.csproj", "RentCarX.Infrastructure/"]
COPY ["RentCarX/RentCarX.Presentation.csproj", "RentCarX/"]
RUN dotnet restore "./RentCarX.HangfireWorker/RentCarX.HangfireWorker.csproj"
COPY . .
WORKDIR "/src/RentCarX.HangfireWorker"
RUN dotnet build "./RentCarX.HangfireWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RentCarX.HangfireWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentCarX.HangfireWorker.dll"]