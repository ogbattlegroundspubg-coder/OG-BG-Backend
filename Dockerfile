# Use official .NET 6 SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY PUBGLiteBackendWV.csproj .
RUN dotnet restore PUBGLiteBackendWV.csproj

# Copy all source files
COPY . .

# Build in Release mode
RUN dotnet publish PUBGLiteBackendWV.csproj -c Release -o /app

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

# Render provides $PORT dynamically, so bind to it
ENV ASPNETCORE_URLS=http://+:${PORT}

# Expose the Render port
EXPOSE 10000

# Start backend
ENTRYPOINT ["dotnet", "PUBGLiteBackendWV.dll"]
