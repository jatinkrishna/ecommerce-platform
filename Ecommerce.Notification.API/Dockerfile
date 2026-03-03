# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching
COPY Ecommerce.Notification.API/Ecommerce.Notification.API.csproj Ecommerce.Notification.API/
# If you have other projects referenced (likely Ecommerce.Shared.Common), copy them too
COPY Ecommerce.Shared.Common/Ecommerce.Shared.Common.csproj Ecommerce.Shared.Common/

RUN dotnet restore Ecommerce.Notification.API/Ecommerce.Notification.API.csproj

# Copy the rest of the source
COPY . .

RUN dotnet publish Ecommerce.Notification.API/Ecommerce.Notification.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Standardize container port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Ecommerce.Notification.API.dll"]