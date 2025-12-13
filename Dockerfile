# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo da raiz (inclui o .csproj)
COPY . .

# ⚠️ NOME EXATO do csproj
RUN dotnet restore BackendDesapegaJa.csproj
RUN dotnet publish BackendDesapegaJa.csproj -c Release -o /app/publish

# =========================
# Final
# =========================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# ⚠️ NOME EXATO do DLL
ENTRYPOINT ["dotnet", "BackendDesapegaJa.dll"]
