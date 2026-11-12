# API Oportuniza (.NET 10) em contêiner Linux.
# Build:  docker build -t oportuniza-api .
# Uso normal: docker compose up -d   (sobe a API + MySQL, ver docker-compose.yml)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Oportuniza.Domain/Oportuniza.Domain.csproj src/Oportuniza.Domain/
COPY src/Oportuniza.Application/Oportuniza.Application.csproj src/Oportuniza.Application/
COPY src/Oportuniza.Infrastructure/Oportuniza.Infrastructure.csproj src/Oportuniza.Infrastructure/
COPY src/Oportuniza.Api/Oportuniza.Api.csproj src/Oportuniza.Api/
RUN dotnet restore src/Oportuniza.Api/Oportuniza.Api.csproj
COPY src/ src/
RUN dotnet publish src/Oportuniza.Api/Oportuniza.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Usuário sem privilégios (já existe na imagem oficial).
USER app
ENTRYPOINT ["dotnet", "Oportuniza.Api.dll"]
