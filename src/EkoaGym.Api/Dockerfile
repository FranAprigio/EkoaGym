# Build stage: full SDK to restore/compile/publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/EkoaGym.Api/EkoaGym.Api.csproj src/EkoaGym.Api/
COPY src/EkoaGym.Application/EkoaGym.Application.csproj src/EkoaGym.Application/
COPY src/EkoaGym.Infrastructure/EkoaGym.Infrastructure.csproj src/EkoaGym.Infrastructure/
COPY src/EkoaGym.Domain/EkoaGym.Domain.csproj src/EkoaGym.Domain/
RUN dotnet restore src/EkoaGym.Api/EkoaGym.Api.csproj

COPY src/ src/
RUN dotnet publish src/EkoaGym.Api/EkoaGym.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage: slim ASP.NET runtime image only
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "EkoaGym.Api.dll"]
