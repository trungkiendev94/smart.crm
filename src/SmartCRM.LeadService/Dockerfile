# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/SmartCRM.LeadService/SmartCRM.LeadService.csproj", "src/SmartCRM.LeadService/"]
COPY ["src/SmartCRM.Infrastructure/SmartCRM.Infrastructure.csproj", "src/SmartCRM.Infrastructure/"]
COPY ["src/SmartCRM.Domain/SmartCRM.Domain.csproj", "src/SmartCRM.Domain/"]
COPY ["src/SmartCRM.Shared/SmartCRM.Shared.csproj", "src/SmartCRM.Shared/"]

RUN dotnet restore "src/SmartCRM.LeadService/SmartCRM.LeadService.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/src/SmartCRM.LeadService"
RUN dotnet build "SmartCRM.LeadService.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SmartCRM.LeadService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartCRM.LeadService.dll"]
