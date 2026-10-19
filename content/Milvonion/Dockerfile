# Production image. Build context = the solution root (the .dockerignore applies and the src/* projects are copied).
# Runtime settings come from the environment, e.g. ConnectionStrings__DefaultConnectionString,
# Milvasoft__Caching__Redis__ConnectionString, Milvasoft__Identity__Token__SymmetricPublicKey, SeqUri,
# OpenTelemetry__CollectorUrl (see README). The signing key and the connection string are not in the appsettings files:
# without Milvasoft__Identity__Token__SymmetricPublicKey the application stops at startup (README "Secrets").
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "src/Milvonion.Api/Milvonion.Api.csproj"
RUN dotnet publish "src/Milvonion.Api/Milvonion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# chiseled-extra: no shell (small attack surface), with ICU and tzdata (localization and time zones).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
USER app
WORKDIR /app
EXPOSE 8080
# MILVA_ENV=prod closes the development endpoints and selects the production log sinks. It is baked into the image so a
# forgotten variable can never open them.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    MILVA_ENV=prod
COPY --from=build --chown=app:app /app/publish .
ENTRYPOINT ["dotnet", "Milvonion.Api.dll"]
