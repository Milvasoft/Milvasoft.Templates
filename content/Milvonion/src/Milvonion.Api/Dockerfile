#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0.7-jammy AS base

USER app
WORKDIR /app
EXPOSE xhttpportxx
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:xhttpportxx

FROM mcr.microsoft.com/dotnet/sdk:8.0.303-jammy AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY *.sln .

COPY ["tests/Milvonion.UnitTests/Milvonion.UnitTests.csproj", "Milvonion.UnitTests/"]
COPY ["tests/Milvonion.IntegrationTests/Milvonion.IntegrationTests.csproj", "Milvonion.IntegrationTests/"]
COPY ["src/Milvonion.Application/Milvonion.Application.csproj", "Milvonion.Application/"]
COPY ["src/Milvonion.Domain/Milvonion.Domain.csproj", "Milvonion.Domain/"]
COPY ["src/Milvonion.Infrastructure/Milvonion.Infrastructure.csproj", "Milvonion.Infrastructure/"]
COPY ["src/Milvonion.Api/Milvonion.Api.csproj", "Milvonion.Api/"]

RUN dotnet restore "./Milvonion.Api/Milvonion.Api.csproj"

COPY ./src/. .

RUN dotnet build "./Milvonion.Api/Milvonion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Milvonion.Api/Milvonion.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Milvonion.Api.dll"]