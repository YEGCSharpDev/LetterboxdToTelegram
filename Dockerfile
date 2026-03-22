# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app
# UID 1654 is the standard 'app' user in .NET 8+ Microsoft images
USER root
RUN mkdir -p /app/data && chown -R 1654:1654 /app/data && chown -R 1654:1654 /app
# Environment variable for the database location
ENV ConnectionStrings__Default="Data Source=/app/data/movies.db"
# PERSISTENT STORAGE
VOLUME /app/data

# Stage 2: SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj", "LetterboxdToCinephilesChannel/"]
RUN dotnet restore "LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj"
COPY . .
WORKDIR "/src/LetterboxdToCinephilesChannel"
RUN dotnet build "LetterboxdToCinephilesChannel.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LetterboxdToCinephilesChannel.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER 1654
ENTRYPOINT ["dotnet", "LetterboxdToCinephilesChannel.dll"]
