# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CloudGame.API/CloudGame.API.csproj", "CloudGame.API/"]
COPY ["CloudGame.Application/CloudGame.Application.csproj", "CloudGame.Application/"]
COPY ["CloudGame.Domain/CloudGame.Domain.csproj", "CloudGame.Domain/"]
COPY ["CloudGame.Infrastructure/CloudGame.Infrastructure.csproj", "CloudGame.Infrastructure/"]
RUN dotnet restore "./CloudGame.API/CloudGame.API.csproj"
COPY . .
WORKDIR "/src/CloudGame.API"
RUN dotnet build "./CloudGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudGame.API.dll"]