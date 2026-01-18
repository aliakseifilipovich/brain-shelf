# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/BrainShelf.Api/BrainShelf.Api.csproj", "src/BrainShelf.Api/"]
COPY ["src/BrainShelf.Core/BrainShelf.Core.csproj", "src/BrainShelf.Core/"]
COPY ["src/BrainShelf.Infrastructure/BrainShelf.Infrastructure.csproj", "src/BrainShelf.Infrastructure/"]
COPY ["src/BrainShelf.Services/BrainShelf.Services.csproj", "src/BrainShelf.Services/"]

# Restore dependencies
RUN dotnet restore "src/BrainShelf.Api/BrainShelf.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/BrainShelf.Api"
RUN dotnet build "BrainShelf.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BrainShelf.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5000

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrainShelf.Api.dll"]
