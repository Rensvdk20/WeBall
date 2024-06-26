﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InventoryManagement/InventoryManagement.csproj", "InventoryManagement/"]
COPY ["InventoryManagement.DomainServices/InventoryManagement.DomainServices.csproj", "InventoryManagement.DomainServices/"]
COPY ["InventoryManagement.Domain/InventoryManagement.Domain.csproj", "InventoryManagement.Domain/"]
COPY ["InventoryManagement.Infrastructure/InventoryManagement.Infrastructure.csproj", "InventoryManagement.Infrastructure/"]
RUN dotnet restore "InventoryManagement/InventoryManagement.csproj"
COPY . .
WORKDIR "/src/InventoryManagement"
RUN dotnet build "InventoryManagement.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InventoryManagement.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagement.dll"]
