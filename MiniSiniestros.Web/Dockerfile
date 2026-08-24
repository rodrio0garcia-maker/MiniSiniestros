FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MiniSiniestros.Web/MiniSiniestros.Web.csproj", "MiniSiniestros.Web/"]
COPY ["MiniSiniestros.Services/MiniSiniestros.Services.csproj", "MiniSiniestros.Services/"]
COPY ["MiniSiniestros.Data/MiniSiniestros.Data.csproj", "MiniSiniestros.Data/"]
COPY ["MiniSiniestros.Entities/MiniSiniestros.Entities.csproj", "MiniSiniestros.Entities/"]
COPY ["MiniSiniestros.ViewModels/MiniSiniestros.ViewModels.csproj", "MiniSiniestros.ViewModels/"]
COPY ["MiniSiniestros.Dto/MiniSiniestros.Dto.csproj", "MiniSiniestros.Dto/"]

RUN dotnet restore "MiniSiniestros.Web/MiniSiniestros.Web.csproj"

COPY . .
WORKDIR "/src/MiniSiniestros.Web"
RUN dotnet build "MiniSiniestros.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniSiniestros.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniSiniestros.Web.dll"]