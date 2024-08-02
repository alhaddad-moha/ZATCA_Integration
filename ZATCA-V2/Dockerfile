# Use a lightweight base image optimized for .NET 6
FROM mcr.microsoft.com/dotnet/sdk:6.0 
# Set the working directory within the container
WORKDIR /app

# Copy the project files from the current context
COPY . .

# Restore dependencies (if applicable)
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Set the entry point for the container
ENTRYPOINT ["dotnet", "ZATCA-V2.dll"]  
# Expose the port where the API will listen (usually port 80)
EXPOSE 80
