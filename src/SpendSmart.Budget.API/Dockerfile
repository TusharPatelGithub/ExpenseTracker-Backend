FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY "src/SpendSmart.Budget.API/SpendSmart.Budget.API.csproj" "src/SpendSmart.Budget.API/"
RUN dotnet restore "src/SpendSmart.Budget.API/SpendSmart.Budget.API.csproj"
COPY . .
WORKDIR "/src/src/SpendSmart.Budget.API"
RUN dotnet build "SpendSmart.Budget.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpendSmart.Budget.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpendSmart.Budget.API.dll"]
