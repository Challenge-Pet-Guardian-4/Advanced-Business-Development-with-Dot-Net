FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PetGuardian.API/PetGuardian.API.csproj", "PetGuardian.API/"]
COPY ["PetGuardian.Application/PetGuardian.Application.csproj", "PetGuardian.Application/"]
COPY ["PetGuardian.Domain/PetGuardian.Domain.csproj", "PetGuardian.Domain/"]
COPY ["PetGuardian.Infrastructure/PetGuardian.Infrastructure.csproj", "PetGuardian.Infrastructure/"]
RUN dotnet restore "PetGuardian.API/PetGuardian.API.csproj"
COPY . .
WORKDIR "/src/PetGuardian.API"
RUN dotnet build "./PetGuardian.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PetGuardian.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetGuardian.API.dll"]
