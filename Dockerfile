FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Atendimento/OficinaMecanica.Atendimento.Domain/OficinaMecanica.Atendimento.Domain.csproj src/Atendimento/OficinaMecanica.Atendimento.Domain/
COPY src/Atendimento/OficinaMecanica.Atendimento.Application/OficinaMecanica.Atendimento.Application.csproj src/Atendimento/OficinaMecanica.Atendimento.Application/
COPY src/Atendimento/OficinaMecanica.Atendimento.Infrastructure/OficinaMecanica.Atendimento.Infrastructure.csproj src/Atendimento/OficinaMecanica.Atendimento.Infrastructure/
COPY src/Atendimento/OficinaMecanica.Atendimento.API/OficinaMecanica.Atendimento.API.csproj src/Atendimento/OficinaMecanica.Atendimento.API/

RUN dotnet restore src/Atendimento/OficinaMecanica.Atendimento.API/OficinaMecanica.Atendimento.API.csproj

COPY src/ src/

RUN dotnet publish src/Atendimento/OficinaMecanica.Atendimento.API/OficinaMecanica.Atendimento.API.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OficinaMecanica.Atendimento.API.dll"]
