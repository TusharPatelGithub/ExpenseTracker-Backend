FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY "src/SpendSmart.Expense.API/SpendSmart.Expense.API.csproj" "src/SpendSmart.Expense.API/"
RUN dotnet restore "src/SpendSmart.Expense.API/SpendSmart.Expense.API.csproj"
COPY . .
WORKDIR "/src/src/SpendSmart.Expense.API"
RUN dotnet build "SpendSmart.Expense.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpendSmart.Expense.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpendSmart.Expense.API.dll"]
