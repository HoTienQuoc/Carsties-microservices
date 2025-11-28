# ---------------- BUILD STAGE ----------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution file và csproj để restore dependencies
COPY Carsties.sln ./
COPY src/SearchService/SearchService.csproj ./src/SearchService/
COPY src/Contracts/Contracts.csproj ./src/Contracts/

# Restore dependencies
RUN dotnet restore ./src/SearchService/SearchService.csproj

# Copy toàn bộ source code
COPY . .

# Publish app
RUN dotnet publish ./src/SearchService/SearchService.csproj -c Release -o /app/publish


# ---------------- RUNTIME STAGE ----------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80

# Copy từ stage build
COPY --from=build /app/publish .

# Chạy service
ENTRYPOINT ["dotnet", "SearchService.dll"]
