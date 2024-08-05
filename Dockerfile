# Use the official Microsoft ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install OpenJDK 17
RUN apt-get update && apt-get install -y openjdk-17-jdk

# Use the official Microsoft .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["ZATCA-V2/ZATCA-V2.csproj", "ZATCA-V2/"]
RUN dotnet restore "ZATCA-V2/ZATCA-V2.csproj" --verbosity detailed

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

# Copy published files
COPY --from=publish /app/publish .

# Copy the libs folder to the Docker image
COPY ZATCA-V2/libs /app/libs

# Set the IKVM classpath environment variable
ENV IKVM_CLASSPATH /app/libs/*.jar

ENTRYPOINT ["dotnet", "ZATCA-V2.dll"]
