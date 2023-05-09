FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "backend.kapace/backend.kapace.csproj"
COPY . .
WORKDIR "/src/backend-kapace"
RUN dotnet build "backend.kapace.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "backend.kapace.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend-kapace.dll"]