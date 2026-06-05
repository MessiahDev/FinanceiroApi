# ─────────────────────────────────────────
# Stage 1: Build
# ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY NuGet.Config ./
COPY FinanceiroApi.slnx ./
COPY src/FinanceiroApi.Domain/FinanceiroApi.Domain.csproj             src/FinanceiroApi.Domain/
COPY src/FinanceiroApi.Application/FinanceiroApi.Application.csproj   src/FinanceiroApi.Application/
COPY src/FinanceiroApi.Infrastructure/FinanceiroApi.Infrastructure.csproj src/FinanceiroApi.Infrastructure/
COPY src/FinanceiroApi.CrossCutting/FinanceiroApi.CrossCutting.csproj src/FinanceiroApi.CrossCutting/
COPY src/FinanceiroApi.API/FinanceiroApi.API.csproj                   src/FinanceiroApi.API/

RUN dotnet restore src/FinanceiroApi.API/FinanceiroApi.API.csproj

COPY src/ src/

RUN dotnet publish src/FinanceiroApi.API/FinanceiroApi.API.csproj \
    -c Release \
    -o /app/publish

# ─────────────────────────────────────────
# Stage 2: Runtime
# ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

RUN groupadd --system appgroup && \
    useradd --system --gid appgroup appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_PATHBASE=/scala/v1
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/scala/v1/health || exit 1

ENTRYPOINT ["dotnet", "FinanceiroApi.API.dll"]