# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/SmartCRM.WebAPI/SmartCRM.WebAPI.csproj", "src/SmartCRM.WebAPI/"]
COPY ["src/SmartCRM.Application/SmartCRM.Application.csproj", "src/SmartCRM.Application/"]
COPY ["src/SmartCRM.Infrastructure/SmartCRM.Infrastructure.csproj", "src/SmartCRM.Infrastructure/"]
COPY ["src/SmartCRM.Domain/SmartCRM.Domain.csproj", "src/SmartCRM.Domain/"]
COPY ["src/SmartCRM.Shared/SmartCRM.Shared.csproj", "src/SmartCRM.Shared/"]

RUN dotnet restore "src/SmartCRM.WebAPI/SmartCRM.WebAPI.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/src/SmartCRM.WebAPI"
RUN dotnet build "SmartCRM.WebAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SmartCRM.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartCRM.WebAPI.dll"]
