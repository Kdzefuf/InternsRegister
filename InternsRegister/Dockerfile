FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InternsRegister.Web/InternsRegister.Web.csproj", "InternsRegister.Web/"]
COPY ["InternsRegister.Application/InternsRegister.Application.csproj", "InternsRegister.Application/"]
COPY ["InternsRegister.Domain/InternsRegister.Domain.csproj", "InternsRegister.Domain/"]
COPY ["InternsRegister.Infrastructure/InternsRegister.Infrastructure.csproj", "InternsRegister.Infrastructure/"]
COPY ["InternsRegister.Persistence/InternsRegister.Persistence.csproj", "InternsRegister.Persistence/"]
RUN dotnet restore "./InternsRegister.Web/InternsRegister.Web.csproj"
COPY . .
WORKDIR "/src/InternsRegister.Web"
RUN dotnet build "./InternsRegister.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InternsRegister.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternsRegister.Web.dll"]