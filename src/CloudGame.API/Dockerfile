FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER $APP_UID

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CloudGame.API/CloudGame.API.csproj", "src/CloudGame.API/"]
COPY ["CloudGame.Application/CloudGame.Application.csproj", "src/CloudGame.Application/"]
COPY ["CloudGame.Domain/CloudGame.Domain.csproj", "src/CloudGame.Domain/"]
COPY ["CloudGame.Infrastructure/CloudGame.Infrastructure.csproj", "src/CloudGame.Infrastructure/"]
RUN dotnet restore "./CloudGame.API/CloudGame.API.csproj"
COPY . .
WORKDIR "/src/src/CloudGame.API"
RUN dotnet build "./CloudGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudGame.API.dll"]