FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CRM.AplicacionWeb/CRM.AplicacionWeb.csproj", "CRM.AplicacionWeb/"]
COPY ["CRM.Control/CRM.Control.csproj", "CRM.Control/"]
COPY ["CRM.Datos/CRM.Datos.csproj", "CRM.Datos/"]
COPY ["CRM.Dependencia/CRM.Dependencia.csproj", "CRM.Dependencia/"]
COPY ["CRM.Entidad/CRM.Entidad.csproj", "CRM.Entidad/"]
COPY ["CRM.Proyecto/CRM.Proyecto.csproj", "CRM.Proyecto/"]
RUN dotnet restore "./CRM.AplicacionWeb/CRM.AplicacionWeb.csproj"
COPY . .
WORKDIR "/src/CRM.AplicacionWeb"
RUN dotnet build "./CRM.AplicacionWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CRM.AplicacionWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM.AplicacionWeb.dll"]
