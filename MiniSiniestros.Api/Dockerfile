# Etapa 1: build - compila el proyecto con el SDK completo de .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos primero solo los .csproj de TODOS los proyectos que la Api
# necesita, para aprovechar el cache de capas de Docker - si el código
# cambia pero las dependencias no, Docker no vuelve a restaurar paquetes
COPY ["MiniSiniestros.Api/MiniSiniestros.Api.csproj", "MiniSiniestros.Api/"]
COPY ["MiniSiniestros.Services/MiniSiniestros.Services.csproj", "MiniSiniestros.Services/"]
COPY ["MiniSiniestros.Data/MiniSiniestros.Data.csproj", "MiniSiniestros.Data/"]
COPY ["MiniSiniestros.Entities/MiniSiniestros.Entities.csproj", "MiniSiniestros.Entities/"]
COPY ["MiniSiniestros.Dto/MiniSiniestros.Dto.csproj", "MiniSiniestros.Dto/"]

RUN dotnet restore "MiniSiniestros.Api/MiniSiniestros.Api.csproj"

# Ahora sí copiamos todo el código fuente y compilamos
COPY . .
WORKDIR "/src/MiniSiniestros.Api"
RUN dotnet build "MiniSiniestros.Api.csproj" -c Release -o /app/build

# Etapa 2: publish - genera los archivos finales listos para ejecutar
FROM build AS publish
RUN dotnet publish "MiniSiniestros.Api.csproj" -c Release -o /app/publish

# Etapa 3: runtime - imagen final, liviana, solo con el runtime
# (no el SDK completo) necesario para EJECUTAR, no compilar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniSiniestros.Api.dll"]