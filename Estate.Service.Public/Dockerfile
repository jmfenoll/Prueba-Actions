FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["File.Service/File.Service.csproj", "File.Service/"]
RUN dotnet restore "File.Service/File.Service.csproj"
COPY . .
WORKDIR "/src/File.Service"
RUN dotnet build "File.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "File.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "File.Service.dll"]