# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# ソリューションと csproj だけ先にコピーして restore をキャッシュ可能にする
COPY Bridge.slnx ./
COPY src/Bridge.Api/Bridge.Api.csproj src/Bridge.Api/
COPY src/Bridge.Domain/Bridge.Domain.csproj src/Bridge.Domain/
COPY src/Bridge.Infrastructure/Bridge.Infrastructure.csproj src/Bridge.Infrastructure/
COPY tests/Bridge.Tests/Bridge.Tests.csproj tests/Bridge.Tests/
RUN dotnet restore Bridge.slnx

# ソース全体をコピーしてビルド
COPY . .
RUN dotnet publish src/Bridge.Api/Bridge.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-self-contained \
    /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Fly.io の内部ポートは 8080 で揃えるのが慣例
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "Bridge.Api.dll"]
