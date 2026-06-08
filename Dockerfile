# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution
COPY . .

# Restore & publish
RUN dotnet restore "PipeVolt_Api/PipeVolt_Api.csproj"
RUN dotnet publish "PipeVolt_Api/PipeVolt_Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Cài libgdiplus cho System.Drawing (dùng cho upload ảnh)
RUN apt-get update && apt-get install -y libgdiplus libc6-dev && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Tạo thư mục wwwroot/images để mount volume upload ảnh
RUN mkdir -p /app/wwwroot/images/products /app/wwwroot/images/categories

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "PipeVolt_Api.dll"]
