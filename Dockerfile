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

# ⚠️ Nome EXATO do csproj (case-sensitive)
RUN dotnet restore BackendDesapegaja.csproj
RUN dotnet publish BackendDesapegaja.csproj -c Release -o /app/publish

# =========================
# Final
# =========================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# ⚠️ Nome EXATO do DLL
ENTRYPOINT ["dotnet", "BackendDesapegaja.dll"]
