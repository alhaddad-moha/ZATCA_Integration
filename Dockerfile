# Use the official Microsoft ASP.NET Core runtime image
# This image includes the .NET runtime and ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official Microsoft .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# Copy csproj and restore as distinct layers
COPY ["ZATCA-V2/ZATCA-V2.csproj", "ZATCA-V2/"]
RUN dotnet restore "ZATCA-V2/ZATCA-V2.csproj"
# Copy everything else and build
COPY . .
WORKDIR "/src/ZATCA-V2"
RUN dotnet build "ZATCA-V2.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ZATCA-V2.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZATCA-V2.dll"]