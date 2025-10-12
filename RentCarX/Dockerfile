# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RentCarX/RentCarX.Presentation.csproj", "RentCarX/"]
COPY ["RentCarX.Application/RentCarX.Application.csproj", "RentCarX.Application/"]
COPY ["RentCarX.Domain/RentCarX.Domain.csproj", "RentCarX.Domain/"]
COPY ["RentCarX.Infrastructure/RentCarX.Infrastructure.csproj", "RentCarX.Infrastructure/"]
RUN dotnet restore "./RentCarX/RentCarX.Presentation.csproj"
COPY . .
WORKDIR "/src/RentCarX"
RUN dotnet build "./RentCarX.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RentCarX.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentCarX.Presentation.dll"]