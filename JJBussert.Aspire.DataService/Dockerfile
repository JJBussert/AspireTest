FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JJBussert.Aspire.DataService/JJBussert.Aspire.DataService.csproj", "JJBussert.Aspire.DataService/"]
COPY ["JJBussert.Aspire.Data/JJBussert.Aspire.Data.csproj", "JJBussert.Aspire.Data/"]
COPY ["JJBussert.Aspire.Domain/JJBussert.Aspire.Domain.csproj", "JJBussert.Aspire.Domain/"]
COPY ["JJBussert.Aspire.ServiceDefaults/JJBussert.Aspire.ServiceDefaults.csproj", "JJBussert.Aspire.ServiceDefaults/"]
RUN dotnet restore "JJBussert.Aspire.DataService/JJBussert.Aspire.DataService.csproj"
COPY . .
WORKDIR "/src/JJBussert.Aspire.DataService"
RUN dotnet build "JJBussert.Aspire.DataService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JJBussert.Aspire.DataService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JJBussert.Aspire.DataService.dll"]
