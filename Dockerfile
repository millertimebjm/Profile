# Stage 1: Build
# Using the .NET 9 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Copy the project file first to leverage layer caching
COPY ["Profile.csproj", "Profile.csproj"]
# Restore dependencies
RUN dotnet restore "Profile.csproj"
# Copy the rest of the source tree
COPY . .
# Publish the application
WORKDIR "/src"
RUN dotnet publish "Profile.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
# Using the .NET 10 ASP.NET Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 10200
# .NET 10 often defaults to port 8080 for non-root users,
# so we explicitly set our desired port here.
ENV ASPNETCORE_URLS=http://+:10200
ENV DOTNET_gcServer=0
# Copy the published output from the build stage
COPY --from=build /app/publish .
# Standard entry point
ENTRYPOINT ["dotnet", "Profile.dll"]
