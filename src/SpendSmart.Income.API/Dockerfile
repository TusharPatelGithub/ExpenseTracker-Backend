FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY "src/SpendSmart.Income.API/SpendSmart.Income.API.csproj" "src/SpendSmart.Income.API/"
RUN dotnet restore "src/SpendSmart.Income.API/SpendSmart.Income.API.csproj"
COPY . .
WORKDIR "/src/src/SpendSmart.Income.API"
RUN dotnet build "SpendSmart.Income.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpendSmart.Income.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpendSmart.Income.API.dll"]
