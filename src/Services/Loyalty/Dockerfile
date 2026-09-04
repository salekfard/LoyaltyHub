# Build context must be the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LoyaltyHub.slnx ./
COPY src/Services/Loyalty/LoyaltyHub.Loyalty.Domain/LoyaltyHub.Loyalty.Domain.csproj src/Services/Loyalty/LoyaltyHub.Loyalty.Domain/
COPY src/Services/Loyalty/LoyaltyHub.Loyalty.Application/LoyaltyHub.Loyalty.Application.csproj src/Services/Loyalty/LoyaltyHub.Loyalty.Application/
COPY src/Services/Loyalty/LoyaltyHub.Loyalty.Infrastructure/LoyaltyHub.Loyalty.Infrastructure.csproj src/Services/Loyalty/LoyaltyHub.Loyalty.Infrastructure/
COPY src/Services/Loyalty/LoyaltyHub.Loyalty.Api/LoyaltyHub.Loyalty.Api.csproj src/Services/Loyalty/LoyaltyHub.Loyalty.Api/

RUN dotnet restore src/Services/Loyalty/LoyaltyHub.Loyalty.Api/LoyaltyHub.Loyalty.Api.csproj

COPY src/Services/Loyalty/ src/Services/Loyalty/

RUN dotnet publish src/Services/Loyalty/LoyaltyHub.Loyalty.Api/LoyaltyHub.Loyalty.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LoyaltyHub.Loyalty.Api.dll"]
