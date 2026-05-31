FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to maximize layer cache for restore.
COPY ["src/AshWatch.Api/AshWatch.Api.csproj", "src/AshWatch.Api/"]
COPY ["src/AshWatch.Application/AshWatch.Application.csproj", "src/AshWatch.Application/"]
COPY ["src/AshWatch.Domain/AshWatch.Domain.csproj", "src/AshWatch.Domain/"]
COPY ["src/AshWatch.Infrastructure/AshWatch.Infrastructure.csproj", "src/AshWatch.Infrastructure/"]

RUN dotnet restore "src/AshWatch.Api/AshWatch.Api.csproj"

COPY src/ ./src/
WORKDIR /src/src/AshWatch.Api
RUN dotnet publish "AshWatch.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AshWatch.Api.dll"]
