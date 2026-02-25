# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Ecommerce.Identity.API/Ecommerce.Identity.API.csproj", "Ecommerce.Identity.API/"]
COPY ["Ecommerce.Shared.Common/Ecommerce.Shared.Common.csproj", "Ecommerce.Shared.Common/"]

# Restore dependencies
RUN dotnet restore "Ecommerce.Identity.API/Ecommerce.Identity.API.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/Ecommerce.Identity.API"
RUN dotnet build "Ecommerce.Identity.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Ecommerce.Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "Ecommerce.Identity.API.dll"]
