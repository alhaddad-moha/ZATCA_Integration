# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ZATCA-V2/ZATCA-V2.csproj", "ZATCA-V2/"]
COPY nuget.config .  
RUN dotnet restore -v diag "ZATCA-V2/ZATCA-V2.csproj"
COPY . .
WORKDIR "/src/ZATCA-V2"
RUN dotnet build "ZATCA-V2.csproj" -c Release -o /app/build

# Use the ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM build AS publish
RUN dotnet publish "ZATCA-V2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZATCA-V2.dll"]
