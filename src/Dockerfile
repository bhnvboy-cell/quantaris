FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Quantaris.Api/Quantaris.Api.csproj", "Quantaris.Api/"]
COPY ["src/Quantaris.Core/Quantaris.Core.csproj", "Quantaris.Core/"]
COPY ["src/Quantaris.Infrastructure/Quantaris.Infrastructure.csproj", "Quantaris.Infrastructure/"]
RUN dotnet restore "Quantaris.Api/Quantaris.Api.csproj"
COPY src/ .
WORKDIR "/src/Quantaris.Api"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Quantaris.Api.dll"]
