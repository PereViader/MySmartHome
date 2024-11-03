FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PereViader.MusicCaster/PereViader.MusicCaster.csproj", "PereViader.MusicCaster/"]
RUN dotnet restore "PereViader.MusicCaster/PereViader.MusicCaster.csproj"
COPY . .
WORKDIR "/src/PereViader.MusicCaster"
RUN dotnet build "PereViader.MusicCaster.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PereViader.MusicCaster.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PereViader.MusicCaster.dll"]
