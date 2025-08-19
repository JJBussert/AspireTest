FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JJBussert.Aspire.AppHost/JJBussert.Aspire.AppHost.csproj", "JJBussert.Aspire.AppHost/"]
RUN dotnet restore "JJBussert.Aspire.AppHost/JJBussert.Aspire.AppHost.csproj"
COPY . .
WORKDIR "/src/JJBussert.Aspire.AppHost"
RUN dotnet build "JJBussert.Aspire.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JJBussert.Aspire.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JJBussert.Aspire.AppHost.dll"]
