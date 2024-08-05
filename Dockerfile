# Use the official Microsoft ASP.NET Core runtime image
# This image includes the .NET runtime and ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install OpenJDK
RUN apt-get update && apt-get install -y openjdk-11-jdk

# Use the official Microsoft .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Log the start of the build process
RUN echo "Starting build process..."

# Copy csproj and restore as distinct layers
COPY ["ZATCA-V2/ZATCA-V2.csproj", "ZATCA-V2/"]
RUN echo "Copied csproj file."
RUN dotnet restore "ZATCA-V2/ZATCA-V2.csproj" --verbosity detailed

# Copy everything else and build
COPY . .
RUN echo "Copied all project files."
WORKDIR "/src/ZATCA-V2"
RUN echo "Building the project..."
RUN dotnet build "ZATCA-V2.csproj" -c Release -o /app/build --verbosity detailed

# Publish the application
FROM build AS publish
RUN echo "Publishing the application..."
RUN dotnet publish "ZATCA-V2.csproj" -c Release -o /app/publish --verbosity detailed

# Final stage/image
FROM base AS final
WORKDIR /app
RUN echo "Copying published files..."
COPY --from=publish /app/publish .
RUN echo "Starting the application..."
ENTRYPOINT ["dotnet", "ZATCA-V2.dll"]
