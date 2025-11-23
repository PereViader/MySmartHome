FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MySmartHome.Automations.dll"]
