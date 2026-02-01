# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/DotaDashboard/DotaDashboard.csproj", "DotaDashboard/"]
RUN dotnet restore "DotaDashboard/DotaDashboard.csproj"

# Copy everything else and build
COPY src/DotaDashboard/. DotaDashboard/
RUN dotnet build "DotaDashboard/DotaDashboard.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DotaDashboard/DotaDashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=publish /app/publish .

# Run migrations and start app
ENTRYPOINT ["dotnet", "DotaDashboard.dll"]