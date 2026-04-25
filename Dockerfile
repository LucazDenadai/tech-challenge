FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TechChallenge.Domain/TechChallenge.Domain.csproj", "src/TechChallenge.Domain/"]
COPY ["src/TechChallenge.Application/TechChallenge.Application.csproj", "src/TechChallenge.Application/"]
COPY ["src/TechChallenge.Infrastructure/TechChallenge.Infrastructure.csproj", "src/TechChallenge.Infrastructure/"]
COPY ["src/TechChallenge.API/TechChallenge.API.csproj", "src/TechChallenge.API/"]

RUN dotnet restore "src/TechChallenge.API/TechChallenge.API.csproj"

COPY . .

WORKDIR "/src/src/TechChallenge.API"
RUN dotnet build "TechChallenge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechChallenge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "TechChallenge.API.dll"]
