# Usa la imagen oficial de .NET SDK para la construcción de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Establece el directorio de trabajo
WORKDIR /app

# Copia el archivo .csproj y restaura las dependencias (esto se hace para aprovechar la cache de Docker)
COPY *.csproj ./
RUN dotnet restore

# Copia el resto del código fuente
COPY . ./

# Publica la aplicación en el directorio de salida
RUN dotnet publish -c Release -o /app/publish

# Usa una imagen más ligera para la ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Copia los archivos publicados desde la etapa de construcción
COPY --from=build /app/publish .

# Establece el punto de entrada para la aplicación
ENTRYPOINT ["dotnet", "EcommerceML.dll"]
