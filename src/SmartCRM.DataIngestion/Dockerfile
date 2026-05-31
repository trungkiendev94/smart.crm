# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/SmartCRM.DataIngestion/SmartCRM.DataIngestion.csproj", "src/SmartCRM.DataIngestion/"]
COPY ["src/SmartCRM.Infrastructure/SmartCRM.Infrastructure.csproj", "src/SmartCRM.Infrastructure/"]
COPY ["src/SmartCRM.Domain/SmartCRM.Domain.csproj", "src/SmartCRM.Domain/"]
COPY ["src/SmartCRM.Shared/SmartCRM.Shared.csproj", "src/SmartCRM.Shared/"]

RUN dotnet restore "src/SmartCRM.DataIngestion/SmartCRM.DataIngestion.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/src/SmartCRM.DataIngestion"
RUN dotnet build "SmartCRM.DataIngestion.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SmartCRM.DataIngestion.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Since this is a console app for ingestion, it might need to run and then exit.
ENTRYPOINT ["dotnet", "SmartCRM.DataIngestion.dll"]
