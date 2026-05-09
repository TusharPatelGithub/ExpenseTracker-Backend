FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY "src/SpendSmart.Report.API/SpendSmart.Report.API.csproj" "src/SpendSmart.Report.API/"
RUN dotnet restore "src/SpendSmart.Report.API/SpendSmart.Report.API.csproj"
COPY . .
WORKDIR "/src/src/SpendSmart.Report.API"
RUN dotnet build "SpendSmart.Report.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpendSmart.Report.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpendSmart.Report.API.dll"]
