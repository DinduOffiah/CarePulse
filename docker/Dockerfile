FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/CarePulse.Api/CarePulse.Api.csproj", "src/CarePulse.Api/"]
COPY ["src/CarePulse.Application/CarePulse.Application.csproj", "src/CarePulse.Application/"]
COPY ["src/CarePulse.Domain/CarePulse.Domain.csproj", "src/CarePulse.Domain/"]
COPY ["src/CarePulse.Infrastructure/CarePulse.Infrastructure.csproj", "src/CarePulse.Infrastructure/"]
COPY ["src/CarePulse.Shared/CarePulse.Shared.csproj", "src/CarePulse.Shared/"]

RUN dotnet restore "src/CarePulse.Api/CarePulse.Api.csproj"

COPY . .
WORKDIR "/src/src/CarePulse.Api"
RUN dotnet build "CarePulse.Api.csproj" -c Release -o /app/build
RUN dotnet publish "CarePulse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CarePulse.Api.dll"]
