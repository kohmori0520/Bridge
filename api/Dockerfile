# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# csproj だけ先にコピーして restore をキャッシュ可能にする
COPY Bridge.sln ./
COPY src/Bridge.Api/Bridge.Api.csproj src/Bridge.Api/
COPY src/Bridge.Domain/Bridge.Domain.csproj src/Bridge.Domain/
COPY src/Bridge.Infrastructure/Bridge.Infrastructure.csproj src/Bridge.Infrastructure/
COPY tests/Bridge.Tests/Bridge.Tests.csproj tests/Bridge.Tests/
RUN dotnet restore

# ソース全体をコピーしてビルド
COPY . .
RUN dotnet publish src/Bridge.Api/Bridge.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Fly.io の内部ポートは 8080 で揃えるのが慣例
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Bridge.Api.dll"]
