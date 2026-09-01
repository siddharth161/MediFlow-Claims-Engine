## Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/MediFlow.Domain/MediFlow.Domain.csproj", "src/MediFlow.Domain/"]
COPY ["src/MediFlow.Application/MediFlow.Application.csproj", "src/MediFlow.Application/"]
COPY ["src/MediFlow.Infrastructure/MediFlow.Infrastructure.csproj", "src/MediFlow.Infrastructure/"]
COPY ["src/MediFlow.Api/MediFlow.Api.csproj", "src/MediFlow.Api/"]

RUN dotnet restore "src/MediFlow.Api/MediFlow.Api.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/src/MediFlow.Api"
RUN dotnet publish "MediFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

## Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MediFlow.Api.dll"]
